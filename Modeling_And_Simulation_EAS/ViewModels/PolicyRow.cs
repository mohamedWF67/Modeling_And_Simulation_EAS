namespace Modeling_And_Simulation_EAS.ViewModels;

public sealed record PolicyRow(
    int ReviewPeriod,
    int LevelA,
    int LevelB,
    int Space,
    double ProfitPerDay,
    double LostProfitPerDay,
    double TrimmedPerRun,
    double DiffVsCurrent);
