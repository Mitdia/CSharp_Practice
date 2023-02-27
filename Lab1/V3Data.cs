using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.IO.Pipes;
using System.Reflection;

namespace Lab1
{
    public abstract class V3Data : IEnumerable<DataItem>
    {
        public string identificator { get; set; }
        public DateTime timeAquired { get; set; }
        public V3Data(string identificator, DateTime timeAquired)
        {
            this.identificator = identificator;
            this.timeAquired = timeAquired;
        }
        public abstract double MaxDistance { get; }
        public abstract string ToLongString(string format);
        public override string ToString()
        {
            return $"Id = {identificator}, Aquired at {timeAquired}";
        }
        public abstract IEnumerator<DataItem> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }   

    public class V3DataList : V3Data
    {
        public List<DataItem> dataItems { get; set; }
        public V3DataList(string identificator, DateTime timeAquired) : base(identificator, timeAquired)
        {
            dataItems = new List<DataItem>();
        }
        public bool Add(double x, double v1, double v2)
        {
            if (dataItems.Any(item => item.xCoordinate == x))
            {
                return false;
            } else
            {
                dataItems.Add(new DataItem(x, v1, v2));
                return true;
            }
        }
        public void AddDefaults(int nItems, FuncEnum F)
        {
            MethodInfo? method = typeof(ForceField).GetMethod(F.ToString());
            if (method is null) {
                return;
            }
            F2Double function = (F2Double) Delegate.CreateDelegate(typeof(F2Double), method);
            AddDefaults(nItems, function);
        }
        public void AddDefaults(int nItems, F2Double F)
        {
            double x = -nItems / 2;
            double[] fieldValues;
            while (nItems > 0)
            {
                fieldValues = F(x);
                if (Add(x, fieldValues[0], fieldValues[1]))
                {
                    nItems--;
                }
                x++;
            }
        }
        public override double MaxDistance => dataItems.Max(item => item.xCoordinate) - dataItems.Min(item => item.xCoordinate);

        public override IEnumerator<DataItem> GetEnumerator()
        {
            return dataItems.GetEnumerator();
        }
        public override string ToString()
        {
            return $"Type: {GetType().ToString()}\nBase info: {base.ToString()}\nNumber of nodes: {dataItems.Count()}\n";
        }
        public override string ToLongString(string format)
        {
            StringBuilder itemsCoordinateInfo = new StringBuilder();
            foreach (DataItem item in dataItems)
            {
                string xCoordinateFormatted = String.Format(format, item.xCoordinate);
                string firstFieldFormatted = String.Format(format, item.field[0]);
                string secondFieldFormatted = String.Format(format, item.field[1]);
                itemsCoordinateInfo.Append($"\tIn node with coordinate = {xCoordinateFormatted}: First force field value = {firstFieldFormatted}, Second force field value = {secondFieldFormatted}\n");
            }
            return $"{ToString()}Node coordinates:\n{itemsCoordinateInfo}\n";
        }
    }

    class V3DataUGrid: V3Data
    {
        public UniformGrid gridParams { get; set; }
        public double[] firstFieldNodeValue { get; set; }
        public double[] secondFieldNodeValue { get; set; }
        public V3DataUGrid(string identificator, DateTime timeAquired, UniformGrid gridParams, F2Double F): base(identificator, timeAquired)
        {
            this.gridParams = gridParams;
            firstFieldNodeValue = new double[gridParams.nodesNum];
            secondFieldNodeValue = new double[gridParams.nodesNum];
            double[] fieldValues = new double[2];
            for (int i = 0; i < gridParams.nodesNum; i++)
            {
                fieldValues = F(gridParams.leftEnd + i * gridParams.step);
                firstFieldNodeValue[i] = fieldValues[0];
                secondFieldNodeValue[i] = fieldValues[1];
            }
        }
        public override double MaxDistance => gridParams.rightEnd - gridParams.leftEnd;
        public override IEnumerator<DataItem> GetEnumerator()
        {
            double xCoordinate = gridParams.leftEnd;
            for (int i = 0; i < gridParams.nodesNum; i++)
            {
                DataItem item = new DataItem(xCoordinate, firstFieldNodeValue[i], secondFieldNodeValue[i]);
                yield return item;
                xCoordinate += gridParams.step;

            }
        }
        public override string ToString()
        {
            return $"Type: {GetType().ToString()}\nBase info: {base.ToString()}\nGrid Parametrs: {gridParams.ToString()}\n";
        }
        public override string ToLongString(string format)
        {
            StringBuilder itemsCoordinateInfo = new StringBuilder();
            double xCoordinate = gridParams.leftEnd;
            for (int i = 0; i < gridParams.nodesNum; i++)
            {
                string xCoordinateFormatted = String.Format(format, xCoordinate);
                string firstFieldFormatted = String.Format(format, firstFieldNodeValue[i]);
                string secondFieldFormatted = String.Format(format, secondFieldNodeValue[i]);
                itemsCoordinateInfo.Append($"\tIn node with coordinate = {xCoordinateFormatted}: First force field value = {firstFieldFormatted}, Second force field value = {secondFieldFormatted}\n");

                xCoordinate += gridParams.step;
            }
            return $"{ToString()}Node:\n{itemsCoordinateInfo}\n";
        }
    }
    [Serializable]
    class V3DataNUGrid: V3Data
    {
        double[] gridCoords { get; set; }
        double[] firstFieldNodeValue { get; set; }
        double[] secondFieldNodeValue { get; set; }
        public V3DataNUGrid(string identificator, DateTime timeAquired, double[] gridCoords, F2Double F) : base(identificator, timeAquired)
        {
            this.gridCoords = gridCoords;
            firstFieldNodeValue = new double[gridCoords.Length];
            secondFieldNodeValue = new double[gridCoords.Length];
            double[] fieldValues = new double[2];
            for (int i = 0; i < gridCoords.Length; i++)
            {
                fieldValues = F(gridCoords[i]);
                firstFieldNodeValue[i] = fieldValues[0];
                secondFieldNodeValue[i] = fieldValues[1];
            }
        }
        public override double MaxDistance => gridCoords.Max() - gridCoords.Min();

        public override IEnumerator<DataItem> GetEnumerator()
        {
            for (int i = 0; i < gridCoords.Length; i++)
            {
                DataItem item = new DataItem(gridCoords[i], firstFieldNodeValue[i], secondFieldNodeValue[i]);
                yield return item;
            }
        }

        public static explicit operator V3DataList(V3DataNUGrid source)
        {
            V3DataList result = new V3DataList(source.identificator, source.timeAquired);
            for (int i = 0; i < source.gridCoords.Length; i++)
            {
                result.Add(source.gridCoords[i], source.firstFieldNodeValue[i], source.secondFieldNodeValue[i]);
            }
            return result;
        }
        public static bool Save(string filename, V3DataNUGrid V3)
        {
            FileStream? fileStream = null;
            bool answer = false;
            try
            {
                fileStream = File.Create(filename);
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(V3DataNUGrid));
                jsonSerializer.WriteObject(fileStream, V3);
                answer = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Save failed\n " + ex.Message);
            }
            finally
            {
                fileStream?.Close();
            }
            return answer;
        }
        public static V3DataNUGrid? Load(string filename)
        {
            FileStream? fileStream = null;
            V3DataNUGrid? v3DataNUGrid = null;
            try
            {
                fileStream = File.OpenRead(filename);
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(V3DataNUGrid));
                v3DataNUGrid = (V3DataNUGrid?) jsonSerializer.ReadObject(fileStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Load failed:\n " + ex.Message);
                
            }
            finally
            {
                fileStream?.Close();
            }
            return v3DataNUGrid;

        }
        public override string ToString()
        {
            return $"Type: {GetType().ToString()}\nBase Info: {base.ToString()}\n";
        }
        public override string ToLongString(string format)
        {
            StringBuilder itemsCoordinateInfo = new StringBuilder();
            for (int i = 0; i < gridCoords.Length; i++)
            {
                string xCoordinateFormatted = String.Format(format, gridCoords[i]);
                string firstFieldFormatted = String.Format(format, firstFieldNodeValue[i]);
                string secondFieldFormatted = String.Format(format, secondFieldNodeValue[i]);
                itemsCoordinateInfo.Append($"\tIn node with coordinate = {xCoordinateFormatted}: First force field value = {firstFieldFormatted}, Second force field value = {secondFieldFormatted}\n");

            }
            return $"{ToString()}Nodes:\n{ itemsCoordinateInfo}\n";
        }
    }

    class V3DataCollection : System.Collections.ObjectModel.ObservableCollection<V3Data>
    {
        public new V3Data this[int index]
        {
            get => Items[index];
        }
        public DataItem? MaxCoordinate
        {
            get
            {
                IEnumerable<DataItem> coordinatesSorted = Items.SelectMany(item => item).OrderByDescending(item => Math.Abs(item.xCoordinate));
                if (coordinatesSorted.Any())
                {
                    return coordinatesSorted.First();
                } else
                {
                    return null;
                }
            }
        }
        public IEnumerable<double> nonUniqueCoords
        {
            get => Items.SelectMany(item => item).Select(item => item.xCoordinate).GroupBy(coord => coord).Where(coord => coord.Count() > 1).Select(group => group.Key).Distinct();            
        }
        public IEnumerable<IGrouping<double, DataItem>> groupedByCoord
        {
            get => from item in Items.SelectMany(item => item) group item by item.xCoordinate;
        }
        public bool Contains(string ID)
        {
            return Items.Any(item => item.identificator == ID);
        }
        public bool Remove(string ID)
        {
            if (Contains(ID))
            {
                V3Data dataToRemove = Items.First(item => item.identificator == ID);
                return Items.Remove(dataToRemove);
            }
            else return false;
        }
        public new bool Add(V3Data v3Data)
        {
            if (!Contains(v3Data.identificator))
            {
                Items.Add(v3Data);
                return true;
            } else
            {
                return false;
            }
            
        }
        public void AddDefaults()
        {
            F2Double forceField = ForceField.getFieldValues;
            UniformGrid grid = new UniformGrid(0, 1, 3);
            V3DataUGrid v3DataUGrid = new V3DataUGrid("DataUGrid", DateTime.Now, grid, forceField);
            V3DataList v3DataList = new V3DataList("DataList", DateTime.Now);
            v3DataList.AddDefaults(3, forceField);
            double[] nUGridParams = { 0.5, -0.6 };
            V3DataNUGrid v3DataNUGrid = new V3DataNUGrid("DataNUGrid", DateTime.Now, nUGridParams, forceField);
            Add(v3DataList);
            Add(v3DataUGrid);
            Add(v3DataNUGrid);
        }
        public string ToLongString(string format)
        {
            StringBuilder itemsInfo = new StringBuilder();
            foreach (V3Data v3Data in Items)
            {
                itemsInfo.Append($"Element:\n{v3Data.ToLongString(format)}\n");
            }
            return "Elements:\n" + itemsInfo.ToString();
        }
        public override string ToString()
        {
            StringBuilder itemsInfo = new StringBuilder();
            foreach (V3Data v3Data in Items)
            {
                itemsInfo.Append($"Element:\n{v3Data.ToString()}\n");
            }
            return "Elements:\n" + itemsInfo.ToString();
        }
    }
 }
