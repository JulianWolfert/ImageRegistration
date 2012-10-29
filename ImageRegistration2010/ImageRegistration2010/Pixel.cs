using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageRegistration2010
{
    class Pixel
    {
        private int x;
        private int y;

        public Pixel()
        {
            this.x = -1;
            this.y = -1;
        }

        public int getX()
        {
            return x;
        }

        public void setX(int x)
        {
            this.x = x;
        }

        public int getY()
        {
            return y;
        }

        public void setY(int y)
        {
            this.y = y;
        }

        public override bool Equals(Object obj)
        {
            Pixel pixelObj = obj as Pixel;
            if (pixelObj == null)
                return false;
            else
                return (x.Equals(pixelObj.x) && y.Equals(pixelObj.y));
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
