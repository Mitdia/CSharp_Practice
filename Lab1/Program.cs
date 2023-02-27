// See https://aka.ms/new-console-template for more information
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lab1
{
    static class ForceField
    {
        static public double[] getFieldValues(double xCoordinate)
        {
            return new double[] {Math.Pow(xCoordinate, 3),
                                 Math.Pow(xCoordinate, 3)};
        }
    }


    public delegate double[] F2Double(double x);

    public enum FuncEnum {getFieldValues}
    class Program
    {
        static void TestSaveLoad()
        {
            string format = "{0:0.00}";
            double[] nUGridParams = { 1.5, -1.5, Math.PI };
            F2Double forceField = ForceField.getFieldValues;
            V3DataNUGrid v3DataNUGrid = new V3DataNUGrid("Test!", DateTime.Now, nUGridParams, forceField);
            Console.WriteLine(v3DataNUGrid.ToLongString(format));
            if (V3DataNUGrid.Save("Test", v3DataNUGrid))
            {
                Console.WriteLine("Saved sucsessfully!\n\n--------------------------------\n");
                V3DataNUGrid? loadedObject = V3DataNUGrid.Load("Lab1.dll");
                if (loadedObject != null)
                {
                    Console.WriteLine("Loaded Sucsessfully!\n");
                    Console.WriteLine(loadedObject.ToLongString(format));
                }
            }
        }
        static void TestLINQ()
        {
            string format = "{0:0.00}";
            F2Double forceField = ForceField.getFieldValues;
            V3DataCollection v3DataCollection = new V3DataCollection();
            v3DataCollection.AddDefaults();
            V3DataList v3DataList = new V3DataList("Empty DataList", DateTime.Now);
            v3DataCollection.Add(v3DataList);
            double[] nUGridParams = { };
            V3DataNUGrid v3DataNUGrid = new V3DataNUGrid("Empty DataNUGrid", DateTime.Now, nUGridParams, forceField);
            v3DataCollection.Add(v3DataNUGrid);
            UniformGrid grid = new UniformGrid(0, 0, 0);
            V3DataUGrid v3DataUGrid = new V3DataUGrid("Empty DataUGrid", DateTime.Now, grid, forceField);
            v3DataCollection.Add(v3DataUGrid);
            Console.WriteLine(v3DataCollection.ToLongString(format));
            DataItem? dataItemWithMaxCoordinate = v3DataCollection.MaxCoordinate;
            if (dataItemWithMaxCoordinate != null)
            {
                Console.WriteLine($"Data Item with max coordinate {dataItemWithMaxCoordinate}\n");
            } else
            {
                Console.WriteLine("There is no elements in the V3DataCollection");
            }
            
            Console.WriteLine($"Nodes with unique coordinates:");            
            Console.WriteLine(string.Join(", ", v3DataCollection.nonUniqueCoords) + "\n");
            Console.WriteLine($"Grouped by coordinate:");
            foreach (var group in v3DataCollection.groupedByCoord)
            {
                Console.WriteLine($"Key = {group.Key}");
                foreach (DataItem item in group) Console.WriteLine(item);
            }

        }
        static void Main(string[] args)
        {
            string format = "{0:0.00}";
            UniformGrid grid = new UniformGrid(1, 2, 3);
            V3DataUGrid source = new V3DataUGrid("Source", DateTime.Now, grid, ForceField.getFieldValues);
            Console.WriteLine(source.ToLongString(format));
            double[] nodes = { 1, 2, 3, 4, 5 };
            double[] secondDerivativeOnSegmentEnds1 = { 6, 30 };
            double[] segmentForIntegrationEnds = { 1, 3 };
            V3DataUGridSpline firstObject = new V3DataUGridSpline(source, secondDerivativeOnSegmentEnds1, nodes, segmentForIntegrationEnds);
            firstObject.Save("info.txt", format);
            double[] secondDerivativeOnSegmentEnds2 = { 6, 30 };
            V3DataUGridSpline secondObject = new V3DataUGridSpline(source, secondDerivativeOnSegmentEnds2, nodes, segmentForIntegrationEnds);
            firstObject.PrintDifference(secondObject, format);

        }
    }
    
    
}

