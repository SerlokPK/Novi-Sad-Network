using opet.Models;
using System.Collections.Generic;

namespace PZ3.Helpers
{
    public static class ClosestPathFinder
    {
        public static List<Point> Search(Point startPoint,Point endPoint)
        {
            List<Point> visitedPoints = new List<Point>();
            Point currentPos = startPoint;

            //Level in Y
            while (currentPos.Y != endPoint.Y)
            {
                visitedPoints.Add(new Point {
                    X = currentPos.X + 5,
                    Y = currentPos.Y + 5
                });
                currentPos.Y += (currentPos.Y > endPoint.Y ? -10 : 10);
            }
            //Level in X
            while (currentPos.X != endPoint.X)
            {
                visitedPoints.Add(new Point
                {
                    X = currentPos.X + 5,
                    Y = currentPos.Y + 5
                });
                currentPos.X += (currentPos.X > endPoint.X ? -10 : 10);
            }

            visitedPoints.Add(new Point
            {
                X = currentPos.X + 5,
                Y = currentPos.Y + 5
            });

            return visitedPoints;
        }
    }
}
