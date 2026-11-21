using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Modeling_And_Simulation_EAS.Entities;
using Modeling_And_Simulation_EAS.Simulation;

namespace Modeling_And_Simulation_EAS.ViewModels;

public class ResultsViewModel : ObservableObject
{
    public static ResultsViewModel Instance { get; } = new();

    private SimulationRun? run;

    private IReadOnlyList<RunResult> Runs => run?.Runs ?? [];

    public void ConnectRuns(SimulationRun newRun)
    {
        run = newRun;
        OnPropertyChanged(string.Empty);
        UpdateCharts();
    }

    private double Avg(Func<RunResult, double> pick) =>
        Runs.Count == 0 ? 0 : Math.Round(Runs.Average(pick), 2);

    private static double Error(double simulated, double theory) =>
        theory == 0 ? 0 : Math.Round(Math.Abs(simulated - theory) / theory * 100, 2);

    public int RunCount => Runs.Count;
    public int Days => run?.Parameters.Days ?? 0;

    #region Q1
    public double EndA => Avg(r => r.AverageEndA);
    public double EndB => Avg(r => r.AverageEndB);

    public ObservableCollection<double> Q1Values { get; } = new();
    public string[] Q1Labels { get; } = ["Product A", "Product B"];
    #endregion

    #region Q2
    public double ProfitPerDay => Avg(r => r.AverageDailyProfit);
    public double ProfitPerCycle => Math.Round(ProfitPerDay * (run?.Parameters.ReviewPeriod ?? 0), 2);
    public double Ceiling => Math.Round(run?.Ceiling ?? 0, 2);
    public double ShareOfCeiling => Ceiling == 0 ? 0 : ProfitPerDay / Ceiling;
    public double LostProfitPerDay => Avg(r => (double)r.TotalLostProfit / r.Days);

    public ObservableCollection<double> Q2Values { get; } = new();
    public ObservableCollection<double> Q2Ceiling { get; } = new();
    public string[] Q2Labels => Runs.Select((_, i) => $"{i + 1}").ToArray();
    #endregion

    #region Q3
    public double DemandA => Avg(r => r.AverageDemandA);
    public double DemandB => Avg(r => r.AverageDemandB);
    public double TheoryDemandA => Math.Round(run?.MeanDemandA ?? 0, 2);
    public double TheoryDemandB => Math.Round(run?.MeanDemandB ?? 0, 2);
    public double ErrorDemandA => Error(DemandA, TheoryDemandA);
    public double ErrorDemandB => Error(DemandB, TheoryDemandB);

    public ObservableCollection<double> Q3Theory { get; } = new();
    public ObservableCollection<double> Q3Simulated { get; } = new();
    public string[] Q3Labels { get; } = ["Demand A", "Demand B"];
    #endregion

    #region Q4
    public double LeadTime => Avg(r => r.AverageLeadTime);
    public double TheoryLeadTime => Math.Round(run?.MeanLeadTime ?? 0, 2);
    public double ErrorLeadTime => Error(LeadTime, TheoryLeadTime);

    public ObservableCollection<double> Q4Values { get; } = new();
    public string[] Q4Labels { get; } = ["Theoretical", "Simulated"];
    #endregion

    private void UpdateCharts()
    {
        Fill(Q1Values, EndA, EndB);
        Fill(Q2Values, Runs.Select(r => Math.Round(r.AverageDailyProfit, 1)).ToArray());
        Fill(Q2Ceiling, Runs.Select(_ => Ceiling).ToArray());
        Fill(Q3Theory, TheoryDemandA, TheoryDemandB);
        Fill(Q3Simulated, DemandA, DemandB);
        Fill(Q4Values, TheoryLeadTime, LeadTime);
    }

    private static void Fill(ObservableCollection<double> collection, params double[] values)
    {
        collection.Clear();
        foreach (double value in values)
            collection.Add(value);
    }
}