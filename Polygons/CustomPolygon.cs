using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Polygons
{
    public class CustomPolygon : Window
    {
        public Polygon polygon;
        public Point center;
        public Ellipse pointer;
        public List<string> objectiveNames = new List<string>();
        public List<TextBlock> textBlocks = new List<TextBlock>();
        public List<Double> weights;

        public CustomPolygon(Polygon polygon, Point center)
        {
            this.polygon = polygon;
            this.center = center;
            this.pointer = SetPointer(center);
        }

        private Ellipse SetPointer(Point center)
        {
            this.pointer = new Ellipse();
            pointer.Fill = Brushes.Black;
            pointer.StrokeThickness = 2;
            pointer.Stroke = Brushes.Black;
            pointer.Width = 10;
            pointer.Height = 10;
            Canvas.SetTop(pointer, center.Y - pointer.Height / 2);
            Canvas.SetLeft(pointer, center.X - pointer.Width / 2);

            return pointer;
        }

        public Ellipse MovePointer(Point clickPoint)
        {
            Canvas.SetLeft(pointer, clickPoint.X - pointer.Width / 2);
            Canvas.SetTop(pointer, clickPoint.Y - pointer.Height / 2);

            return pointer;
        }

        public bool IsPointInPolygon(PointCollection points, Point p)
        {
            bool inside = false;
            double minX = points[0].X;
            double maxX = points[0].X;
            double minY = points[0].Y;
            double maxY = points[0].Y;
            for (int i = 1; i < points.Count; i++)
            {
                Point q = points[i];
                minX = Math.Min(q.X, minX);
                maxX = Math.Max(q.X, maxX);
                minY = Math.Min(q.Y, minY);
                maxY = Math.Max(q.Y, maxY);
            }


            if (points.Count == 2)  // line
            {

                inside = (minX <= p.X && p.X <= maxX) && (minY - 2 <= p.Y && p.Y <= maxY + 2);
                return inside;
            }


            if (p.X < minX || p.X > maxX || p.Y < minY || p.Y > maxY)
            {
                return false;
            }


            for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
            {
                if ((points[i].Y > p.Y) != (points[j].Y > p.Y) &&
                     p.X < (points[j].X - points[i].X) * (p.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        public Point ClosestPointFromPointToPolygon(CustomPolygon customPolygon, Point point)
        {
            if (customPolygon.GetPolygonVertices().Count == 2)  // horizontal line
            {
                double minX = customPolygon.polygon.Points[0].X;
                double maxX = customPolygon.polygon.Points[0].X;
                for (int i = 1; i < customPolygon.polygon.Points.Count; i++)
                {
                    Point q = customPolygon.polygon.Points[i];
                    minX = Math.Min(q.X, minX);
                    maxX = Math.Max(q.X, maxX);
                }
                if (point.X < minX)
                {
                    point.X = minX;
                }
                if (point.X > maxX)
                {
                    point.X = maxX;
                }
                point.Y = customPolygon.center.Y;
            }
            else  // n > 2
            {
                // list of lines in polygon
                List<Line> polygonLines = customPolygon.GetPolygonLines();
                Point? minPoint = null;
                double minDist = double.MaxValue;

                // for each line in polygon, find the perpendicular line from point 
                foreach (var line in polygonLines)
                {
                    Point p1 = new Point(line.X1, line.Y1);
                    Point p2 = new Point(line.X2, line.Y2);
                    // calculate slopes
                    double slope = CustomPolygon.GetSlope(p1, p2);
                    double perpSlope = CustomPolygon.GetPerpendicularSlope(slope);
                    Debug.WriteLine("slope:{0}  perp:{1}", slope, perpSlope);

                    // perpendicular line from point 
                    Line perpLine = CustomPolygon.GetPerpLine(point, perpSlope);

                    // find intersection point
                    Point? intersectionPoint = CustomPolygon.GetIntersectionPoint(line, perpLine);

                    if (intersectionPoint.HasValue)
                    {
                        //get distance from point 
                        double dist = CustomPolygon.GetDistance(intersectionPoint.Value, point);
                        Debug.WriteLine("dist: {0}", dist);
                        // does this line intersect the polygon line 
                        // is the intersection point and the point a min distance
                        if (CustomPolygon.IsPointInLine(intersectionPoint.Value, line) && dist < minDist)
                        {
                            minDist = dist;
                            minPoint = intersectionPoint;
                        }
                    }
                    else
                    {
                        throw new Exception("Intersection Point is null even though it should have a coordinate");
                    }

                }

                // If there is no minpoint we calculate the closest distance from the point to the polygon corners
                if (!minPoint.HasValue)
                {
                    minDist = double.MaxValue;
                    PointCollection points = customPolygon.polygon.Points;
                    for (int i = 0; i < points.Count; i++)
                    {
                        double dist = CustomPolygon.GetDistance(points[i], point);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            minPoint = points[i];
                        }
                    }
                }

                point = minPoint.Value;

            }

            return point;
        }

        public Polygon DrawRegularPolygon(int sides, int radius, int startingAngle, Point center)
        {
            this.polygon.Stroke = Brushes.Black;
            this.polygon.Fill = Brushes.Transparent;

            //Get the location for each vertex of the polygon
            PointCollection polygonPoints = CalculateVertices(sides, radius, startingAngle, center);
            this.polygon.Points = polygonPoints;

            return this.polygon;
        }

        private PointCollection CalculateVertices(int sides, int radius, int startingAngle, Point center)
        {
            PointCollection points = new PointCollection();
            float step = 360.0f / sides;

            float angle = startingAngle; //starting angle
            for (double i = startingAngle; i < startingAngle + 360.0; i += step) //go in a circle
            {
                points.Add(DegreesToXY(angle, radius, center));
                angle += step;
            }

            return points;
        }

        private Point DegreesToXY(float degrees, float radius, Point origin)
        {
            Point xy = new Point();
            double radians = degrees * Math.PI / 180.0;

            xy.X = (int)(Math.Cos(radians) * radius + origin.X);
            xy.Y = (int)(Math.Sin(-radians) * radius + origin.Y);

            return xy;
        }

        public PointCollection GetPolygonVertices()
        {
            return this.polygon.Points;
        }

        public List<Line> GetPolygonLines()
        {
            List<Line> polygonLines = new List<Line>();
            for (int i = 0; i < this.polygon.Points.Count; i++)
            {
                Debug.WriteLine(i);
                Point firstPoint = this.polygon.Points[i];
                Point secondPoint;
                // if this is the last point
                if (i == this.polygon.Points.Count - 1)
                {
                    secondPoint = this.polygon.Points[0];
                }
                else
                {
                    secondPoint = this.polygon.Points[i + 1];
                }

                Line line = new Line();
                line.X1 = firstPoint.X;
                line.X2 = secondPoint.X;
                line.Y1 = firstPoint.Y;
                line.Y2 = secondPoint.Y;
                polygonLines.Add(line);
                Debug.WriteLine("P1:{0},{1}  P2:{2},{3} ", line.X1, line.Y1, line.X2, line.Y2);
            }
            return polygonLines;
        }

        public static Point? GetIntersectionPoint(Line l1, Line l2)
        {
            Point intersectionPoint = new Point();
            Point l1p1 = new Point(l1.X1, l1.Y1);
            Point l1p2 = new Point(l1.X2, l1.Y2);
            Point l2p1 = new Point(l2.X1, l2.Y1);
            Point l2p2 = new Point(l2.X2, l2.Y2);
            double l1Slope = CustomPolygon.GetSlope(l1p1, l1p2);
            double l2Slope = CustomPolygon.GetSlope(l2p1, l2p2);

            // if parallel
            if (l1Slope == l2Slope)
            {
                return null;
            }

            // if either one is vertical (but not both because we would have returned null already)
            else if (CustomPolygon.IsVertical(new Point(l1.X1, l1.Y1), new Point(l1.X2, l1.Y2)))
            {
                Debug.WriteLine("L1 vertical");
                intersectionPoint.X = l1p1.X;
                Point l2Point = l2p1;
                double b = l2p1.Y - (l2Slope * l2p1.X); //  (p1.Y - (slope * p1.X));     
                intersectionPoint.Y = (intersectionPoint.X * l2Slope) + b;   // y=ax +b
            }

            else if (CustomPolygon.IsVertical(new Point(l2.X1, l2.Y1), new Point(l2.X2, l2.Y2)))
            {
                Debug.WriteLine("L2 vertical");
                intersectionPoint.X = l2p1.X;
                Point l1Point = l1p1;
                double b = l1p1.Y - (l1Slope * l1p1.X);
                intersectionPoint.Y = (intersectionPoint.X * l1Slope) + b;
            }

            else
            {
                Point l1Point = l1p1;
                Point l2Point = l2p1;
                double a1 = l1Point.Y - l1Slope * l1Point.X;
                double a2 = l2Point.Y - l2Slope * l2Point.X;
                intersectionPoint.X = (a1 - a2) / (l2Slope - l1Slope);
                intersectionPoint.Y = a2 + l2Slope * intersectionPoint.X;
            }
            Debug.WriteLine("Intersection point: {0}", intersectionPoint);
            return intersectionPoint;
        }

        public static double GetSlope(Point p1, Point p2)
        {
            // if x = constant then just make gradient to be 0
            if ((p2.X - p1.X) != 0) { return (p2.Y - p1.Y) / (p2.X - p1.X); }
            else { return 1e+10; }
        }

        public static double GetPerpendicularSlope(double slope)
        {
            if (slope == 0) { return 1e+10; }
            if (slope == 1e+10) { return 0; }
            return -1 / slope;
        }

        public static Line GetPerpLine(Point point, double perpSlope)
        {
            Line perpLine = new Line();
            perpLine.X1 = point.X;
            perpLine.Y1 = point.Y;
            if (perpSlope == 1e+10)  // vertical line
            {
                perpLine.X2 = point.X;
                perpLine.Y2 = point.Y + 1;
            }
            else
            {
                perpLine.X2 = point.X + 1;
                // y = ax+b    a=slope,  b = y -ax
                perpLine.Y2 = perpSlope * perpLine.X2 + (point.Y - (perpSlope * point.X));
            }
            Debug.WriteLine("perp line: ({0},{1}) ; ({2},{3})", perpLine.X1, perpLine.Y1, perpLine.X2, perpLine.Y2);
            return perpLine;
        }

        public static bool IsVertical(Point p1, Point p2)
        {
            bool isVertical = false;
            double slope = CustomPolygon.GetSlope(p1, p2);
            if (slope == 1e+10) { isVertical = true; }
            return isVertical;
        }

        public static double GetDistance(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        public static bool ContainsNegative(List<Double> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] < 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsPointInLine(Point point3, Line line)
        {
            Point point1 = new Point(line.X1, line.Y1);
            Point point2 = new Point(line.X2, line.Y2);
            // we use sqrt here because getDistance does not actually sqrt
            double p1p2 = CustomPolygon.GetDistance(point1, point2);
            double p1p3 = CustomPolygon.GetDistance(point1, point3);
            double p2p3 = CustomPolygon.GetDistance(point2, point3);
            if (p1p2 == (p1p3 + p2p3)) { return true; }
            else { return false; }
        }

        public static bool FloatsAreEqual(double n1, double n2, double offset)
        {
            if ((n2 <= n1 + offset) && (n2 >= n1 - offset))
            {
                return true;
            }
            return false;
        }

        private List<Double> NormalizeWeights(List<Double> weights)
        {
            List<Double> normalizedWeights = new List<Double>();
            double sum = weights.Sum();
            for (int i = 0; i < weights.Count; i++)
            {
                normalizedWeights.Add((weights[i] / sum) * 100);
            }
            return normalizedWeights;
        }
        private List<Double> NormalizeWeightsTwoSide(List<Double> weights, double max)
        {
            List<Double> normalizedWeights = new List<Double>();
  
            for (int i = 0; i < weights.Count; i++)
            {
                normalizedWeights.Add( (1- weights[i] / max) * 100);
                    Debug.WriteLine("weight {1}:{0}", normalizedWeights[i], i);
            }
            return normalizedWeights;
        }

        private List<Double> GetClickDistanceList(PointCollection points, Point clickPoint)
        {
            List<Double> distances = points.Aggregate(
                new List<Double>(),
                (acc, p) =>
                {
                    acc.Add(CustomPolygon.GetDistance(p, clickPoint));
                    return acc;
                });
            return distances;
        }

        private List<Double> GetExtendedDistanceList(PointCollection points, Point clickPoint)
        {
            List<Double> extendedDistances = new List<Double>();
            const double offset = 0.01;

            for (int i = 0; i < points.Count; i++)
            {
                Point objectivePoint = points[i];

                // get the line from objective point to the clickpoint
                Line clickLine = new Line();
                clickLine.X1 = objectivePoint.X;
                clickLine.X2 = clickPoint.X;
                clickLine.Y1 = objectivePoint.Y;
                clickLine.Y2 = clickPoint.Y;

                Point? intersectionPoint = null;

                // we cycle through the polygon points 2 at a time, finding the polygon line
                // that intersects with the extended clickLine
                for (int j = 0; j < points.Count; j++)
                {
                    Point currentPoint = points[j];
                    Point nextPoint;

                    if ((j + 1) < points.Count)
                    {
                        nextPoint = points[j + 1];
                    }
                    else
                    {
                        // if the point is the last point, we cycle back to get the point in the 0th index
                        nextPoint = points[0];
                    }

                    Line polygonLine = new Line();
                    polygonLine.X1 = currentPoint.X;
                    polygonLine.X2 = nextPoint.X;
                    polygonLine.Y1 = currentPoint.Y;
                    polygonLine.Y2 = nextPoint.Y;

                    intersectionPoint = CustomPolygon.GetIntersectionPoint(clickLine, polygonLine);

                    // Make sure the intersection point calculated is not the objectivePoint
                    if (intersectionPoint.HasValue &&
                        // this part is trick af because it is floating point calculation. Need some offset
                        (!CustomPolygon.FloatsAreEqual(objectivePoint.X, intersectionPoint.Value.X, offset) ||
                        !CustomPolygon.FloatsAreEqual(objectivePoint.Y, intersectionPoint.Value.Y, offset))
                        &&
                        CustomPolygon.IsPointInLine(intersectionPoint.Value, polygonLine))
                    {
                        // if we have discovered the point we break out of the for loop
                        Console.WriteLine("the valid intersection point is ({0}, {1})", intersectionPoint.Value.X, intersectionPoint.Value.Y);
                        break;
                    }
                }

                // Get distance between the polygon corner and the intersecting coordinate
                if (intersectionPoint.HasValue)
                {
                    double extendedDistance = CustomPolygon.GetDistance(intersectionPoint.Value, objectivePoint);
                    extendedDistances.Add(extendedDistance);
                }
                //else
                //{
                //    // I should be getting some kind of intersection here if not 
                //    throw new Exception("Intersection Point is null even though it should have a coordinate");
                //}
            }

            return extendedDistances;
        }

        private List<Double> GetUnnormalizedWeights(List<Double> longDist, List<Double> clickDist)
        {
            List<Double> unnormWeights = new List<Double>();
            for (int i = 0; i < longDist.Count; i++)
            {
                unnormWeights.Add(1 - (clickDist[i] / longDist[i]));
            }
            return unnormWeights;
        }

        public void CalculateWeights(Point clickPoint)
        {
            List<Double> clickDistances = this.GetClickDistanceList(this.polygon.Points, clickPoint);

            if (this.polygon.Points.Count == 2) //line
            {
                double max = CustomPolygon.GetDistance(this.polygon.Points[0], this.polygon.Points[1]);
                Debug.WriteLine("max distance:{0}", max);
                weights = this.NormalizeWeightsTwoSide(clickDistances, max);
            }
            else
            {
                List<Double> extendedDistances = this.GetExtendedDistanceList(this.polygon.Points, clickPoint);
                List<Double> unnormalizedWeights = this.GetUnnormalizedWeights(extendedDistances, clickDistances);

                // Need to include this because line thickness of the polygon causes it to "bleed outwards" from the polygon's points
                if (CustomPolygon.ContainsNegative(unnormalizedWeights))
                {
                    Console.WriteLine("Out of bounds of the polygon");
                    weights = this.NormalizeWeights(unnormalizedWeights);
                }
                else
                {
                    weights = this.NormalizeWeights(unnormalizedWeights);
                }
            }
  
        }
    }
}
