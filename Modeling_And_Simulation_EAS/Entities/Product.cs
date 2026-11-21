namespace Modeling_And_Simulation_EAS.Entities;

public sealed class Product
{
    /// <summary>Short name shown in the tables ("A" or "B").</summary>
    public string Name { get; }
    /// <summary>Space one unit takes in the store (ft³).</summary>
    public int Size { get; }
    /// <summary>Net profit for each unit sold (LE).</summary>
    public int ProfitPerUnit { get; }
    /// <summary>m: the order-up-to level used at every review.</summary>
    public int OrderUpToLevel { get; }
    /// <summary>Daily demand distribution (values, probabilities, RN ranges).</summary>
    public DiscreteDistribution Demand { get; }
    /// <summary>Units in the store right now.</summary>
    public int OnHand { get; private set; }
    /// <summary>Units ordered but not delivered yet.</summary>
    public int OnOrder { get; private set; }
    /// <summary>Stock the store can count on: on hand plus on order.</summary>
    public int InventoryPosition => OnHand + OnOrder;
    /// <summary>Space the units on hand take right now (ft³).</summary>
    public int SpaceUsed => OnHand * Size;
    
    public Product(string name, int size, int profitPerUnit, int orderUpToLevel,
        DiscreteDistribution demand, int initialStock)
    {
        Name = name;
        Size = size;
        ProfitPerUnit = profitPerUnit;
        OrderUpToLevel = orderUpToLevel;
        Demand = demand;
        OnHand = initialStock;
    }
    
    public int QuantityToOrder() => Math.Max(0, OrderUpToLevel - InventoryPosition);

    public void PlaceOrder(int quantity)
    {
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        OnOrder += quantity;
    }
    
    public void Receive(int ordered, int accepted)
    {
        if (accepted < 0 || accepted > ordered) throw new ArgumentOutOfRangeException(nameof(accepted));
        OnOrder -= ordered;
        OnHand += accepted;
    }
    
    public int Sell(int demand)
    {
        int sold = Math.Min(demand, OnHand);
        OnHand -= sold;
        return sold;
    }
}