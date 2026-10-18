using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcopathGroupNode : cEwECoreNode
    {
        public cEcopathGroupNode(ICoreService coreService, cEcoPathGroupInput group, ILogger logger) : base(coreService, group, logger)
        {
        }

        protected cEcoPathGroupInput Group => (cEcoPathGroupInput)CoreObj;

        [Description("Set the PB value")]
        public bool pb([Description("The PB value to set")] float val)
        {
            return SetVariable(eVarNameFlags.PBInput, val);
        }

        [Description("Set the QB (consumption/biomass) ratio")]
        public bool qb(float val)
        {
            return SetVariable(eVarNameFlags.QBInput, val);
        }

        [Description("Set the EE (ecotrophic efficiency)")]
        public bool ee(float val)
        {
            return SetVariable(eVarNameFlags.EEInput, val);
        }

        [Description("Set the biomass")]
        public bool b(float val)
        {
            return SetVariable(eVarNameFlags.BiomassAreaInput, val);
        }

        [Description("Set the biomass accumulation")]
        public bool ba(float val)
        {
            return SetVariable(eVarNameFlags.BioAccumInput, val);
        }

        [Description("Set the biomass accumulation rate")]
        public bool ba_rate(float val)
        {
            return SetVariable(eVarNameFlags.BioAccumRate, val);
        }

        [Description("Set the diet proportion for a specific prey by 1-based prey index")]
        public bool diet_of(int iPrey, float val)
        {
            if (this.Group.IsProducer)
                return false;
            return SetVariable(eVarNameFlags.DietComp, iPrey, val);
        }

        [Description("Set the full diet composition vector")]
        public bool diet(int[] diet_of)
        {
            if (this.Group.IsProducer)
                return false;
            bool bOK = true;
            for (int i = 0; i < Math.Min(diet_of.Length, CoreService.nGroups); i++)
                bOK &= SetVariable(eVarNameFlags.DietComp, i, diet_of[i]);
            return bOK;
        }

    }
}
