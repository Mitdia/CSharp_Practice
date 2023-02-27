using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    public struct DataItem
    {
        public DataItem(double x, double field_first, double field_second)
        {
            this.xCoordinate = x;
            this.field = new double[] {field_first, field_second};
        }
        public double xCoordinate { get; set; }
        public double[] field { get; set; }
        string ToLongString(string format)
        {
            string CoordFormated = String.Format(format, xCoordinate);
            string Field1Formated = String.Format(format, field[0]);
            string Field2Formated = String.Format(format, field[1]);
            return $"x = {CoordFormated}, field = [{Field1Formated}, {Field2Formated}]";
        }
        public override string ToString()
        {
            return $"x = {xCoordinate}, field = [{field[0]}, {field[1]}]";
        }
    }
}
