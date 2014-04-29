using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Emgu.CV;
using ImageRegistration2010;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

namespace ImageRegistrationConsole
{
    class ImageProcessor
    {
        //Filteroptions
        private bool noiseFilter = false;
        private bool filterContoursBySize = false;

        //Parameters
        private int minContourLength = 10;
        private int minContourArea = 10;
        private double minFormFactor = 0.5;
        private int cannyThreshold = 10;

        private bool equalizeHist = false;
        private bool blur = false;
        private int adaptiveThresholdBlockSize = 4;
        private double adaptiveThresholdParameter = 1.2d;
        private bool addCanny = false;



        public ImageProcessor()
        {

        }


        //Create binary image with otsu threshold
        public Bitmap createBinaryOtsu(Bitmap img)
        {
            Image<Bgra, Byte> image = new Image<Bgra, Byte>(img);

            if (Config.dilateBefore)
                image._Dilate(Config.dilate);
            

            //image._SmoothGaussian(51);
            image = image.SmoothMedian(Config.median);
            //image = image.SmoothBlur(10, 10);



            image._Dilate(Config.dilate);
            //image._Erode(7);


            //Vorder und Hintergrundtrennung mit OTSU.
            Image<Gray, Byte> greyImage = image.Convert<Gray,Byte>();

            Image<Gray, Byte> otsuImage = new Image<Gray, Byte>(img.Size);           

            CvInvoke.cvThreshold(greyImage, otsuImage, 0, 255, Emgu.CV.CvEnum.THRESH.CV_THRESH_OTSU);


            return otsuImage.ToBitmap();
        }



        //Create contour
        public List<Contour<Point>> findContoursWithOpenCV(Bitmap img)
        {
            Image<Gray, Byte> grayFrame = new Image<Gray, byte>(img);

            //find contours
            var sourceContours = grayFrame.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST);
            //filter contours
            List<Contour<Point>> contours = NoFilterContours(sourceContours, grayFrame, grayFrame.Width, grayFrame.Height);

            List<Contour<Point>> onlyMaxContour = new List<Contour<Point>>();

            int maxcontour = 0;
            for (int i = 1; i < contours.Count - 1; i++)
            {
                if (contours[i].Area > contours[maxcontour].Area)
                    maxcontour = i;
            }

            onlyMaxContour.Add(contours[maxcontour]);

            return onlyMaxContour;


        }

        //Utilfunction for filter contours
        private List<Contour<Point>> FilterContours(Contour<Point> contours, Image<Gray, byte> cannyFrame, int frameWidth, int frameHeight)
        {
            int maxArea = frameWidth * frameHeight / 5;
            var c = contours;
            List<Contour<Point>> result = new List<Contour<Point>>();
            while (c != null)
            {
                if (filterContoursBySize)
                    if (c.Total < minContourLength ||
                        c.Area < minContourArea || c.Area > maxArea ||
                        c.Area / c.Total <= minFormFactor)
                        goto next;

                if (noiseFilter)
                {
                    Point p1 = c[0];
                    Point p2 = c[(c.Total / 2) % c.Total];
                    if (cannyFrame[p1].Intensity <= double.Epsilon && cannyFrame[p2].Intensity <= double.Epsilon)
                        goto next;
                }
                result.Add(c);

            next:
                c = c.HNext;
            }

            return result;
        }

        private List<Contour<Point>> NoFilterContours(Contour<Point> contours, Image<Gray, byte> cannyFrame, int frameWidth, int frameHeight)
        {
            int maxArea = frameWidth * frameHeight / 5;
            var c = contours;
            List<Contour<Point>> result = new List<Contour<Point>>();
            while (c != null)
            {
                result.Add(c);
                c = c.HNext;
            }

            return result;
        }


        //Not used

        //Create grayscale image - Version 1
        public Bitmap createGreyImage(Bitmap img)
        {

            Bitmap original = new Bitmap(img);

            //make an empty bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    //get the pixel from the original image
                    Color originalColor = original.GetPixel(i, j);

                    //create the grayscale version of the pixel
                    int grayScale = (int)((originalColor.R * .3) + (originalColor.G * .59)
                        + (originalColor.B * .11));

                    //create the color object
                    Color newColor = Color.FromArgb(grayScale, grayScale, grayScale);

                    //set the new image's pixel to the grayscale version
                    newBitmap.SetPixel(i, j, newColor);
                }
            }

            return newBitmap;
        }
        //Create grayscale image - Version 2
        public Bitmap MakeGrayscale(Bitmap img)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(img.Width, img.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][] 
            {
              new float[] {.3f, .3f, .3f, 0, 0},
              new float[] {.59f, .59f, .59f, 0, 0},
              new float[] {.11f, .11f, .11f, 0, 0},
              new float[] {0, 0, 0, 1, 0},
              new float[] {0, 0, 0, 0, 1}
            });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height),
               0, 0, img.Width, img.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }


        //Not Working properly
        private int findNextPixel(List<Pixel> contourPixel, Bitmap img, int direction)
        {
            Pixel currentPixel = contourPixel[contourPixel.Count - 1];
            int x = currentPixel.getX();
            int y = currentPixel.getY();

            Pixel nextPixel = new Pixel();
            //int newDirection = -1;
            if (direction == 4)
            {
                if (img.GetPixel(x - 1, y) == Color.FromArgb(0, 0, 0))
                {
                    nextPixel.setX(x - 1);
                    nextPixel.setY(y);
                }
            }

            return 0;
        }
        private List<Pixel> findContour(Bitmap img)
        {
            List<Pixel> contourPixel = new List<Pixel>();

            int height = img.Height;
            int width = img.Width;

            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    Color c = img.GetPixel(j, i);

                    //Ersten Schwarzen Pixel gefunden
                    if (c == Color.FromArgb(0, 0, 0))
                    {
                        Pixel startPixel = new Pixel();
                        startPixel.setX(j);
                        startPixel.setY(i);
                        contourPixel.Add(startPixel);

                        //Da ich von links nach rechts laufe
                        int direction = 4;

                        Pixel nextPixel = new Pixel();

                        do
                        {
                            direction = searchNextPixel(contourPixel, img, direction);
                            if (direction == -1) break;
                            Console.WriteLine("New Direction: " + direction);
                            Console.WriteLine("New Pixel: " + contourPixel[contourPixel.Count - 1].getX() + " " + contourPixel[contourPixel.Count - 1].getY());
                        } while ((contourPixel[contourPixel.Count - 1] != startPixel));

                        Bitmap testbmp = new Bitmap(886, 248);
                        for (int k = 0; k < contourPixel.Count; k++)
                        {
                            testbmp.SetPixel(contourPixel[k].getX(), contourPixel[k].getY(), Color.FromArgb(0, 0, 0));
                        }

                        testbmp.Save("C:\\Users\\Jules\\Documents\\Medizinische Bildverarbeitung\\Pictures\\test.png");

                    }


                }
            }

            return contourPixel;
        }
        private int searchNextPixel(List<Pixel> pixels, Bitmap img, int direction)
        {
            Pixel lastPixel = pixels.Last();
            if (direction == 4)
            {
                if (check5(pixels, img, lastPixel.getX(), lastPixel.getY())) return 2;
                if (check6(pixels, img, lastPixel.getX(), lastPixel.getY())) return 4;
                if (check7(pixels, img, lastPixel.getX(), lastPixel.getY())) return 4;
                if (check0(pixels, img, lastPixel.getX(), lastPixel.getY())) return 6;
                if (check1(pixels, img, lastPixel.getX(), lastPixel.getY())) return 6;
                if (check2(pixels, img, lastPixel.getX(), lastPixel.getY())) return 0;
                if (check3(pixels, img, lastPixel.getX(), lastPixel.getY())) return 0;
            }
            if (direction == 2)
            {
                if (check3(pixels, img, lastPixel.getX(), lastPixel.getY())) return 0;
                if (check4(pixels, img, lastPixel.getX(), lastPixel.getY())) return 2;
                if (check5(pixels, img, lastPixel.getX(), lastPixel.getY())) return 2;
                if (check6(pixels, img, lastPixel.getX(), lastPixel.getY())) return 4;
                if (check7(pixels, img, lastPixel.getX(), lastPixel.getY())) return 4;
                if (check0(pixels, img, lastPixel.getX(), lastPixel.getY())) return 6;
                if (check1(pixels, img, lastPixel.getX(), lastPixel.getY())) return 6;
            }
            if (direction == 0)
            {
                if (check1(pixels, img, lastPixel.getX(), lastPixel.getY())) return 6;
                if (check2(pixels, img, lastPixel.getX(), lastPixel.getY())) return 0;
                if (check3(pixels, img, lastPixel.getX(), lastPixel.getY())) return 0;
                if (check4(pixels, img, lastPixel.getX(), lastPixel.getY())) return 2;
                if (check5(pixels, img, lastPixel.getX(), lastPixel.getY())) return 2;
                if (check6(pixels, img, lastPixel.getX(), lastPixel.getY())) return 4;
                if (check7(pixels, img, lastPixel.getX(), lastPixel.getY())) return 4;
            }
            if (direction == 6)
            {
                if (check7(pixels, img, lastPixel.getX(), lastPixel.getY())) return 4;
                if (check0(pixels, img, lastPixel.getX(), lastPixel.getY())) return 6;
                if (check1(pixels, img, lastPixel.getX(), lastPixel.getY())) return 6;
                if (check2(pixels, img, lastPixel.getX(), lastPixel.getY())) return 0;
                if (check3(pixels, img, lastPixel.getX(), lastPixel.getY())) return 0;
                if (check4(pixels, img, lastPixel.getX(), lastPixel.getY())) return 2;
                if (check5(pixels, img, lastPixel.getX(), lastPixel.getY())) return 2;
            }
            return -1;
        }

        private bool check1(List<Pixel> pixels, Bitmap img, int x, int y)
        {
            Color c = img.GetPixel(x + 1, y - 1);
            if (c == Color.FromArgb(0, 0, 0))
            {
                Pixel newPixel = new Pixel();
                newPixel.setX(x + 1);
                newPixel.setY(y - 1);
                pixels.Add(newPixel);
                return true;
            }
            else
                return false;
        }
        private bool check2(List<Pixel> pixels, Bitmap img, int x, int y)
        {
            Color c = img.GetPixel(x, y - 1);
            if (c == Color.FromArgb(0, 0, 0))
            {
                Pixel newPixel = new Pixel();
                newPixel.setX(x);
                newPixel.setY(y - 1);
                pixels.Add(newPixel);
                return true;
            }
            else
                return false;
        }
        private bool check3(List<Pixel> pixels, Bitmap img, int x, int y)
        {
            Color c = img.GetPixel(x - 1, y - 1);
            if (c == Color.FromArgb(0, 0, 0))
            {
                Pixel newPixel = new Pixel();
                newPixel.setX(x - 1);
                newPixel.setY(y - 1);
                pixels.Add(newPixel);
                return true;
            }
            else
                return false;
        }
        private bool check4(List<Pixel> pixels, Bitmap img, int x, int y)
        {
            Color c = img.GetPixel(x - 1, y);
            if (c == Color.FromArgb(0, 0, 0))
            {
                Pixel newPixel = new Pixel();
                newPixel.setX(x - 1);
                newPixel.setY(y);
                pixels.Add(newPixel);
                return true;
            }
            else
                return false;
        }
        private bool check5(List<Pixel> pixels, Bitmap img, int x, int y)
        {
            Color c = img.GetPixel(x - 1, y + 1);
            if (c == Color.FromArgb(0, 0, 0))
            {
                Pixel newPixel = new Pixel();
                newPixel.setX(x - 1);
                newPixel.setY(y + 1);
                pixels.Add(newPixel);
                return true;
            }
            else
                return false;
        }
        private bool check6(List<Pixel> pixels, Bitmap img, int x, int y)
        {
            Color c = img.GetPixel(x, y + 1);
            if (c == Color.FromArgb(0, 0, 0))
            {
                Pixel newPixel = new Pixel();
                newPixel.setX(x);
                newPixel.setY(y + 1);
                pixels.Add(newPixel);
                return true;
            }
            else
                return false;
        }
        private bool check7(List<Pixel> pixels, Bitmap img, int x, int y)
        {
            Color c = img.GetPixel(x + 1, y + 1);
            if (c == Color.FromArgb(0, 0, 0))
            {
                Pixel newPixel = new Pixel();
                newPixel.setX(x + 1);
                newPixel.setY(y + 1);
                pixels.Add(newPixel);
                return true;
            }
            else
                return false;
        }
        private bool check0(List<Pixel> pixels, Bitmap img, int x, int y)
        {
            Color c = img.GetPixel(x + 1, y);
            if (c == Color.FromArgb(0, 0, 0))
            {
                Pixel newPixel = new Pixel();
                newPixel.setX(x + 1);
                newPixel.setY(y);
                pixels.Add(newPixel);
                return true;
            }
            else
                return false;
        }


    }
}
