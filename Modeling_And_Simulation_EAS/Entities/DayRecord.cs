namespace Modeling_And_Simulation_EAS.Entities;

public class DayRecord
{
    public int Day { get; }
    public int Cycle { get; }
    public int DayInCycle { get; }
    public ProductDay A { get; } = new();
    public ProductDay B { get; } = new();
    public int SpaceUsed { get; set; }
    public int Profit { get; set; }
    public int LostProfit { get; set; }
    public Order? OrderPlaced { get; set; }
    public int? DaysUntilArrival { get; set; }
    public bool HadDelivery => A.Received > 0 || B.Received > 0;
    
    public DayRecord(int day, int reviewPeriod)
    {
        Day = day;
        Cycle = (day - 1) / reviewPeriod + 1;
        DayInCycle = (day - 1) % reviewPeriod + 1;
    }
}