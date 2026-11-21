namespace Modeling_And_Simulation_EAS.Entities;

public class ProductDay
{
    public int Received { get; set; }
    public int Begin { get; set; }
    public int DemandRn { get; set; }
    public int Demand { get; set; }
    public int Sold { get; set; }
    public int End { get; set; }
    
    public int Lost => Demand - Sold;
    public bool StockedOut => Lost > 0;
}