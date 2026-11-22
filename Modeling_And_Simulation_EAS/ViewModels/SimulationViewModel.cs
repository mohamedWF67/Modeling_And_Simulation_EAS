using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Modeling_And_Simulation_EAS.Entities;
using Modeling_And_Simulation_EAS.Simulation;
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
    
    private static readonly StoreParameters BriefDefaults = new();
    [ObservableProperty]
    private double? reviewPeriod = 4;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpaceNeeded), nameof(IsOverCapacity), nameof(SpaceWarning))]
    private double? levelA = 160;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpaceNeeded), nameof(IsOverCapacity), nameof(SpaceWarning))]
    private double? levelB = 40;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOverCapacity), nameof(SpaceWarning))]
    private double? capacity = 300;
    public int SpaceNeeded => (int)((LevelA ?? 0) * BriefDefaults.SizeA + (LevelB ?? 0) * BriefDefaults.SizeB);
    public bool IsOverCapacity => SpaceNeeded > (Capacity ?? 0);
    public string SpaceWarning =>
        $"mA·{BriefDefaults.SizeA} + mB·{BriefDefaults.SizeB} = {SpaceNeeded} ft³, but the store holds {Capacity ?? 0} ft³. " +
        "Deliveries that don't fit will be cut down.";

    // ---------------- Table of the last run ----------------

    /// <summary>One row per day (Simulation table page).</summary>
    [ObservableProperty]
    private IReadOnlyList<DayRow> dayRows = [];

    /// <summary>One row per order (Orders view).</summary>
    [ObservableProperty]
    private IReadOnlyList<OrderRow> orderRows = [];

    /// <summary>Which table is shown: 0 combined, 1 product A, 2 product B, 3 orders.</summary>
    [ObservableProperty]
    private int selectedTableIndex;

    [ObservableProperty]
    private IReadOnlyList<RunResult> runResults = [];

    public SimulationSetup? LastSetup { get; private set; }
    
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
            
            var parameters = new StoreParameters(
                Days: count,
                ReviewPeriod: ReadWholeNumber(ReviewPeriod, "Review period N", 1, 30),
                LevelA: ReadWholeNumber(LevelA, "mA", 0, 10_000),
                LevelB: ReadWholeNumber(LevelB, "mB", 0, 10_000),
                Capacity: ReadWholeNumber(Capacity, "Capacity", 1, 100_000));

            var results = new List<RunResult>();
            Store? firstRun = null;

            for (int r = 0; r < reps; r++)
            {
                var store = new Store(StoreName, (ulong)seedValue + (ulong)r);
                store.SetDistributionTables(demanda, demandb, lead);
                store.KeepTable = r == 0;              // only run 1 is shown in the table
                results.Add(store.Run(parameters));
                firstRun ??= store;
            }

            RunResults = results;
            LastSetup = new SimulationSetup(parameters, demanda, demandb, lead, seedValue, reps);
            DayRows = firstRun!.Days.Select(DayRow.From).ToList();
            OrderRows = firstRun.Orders.Select(OrderRow.From).ToList();

            RunResult first = results[0];
            Summary = $"Profit/day {first.AverageDailyProfit:0.0} LE, end A {first.AverageEndA:0.0}, " +
                      $"end B {first.AverageEndB:0.0}, orders {first.OrderCount}, trimmed {first.TrimmedDeliveries}";
            ErrorMessage = null;
            
            ResultsViewModel.Instance.ConnectRuns(new SimulationRun(results, parameters,
                demanda.TheoreticalMean, demandb.TheoreticalMean, lead.TheoreticalMean));
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    public void ApplyPolicy(int reviewPeriod, int levelA, int levelB)
    {
        ReviewPeriod = reviewPeriod;
        LevelA = levelA;
        LevelB = levelB;
        Compute();
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
