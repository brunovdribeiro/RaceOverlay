using System.Windows.Controls;
using RaceOverlay.Engine.ViewModels;

namespace RaceOverlay.Engine.Views;

/// <summary>
/// Interaction logic for RelativeOverlayView.xaml
/// </summary>
public partial class RelativeOverlayView : UserControl
{
    public RelativeOverlayView()
    {
        InitializeComponent();
        
        // Set the DataContext to the ViewModel
        DataContext = new RelativeOverlayViewModel();
    }
}
