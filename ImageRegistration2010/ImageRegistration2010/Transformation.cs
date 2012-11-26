using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageRegistration2010
{
    //Represents a transformation
    class Transformation
    {
        public int translation_x {get; set;}
        public int translation_y { get; set; }
        public int scale { get; set; }
        public int rotation { get; set; } 
    }
}
