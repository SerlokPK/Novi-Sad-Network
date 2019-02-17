using opet.Models;
using System.Collections.Generic;

namespace PZ3.Helpers
{
    public static class BreadthFirstSearch
    {
        //public List<Point> Search(Point endPoint)
        //{
        //    Point current = _root;
        //    searchQueue.Enqueue(_root);
        //    List<Point> visitedPoints = new List<Point>();

        //    while (searchQueue.Count != 0)
        //    {
        //        current = (Point)searchQueue.Dequeue();

        //        if (current.X == endPoint.X && current.Y == endPoint.Y)
        //        {
        //            return visitedPoints;
        //        }
        //        else
        //        {
        //            while (current.Y != endPoint.Y)
        //            {
        //                current.Y += (current.Y > endPoint.Y ? -10 : 10);
        //                visitedPoints.Add(new );
        //            }
        //            xPosition = CheckIfGreater(current.X, endPoint.X);
        //            yPosition = CheckIfGreater(current.Y, endPoint.Y);
        //            visitedPoints.Add(current);
        //        }
        //    }
        //    return null;
        //}

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

        //private int CheckIfGreater(double current, double end)
        //{
        //    if(current < end)
        //    {
        //        return 10;
        //    }else if(current > end)
        //    {
        //        return -10;
        //    }
        //    else
        //    {
        //        return 0;
        //    }
        //}
    }
}
