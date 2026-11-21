using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Modeling_And_Simulation_EAS.Entities;
using Wpf.Ui.Controls;

namespace Modeling_And_Simulation_EAS.ViewModels;

/// <summary>
/// Shared state of the window: the distribution tables, the inputs of a run and the results
/// of the last run (simulation table and results). Every page binds to <see cref="Instance"/>.
/// </summary>
public partial class SimulationViewModel : ObservableObject
{
    public const string StoreName = "El Shazly";
    public const int MaxDays = 5000;
    [ObservableProperty]
    private double? replications = 10;

    public const int MaxReplications = 500;

    /// <summary>The one instance used by the window and all pages.</summary>
    public static SimulationViewModel Instance { get; } = new();

    private SimulationViewModel()
    {
        DemandA = new DistributionTableViewModel(
            "Demand A", "Units of A sold per day", SymbolRegular.Box20);
        DemandB = new DistributionTableViewModel(
            "Demand B", "Units of B sold per day", SymbolRegular.BoxMultiple20);
        LeadTime = new DistributionTableViewModel(
            "Lead time", "Days until an order arrives", SymbolRegular.Clock20);

        Distributions = [DemandA, DemandB, LeadTime];

        LoadDefaultDistributions();
        Compute();
    }

    // ---------------- Distributions ----------------

    public DistributionTableViewModel DemandA { get; private set; }
    public DistributionTableViewModel DemandB { get; private set; }
    public DistributionTableViewModel LeadTime { get; private set; }

    /// <summary>The three tables in the order they are shown.</summary>
    public IReadOnlyList<DistributionTableViewModel> Distributions { get; }

    // ---------------- Inputs ----------------

    /// <summary>Number of days to simulate in one run (NumberBox values are doubles).</summary>
    [ObservableProperty]
    private double? days = 100;

    /// <summary>Seed of the random number generator.</summary>
    [ObservableProperty]
    private double? seed = Store.DefaultSeed;

    // ---------------- Results of the last run ----------------

    /// <summary>One line describing the last run.</summary>
    [ObservableProperty]
    private string summary = "";

    /// <summary>Why the last compute failed, or null if it worked.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    public bool HasError => ErrorMessage != null;
    

    // ---------------- Commands ----------------

    /// <summary>
    /// Builds the distribution tables from the probabilities (cumulative column and RN ranges),
    /// then simulates the store day by day and refreshes the simulation table and the results.
    /// </summary>
    [RelayCommand]
    private void Compute()
    {
        try
        {
            int count = ReadWholeNumber(Days, "Days", 1, MaxDays);
            int seedValue = ReadWholeNumber(Seed, "Seed", 1, int.MaxValue - MaxReplications);
            int reps = ReadWholeNumber(Replications, "Replications", 1, MaxReplications); 

            DiscreteDistribution demanda = DemandA.Build();
            DiscreteDistribution demandb = DemandB.Build();
            DiscreteDistribution lead = LeadTime.Build();
            
            List<Store> runs = new();
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>Puts the original distribution tables back and computes again.</summary>
    [RelayCommand]
    private void ResetDistributions()
    {
        LoadDefaultDistributions();
        Compute();
    }

    // ---------------- Helpers ----------------

    /// <summary>Loads the tables from Store.BuildDistributionTable.</summary>
    private void LoadDefaultDistributions()
    {
        var defaults = new Store(StoreName);
        defaults.BuildDistributionTable();

        DemandA.Load(defaults.DemandA);
        DemandB.Load(defaults.DemandB);
        LeadTime.Load(defaults.LeadTime);
    }

    /// <summary>Reads a NumberBox value that must be a whole number between min and max.</summary>
    private static int ReadWholeNumber(double? value, string name, int min, int max)
    {
        if (value is not { } number || number % 1 != 0 || number < min || number > max)
            throw new ArgumentException($"{name} must be a whole number from {min} to {max}.");
        return (int)number;
    }
}
