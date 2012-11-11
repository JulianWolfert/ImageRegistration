using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageRegistration2010
{
    class Line
    {
        //Steigung
        private double b;

        //Y-Abschnitt
        private double a;

        public double getB()
        {
            return b;
        }

        public void setB(double b)
        {
            this.b = b;
        }

        public double getA()
        {
            return a;
        }

        public void setA(double a)
        {
            this.a = a;
        }


        internal double calculateAngel(Line line2)
        {
            double tan_phi = Math.Abs((this.getB() - line2.getB()) / (1 + (this.getB() * line2.getB())));

            double arctan_bogen = Math.Atan(tan_phi);

            return (arctan_bogen * (180 / Math.PI));
        }
    }
}
