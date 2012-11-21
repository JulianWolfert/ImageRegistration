using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Single;


namespace ImageRegistration2010
{

    public struct Coordinates
    {
        public double x, y;

        public Coordinates(double p1, double p2)
        {
            x = p1;
            y = p2;
        }

    }
    class RegistrationProcessor
    {

        public bool registration(List<Contour<Point>> contours_image1, List<Contour<Point>> contours_image2)
        {

            Point[] points1  = contours_image1[0].ToArray();

            Dictionary<int,double> angle_at_pixel_image1 = new Dictionary<int,double>();

            for (int i = 0; i < points1.Length; i++)
            {
                double angle = calculateAngle(points1, i);
                angle_at_pixel_image1.Add(i, angle);
            }

            List<PixelUtil> pixelList_image1 = new List<PixelUtil>();
            for (int i = 0; i < angle_at_pixel_image1.Count; i++)
            {
                PixelUtil pU = new PixelUtil();
                pU.point = points1[i];
                pU.index_in_contour = i;
                pU.angle_at_pixel = angle_at_pixel_image1[i];
                if (i == 0)
                {
                    pU.angle_forw = angle_at_pixel_image1[i + 1];
                    pU.angle_back = angle_at_pixel_image1[points1.Length - 1];
                }
                if (i == points1.Length - 1)
                {
                    pU.angle_forw = angle_at_pixel_image1[0];
                    pU.angle_back = angle_at_pixel_image1[i - 1];
                }
                pixelList_image1.Add(pU);
            }
                    

            Point[] points2 = contours_image2[0].ToArray();

            Dictionary<int, double> angle_at_pixel_image2 = new Dictionary<int, double>();

            for (int i = 0; i < points2.Length; i++)
            {
                double angle = calculateAngle(points2, i);
                angle_at_pixel_image2.Add(i, angle);
            }

            List<PixelUtil> pixelList_image2 = new List<PixelUtil>();
            for (int i = 0; i < angle_at_pixel_image2.Count; i++)
            {
                PixelUtil pU = new PixelUtil();
                pU.point = points2[i];
                pU.index_in_contour = i;
                pU.angle_at_pixel = angle_at_pixel_image2[i];
                if (i == 0)
                {
                    pU.angle_forw = angle_at_pixel_image2[i + 1];
                    pU.angle_back = angle_at_pixel_image2[points2.Length - 1];
                }
                if (i == points2.Length - 1)
                {
                    pU.angle_forw = angle_at_pixel_image2[0];
                    pU.angle_back = angle_at_pixel_image2[i - 1];
                }
                if (i != 0 && i != (points2.Length - 1))
                {
                    pU.angle_forw = angle_at_pixel_image2[i + 1];
                    pU.angle_back = angle_at_pixel_image2[i - 1];
                }
                pixelList_image2.Add(pU);
            }

            //Minimale Differenzberechnung von allen Pixeln

            double min_diff = double.MaxValue;
            int index_image1;
            int index_image2;
            for (int i = 0; i < pixelList_image1.Count; i++)
            {
                for (int j = 0; j < pixelList_image2.Count; j++)
                {
                    if (pixelList_image1[i].angle_at_pixel > 45 && pixelList_image2[j].angle_at_pixel > 45)
                    {
                        if (pixelList_image1[i].calculateDifference(pixelList_image2[j]) < min_diff)
                        {
                            min_diff = pixelList_image1[i].calculateDifference(pixelList_image2[j]);
                            index_image1 = i;
                            index_image2 = j;
                        }
                    }
                }
            }

            return true;
        }

        private double calculateAngle(Point[] points, int point)
        {
            int next_neighbours = 50;

            float[] xdata_line_back = new float[next_neighbours+1];
            float[] ydata_line_back = new float[next_neighbours+1];

            xdata_line_back[0] = points[point].X;
            ydata_line_back[0] = points[point].Y;

            int n=1;
            for (int i=point-next_neighbours; i < point; i++)
            {
                if (i < 0)
                {
                    xdata_line_back[n] = points[i + (points.Length)].X;
                    ydata_line_back[n] = points[i + (points.Length)].Y;
                }
                else
                {
                    xdata_line_back[n] = points[i].X;
                    ydata_line_back[n] = points[i].Y;
                }
                n++;
            }
            Line line_back = linearRegression(xdata_line_back,ydata_line_back);


            float[] xdata_line_for = new float[next_neighbours+1];
            float[] ydata_line_for = new float[next_neighbours+1];

            xdata_line_for[0] = points[point].X;
            ydata_line_for[0] = points[point].Y;

            n=1;
            for (int i=point+next_neighbours; i > point; i--)
            {
                if (i > points.Length-1)
                {
                    xdata_line_for[n] = points[i - (points.Length)].X;
                    ydata_line_for[n] = points[i - (points.Length)].Y;
                }
                else
                {
                    xdata_line_for[n] = points[i].X;
                    ydata_line_for[n] = points[i].Y;
                }
                n++;
            }
            Line line_for = linearRegression(xdata_line_for, ydata_line_for);

            Coordinates intersection = calculateIntersection(line_for,line_back);

            System.Windows.Vector vector_for = findCorrectVector(line_for, intersection, xdata_line_for[0], ydata_line_for[0]);
            System.Windows.Vector vector_back = findCorrectVector(line_back, intersection, xdata_line_back[0], ydata_line_back[0]);

            return System.Windows.Vector.AngleBetween(vector_for, vector_back);

            
           
            //double angle = Math.Round(line1.calculateAngel(line2),2);
            
            //return angle;
        }

        private System.Windows.Vector findCorrectVector(Line line, Coordinates intersection, float contour_x, float contour_y)
        {
            System.Windows.Vector refVector = new System.Windows.Vector(contour_x, contour_y) - new System.Windows.Vector(intersection.x, intersection.y);
            
            Coordinates line_plus_x = new Coordinates(intersection.x + 1,line.getB() * (intersection.x + 1) + line.getA());
            System.Windows.Vector lineVector_plus = new System.Windows.Vector(line_plus_x.x, line_plus_x.y) - new System.Windows.Vector(intersection.x, intersection.y);

            Coordinates line_minus_x = new Coordinates(intersection.x - 1, line.getB() * (intersection.x - 1) + line.getA());
            System.Windows.Vector lineVector_minus = new System.Windows.Vector(line_minus_x.x, line_minus_x.y) - new System.Windows.Vector(intersection.x, intersection.y);

            double angle1 = System.Windows.Vector.AngleBetween(lineVector_plus,refVector);
            double angle2 = System.Windows.Vector.AngleBetween(refVector, lineVector_minus);

            if (angle1 < angle2)
                return lineVector_plus;
            else
                return lineVector_plus;
        }

        private Coordinates calculateIntersection(Line line_for, Line line_back)
        {
            double x = (line_back.getA() - line_for.getA()) / (line_for.getB() - line_back.getB());
            double y = line_for.getB() * x + line_for.getA();
            return (new Coordinates(x, y));
        }

        public Line linearRegression(float[] xdata, float[] ydata)
        {
            // build matrices
            var X = DenseMatrix.CreateFromColumns(new[] { new DenseVector(xdata.Length, 1), new DenseVector(xdata) });
            var y = new DenseVector(ydata);

            // solve
            var p = X.QR().Solve(y);

            Line line = new Line();
            //Y-Abschnitt
            line.setA(p[0]);
            //Steigung
            line.setB(p[1]);

            return line;
        }

        public Bitmap findCornersHarris(Bitmap img)
        {

            Image<Gray, Byte> grayImage = new Image<Gray, Byte>(img);

            PointF[][] points = grayImage.GoodFeaturesToTrack(100, 0.1, 5, 3);

            return img;
        }


    }
}
