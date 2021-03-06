﻿using System;
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
            Console.WriteLine("#########################################");
            Console.WriteLine("######## IMAGE-REGISTRATION V2.0 ########");
            Console.WriteLine("########## By Julian Wolfert ############");
            Console.WriteLine("#########################################");
            Console.WriteLine("########## -r= NachbarnRegression #######");
            Console.WriteLine("########## -m= MedianValue ##############");
            Console.WriteLine("########## -d= DilateValue ##############");
            Console.WriteLine("########## -b= DilateBeforeMedian #######");
            Console.WriteLine("#########################################\n");

            bool error = false;

            //folderPath = "C:\\Users\\Jules\\Dropbox\\Semester 2\\Medizinische Bildverarbeitung\\PairsRotate90";
            Config.folderpath = "C:\\Users\\Jules\\Dropbox\\Semester 2\\Medizinische Bildverarbeitung\\Test";
            //folderPath = "C:\\Users\\Jules\\Dropbox\\Semester 2\\Medizinische Bildverarbeitung\\pairs";
            //folderPath = "C:\\Users\\Jules\\Dropbox\\Semester 2\\Medizinische Bildverarbeitung\\pictures";

            if (args.Length != 0)
                Config.folderpath = args[0];
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i].Substring(0, 3).ToLower())
                {
                    case "-r=":
                        Config.linear_regression_neighbours = Convert.ToInt32(args[i].Substring(3));
                        break;
                    case "-m=":
                        Config.median = Convert.ToInt32(args[i].Substring(3));
                        break;
                    case "-d=":
                        Config.dilate = Convert.ToInt32(args[i].Substring(3));
                        break;
                    case "-b=":
                        Config.dilateBefore = Convert.ToBoolean(args[i].Substring(3));
                        break;
                    default:
                        error = true;
                        break;
                }
            }
            if (error)
            {
                Console.WriteLine("Unknown option");
                return;
            }

            String folderPath = Config.folderpath;

            Console.WriteLine("Nachbarn fuer Regressionsberechnung: " + Config.linear_regression_neighbours);
            Console.WriteLine("Medianfilter-Wert: " + Config.median);
            Console.WriteLine("Dilate-Wert: " + Config.dilate);
            Console.WriteLine("Dilate vor Median: " + Config.dilateBefore);
            Console.WriteLine("");

            DirectoryInfo dir = new DirectoryInfo(folderPath);
            Console.WriteLine("Lade Bilder aus dem Verzeichnis: " + folderPath);
            int count = dir.GetFiles().Count();
            Console.WriteLine(count + " Bilder gefunden\n");


            //Check picture pair 0-99
            for (int i = 1; i <= 99; i++)
            {
                String pattern;
                String outputfolder;
                if (i < 10)
                {
                    pattern = "00" + i + "*";
                    outputfolder = "00" + i;
                }
                else
                {
                    pattern = "0" + i + "*";
                    outputfolder = "0" + i;
                }

                //Get pair of picture
                FileInfo[] files = dir.GetFiles(pattern);

                if (files.Length == 2)
                {
                    Console.WriteLine("\nVearbeite Bilder " + files[0] + " und " + files[1]);
                    Bitmap image1 = new Bitmap(folderPath + "\\" + files[0]);
                    Bitmap image2 = new Bitmap(folderPath + "\\" + files[1]);
                    String complete_outputfolder = folderPath + "\\" + outputfolder;
                    Directory.CreateDirectory(complete_outputfolder);
                    startImageProcessing(image1, image2, complete_outputfolder);
                }

            }


        }

        private static void startImageProcessing(Bitmap A, Bitmap B, string outputfolder)
        {
            ImageProcessor imageProcessor = new ImageProcessor();
            Exporter exporter = new Exporter();
            RegistrationProcessor registrationProcessor = new RegistrationProcessor();



            //Create binary image of A with otsu threshold and save
            Bitmap image1bin = imageProcessor.createBinaryOtsu(A);
            image1bin.Save(outputfolder + "\\Abin.png", ImageFormat.Png);

            //Create binary image of B with otsu threshold and save
            Bitmap image2bin = imageProcessor.createBinaryOtsu(B);
            image2bin.Save(outputfolder + "\\Bbin.png", ImageFormat.Png);

            //Find contour of A - Only longest!
            List<Contour<Point>> contours_image1 = imageProcessor.findContoursWithOpenCV(image1bin);
            exporter.exportToCSV(contours_image1, outputfolder, "contourA.csv");
            Bitmap contour_image1 = exporter.exportToImage(contours_image1, outputfolder, "contourA.png", A.Height, A.Width);

            //Find contour of B - Only longest!
            List<Contour<Point>> contours_image2 = imageProcessor.findContoursWithOpenCV(image2bin);
            exporter.exportToCSV(contours_image2, outputfolder, "contourB.csv");
            Bitmap contour_image2 = exporter.exportToImage(contours_image2, outputfolder, "contourB.png", B.Height, B.Width);

            //Calculate transformation with help of the two contours
            try
            {
                Transformation t1 = registrationProcessor.calculateTransformation(contours_image1, contours_image2);
                //Start registration of contours
                Bitmap registrated_Contours = registrationProcessor.registrationContour(t1, contour_image1, contour_image2);
                registrated_Contours.Save(outputfolder + "\\contoursR.png", ImageFormat.Png);

                //Start registration of orginial images
                Bitmap registrated_originals = registrationProcessor.registrationBitmap(t1, A, B);
                registrated_originals.Save(outputfolder + "\\registration.png", ImageFormat.Png);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }





        }
    }
}
