using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Lab1
{
    internal class V3DataUGridSpline
    {
        public V3DataUGrid sourceData { get; private set; }
        public double[] secondDerivativeOnSegmentEnds { get; private set; }
        public double[] nodes { get; private set; }
        public double[] firstFieldNodeValue { get; private set; }
        public double[] secondFieldNodeValue { get; private set; }
        public double[] firstFieldNodeSecondDerivative{ get; private set; }
        public double[] secondFieldNodeSecondDerivative { get; private set; }
        public double[] segmentForIntegrationEnds { get; private set; }
        public double firstFieldIntegralValue { get; private set; }
        public double secondFieldIntegralValue { get; private set; }
        public V3DataUGridSpline(V3DataUGrid sourceData, double[] secondDerivativeOnSegmentEnds, double[] nodes, double[] segmentEnds)
        {
            this.sourceData = sourceData;
            this.secondDerivativeOnSegmentEnds = secondDerivativeOnSegmentEnds;
            this.nodes = nodes;
            this.segmentForIntegrationEnds = segmentEnds;
            double[] interpolationSegmentEnds = { sourceData.gridParams.leftEnd, sourceData.gridParams.rightEnd };
            double[] leftIntegralEnds = { segmentForIntegrationEnds[0] };
            double[] rightIntegralEnds = { segmentForIntegrationEnds[1] };
            double[] nodesValues = new double[nodes.Length * 4];
            double[] integralValues = new double[2];            
            int error = interpolate(sourceData.gridParams.nodesNum,
                interpolationSegmentEnds,
                this.sourceData.firstFieldNodeValue.Concat(this.sourceData.secondFieldNodeValue).ToArray(),
                secondDerivativeOnSegmentEnds,
                nodes.Length, nodes,
                leftIntegralEnds, rightIntegralEnds,
                nodesValues,
                integralValues
                );
            this.firstFieldIntegralValue = integralValues[0];
            this.secondFieldIntegralValue = integralValues[1];
            this.firstFieldNodeValue = new double[this.nodes.Length];
            this.firstFieldNodeSecondDerivative = new double[this.nodes.Length];
            this.secondFieldNodeValue = new double[this.nodes.Length];
            this.secondFieldNodeSecondDerivative = new double[this.nodes.Length];
            for (int i = 0; i < this.nodes.Length; ++i)
            {
                this.firstFieldNodeValue[i] = nodesValues[i * 2];
                this.firstFieldNodeSecondDerivative[i] = nodesValues[i * 2 + 1];
            }
            for (int i = 0; i < this.nodes.Length; ++i)
            {
                this.secondFieldNodeValue[i] = nodesValues[this.nodes.Length * 2 + i * 2];
                this.secondFieldNodeSecondDerivative[i] = nodesValues[this.nodes.Length * 2 + i * 2 + 1];
            }
        }

        [DllImport("C:\\Users\\knorr\\source\\repos\\Lab1\\x64\\Debug\\SplinesDLL.dll")]
        public static extern int interpolate(int amount,
            double[] segmentEnds, double[] fieldValues,
            double[] secondDerivativeOnSegmentEnds,
            int newNodesAmount, double[] nodes,
            double[] leftIntegralEnds, double[] rightIntegralEnds,
            double[] nodeValues, double[] integralValues);
       
        public string ToLongString(string format)
        {
            StringBuilder info = new StringBuilder();
            info.Append($"Base object:\n{this.sourceData.ToLongString(format)}\n");
            info.Append($"Second derivative on segment ends:" +
                        $" [{this.secondDerivativeOnSegmentEnds[0]}, {this.secondDerivativeOnSegmentEnds[1]}]\n");
            info.Append($"Integrals on segment [{this.segmentForIntegrationEnds[0]}, {this.segmentForIntegrationEnds[1]}]:" +
                        $"\n\tfirst field: {this.firstFieldIntegralValue}" +
                        $"\n\tsecond field: {this.secondFieldIntegralValue}\n\n\n");
            info.Append($"Nodes info:\n\n");

            for (int i = 0; i < this.nodes.Length; ++i)
            {
                string xCoordinateFormatted = String.Format(format, this.nodes[i]);
                string firstFieldFormatted = String.Format(format, this.firstFieldNodeValue[i]);
                string secondFieldFormatted = String.Format(format, this.secondFieldNodeValue[i]);
                string firstFieldDerivativeFormatted = String.Format(format, this.firstFieldNodeSecondDerivative[i]);
                string secondFieldDerivativeFormatted = String.Format(format, this.secondFieldNodeSecondDerivative[i]);
                info.Append($"\tIn node with coordinate = {xCoordinateFormatted}:\n" +
                    $"\t\tFirst force field value = {firstFieldFormatted}," +
                    $" Second force field value = {secondFieldFormatted} \n" +
                    $"\t\tFirst field second derivative = {firstFieldDerivativeFormatted}" +
                    $" Second field second derivative = {secondFieldDerivativeFormatted}\n\n");
            }
            return info.ToString();

        }

        public bool Save(string filename, string format)
        {
            bool answer = false;
            try
            {
                using (StreamWriter writetext = new StreamWriter(filename))
                {
                    writetext.WriteLine(this.ToLongString(format));
                }
                answer = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Save failed\n " + ex.Message);
            }
            return answer;
        }

        public void PrintDifference(V3DataUGridSpline other, string format) 
        {
            StringBuilder info = new StringBuilder();
            info.Append($"Integrals difference on segment [{this.segmentForIntegrationEnds[0]}, {this.segmentForIntegrationEnds[1]}]:" +
                        $"\n\tfirst field: {this.firstFieldIntegralValue - other.firstFieldIntegralValue}" +
                        $"\n\tsecond field: {this.secondFieldIntegralValue - other.secondFieldIntegralValue}\n\n\n");
            info.Append($"Nodes difference info:\n\n");

            for (int i = 0; i < this.nodes.Length; ++i)
            {
                string xCoordinateFormatted = String.Format(format, this.nodes[i]);
                string firstFieldFormatted = String.Format(format, this.firstFieldNodeValue[i] - other.firstFieldNodeValue[i]);
                string secondFieldFormatted = String.Format(format, this.secondFieldNodeValue[i] - other.secondFieldNodeValue[i]);
                string firstFieldDerivativeFormatted = String.Format(format, this.firstFieldNodeSecondDerivative[i] - other.firstFieldNodeSecondDerivative[i]);
                string secondFieldDerivativeFormatted = String.Format(format, this.secondFieldNodeSecondDerivative[i] - other.secondFieldNodeSecondDerivative[i]);
                info.Append($"\tIn node with coordinate = {xCoordinateFormatted}:\n" +
                    $"\t\tFirst force field value = {firstFieldFormatted}," +
                    $" Second force field value = {secondFieldFormatted} \n" +
                    $"\t\tFirst field second derivative = {firstFieldDerivativeFormatted}" +
                    $" Second field second derivative = {secondFieldDerivativeFormatted}\n\n");
                Console.WriteLine(info.ToString());
            }
        }

    }
}
