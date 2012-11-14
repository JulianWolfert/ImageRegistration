using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;


namespace ImageRegistration2010
{
    class PixelUtil
    {

        public Point point {get; set;} 
        public int index_in_contour {get; set;} 
        public double angle_at_pixel {get; set;} 
        public double angle_back {get; set;} 
        public double angle_forw {get; set;} 

        public double calculateDifference(PixelUtil anotherPixel)
        {
            double diff = Math.Abs(anotherPixel.angle_at_pixel - this.angle_at_pixel);
            diff = diff + (Math.Abs(anotherPixel.angle_back - this.angle_back));
            diff = diff + (Math.Abs(anotherPixel.angle_forw - this.angle_forw));

            return diff;
        }

    }
}
