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
        internal void exportToCSV(List<Contour<Point>> contourPixel_image1, string outputfolder, string filename)
        {
            string aFileName = outputfolder + "\\" + filename;
            FileStream aFileStream = new FileStream(aFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            StreamWriter m_StreamWriter = new StreamWriter(aFileStream);
            for (int i = 0; i < contourPixel_image1.Count; i++)
            {
                m_StreamWriter.Write(contourPixel_image1[0][i].X.ToString());
                m_StreamWriter.Write(";");
                m_StreamWriter.Write(contourPixel_image1[0][i].Y.ToString());
                m_StreamWriter.Write("\n");
            }
        }

        internal Bitmap exportToImage(List<Contour<Point>> contourPixel_image, string outputfolder, string filename, int height, int width)
        {      
            Bitmap newbmp = new Bitmap(width, height);

            //for (int i = 0; i < contourPixel_image.Count-1; i++)
           // {
                Point[] points = contourPixel_image[0].ToArray();
                for (int k = 0; k < points.Length; k++)
                {
                    newbmp.SetPixel(points[k].X, points[k].Y, Color.FromArgb(0, 0, 0));
                }
           // }

            
            newbmp.Save(outputfolder + "\\" + filename, ImageFormat.Png);

            return newbmp;
        }
    }
}
