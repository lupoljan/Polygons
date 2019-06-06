using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Polygon polygon;
        CustomPolygon customPolygon;

        public MainWindow()
        {
            //default initialization in case of null exeption
            this.polygon = new Polygon();
            this.customPolygon = new CustomPolygon(polygon, new Point(1,1));
            this.customPolygon.polygon.Points = new PointCollection();
            this.customPolygon.polygon.Points.Add(new Point(1, 1));

            InitializeComponent();
            this.MouseMove += this.onMouseMove;
        }

        private void onMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point clickPoint = e.GetPosition(this);  
                //Map point positon inside canvas
                clickPoint = mainWindow.TranslatePoint(clickPoint, polygonCanvas);

                if (!this.customPolygon.IsPointInPolygon(this.customPolygon.GetPolygonVertices(), clickPoint))
                {
                    Debug.WriteLine("out");
                    clickPoint = customPolygon.ClosestPointFromPointToPolygon(this.customPolygon, clickPoint);
                }
                this.customPolygon.MovePointer(clickPoint);

                //calculate weights
            }

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
            this.customPolygon = new CustomPolygon(this.polygon, center);
            polygonCanvas.Children.Add(customPolygon.pointer);
            
            //Drawing the polygon
            polygonCanvas.Children.Add(customPolygon.DrawRegularPolygon(sides, radius, angle, center));
        }
    }
}
