using Modeling_And_Simulation_EAS.Entities;

namespace Modeling_And_Simulation_EAS.Simulation;

public sealed record SimulationRun(
    IReadOnlyList<RunResult> Runs,
    StoreParameters Parameters,
    double MeanDemandA,
    double MeanDemandB,
    double MeanLeadTime)
{
    public double Ceiling => MeanDemandA * Parameters.ProfitA + MeanDemandB * Parameters.ProfitB;
}