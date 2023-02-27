using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    internal struct UniformGrid
    {
        public UniformGrid(double leftEnd, double step, int nodesNum)
        {
            this.leftEnd = leftEnd;
            this.step = step;
            this.nodesNum = nodesNum;
            this.rightEnd = leftEnd + step * (nodesNum - 1);
        }
        public double leftEnd { get; set; }
        public double step { get; set; }
        public int nodesNum { get; set; }
        public double rightEnd { get; }

        public string ToLongString(string format)
        {
            string leftEndFormatted = String.Format(format, leftEnd);
            string rightEndFormatted = String.Format(format, rightEnd);
            string stepFormatted = String.Format(format, step);
            return $"segment = [{leftEndFormatted}, {rightEndFormatted}], with {this.nodesNum} nodes and with step {stepFormatted}";
        }

        public override string ToString()
        {
            return $"segment = [{this.leftEnd}, {this.rightEnd}], with {this.nodesNum} nodes and with step {this.step}";
        }

    }
}
