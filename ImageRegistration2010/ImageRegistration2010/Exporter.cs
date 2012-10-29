using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV;
using System.Drawing.Imaging;

namespace ImageRegistration2010
{
    class Exporter
    {
        internal void exportToCSV(List<Contour<Point>> contourPixel_image1, string outputfolder, string filename)
        {
            throw new NotImplementedException();
        }

        internal void exportToImage(List<Contour<Point>> contourPixel_image, string outputfolder, string filename, int height, int width)
        {
            Bitmap newbmp = new Bitmap(width, height);

            for (int i = 0; i < contourPixel_image.Count; i++)
            {
                Point[] points = contourPixel_image[i].ToArray();
                for (int k = 0; k < points.Length; k++)
                {
                    newbmp.SetPixel(points[k].X, points[k].Y, Color.FromArgb(0, 0, 0));
                }
            }

            newbmp.Save(outputfolder + "\\" + filename, ImageFormat.Png);
        }
    }
}
