using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageRegistration2010;
using Emgu.CV;

namespace ImageRegistrationConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("######## IMAGE-REGISTRATION V1.0 ########\n");
            //String folderPath = "C:\\Users\\Jules\\Documents\\Medizinische Bildverarbeitung\\Pictures";
            String folderPath = "C:\\Users\\Jules\\Dropbox\\Semester 2\\Medizinische Bildverarbeitung\\Pictures";
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            Console.WriteLine("Lade Bilder aus dem Verzeichnis: " + folderPath);
            int count = dir.GetFiles().Count();
            Console.WriteLine(count + " Bilder gefunden\n");

            //Verarbeite Bilderpaare 0-9
            for (int i = 0; i <= 9; i++)
            {

                //Hole Bilderpaar
                FileInfo[] files = dir.GetFiles(i +"*");
           
                if (files.Length != 0)
                {
                    Console.WriteLine("\nVearbeite Bilder " + files[0] + " und " + files[1]);
                    Bitmap image1 = new Bitmap(folderPath + "\\" + files[0]);
                    Bitmap image2 = new Bitmap(folderPath + "\\" + files[1]);
                    String outputfolder = folderPath + "\\" + i;
                    Directory.CreateDirectory(outputfolder);
                    startImageProcessing(image1, image2, outputfolder);
                }

            }


        }

        private static void startImageProcessing(Bitmap A, Bitmap B, string outputfolder)
        {
            ImageProcessor imageProcessor = new ImageProcessor();
            Exporter exporter = new Exporter();
            RegistrationProcessor registrationProcessor = new RegistrationProcessor();


            ////Erstelle Grauwertbild von A und speichere ab
            //Bitmap image1grey = imageProcessor.MakeGrayscale(A);
            ////image1grey.Save(outputfolder + "\\Agrey.png", ImageFormat.Png);

            ////Erstelle Grauwertbild von B und speichere ab
            //Bitmap image2grey = imageProcessor.MakeGrayscale(B);
            ////image2grey.Save(outputfolder + "\\Bgrey.png", ImageFormat.Png);

            ////Erstelle Binaerbild von A mit Otsu-Methode und speichere ab
            Bitmap image1bin = imageProcessor.createBinaryOtsu(A);
            image1bin.Save(outputfolder + "\\Abin.png", ImageFormat.Png);

            ////Erstelle Binaerbild von A mit Otsu-Methode und speichere ab
            Bitmap image2bin = imageProcessor.createBinaryOtsu(B);
            image2bin.Save(outputfolder + "\\Bbin.png", ImageFormat.Png);


            List<Contour<Point>> contours_image1 = imageProcessor.findContoursWithOpenCV(image1bin);
            //exporter.exportToCSV(contours_image1, outputfolder, "contourA.csv");
            exporter.exportToImage(contours_image1, outputfolder, "contourA.png", A.Height, A.Width);

            List<Contour<Point>> contours_image2 = imageProcessor.findContoursWithOpenCV(image2bin);
            //exporter.exportToCSV(contours_image2, outputfolder, "contourB.csv");
            exporter.exportToImage(contours_image2, outputfolder, "contourB.png", B.Height, B.Width);

            registrationProcessor.registration(contours_image1, contours_image2);
           
        }
    }
}
