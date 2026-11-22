using System.Windows.Controls;
using Modeling_And_Simulation_EAS.ViewModels;

namespace Modeling_And_Simulation_EAS.Pages;

/// <summary>Q5 and Q6: searches for a better review period N and better order-up-to levels mA, mB.</summary>
public partial class OptimizationPage : Page
{
    /// <summary>Builds the page and binds it to the optimization view model.</summary>
    public OptimizationPage()
    {
        InitializeComponent();
        DataContext = OptimizationViewModel.Instance;
    }
}
