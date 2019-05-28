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

namespace Polygons
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int sides = (int)nSides.Value;
            int radius = (int)nRadius.Value;
            int angle = (int)nAngle.Value;

            //Clear canvas
            polygonCanvas.Children.Clear();

            //Drawing center point
            Point center = new Point(polygonCanvas.ActualWidth / 2, polygonCanvas.ActualHeight / 2);
            Ellipse myEllipse = new Ellipse();
            myEllipse.Fill = Brushes.Black;
            myEllipse.StrokeThickness = 2;
            myEllipse.Stroke = Brushes.Black;
            myEllipse.Width = 10;
            myEllipse.Height = 10;
            Canvas.SetTop(myEllipse, center.Y - myEllipse.Height / 2);
            Canvas.SetLeft(myEllipse, center.X - myEllipse.Width / 2);
            polygonCanvas.Children.Add(myEllipse);

            //Drawing the polygon
            polygonCanvas.Children.Add(DrawRegularPolygon(sides, radius, angle, center));
        }


        private Polygon DrawRegularPolygon(int sides, int radius, int startingAngle, Point center)
        {
            Polygon  polygon = new Polygon();
            polygon.Stroke = Brushes.Black;
            polygon.Fill = Brushes.Transparent;

            //Get the location for each vertex of the polygon
            PointCollection polygonPoints = CalculateVertices(sides, radius, startingAngle, center);
            polygon.Points = polygonPoints;

            return polygon;
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
    }
}
