using Modeling_And_Simulation_EAS.Entities;

namespace Modeling_And_Simulation_EAS.ViewModels;
public sealed record OrderRow(
    int Id, int PlacedOnDay, int QuantityA, int QuantityB, int Rn, int LeadTime, int ArrivalDay,
    int? ReceivedA, int? ReceivedB, int? ReturnedA, int? ReturnedB, bool Trimmed)
{
    public static OrderRow From(Order o) => new(
        o.Id, o.PlacedOnDay, o.QuantityA, o.QuantityB, o.LeadTimeRn, o.LeadTime, o.ArrivalDay,
        o.IsReceived ? o.ReceivedA : null, o.IsReceived ? o.ReceivedB : null,
        o.IsReceived ? o.ReturnedA : null, o.IsReceived ? o.ReturnedB : null,
        o.WasTrimmed);
}