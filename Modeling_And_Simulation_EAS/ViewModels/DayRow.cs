using Modeling_And_Simulation_EAS.Entities;

namespace Modeling_And_Simulation_EAS.ViewModels;

public sealed record DayRow(
    int Day, int Cycle,
    int? ReceivedA, int BeginA, int RnA, int DemandA, int SoldA, int LostA, int EndA,
    int? ReceivedB, int BeginB, int RnB, int DemandB, int SoldB, int LostB, int EndB,
    int SpaceUsed, int Profit, int LostProfit,
    int? OrderA, int? OrderB, int? RnLeadTime, int? LeadTime, int? ArrivalDay, int? DaysUntilArrival,
    bool StockoutA, bool StockoutB, bool HadDelivery)
{
    public static DayRow From(DayRecord r) => new(
        r.Day, r.Cycle,
        r.A.Received == 0 ? null : r.A.Received, r.A.Begin, r.A.DemandRn, r.A.Demand, r.A.Sold, r.A.Lost, r.A.End,
        r.B.Received == 0 ? null : r.B.Received, r.B.Begin, r.B.DemandRn, r.B.Demand, r.B.Sold, r.B.Lost, r.B.End,
        r.SpaceUsed, r.Profit, r.LostProfit,
        r.OrderPlaced?.QuantityA, r.OrderPlaced?.QuantityB, r.OrderPlaced?.LeadTimeRn,
        r.OrderPlaced?.LeadTime, r.OrderPlaced?.ArrivalDay, r.DaysUntilArrival,
        r.A.StockedOut, r.B.StockedOut, r.HadDelivery);
}