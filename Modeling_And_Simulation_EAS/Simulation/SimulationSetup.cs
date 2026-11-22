using Modeling_And_Simulation_EAS.Entities;

namespace Modeling_And_Simulation_EAS.Simulation;

public sealed record SimulationSetup(
    StoreParameters Parameters,
    DiscreteDistribution DemandA,
    DiscreteDistribution DemandB,
    DiscreteDistribution LeadTime,
    int Seed,
    int Replications)
{
    public List<RunResult> RunAll(StoreParameters parameters)
    {
        var results = new List<RunResult>(Replications);
        for (int r = 0; r < Replications; r++)
        {
            var store = new Store("Optimization", (ulong)Seed + (ulong)r) { KeepTable = false };
            store.SetDistributionTables(DemandA, DemandB, LeadTime);
            results.Add(store.Run(parameters));
        }
        return results;
    }
}
