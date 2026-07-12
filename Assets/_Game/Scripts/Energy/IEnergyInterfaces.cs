using CleanEnergy.Simulation;

namespace CleanEnergy.Energy
{
    public interface IEnergyProducer
    {
        string NodeId { get; }
        float GetAvailableProduction(SimulationContext context);
    }

    public interface IEnergyConsumer
    {
        string NodeId { get; }
        float GetDemand(SimulationContext context);
    }

    public interface IEnergyStorage
    {
        string NodeId { get; }
        float StoredEnergy { get; }
        float Capacity { get; }
        float MaxChargeRate { get; }
        float MaxDischargeRate { get; }
        float Charge(float amount);
        float Discharge(float amount);
    }
}
