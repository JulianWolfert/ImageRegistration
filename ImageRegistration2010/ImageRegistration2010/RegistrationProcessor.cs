using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Single;

namespace ImageRegistration2010
{
    class RegistrationProcessor
    {

        public bool registration(List<Contour<Point>> contours_image1, List<Contour<Point>> contours_image2)
        {

            Point[] points = contours_image1[0].ToArray();

            Dictionary<int,double> angle_at_pixel = new Dictionary<int,double>();

            for (int i = 0; i < points.Length; i++)
            {
                double angle = calculateAngle(points, i);
                angle_at_pixel.Add(i, angle);
            }

            List<KeyValuePair<int, double>> myList = angle_at_pixel.ToList();

            myList.Sort(
                delegate(KeyValuePair<int, double> firstPair,
                KeyValuePair<int, double> nextPair)
                {
                    return firstPair.Value.CompareTo(nextPair.Value);
                }
            );

            return true;
        }

        private double calculateAngle(Point[] points, int point)
        {
            float[] xdata_line1 = new float[10];
            float[] ydata_line1 = new float[10];

            int n=0;
            for (int i=point-10; i < point; i++)
            {
                if (i < 0)
                {
                    xdata_line1[n] = points[i + (points.Length-1)].X;
                    ydata_line1[n] = points[i + (points.Length-1)].Y;
                }
                else
                {
                    xdata_line1[n] = points[i].X;
                    ydata_line1[n] = points[i].Y;
                }
                n++;
            }
            Line line1 = linearRegression(xdata_line1,ydata_line1);


            float[] xdata_line2 = new float[10];
            float[] ydata_line2 = new float[10];
            n=0;
            for (int i=point+10; i > point; i--)
            {
                if (i > points.Length-1)
                {
                    int test = i - (points.Length - 1);
                    xdata_line2[n] = points[i - (points.Length-1)].X;
                    ydata_line2[n] = points[i - (points.Length-1)].Y;
                }
                else
                {
                    xdata_line2[n] = points[i].X;
                    ydata_line2[n] = points[i].Y;
                }
                n++;
            }
            Line line2 = linearRegression(xdata_line2, ydata_line2);


            double angle = Math.Round(line1.calculateAngel(line2),2);
            
            return angle;
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
    }
}
