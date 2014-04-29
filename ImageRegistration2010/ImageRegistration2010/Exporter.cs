using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using System.Drawing.Imaging;
using System.IO;
using AForge.Imaging.Filters;

namespace ImageRegistration2010
{
    class Exporter
    {

        //Export the contours to a csv file
        public void exportToCSV(List<Contour<Point>> contourPixel_image1, string outputfolder, string filename)
        {

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@outputfolder + "\\" + filename))
            {
                Point[] points = contourPixel_image1[0].ToArray();
                for (int i = points.Length - 1; i >= 0; i--)
                {
                    String line = points[i].X + "," + points[i].Y;
                    file.WriteLine(line);
                }
                file.WriteLine(points[points.Length - 1].X + "," + points[points.Length - 1].Y);
            }
        }

        //Create a bitmap of the pixel contour list and save it
        public Bitmap exportToImage(List<Contour<Point>> contourPixel_image, string outputfolder, string filename, int height, int width)
        {
            Bitmap newbmp = new Bitmap(width, height);


            //Only the FIRST contour!
            Point[] points = contourPixel_image[0].ToArray();
            for (int k = 0; k < points.Length; k++)
            {
                newbmp.SetPixel(points[k].X, points[k].Y, Color.FromArgb(0, 0, 0));
            }



            newbmp.Save(outputfolder + "\\" + filename, ImageFormat.Png);

            return newbmp;
        }

        public void exportFeatures(List<Feature> image1_features, List<Feature> image2_features)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Config.folderpath + "\\" + "angles.csv"))
            {
                int size;
                if (image1_features.Count < image2_features.Count)
                    size = image1_features.Count;
                else
                    size = image2_features.Count;

                for (int i = 0; i < size; i++)
                {
                    String line;
                    line = image1_features.ElementAt(i).angle_at_pixel + ";" + image2_features.ElementAt(i).angle_at_pixel;
                    file.WriteLine(line);
                }
            }
        }

    }
}
