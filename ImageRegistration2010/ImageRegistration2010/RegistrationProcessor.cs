using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra.Single;
using System.IO;
using System.Collections;
using System.Linq;


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

        private List<Feature> bestFeatureStorage1 = new List<Feature>();
        private List<Feature> bestFeatureStorage2 = new List<Feature>();

        public Transformation calculateTransformation(List<Contour<Point>> contours_image1, List<Contour<Point>> contours_image2)
        {

            List<Feature> image1_features = new List<Feature>();
            List<Feature> image2_features = new List<Feature>();

            //Get the points of the FIRST (=0) contour
            Point[] points1 = contours_image1[0].ToArray();

            //Calculate the angle at every pixel for picture 1
            for (int i = 0; i < points1.Length; i++)
            {
                AngleCalculator angleCalculator = new AngleCalculator();
                double angle = angleCalculator.calculateAngle (points1, i);
                Feature f = new Feature();
                f.point = points1[i];
                f.angle_at_pixel = angle;
                image1_features.Add(f);
            }

            //Get the points of the FIRST (=0) contour
            Point[] points2 = contours_image2[0].ToArray();

            //Calculate the angle at every pixel for picture 2
            for (int i = 0; i < points2.Length; i++)
            {
                AngleCalculator angleCalculator = new AngleCalculator();
                double angle = angleCalculator.calculateAngle(points2, i);
                Feature f = new Feature();
                f.point = points2[i];
                f.angle_at_pixel = angle;
                image2_features.Add(f);
            }

            GraphAnalyzer graphanalyzer = new GraphAnalyzer();

            //Normalize the shorter contour to longer one
            List<Feature> image1_features_normalized;
            List<Feature> image2_features_normalized;
            if (image1_features.Count < image2_features.Count)
            {
                image1_features_normalized = graphanalyzer.normalize(image2_features, image1_features);
                image2_features_normalized = image2_features;
            }
            else
            {
                image2_features_normalized = graphanalyzer.normalize(image1_features, image2_features);
                image1_features_normalized = image1_features;
            }

            //Moves the contour(graph) of image1 to best matching
            image1_features_normalized = graphanalyzer.matchGraphs(image1_features_normalized, image2_features_normalized);

            //Get the best Features by using extrema [0] image1 --> [1] image2, ....
            List<Feature> bestFeatures = graphanalyzer.findBestFeaturesWithExtrema(image1_features_normalized, image2_features_normalized);
            bestFeatureStorage1.Add(bestFeatures[0]);
            bestFeatureStorage1.Add(bestFeatures[2]);
            bestFeatureStorage2.Add(bestFeatures[1]);
            bestFeatureStorage2.Add(bestFeatures[3]);

            //Calculating the transformation for the best features
            Transformation transformation = calculateTransformationValues(bestFeatures);

            //Exporting the graph
            Exporter exporter = new Exporter();
            exporter.exportFeatures(image1_features_normalized, image2_features_normalized);

            return transformation;
        }


        //Calculating the  Transformation Values for 2 Features
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

            double length_diff = vector1.Length / vector2.Length;

            transformation.scale = length_diff;

            return transformation;
        }

        //Registration of the contours
        public Bitmap registrationContour(Transformation t, Bitmap contour_image1, Bitmap contour_image2)
        {
            contour_image1 = addFeaturePoints(contour_image1,bestFeatureStorage1);
            contour_image2 = addFeaturePoints(contour_image2,bestFeatureStorage2);

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

        private Bitmap addFeaturePoints(Bitmap registrated_contours, List<Feature> features)
        {
            for (int i = 0; i < features.Count; i++)
            {
                registrated_contours.SetPixel(features[i].point.X + 3, features[i].point.Y + 3, Color.Black);
                registrated_contours.SetPixel(features[i].point.X + 2, features[i].point.Y + 2, Color.Black);
                registrated_contours.SetPixel(features[i].point.X + 1, features[i].point.Y + 1, Color.Black);
                registrated_contours.SetPixel(features[i].point.X - 1, features[i].point.Y - 1, Color.Black);
                registrated_contours.SetPixel(features[i].point.X - 2, features[i].point.Y - 2, Color.Black);
                registrated_contours.SetPixel(features[i].point.X - 3, features[i].point.Y - 3, Color.Black);
                registrated_contours.SetPixel(features[i].point.X - 3, features[i].point.Y + 3, Color.Black);
                registrated_contours.SetPixel(features[i].point.X - 2, features[i].point.Y + 2, Color.Black);
                registrated_contours.SetPixel(features[i].point.X - 1, features[i].point.Y + 1, Color.Black);
                registrated_contours.SetPixel(features[i].point.X + 1, features[i].point.Y - 1, Color.Black);
                registrated_contours.SetPixel(features[i].point.X + 2, features[i].point.Y - 2, Color.Black);
                registrated_contours.SetPixel(features[i].point.X + 3, features[i].point.Y - 3, Color.Black); 
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
