﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;


namespace ImageRegistration2010
{
    //Represents a feature
    class Feature
    {

        public Point point {get; set;} 
        public int index_in_contour {get; set;} 
        public double angle_at_pixel {get; set;} 
        public double angle_left {get; set;} 
        public double angle_right {get; set;} 

        //Calculates the difference to another feature
        public double calculateDifference(Feature anotherFeature)
        {
            double diff = Math.Abs(anotherFeature.angle_at_pixel - this.angle_at_pixel);
            diff = diff + (Math.Abs(anotherFeature.angle_left - this.angle_left));
            diff = diff + (Math.Abs(anotherFeature.angle_right - this.angle_right));

            return diff;
        }

    }
}
