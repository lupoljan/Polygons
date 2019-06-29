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
                //calculate weights and update labels
                this.customPolygon.CalculateWeights(clickPoint);
                this.RefreshObjectiveText();
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

            //Draw labels near polygon points
            this.PopulateObjectiveNames();
            this.LabelPolygonPoints();
            this.customPolygon.CalculateWeights(center);
            this.AddObjectiveText();
        }

        private TextBlock addTextBlockToCanvas(string text, double x, double y)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            Canvas.SetTop(textBlock, y);
            Canvas.SetLeft(textBlock, x);
            polygonCanvas.Children.Add(textBlock);
            return textBlock;
        }
        private void PopulateObjectiveNames()
        {
            customPolygon.objectiveNames.Clear();
            for (int i = 0; i < this.polygon.Points.Count; i++)
            {
                string objectiveName = "O" + i.ToString();
                customPolygon.objectiveNames.Add(objectiveName);
            }
        }
        private void AddObjectiveText()
        {
            customPolygon.textBlocks.Clear();
            for (int i = 0; i < customPolygon.objectiveNames.Count; i++)
            {
                customPolygon.textBlocks.Add(this.addTextBlockToCanvas(customPolygon.objectiveNames[i], 10, (i + 1) * 20));
                customPolygon.textBlocks[i].Text = customPolygon.objectiveNames[i] + ": " + customPolygon.Weights[i].ToString("0.00");
            }
        }

        private void RefreshObjectiveText()
        {
            for (int i = 0; i < customPolygon.objectiveNames.Count; i++)
            {
                customPolygon.textBlocks[i].Text = customPolygon.objectiveNames[i] + ": " + customPolygon.Weights[i].ToString("0.00");
            }
        }

        private void LabelPolygonPoints()
        {
            for (int i = 0; i < customPolygon.objectiveNames.Count; i++)
            {
                this.addTextBlockToCanvas(customPolygon.objectiveNames[i], customPolygon.polygon.Points[i].X, customPolygon.polygon.Points[i].Y);
            }
        }
    }
}
