using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Modeling_And_Simulation_EAS.Entities;
using Modeling_And_Simulation_EAS.Simulation;

namespace Modeling_And_Simulation_EAS.ViewModels;

public partial class OptimizationViewModel : ObservableObject
{
    public static OptimizationViewModel Instance { get; } = new();

    private OptimizationViewModel() { }

    [ObservableProperty] private double? maxReviewPeriod = 10;
    [ObservableProperty] private double? stepA = 10;
    [ObservableProperty] private double? stepB = 5;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SweepReviewPeriodCommand), nameof(SweepLevelsCommand))]
    private bool isBusy;

    [ObservableProperty] private double progress;

    [ObservableProperty]
    private string status =
        "Uses Days, Runs, Seed, the tables and the policy of the last Compute. Every policy is tested on the same seeds.";

    #region Q5
    [ObservableProperty] private IReadOnlyList<PolicyRow> q5Rows = [];
    [ObservableProperty] private string[] q5Labels = [];
    [ObservableProperty] private string q5Best = "";
    [ObservableProperty] private PolicyRow? bestReviewPeriod;
    public ObservableCollection<double> Q5Diff { get; } = new();
    #endregion

    #region Q6
    [ObservableProperty] private IReadOnlyList<PolicyRow> q6Rows = [];
    [ObservableProperty] private string[] q6Labels = [];
    [ObservableProperty] private string q6Best = "";
    [ObservableProperty] private PolicyRow? bestLevels;
    public ObservableCollection<double> Q6Diff { get; } = new();
    #endregion

    private bool CanRun() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task SweepReviewPeriodAsync()
    {
        if (SimulationViewModel.Instance.LastSetup is not { } setup)
        {
            Status = "Press Compute on the Distribution page first.";
            return;
        }

        int maxN = ToInt(MaxReviewPeriod, 1, 30);
        List<StoreParameters> policies = Enumerable.Range(1, maxN)
            .Select(n => setup.Parameters with { ReviewPeriod = n })
            .ToList();

        (PolicyRow current, List<PolicyRow> rows) = await RunAsync(setup, policies);

        Q5Rows = rows;
        Q5Labels = rows.Select(r => $"{r.ReviewPeriod}").ToArray();
        Fill(Q5Diff, rows.Select(r => r.DiffVsCurrent));

        BestReviewPeriod = rows
            .OrderByDescending(r => Math.Round(r.ProfitPerDay, 2))
            .ThenByDescending(r => r.ReviewPeriod)
            .First();
        Q5Best = $"Best N = {BestReviewPeriod.ReviewPeriod}: {BestReviewPeriod.ProfitPerDay:0.00} LE/day " +
                 $"({BestReviewPeriod.DiffVsCurrent:+0.00;-0.00;0.00} vs the current N = {current.ReviewPeriod}: {current.ProfitPerDay:0.00})";
    }

    [RelayCommand(CanExecute = nameof(CanRun))]
    private async Task SweepLevelsAsync()
    {
        if (SimulationViewModel.Instance.LastSetup is not { } setup)
        {
            Status = "Press Compute on the Distribution page first.";
            return;
        }

        StoreParameters p = setup.Parameters;
        int stepA = ToInt(StepA, 1, 100);
        int stepB = ToInt(StepB, 1, 25);

        var policies = new List<StoreParameters>();
        for (int b = 0; b * p.SizeB <= p.Capacity; b += stepB)
        {
            int maxA = (p.Capacity - b * p.SizeB) / p.SizeA;
            for (int a = 0; a <= maxA; a += stepA)
                policies.Add(p with { LevelA = a, LevelB = b });
            if (maxA % stepA != 0)
                policies.Add(p with { LevelA = maxA, LevelB = b });
        }

        (PolicyRow current, List<PolicyRow> rows) = await RunAsync(setup, policies);

        Q6Rows = rows
            .OrderByDescending(r => Math.Round(r.ProfitPerDay, 2))
            .ThenBy(r => r.Space)
            .Take(10)
            .ToList();

        List<PolicyRow> bestPerB = rows
            .GroupBy(r => r.LevelB)
            .OrderBy(g => g.Key)
            .Select(g => g.MaxBy(r => r.ProfitPerDay)!)
            .ToList();
        Q6Labels = bestPerB.Select(r => $"{r.LevelB}").ToArray();
        Fill(Q6Diff, bestPerB.Select(r => r.DiffVsCurrent));

        BestLevels = Q6Rows[0];
        Q6Best = $"Best mA = {BestLevels.LevelA}, mB = {BestLevels.LevelB} ({BestLevels.Space} ft³): " +
                 $"{BestLevels.ProfitPerDay:0.00} LE/day ({BestLevels.DiffVsCurrent:+0.00;-0.00;0.00} vs the current " +
                 $"mA = {current.LevelA}, mB = {current.LevelB}: {current.ProfitPerDay:0.00})";
    }

    [RelayCommand]
    private void UseBestReviewPeriod()
    {
        if (BestReviewPeriod is { } r)
            SimulationViewModel.Instance.ApplyPolicy(r.ReviewPeriod, r.LevelA, r.LevelB);
    }

    [RelayCommand]
    private void UseBestLevels()
    {
        if (BestLevels is { } r)
            SimulationViewModel.Instance.ApplyPolicy(r.ReviewPeriod, r.LevelA, r.LevelB);
    }

    private async Task<(PolicyRow Current, List<PolicyRow> Rows)> RunAsync(
        SimulationSetup setup, List<StoreParameters> policies)
    {
        IsBusy = true;
        Progress = 0;
        Status = $"Testing {policies.Count} policies × {setup.Replications} runs × {setup.Parameters.Days} days…";
        IProgress<double> report = new Progress<double>(value => Progress = value);

        try
        {
            var result = await Task.Run(() =>
            {
                PolicyRow current = Evaluate(setup, setup.Parameters);
                var rows = new List<PolicyRow>(policies.Count);
                for (int i = 0; i < policies.Count; i++)
                {
                    PolicyRow row = Evaluate(setup, policies[i]);
                    rows.Add(row with { DiffVsCurrent = row.ProfitPerDay - current.ProfitPerDay });
                    report.Report(100.0 * (i + 1) / policies.Count);
                }
                return (current, rows);
            });

            Status = $"Tested {policies.Count} policies × {setup.Replications} runs × {setup.Parameters.Days} days.";
            return result;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static PolicyRow Evaluate(SimulationSetup setup, StoreParameters parameters)
    {
        List<RunResult> runs = setup.RunAll(parameters);
        return new PolicyRow(
            parameters.ReviewPeriod,
            parameters.LevelA,
            parameters.LevelB,
            parameters.SpaceNeeded,
            runs.Average(r => r.AverageDailyProfit),
            runs.Average(r => (double)r.TotalLostProfit / r.Days),
            runs.Average(r => (double)r.TrimmedDeliveries),
            0);
    }

    private static void Fill(ObservableCollection<double> collection, IEnumerable<double> values)
    {
        collection.Clear();
        foreach (double value in values)
            collection.Add(Math.Round(value, 2));
    }

    private static int ToInt(double? value, int min, int max) =>
        (int)Math.Clamp(Math.Round(value ?? min), min, max);
}
