using System.Windows.Controls;
using Modeling_And_Simulation_EAS.ViewModels;

namespace Modeling_And_Simulation_EAS.Pages;

public partial class SimulationTablePage : Page
{
    public SimulationTablePage()
    {
        InitializeComponent();
        DataContext = SimulationViewModel.Instance;
    }
}