using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RaceOverlay.Engine.ViewModels;

namespace RaceOverlay.Engine.Views;

public partial class InputTraceView : UserControl
{
    private InputTraceViewModel? _viewModel;

    public InputTraceView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // Unsubscribe from old
        if (_viewModel != null)
        {
            _viewModel.TraceUpdated -= RedrawChart;
        }

        _viewModel = DataContext as InputTraceViewModel;

        if (_viewModel != null)
        {
            _viewModel.TraceUpdated += RedrawChart;
        }
    }

    private void RedrawChart()
    {
        if (_viewModel == null) return;

        var history = _viewModel.TraceHistory;
        if (history.Count == 0) return;

        double width = ChartCanvas.ActualWidth;
        double height = ChartCanvas.ActualHeight;

        if (width <= 0 || height <= 0) return;

        var throttlePoints = new PointCollection(history.Count);
        var brakePoints = new PointCollection(history.Count);

        double step = history.Count > 1 ? width / (history.Count - 1) : 0;

        for (int i = 0; i < history.Count; i++)
        {
            double x = i * step;
            double throttleY = height - (history[i].Throttle * height);
            double brakeY = height - (history[i].Brake * height);

            throttlePoints.Add(new Point(x, throttleY));
            brakePoints.Add(new Point(x, brakeY));
        }

        ThrottleLine.Points = throttlePoints;
        BrakeLine.Points = brakePoints;

        // Update colors from ViewModel
        try
        {
            ThrottleLine.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_viewModel.ThrottleColor));
            BrakeLine.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_viewModel.BrakeColor));
        }
        catch
        {
            // Fallback if color string is invalid
            ThrottleLine.Stroke = Brushes.Green;
            BrakeLine.Stroke = Brushes.Red;
        }
    }
}
