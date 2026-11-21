namespace Modeling_And_Simulation_EAS.Entities;

public class Order
{
    /// <summary>Order number</summary>
    public int Id { get; }
    /// <summary>Day the order was placed</summary>
    public int PlacedOnDay { get; }
    /// <summary>Units of A ordered.</summary>
    public int QuantityA { get; }
    /// <summary>Units of B ordered.</summary>
    public int QuantityB { get; }
    /// <summary>Random number (1–100) used for the lead time</summary>
    public int LeadTimeRn { get; }
    /// <summary>Days between placing the order and receiving it.</summary>
    public int LeadTime { get; }
    
    
    public int ArrivalDay => PlacedOnDay + LeadTime;
    /// <summary>Units of A kept on delivery (less than ordered if the store was too full).</summary>
    public int ReceivedA { get; private set; }
    /// <summary>Units of B kept on delivery.</summary>
    public int ReceivedB { get; private set; }
    /// <summary>True once the order has been delivered.</summary>
    public bool IsReceived { get; private set; }
    /// <summary>Units sent back because they didn't fit in the 300 ft³ store.</summary>
    public int ReturnedA => IsReceived ? QuantityA - ReceivedA : 0;
    /// <summary>Units of B sent back because they didn't fit.</summary>
    public int ReturnedB => IsReceived ? QuantityB - ReceivedB : 0;
    /// <summary>True if part of the delivery had to be sent back for lack of space.</summary>
    public bool WasTrimmed => ReturnedA > 0 || ReturnedB > 0;
    
    public Order(int id, int placedOnDay, int quantityA, int quantityB, int leadTimeRn, int leadTime)
    {
        Id = id;
        PlacedOnDay = placedOnDay;
        QuantityA = quantityA;
        QuantityB = quantityB;
        LeadTimeRn = leadTimeRn;
        LeadTime = leadTime;
    }
    
    public void MarkReceived(int acceptedA, int acceptedB)
    {
        if (IsReceived) throw new InvalidOperationException($"Order {Id} was already received.");
        ReceivedA = acceptedA;
        ReceivedB = acceptedB;
        IsReceived = true;
    }
}