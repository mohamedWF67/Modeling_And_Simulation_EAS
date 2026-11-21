using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Modeling_And_Simulation_EAS.ViewModels;

namespace Modeling_And_Simulation_EAS.Pages;

public partial class DistributedPage : Page
{
    private const double TwoColumnMinWidth = 820;

    public DistributedPage()
    {
        InitializeComponent();
        DataContext = SimulationViewModel.Instance;
    }

    private void TablesGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        TablesGrid.Columns = e.NewSize.Width >= TwoColumnMinWidth ? 2 : 1;
    }

    /// <summary>
    /// Runs before the button's command: a probability that is still being typed is saved,
    /// so Compute uses it instead of the old value.
    /// </summary>
    private void CommitTableEdits(object sender, RoutedEventArgs e)
    {
        foreach (DataGrid grid in FindChildren<DataGrid>(TablesGrid))
            grid.CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);
    }

    private static IEnumerable<T> FindChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match)
                yield return match;

            foreach (T descendant in FindChildren<T>(child))
                yield return descendant;
        }
    }
}