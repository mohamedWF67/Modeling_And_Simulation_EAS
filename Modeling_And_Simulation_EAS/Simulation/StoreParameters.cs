namespace Modeling_And_Simulation_EAS.Simulation;

public enum ShortageRule
{ 
    LostSales,
    Backorders
}

public sealed record StoreParameters(
    int Days = 360,
    int ReviewPeriod = 4,
    int LevelA = 160,
    int LevelB = 40,
    int InitialA = 100,
    int InitialB = 20,
    int Capacity = 300,
    int SizeA = 1,
    int SizeB = 4,
    int ProfitA = 20,
    int ProfitB = 80,
    ShortageRule Shortage = ShortageRule.LostSales)
{
    /// <summary>Space needed to hold both products at their order-up-to levels (ft³).</summary>
    public int SpaceNeeded => LevelA * SizeA + LevelB * SizeB;

    /// <summary>True when the levels can't all fit in the store (160·1 + 40·4 = 320 > 300).</summary>
    public bool LevelsExceedCapacity => SpaceNeeded > Capacity;

    /// <summary>Space taken by the start stock (ft³).</summary>
    public int InitialSpace => InitialA * SizeA + InitialB * SizeB;

    /// <summary>Checks that every value makes sense before a run starts.</summary>
    /// <exception cref="ArgumentException">A value is out of range.</exception>
    public void Validate()
    {
        if (Days < 1) throw new ArgumentException("Days must be at least 1.");
        if (ReviewPeriod < 1) throw new ArgumentException("The review period N must be at least 1 day.");
        if (LevelA < 0 || LevelB < 0) throw new ArgumentException("The levels mA and mB can't be negative.");
        if (InitialA < 0 || InitialB < 0) throw new ArgumentException("The start stock can't be negative.");
        if (SizeA < 1 || SizeB < 1) throw new ArgumentException("Product sizes must be at least 1 ft³.");
        if (InitialSpace > Capacity)
            throw new ArgumentException($"The start stock needs {InitialSpace} ft³ but the store has {Capacity} ft³.");
    }
}