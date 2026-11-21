namespace Modeling_And_Simulation_EAS.Entities;

public sealed record RunResult(
    int Days,
    double AverageEndA,
    double AverageEndB,
    double AverageSpaceUsed,
    int MaxSpaceUsed,
    long TotalProfit,
    double AverageDailyProfit,
    double AverageCycleProfit,
    long LostUnitsA,
    long LostUnitsB,
    long TotalLostProfit,
    int StockoutDaysA,
    int StockoutDaysB,
    double FillRateA,
    double FillRateB,
    double AverageDemandA,
    double AverageDemandB,
    double AverageLeadTime,
    int OrderCount,
    int TrimmedDeliveries);