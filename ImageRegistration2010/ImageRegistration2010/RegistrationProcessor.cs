using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Single;
using System.IO;


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

        //Next neighbours for linear regression
        private int next_neighbours_regression = 150;
        //Lenght of the segment for finding the minima
        private int feature_intervall = 200;


        public Transformation calculateTransformation(List<Contour<Point>> contours_image1, List<Contour<Point>> contours_image2)
        {

            //Get the points of the FIRST (=0) contour
            Point[] points1 = contours_image1[0].ToArray();

            Dictionary<int, double> angle_at_pixel_image1 = new Dictionary<int, double>();

            //Calculate the angle at every pixel for picture 1
            for (int i = 0; i < points1.Length; i++)
            {
                double angle = calculateAngle(points1, i);
                angle_at_pixel_image1.Add(i, angle);
            }

            //Find the minimas vor picture 1
            List<int> minima_index_contour1 = findMinima(angle_at_pixel_image1, feature_intervall);

            //Create and store the features for picture 1
            List<Feature> features1 = new List<Feature>();
            for (int i = 0; i < minima_index_contour1.Count; i++)
            {
                int minima = minima_index_contour1[i];
                int minima_left = 0;
                int minima_right = 0;
                int pixel_to_next_left = 0;
                int pixel_to_next_right = 0;

                if (i == 0)
                {
                    minima_right = minima_index_contour1[i + 1];
                    minima_left = minima_index_contour1[minima_index_contour1.Count - 1];
                    pixel_to_next_right = minima_right - minima;
                    pixel_to_next_left = (angle_at_pixel_image1.Count - minima_left) + minima;
                }
                if (i == minima_index_contour1.Count - 1)
                {
                    minima_right = minima_index_contour1[0];
                    minima_left = minima_index_contour1[i - 1];
                    pixel_to_next_right = (angle_at_pixel_image1.Count - minima) + minima_right;
                    pixel_to_next_left = minima - minima_left;
                }
                if (i != 0 && i != (minima_index_contour1.Count - 1))
                {
                    minima_right = minima_index_contour1[i + 1];
                    minima_left = minima_index_contour1[i - 1];
                    pixel_to_next_left = minima - minima_left;
                    pixel_to_next_right = minima_right - minima;
                }

                Feature newFeature = new Feature();
                newFeature.point = new Point(points1[minima].X, points1[minima].Y);
                newFeature.index_in_contour = minima;
                newFeature.angle_at_pixel = angle_at_pixel_image1[minima];
                newFeature.angle_left = angle_at_pixel_image1[minima_left];
                newFeature.angle_right = angle_at_pixel_image1[minima_right];
                newFeature.pixel_to_next_left = pixel_to_next_left;
                newFeature.pixel_to_next_right = pixel_to_next_right;

                features1.Add(newFeature);

            }


            //Get the points of the FIRST (=0) contour
            Point[] points2 = contours_image2[0].ToArray();

            Dictionary<int, double> angle_at_pixel_image2 = new Dictionary<int, double>();

            //Calculate the angle at every pixel for picture 2
            for (int i = 0; i < points2.Length; i++)
            {
                double angle = calculateAngle(points2, i);
                angle_at_pixel_image2.Add(i, angle);
            }

            //Find the minimas vor picture 2
            List<int> minima_index_contour2 = findMinima(angle_at_pixel_image2, feature_intervall);

            //Create and store the features for picture 2
            List<Feature> features2 = new List<Feature>();
            for (int i = 0; i < minima_index_contour2.Count; i++)
            {
                int minima = minima_index_contour2[i];
                int minima_left = 0;
                int minima_right = 0;
                int pixel_to_next_left = 0;
                int pixel_to_next_right = 0;

                if (i == 0)
                {
                    minima_right = minima_index_contour2[i + 1];
                    minima_left = minima_index_contour2[minima_index_contour2.Count - 1];
                    pixel_to_next_right = minima_right - minima;
                    pixel_to_next_left = (angle_at_pixel_image2.Count - minima_left) + minima;
                }
                if (i == minima_index_contour2.Count - 1)
                {
                    minima_right = minima_index_contour2[0];
                    minima_left = minima_index_contour2[i - 1];
                    pixel_to_next_right = (angle_at_pixel_image2.Count - minima) + minima_right;
                    pixel_to_next_left = minima - minima_left;
                }
                if (i != 0 && i != (minima_index_contour2.Count - 1))
                {
                    minima_right = minima_index_contour2[i + 1];
                    minima_left = minima_index_contour2[i - 1];
                    pixel_to_next_left = minima - minima_left;
                    pixel_to_next_right = minima_right - minima;
                }

                Feature newFeature = new Feature();
                newFeature.point = new Point(points2[minima].X, points2[minima].Y);
                newFeature.index_in_contour = minima;
                newFeature.angle_at_pixel = angle_at_pixel_image2[minima];
                newFeature.angle_left = angle_at_pixel_image2[minima_left];
                newFeature.angle_right = angle_at_pixel_image2[minima_right];
                newFeature.pixel_to_next_left = pixel_to_next_left;
                newFeature.pixel_to_next_right = pixel_to_next_right;



                features2.Add(newFeature);

            }

            //Calculating the best matching features
            List<Feature> bestFeatures = calculateBestMatchingFeatures(features1, features2);


            List<Feature> bestFeatures2 = new List<Feature>();
            Feature f1 = new Feature();
            f1.point = new Point(503, 607);
            bestFeatures2.Add(f1);
            Feature f2 = new Feature();
            f2.point = new Point(405, 591);
            bestFeatures2.Add(f2);
            Feature f3 = new Feature();
            f3.point = new Point(705, 131);
            bestFeatures2.Add(f3);
            Feature f4 = new Feature();
            f4.point = new Point(405, 103);
            bestFeatures2.Add(f4);



            //Calculating the transformation for the best features
            Transformation transformation = calculateTransformationValues(bestFeatures);

            //Write angles to csv
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\\Users\\Jules\\Dropbox\\Semester 2\\Medizinische Bildverarbeitung" + "\\" + "angles.csv"))
            //{
            //    for (int i = 0; i < angle_at_pixel_image1.Count; i++)
            //    {
            //        String line;
            //        if (i < angle_at_pixel_image2.Count)
            //            line = angle_at_pixel_image1[i] + ";" + angle_at_pixel_image2[i];
            //        else
            //            line = angle_at_pixel_image1[i].ToString();
            //        file.WriteLine(line);
            //    }
            //}


            return transformation;
        }

        private Transformation calculateTransformationValues(List<Feature> bestFeatures)
        {
            int trans_x = bestFeatures[0].point.X - bestFeatures[1].point.X;
            int trans_y = bestFeatures[0].point.Y - bestFeatures[1].point.Y;

            System.Windows.Vector vector1 = new System.Windows.Vector(bestFeatures[2].point.X, bestFeatures[2].point.Y) - new System.Windows.Vector(bestFeatures[0].point.X, bestFeatures[0].point.Y);
            System.Windows.Vector vector2 = new System.Windows.Vector(bestFeatures[3].point.X, bestFeatures[3].point.Y) - new System.Windows.Vector(bestFeatures[1].point.X, bestFeatures[1].point.Y);

            double rotation_angle = System.Windows.Vector.AngleBetween(vector1, vector2);

            Transformation transformation = new Transformation();
            transformation.translation_x = trans_x;
            transformation.translation_y = trans_y;

            transformation.rotation = Convert.ToInt32(rotation_angle) * -1;
            transformation.rotation_center_x = bestFeatures[0].point.X;
            transformation.rotation_center_y = bestFeatures[0].point.Y;

            transformation.scale = 1.0;

            return transformation;
        }

        //Calculating the two best matching features from two list of features
        private List<Feature> calculateBestMatchingFeatures(List<Feature> features1, List<Feature> features2)
        {
            double min_diff = double.MaxValue;
            double min_diff_2nd = double.MaxValue;
            int index_feature1 = 0;
            int index_feature1_2nd = 0;
            int index_feature2 = 0;
            int index_feature2_2nd = 0;
            for (int i = 0; i < features1.Count; i++)
            {
                for (int j = 0; j < features2.Count; j++)
                {
                    double diff = features1[i].calculateDifferenceOnlyByAngle(features2[j]);
                    if (diff < min_diff)
                    {
                        min_diff_2nd = min_diff;
                        index_feature1_2nd = index_feature1;
                        index_feature2_2nd = index_feature2;
                        min_diff = diff;
                        index_feature1 = i;
                        index_feature2 = j;

                    }
                }
            }
            List<Feature> bestFeatures = new List<Feature>();
            bestFeatures.Add(features1[index_feature1]);
            bestFeatures.Add(features2[index_feature2]);
            bestFeatures.Add(features1[index_feature1_2nd]);
            bestFeatures.Add(features2[index_feature2_2nd]);

            return bestFeatures;
        }

        //Find the minima in a list of angles with a given intervall
        private List<int> findMinima(Dictionary<int, double> angle_at_pixel_image, int feature_intervall)
        {
            List<int> features = new List<int>();

            int segments;
            if (angle_at_pixel_image.Count % feature_intervall != 0)
                segments = angle_at_pixel_image.Count / feature_intervall + 1;
            else
                segments = angle_at_pixel_image.Count / feature_intervall;

            for (int i = 0; i < segments; i++)
            {
                int lowerbound = i * feature_intervall;
                int upperbound = (i * feature_intervall) + feature_intervall;
                int minima = lowerbound;
                for (int j = lowerbound; j < upperbound; j++)
                {
                    if (j == angle_at_pixel_image.Count)
                        break;
                    else
                    {
                        double test = angle_at_pixel_image[j];
                        double test2 = angle_at_pixel_image[minima];

                        if (angle_at_pixel_image[j] < angle_at_pixel_image[minima])
                            minima = j;
                    }
                }

                features.Add(minima);
            }


            return features;
        }

        //Calculates the angel at a pixel
        private double calculateAngle(Point[] points, int point)
        {

            //Calculation for the line backwards
            float[] xdata_line_back = new float[next_neighbours_regression + 1];
            float[] ydata_line_back = new float[next_neighbours_regression + 1];

            xdata_line_back[0] = points[point].X;
            ydata_line_back[0] = points[point].Y;

            int n = 1;
            for (int i = point - next_neighbours_regression; i < point; i++)
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
            //Line line_back = linearRegression(xdata_line_back,ydata_line_back);
            Line line_back = linearRegressionThroughPixel(xdata_line_back, ydata_line_back, points[point]);


            //Calculation for the line forwards
            float[] xdata_line_for = new float[next_neighbours_regression + 1];
            float[] ydata_line_for = new float[next_neighbours_regression + 1];

            xdata_line_for[0] = points[point].X;
            ydata_line_for[0] = points[point].Y;

            n = 1;
            for (int i = point + next_neighbours_regression; i > point; i--)
            {
                if (i > points.Length - 1)
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
            //Line line_for = linearRegression(xdata_line_for, ydata_line_for);
            Line line_for = linearRegressionThroughPixel(xdata_line_for, ydata_line_for, points[point]);


            //Calculate the intersection of the two lines
            Coordinates intersection = calculateIntersection(line_for, line_back);
            //Calculate the center for the forward points
            Coordinates center_for = calculateCenter(xdata_line_for, ydata_line_for);
            //Calculate the center for the backward points
            Coordinates center_back = calculateCenter(xdata_line_back, ydata_line_back);

            //Create vector on the line with start intersection, end center point
            System.Windows.Vector vector_for = new System.Windows.Vector(center_for.x, center_for.y) - new System.Windows.Vector(intersection.x, intersection.y);
            System.Windows.Vector vector_back = new System.Windows.Vector(center_back.x, center_back.y) - new System.Windows.Vector(intersection.x, intersection.y);

            //System.Windows.Vector vector_for = findCorrectVectorWithSmallerAngle(line_for, intersection, xdata_line_for[1], ydata_line_for[1]);
            //System.Windows.Vector vector_back = findCorrectVectorWithSmallerAngle(line_back, intersection, xdata_line_back[1], ydata_line_back[1]);

            //Calculat angle between vectors
            double angleBetween = System.Windows.Vector.AngleBetween(vector_for, vector_back);

            if (angleBetween < 0)
                angleBetween = 180 + Math.Abs(angleBetween);

            return Math.Abs(angleBetween);
        }

        //Find the correct vector with the help of a reference vector - Use this instead of center vector
        private System.Windows.Vector findCorrectVectorWithSmallerAngle(Line line, Coordinates intersection, float contour_x, float contour_y)
        {
            System.Windows.Vector refVector = new System.Windows.Vector(contour_x, contour_y) - new System.Windows.Vector(intersection.x, intersection.y);

            Coordinates line_plus_x = new Coordinates(intersection.x + 1, line.getB() * (intersection.x + 1) + line.getA());
            System.Windows.Vector lineVector_plus = new System.Windows.Vector(line_plus_x.x, line_plus_x.y) - new System.Windows.Vector(intersection.x, intersection.y);

            Coordinates line_minus_x = new Coordinates(intersection.x - 1, line.getB() * (intersection.x - 1) + line.getA());
            System.Windows.Vector lineVector_minus = new System.Windows.Vector(line_minus_x.x, line_minus_x.y) - new System.Windows.Vector(intersection.x, intersection.y);

            double angle1 = Math.Abs(System.Windows.Vector.AngleBetween(refVector, lineVector_plus));
            double angle2 = Math.Abs(System.Windows.Vector.AngleBetween(refVector, lineVector_minus));

            ////double angle1 = System.Windows.Vector.AngleBetween(lineVector_plus,refVector);
            //double angle1 = System.Windows.Vector.AngleBetween(refVector, lineVector_plus);
            //double angle2 = System.Windows.Vector.AngleBetween(lineVector_minus, refVector);

            if (angle1 < angle2)
                return lineVector_plus;
            else
                return lineVector_minus;
        }

        //Calculate the intersection point for two lines
        private Coordinates calculateIntersection(Line line_for, Line line_back)
        {
            double x = (line_back.getA() - line_for.getA()) / (line_for.getB() - line_back.getB());
            double y = line_for.getB() * x + line_for.getA();
            return (new Coordinates(x, y));
        }

        //Calculate the center of a given set of points
        private Coordinates calculateCenter(float[] xdata, float[] ydata)
        {
            Coordinates center = new Coordinates();

            double x_coord = 0;
            for (int i = 0; i < xdata.Length; i++)
            {
                x_coord = x_coord + xdata[i];
            }

            center.x = x_coord / xdata.Length;

            double y_coord = 0;
            for (int i = 0; i < ydata.Length; i++)
            {
                y_coord = y_coord + ydata[i];
            }

            center.y = y_coord / ydata.Length;


            return center;

        }

        //Create a line with for a given set of points with linear regression
        private Line linearRegression(float[] xdata, float[] ydata)
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

        //Create a line with for a given set of points with linear regression - through a given point
        //Based on: http://christoph.ruegg.name/blog/2012/9/9/linear-regression-mathnet-numerics.html
        private Line linearRegressionThroughPixel(float[] xdata, float[] ydata, Point p)
        {
            // build matrices
            var X = DenseMatrix.CreateFromColumns(new[] { new DenseVector(xdata.Length, 1), new DenseVector(xdata) });
            var y = new DenseVector(ydata);

            // solve
            var s = X.QR().Solve(y);

            //Steigung
            double m = s[1];

            Line line = new Line();
            line.setB(m);
            line.setA(m * (-1 * (p.X)) + p.Y);

            return line;
        }

        //Registration of the contours
        public Bitmap registrationContour(Transformation t, Bitmap contour_image1, Bitmap contour_image2)
        {

            Bitmap trans_image2 = new Bitmap(contour_image1.Width, contour_image1.Height);
            Graphics g = Graphics.FromImage(trans_image2);


            g.TranslateTransform((float)t.rotation_center_x, (float)t.rotation_center_y);
            g.RotateTransform(t.rotation);
            g.ScaleTransform((float)t.scale, (float)t.scale);
            g.TranslateTransform(-(float)t.rotation_center_x, -(float)t.rotation_center_y);

            g.TranslateTransform(t.translation_x, t.translation_y);

            g.DrawImage(contour_image2, new Point(0, 0));


            Color blank = Color.FromArgb(0, 0, 0, 0);

            Bitmap registrated_contours = new Bitmap(contour_image1.Width, contour_image1.Height);

            for (int i = 0; i < contour_image1.Width; i++)
            {
                for (int j = 0; j < contour_image1.Height; j++)
                {
                    Color pixel1 = contour_image1.GetPixel(i, j);
                    Color pixel2 = trans_image2.GetPixel(i, j);

                    if (pixel1.ToArgb() != blank.ToArgb() && pixel2.ToArgb() != blank.ToArgb())
                        registrated_contours.SetPixel(i, j, Color.Yellow);
                    else
                    {
                        if (pixel1.ToArgb() == blank.ToArgb() && pixel2.ToArgb() != blank.ToArgb())
                            registrated_contours.SetPixel(i, j, Color.Blue);
                        if (pixel1.ToArgb() != blank.ToArgb() && pixel2.ToArgb() == blank.ToArgb())
                            registrated_contours.SetPixel(i, j, Color.Red);
                    }
                }
            }



            return registrated_contours;

        }

        //Registration of the original images
        public Bitmap registrationBitmap(Transformation t, Bitmap A, Bitmap B)
        {
            Bitmap trans_imageB = new Bitmap(A.Width, A.Height);
            Graphics g = Graphics.FromImage(trans_imageB);


            g.TranslateTransform((float)t.rotation_center_x, (float)t.rotation_center_y);
            g.RotateTransform(t.rotation);
            g.ScaleTransform((float)t.scale, (float)t.scale);
            g.TranslateTransform(-(float)t.rotation_center_x, -(float)t.rotation_center_y);

            g.TranslateTransform(t.translation_x, t.translation_y);

            g.DrawImage(B, new Point(0, 0));


            //Merging
            Color blank = Color.FromArgb(0, 0, 0, 0);

            Bitmap registrated_bitmaps = new Bitmap(A.Width, A.Height);

            for (int i = 0; i < A.Width; i++)
            {
                for (int j = 0; j < A.Height; j++)
                {
                    Color pixel1 = A.GetPixel(i, j);
                    Color pixel2 = trans_imageB.GetPixel(i, j);

                    if (pixel1.ToArgb() != blank.ToArgb() && pixel2.ToArgb() != blank.ToArgb())
                    {
                        int a_value = (pixel1.A + pixel2.A) / 2;
                        int r_value = (pixel1.R + pixel2.R) / 2;
                        int g_value = (pixel1.G + pixel2.G) / 2;
                        int b_value = (pixel1.B + pixel2.B) / 2;
                        Color merged_pixel = Color.FromArgb(a_value, r_value, g_value, b_value);
                        registrated_bitmaps.SetPixel(i, j, merged_pixel);
                    }
                    else
                    {
                        if (pixel1.ToArgb() == blank.ToArgb() && pixel2.ToArgb() != blank.ToArgb())
                            registrated_bitmaps.SetPixel(i, j, pixel2);
                        if (pixel1.ToArgb() != blank.ToArgb() && pixel2.ToArgb() == blank.ToArgb())
                            registrated_bitmaps.SetPixel(i, j, pixel1);
                    }
                }
            }

            return registrated_bitmaps;

        }
    }
}
