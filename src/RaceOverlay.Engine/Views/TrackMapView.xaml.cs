using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using RaceOverlay.Engine.ViewModels;

namespace RaceOverlay.Engine.Views;

public partial class TrackMapView : UserControl
{
    private TrackMapViewModel? _viewModel;

    public TrackMapView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.MapUpdated -= RedrawMap;
        }

        _viewModel = DataContext as TrackMapViewModel;

        if (_viewModel != null)
        {
            _viewModel.MapUpdated += RedrawMap;
        }
    }

    private void RedrawMap()
    {
        if (_viewModel == null) return;

        var outline = _viewModel.TrackOutline;
        if (outline.Length == 0) return;

        double width = MapCanvas.ActualWidth;
        double height = MapCanvas.ActualHeight;

        if (width <= 0 || height <= 0) return;

        double padding = 20;
        double drawW = width - padding * 2;
        double drawH = height - padding * 2;

        // Draw track outline
        var trackPoints = new PointCollection(outline.Length);
        for (int i = 0; i < outline.Length; i++)
        {
            trackPoints.Add(new Point(
                padding + outline[i].X * drawW,
                padding + outline[i].Y * drawH));
        }

        TrackLine.Points = trackPoints;
        TrackLine.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));

        // Compute cumulative segment lengths for interpolation
        double[] cumLengths = new double[outline.Length];
        cumLengths[0] = 0;
        for (int i = 1; i < outline.Length; i++)
        {
            double dx = outline[i].X - outline[i - 1].X;
            double dy = outline[i].Y - outline[i - 1].Y;
            cumLengths[i] = cumLengths[i - 1] + Math.Sqrt(dx * dx + dy * dy);
        }
        double totalLength = cumLengths[outline.Length - 1];

        // Remove old car dots and labels (everything except the Polyline)
        for (int i = MapCanvas.Children.Count - 1; i >= 0; i--)
        {
            if (MapCanvas.Children[i] != TrackLine)
                MapCanvas.Children.RemoveAt(i);
        }

        var drivers = _viewModel.Drivers;

        foreach (var driver in drivers)
        {
            // Find position on track via interpolation
            double targetDist = driver.TrackProgress * totalLength;

            // Find the segment
            int seg = 0;
            for (int i = 1; i < outline.Length; i++)
            {
                if (cumLengths[i] >= targetDist)
                {
                    seg = i - 1;
                    break;
                }
            }

            double segStart = cumLengths[seg];
            double segEnd = cumLengths[seg + 1];
            double segLen = segEnd - segStart;
            double t = segLen > 0 ? (targetDist - segStart) / segLen : 0;

            double nx = outline[seg].X + t * (outline[seg + 1].X - outline[seg].X);
            double ny = outline[seg].Y + t * (outline[seg + 1].Y - outline[seg].Y);

            double px = padding + nx * drawW;
            double py = padding + ny * drawH;

            // Pit status: if in pit and ShowPitStatus, dim the dot
            double opacity = (_viewModel.ShowPitStatus && driver.IsInPit) ? 0.3 : 1.0;

            if (driver.IsPlayer)
            {
                // Player ring (accent highlight)
                var ring = new Ellipse
                {
                    Width = 16,
                    Height = 16,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F97316")),
                    StrokeThickness = 1.5,
                    Fill = Brushes.Transparent,
                    Opacity = opacity
                };
                Canvas.SetLeft(ring, px - 8);
                Canvas.SetTop(ring, py - 8);
                MapCanvas.Children.Add(ring);

                // Player dot (larger)
                var dot = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(driver.ClassColor)),
                    Opacity = opacity
                };
                Canvas.SetLeft(dot, px - 5);
                Canvas.SetTop(dot, py - 5);
                MapCanvas.Children.Add(dot);
            }
            else
            {
                // Other driver dot
                var dot = new Ellipse
                {
                    Width = 7,
                    Height = 7,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(driver.ClassColor)),
                    Opacity = opacity
                };
                Canvas.SetLeft(dot, px - 3.5);
                Canvas.SetTop(dot, py - 3.5);
                MapCanvas.Children.Add(dot);
            }

            // Optional driver name label
            if (_viewModel.ShowDriverNames)
            {
                var label = new TextBlock
                {
                    Text = driver.DriverName,
                    FontSize = 8,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")),
                    Opacity = opacity
                };
                Canvas.SetLeft(label, px + (driver.IsPlayer ? 10 : 6));
                Canvas.SetTop(label, py - 6);
                MapCanvas.Children.Add(label);
            }
        }
    }
}
