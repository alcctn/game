namespace CleanEnergy.Workers
{
    public enum WorkerType
    {
        Engineer = 0,
        Technician = 1
    }

    public interface IWorkerQuery
    {
        int EngineerCount { get; }
        int TechnicianCount { get; }
        bool HasEngineers(int required);
        bool HasTechnicians(int required);
    }

    /// <summary>Simple staff pool for Level 1 hire gates.</summary>
    public sealed class WorkerPool : IWorkerQuery
    {
        public int EngineerCount { get; private set; }
        public int TechnicianCount { get; private set; }

        public bool HasEngineers(int required) => EngineerCount >= required;
        public bool HasTechnicians(int required) => TechnicianCount >= required;

        public void Reset()
        {
            EngineerCount = 0;
            TechnicianCount = 0;
        }

        public void Restore(int engineers, int technicians)
        {
            EngineerCount = engineers < 0 ? 0 : engineers;
            TechnicianCount = technicians < 0 ? 0 : technicians;
        }

        public bool TryAdd(WorkerType type)
        {
            switch (type)
            {
                case WorkerType.Engineer:
                    EngineerCount++;
                    return true;
                case WorkerType.Technician:
                    TechnicianCount++;
                    return true;
                default:
                    return false;
            }
        }
    }
}
