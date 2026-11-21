using System.Text;
using System.Windows;
using Modeling_And_Simulation_EAS.Pages;
using Modeling_And_Simulation_EAS.ViewModels;

namespace Modeling_And_Simulation_EAS;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = SimulationViewModel.Instance;
        FitToWorkArea();

        Loaded += (_, _) => NavigationView.Navigate(typeof(DistributedPage));
    }

    /// <summary>Keeps the window inside the screen on small or scaled displays.</summary>
    private void FitToWorkArea()
    {
        Rect workArea = SystemParameters.WorkArea;
        Width = Math.Max(MinWidth, Math.Min(Width, workArea.Width * 0.95));
        Height = Math.Max(MinHeight, Math.Min(Height, workArea.Height * 0.95));
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationView.GoBack();
    }

    private void PanelButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationView.IsPaneOpen = !NavigationView.IsPaneOpen;
    }
    

    private void CloseBtn_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Minimize_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow.WindowState = WindowState.Minimized;
    }

    private void Maximize_OnClick(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        else 
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
    }
}