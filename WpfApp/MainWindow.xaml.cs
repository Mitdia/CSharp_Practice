using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Lab1;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String id;
        int nItems = 0;
        FuncEnum forceFunction;
        public MainWindow()
        {
            InitializeComponent();
            id = "";
            forceFunctionComboBox.ItemsSource = Enum.GetValues(typeof(FuncEnum));
        }

        private void Render_Click(object sender, RoutedEventArgs e)
        {
            V3DataList mainObject = new V3DataList(id, DateTime.Now);
            mainObject.AddDefaults(nItems, forceFunction);
            DataItemInfo.Text = $"DataItem ID: {mainObject.identificator}\nDataItem timecode: {mainObject.timeAquired}";
            ListOfItems.ItemsSource = mainObject.dataItems;
            /*
            List<DataItem> listOfItems = mainObject.dataItems;
            ListOfItems.ItemsSource = listOfItems;
            */

        }

        private void idTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            id = idTextBox.Text;
        }
        private void OnKeyDownIDBoxHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                MessageBox.Show("New id: " + idTextBox.Text);
            }
        }
        private void OnKeyDownNItemsHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                nItems = Int32.Parse(nItemsTextBox.Text);
                MessageBox.Show("Number of items: " + nItemsTextBox.Text);
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void nItems_TextChanged(object sender, TextChangedEventArgs e)
        {
            // nItems = 
        }

        private void forceFunctionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            forceFunction = (FuncEnum) forceFunctionComboBox.SelectedItem;
        }
    }
}
