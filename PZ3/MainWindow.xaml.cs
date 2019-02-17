using opet.Models;
using opet.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace PZ3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BaseModel BaseModel{ get; set; }
        public MainWindow()
        {
            InitializeComponent();
            BaseModel = MapService.ParseXml();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            List<Ellipse> elipses = new List<Ellipse>();

            elipses = MapService.DrawSubstations(BaseModel.Substations);
            foreach(var item in elipses)
            {
                Map.Children.Add(item);
            }
            elipses = MapService.DrawSwitches(BaseModel.Switches);
            foreach(var item in elipses)
            {
                Map.Children.Add(item);
            }
            elipses = MapService.DrawNodes(BaseModel.Nodes);
            foreach (var item in elipses)
            {
                Map.Children.Add(item);
            }

            MapService.DrawLines(BaseModel.Lines, Map);
        }
    }
}
