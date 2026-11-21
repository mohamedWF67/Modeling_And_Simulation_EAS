using Modeling_And_Simulation_EAS.Entities;
using Modeling_And_Simulation_EAS.Random;

namespace Modeling_And_Simulation_EAS;

public class Store
{
    public string name;
    /// <summary>Seed used when no seed is given to the constructor.</summary>
    public const ulong DefaultSeed = 123456;

    private LCG rng = new LCG(DefaultSeed);
    
    public DiscreteDistribution DemandA { get; private set; }
    public DiscreteDistribution DemandB { get; private set; }
    public DiscreteDistribution LeadTime { get; private set; }
    
    public Store(string name) => this.name = name;

    /// <summary>Creates a Store whose random numbers start from the given seed.</summary>
    public Store(string name, ulong seed) : this(name) => rng = new LCG(seed);
    
    public void SetDistributionTables(DiscreteDistribution DemandA, DiscreteDistribution DemandB,
        DiscreteDistribution LeadTime)
    {
        this.DemandA = DemandA;
        this.DemandB = DemandB;
        this.LeadTime = LeadTime;
    }
    
    public void BuildDistributionTable()
    {

        DemandA = new DiscreteDistribution
        (
            [5,10, 15, 20,25], [0.05, 0.1,0.25,0.3,0.3]
        );
        
        DemandB = new DiscreteDistribution
        (
            [3, 4, 5], [0.50, 0.35, 0.15]
        );
        
        LeadTime = new DiscreteDistribution
        (
            [1,2,3], [0.4, 0.35, 0.25]
        );
    }
}