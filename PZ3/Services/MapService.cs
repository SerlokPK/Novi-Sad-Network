using opet.Models;
using PZ3.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace opet.Service
{
    public static class MapService
    {
        private static readonly double startLongitude = 19.718794;
        private static readonly double startLatitude = 45.18472762;

        private static readonly double endLongitude = 19.945977;
        private static readonly double endLatitude = 45.322430;

        private static readonly int tileSize = 10;
        private static readonly int mapSize = 2000;
        private static readonly double longitudeScale = mapSize / (endLongitude - startLongitude);
        private static readonly double latitudeScale = mapSize / (endLatitude - startLatitude);

        private static readonly Dictionary<double,Point> ExistingPoints = new Dictionary<double, Point>();

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        public static BaseModel ParseXml()
        {
            var filename = "Geographic.xml";
            var currentDirectory = Directory.GetCurrentDirectory();
            var purchaseOrderFilepath = System.IO.Path.Combine(currentDirectory, filename);

            StringBuilder result = new StringBuilder();

            //Load xml
            XDocument xdoc = XDocument.Load(filename);

            //Run query
            var substations = xdoc.Descendants("SubstationEntity")
                     .Select(sub => new Substation
                     {
                         Id = (double)sub.Element("Id"),
                         Name = (string)sub.Element("Name"),
                         X = (double)sub.Element("X"),
                         Y = (double)sub.Element("Y"),
                     }).ToList();

            double longit = 0;
            double latid = 0;

            foreach (var item in substations)
            {
                ToLatLon(item.X, item.Y, 34, out latid, out longit);
                item.Latitude = latid;
                item.Longitude = longit;
            }

            var nodes = xdoc.Descendants("NodeEntity")
                     .Select(node => new Node
                     {
                         Id = (double)node.Element("Id"),
                         Name = (string)node.Element("Name"),
                         X = (double)node.Element("X"),
                         Y = (double)node.Element("Y"),
                     }).ToList();

            foreach (var item in nodes)
            {
                ToLatLon(item.X, item.Y, 34, out latid, out longit);
                item.Latitude = latid;
                item.Longitude = longit;
            }

            var switches = xdoc.Descendants("SwitchEntity")
                     .Select(sw => new Switch
                     {
                         Id = (double)sw.Element("Id"),
                         Name = (string)sw.Element("Name"),
                         Status = (string)sw.Element("Status"),
                         X = (double)sw.Element("X"),
                         Y = (double)sw.Element("Y"),
                     }).ToList();

            foreach (var item in switches)
            {
                ToLatLon(item.X, item.Y, 34, out latid, out longit);
                item.Latitude = latid;
                item.Longitude = longit;
            }

            var lines = xdoc.Descendants("LineEntity")
                     .Select(line => new opet.Models.Line
                     {
                         Id = (double)line.Element("Id"),
                         Name = (string)line.Element("Name"),
                         ConductorMaterial = (string)line.Element("ConductorMaterial"),
                         IsUnderground = (bool)line.Element("IsUnderground"),
                         R = (double)line.Element("R"),
                         FirstEnd = (double)line.Element("FirstEnd"),
                         SecondEnd = (double)line.Element("SecondEnd"),
                         LineType = (string)line.Element("LineType"),
                         ThermalConstantHeat = (double)line.Element("ThermalConstantHeat"),
                         Vertices = line.Element("Vertices").Descendants("Point").Select(p => new Point
                         {
                             X = (double)p.Element("X"),
                             Y = (double)p.Element("Y"),
                         }).ToList()
                     }).ToList();

            foreach (var line in lines)
            {
                foreach (var point in line.Vertices)
                {
                    ToLatLon(point.X, point.Y, 34, out latid, out longit);
                    point.Latitude = latid;
                    point.Longitude = longit;
                }
            }

            return new BaseModel { Substations = substations, Switches = switches, Nodes = nodes, Lines = lines };
        }

        public static List<Ellipse> DrawSubstations(List<Substation> substations)
        {
            List<Ellipse> subs = new List<Ellipse>();
            foreach (var item in substations)
            {
                Ellipse elipse = InstantiateEllipse(Colors.DarkGreen);

                if (CheckIfCoordinatesAreValid(item.Longitude, item.Latitude) || ExistingPoints.ContainsKey(item.Id))
                {
                    continue;
                }

                double longitude = ConvertMapPoint(item.Longitude, longitudeScale, startLongitude) - 5;
                double latitude = ConvertMapPoint(item.Latitude, latitudeScale, startLatitude) - 5;
                var point = CreatePoint(longitude, latitude, 1);
                ExistingPoints.Add(item.Id,point);

                SetOnCanvas(point.X, point.Y, elipse);

                elipse.ToolTip = "Substation: " + item.Name.ToString();
                subs.Add(elipse);
            }

            return subs;
        }

        public static List<Ellipse> DrawSwitches(List<Switch> switches)
        {
            List<Ellipse> sw = new List<Ellipse>();
            foreach (var item in switches)
            {
                Ellipse elipse = InstantiateEllipse(Colors.DarkBlue);

                if (CheckIfCoordinatesAreValid(item.Longitude, item.Latitude) || ExistingPoints.ContainsKey(item.Id))
                {
                    continue;
                }

                double longitude = ConvertMapPoint(item.Longitude, longitudeScale, startLongitude) - 5;
                double latitude = ConvertMapPoint(item.Latitude, latitudeScale, startLatitude) - 5;
                var point = CreatePoint(longitude, latitude, 1);
                ExistingPoints.Add(item.Id,point);

                SetOnCanvas(point.X, point.Y, elipse);

                elipse.ToolTip = "Switch: " + item.Name.ToString();
                sw.Add(elipse);
            }

            return sw;
        }

        public static List<Ellipse> DrawNodes(List<Node> nodes)
        {
            List<Ellipse> sw = new List<Ellipse>();
            foreach (var item in nodes)
            {
                Ellipse elipse = InstantiateEllipse(Colors.Red);

                if (CheckIfCoordinatesAreValid(item.Longitude, item.Latitude) || ExistingPoints.ContainsKey(item.Id))
                {
                    continue;
                }

                double longitude = ConvertMapPoint(item.Longitude, longitudeScale, startLongitude) - 5;
                double latitude = ConvertMapPoint(item.Latitude, latitudeScale, startLatitude) - 5;
                var point = CreatePoint(longitude, latitude, 1);
                ExistingPoints.Add(item.Id,point);

                SetOnCanvas(point.X, point.Y, elipse);

                elipse.ToolTip = "Node: " + item.Name.ToString();
                sw.Add(elipse);
            }

            return sw;
        }

        public static void DrawLines(List<opet.Models.Line> lines, Canvas map)
        {
            foreach(var item in lines)
            {
                if(!ExistingPoints.ContainsKey(item.FirstEnd) || !ExistingPoints.ContainsKey(item.SecondEnd))
                {
                    continue;
                }

                List<Point> path = BreadthFirstSearch.Search(ExistingPoints[item.FirstEnd],ExistingPoints[item.SecondEnd]);

                if(path != null)
                {
                    for (int i = 0; i < path.Count() - 1; i++)
                    {
                        System.Windows.Shapes.Line l = new System.Windows.Shapes.Line();
                        l.Stroke = Brushes.DeepSkyBlue;
                        l.X1 = path[i].X;
                        l.Y1 = path[i].Y;

                        l.X2 = path[i + 1].X;
                        l.Y2 = path[i + 1].Y;
                        l.StrokeThickness = 2;

                        map.Children.Add(l);
                    }  
                }
            }
        }

        private static bool CheckIfCoordinatesAreValid(double longitude, double latitude)
        {
            return longitude < startLongitude || longitude > endLongitude || latitude < startLongitude || latitude > endLatitude;
        }
        private static void SetOnCanvas(double x, double y,Ellipse ellipse)
        {
            Canvas.SetTop(ellipse, y);
            Canvas.SetLeft(ellipse, x);
        }
        private static Ellipse InstantiateEllipse(Color color)
        {
            Ellipse elipse = new Ellipse();
            elipse.Height = 8;
            elipse.Width = 8;
            elipse.Fill = new SolidColorBrush(color);
            elipse.StrokeThickness = 2;

            return elipse;
        }
        private static double ConvertMapPoint(double point, double scale, double start)
        {
            return Math.Round((point - start) * scale / tileSize) * tileSize;
        }
        private static Point CreatePoint(double longitude, double latitude, int round)
        {
            var point = new Point
            {
                X = longitude,
                Y = latitude,
            };
            if (ExistingPoints.Any(x => x.Value.X == point.X && x.Value.Y == point.Y))
            {
                switch (round)
                {
                    case 1:
                        point = CreatePoint(longitude + tileSize, latitude, 2);
                        return point;
                    case 2:
                        point = CreatePoint(longitude - 2 * tileSize, latitude, 3);
                        return point;
                    case 3:
                        point = CreatePoint(longitude + tileSize, latitude + tileSize, 4);
                        return point;
                    case 4:
                        point = CreatePoint(longitude, latitude - 2 * tileSize, 1);
                        return point;
                }
            }

            return point;
        }
    }
}
