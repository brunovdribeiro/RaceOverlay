using System.Windows;

namespace RaceOverlay.App;

public partial class ErrorDialog : Window
{
    public ErrorDialog(string title, string details)
    {
        InitializeComponent();
        HeaderText.Text = title;
        DetailsTextBox.Text = details;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Clipboard.SetText(DetailsTextBox.Text);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
