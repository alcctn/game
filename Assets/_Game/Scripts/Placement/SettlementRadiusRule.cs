using System.Collections.Generic;
using CleanEnergy.Settlements;
using CleanEnergy.Workers;

namespace CleanEnergy.Placement
{
    public sealed class SettlementRadiusRule : IPlacementRule
    {
        public const string FailReasonEn = "Too far from active settlement.";
        public const string FailReasonTr = "Aktif yerleşimden çok uzak.";

        private readonly IActiveSettlementQuery _settlement;

        public SettlementRadiusRule(IActiveSettlementQuery settlement)
        {
            _settlement = settlement;
        }

        public string RuleId => "settlement_radius";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            if (_settlement == null || !_settlement.HasActiveSettlement)
            {
                return true;
            }

            if (context.Definition != null && context.Definition.Id == "village")
            {
                return true;
            }

            if (_settlement.IsInsideRadius(context.Coordinate))
            {
                return true;
            }

            failureReasons.Add(FailReasonEn);
            return false;
        }
    }

    public sealed class WorkerRequirementRule : IPlacementRule
    {
        public const string FailEngineer = "Not enough Engineers.";
        public const string FailTechnician = "Not enough Technicians.";

        private readonly IWorkerQuery _workers;

        public WorkerRequirementRule(IWorkerQuery workers)
        {
            _workers = workers;
        }

        public string RuleId => "worker_requirement";

        public bool Evaluate(PlacementContext context, List<string> failureReasons)
        {
            if (context.Definition == null || _workers == null)
            {
                return true;
            }

            var needEng = context.Definition.RequiredEngineers;
            var needTech = context.Definition.RequiredTechnicians;
            var ok = true;
            if (needEng > 0 && !_workers.HasEngineers(needEng))
            {
                failureReasons.Add(FailEngineer);
                ok = false;
            }

            if (needTech > 0 && !_workers.HasTechnicians(needTech))
            {
                failureReasons.Add(FailTechnician);
                ok = false;
            }

            return ok;
        }
    }
}
