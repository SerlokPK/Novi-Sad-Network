using opet.Models;
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

        private static readonly List<Point> ExistingPoints = new List<Point>();

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
            var filename = "test.xml";
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

            return new BaseModel { Substation = substations, Switch = switches, Node = nodes, Line = lines };
        }

        public static List<Ellipse> DrawSubstations(List<Substation> substations)
        {
            List<Ellipse> subs = new List<Ellipse>();
            foreach (var item in substations)
            {
                Ellipse elipse = new Ellipse();
                elipse.Height = 8;
                elipse.Width = 8;
                elipse.Fill = new SolidColorBrush(Colors.DarkViolet);
                elipse.StrokeThickness = 2;

                double longitude = ConvertMapPoint(item.Longitude, longitudeScale, startLongitude) - 5;
                double latitude = ConvertMapPoint(item.Latitude, latitudeScale, startLatitude) - 5;
                var point = AddPoint(longitude, latitude, 1);
                ExistingPoints.Add(point);

                Canvas.SetTop(elipse, point.Y);
                Canvas.SetLeft(elipse, point.X);

                elipse.ToolTip = "Substation: " + item.Name.ToString();

                subs.Add(elipse);
            }

            return subs;
        }

        private static double ConvertMapPoint(double point, double scale, double start)
        {
            return Math.Round((point - start) * scale / tileSize) * tileSize;
        }

        private static Point AddPoint(double longitude, double latitude, int round)
        {
            var point = new Point
            {
                X = longitude,
                Y = latitude,
            };
            if (ExistingPoints.Any(x => x.Y == latitude && x.X == longitude))
            {
                switch (round)
                {
                    case 1:
                        point = AddPoint(longitude + tileSize, latitude, 2);
                        return point;
                    case 2:
                        point = AddPoint(longitude - 2 * tileSize, latitude, 3);
                        return point;
                    case 3:
                        point = AddPoint(longitude + tileSize, latitude + tileSize, 4);
                        return point;
                    case 4:
                        point = AddPoint(longitude, latitude - 2 * tileSize, 1);
                        return point;
                }
            }

            return point;
        }
    }
}
