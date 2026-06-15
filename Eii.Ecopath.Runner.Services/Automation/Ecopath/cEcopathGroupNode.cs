using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using System;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcopathGroupNode : cEwECoreNode
    {
        public cEcopathGroupNode(IcCoreService coreService, cEcoPathGroupInput group, ILogger logger) : base(coreService, group, logger)
        {
        }

        protected cEcoPathGroupInput Group => (cEcoPathGroupInput)CoreObj;

        [Description("Set the PB value")]
        public bool pb([Description("The PB value to set")] float val)
        {
            return SetVariable(eVarNameFlags.PBInput, val);
        }

        public bool qb(float val)
        {
            return SetVariable(eVarNameFlags.QBInput, val);
        }

        public bool ee(float val)
        {
            return SetVariable(eVarNameFlags.EEInput, val);
        }

        public bool b(float val)
        {
            return SetVariable(eVarNameFlags.BiomassAreaInput, val);
        }

        public bool ba(float val)
        {
            return SetVariable(eVarNameFlags.BioAccumInput, val);
        }

        public bool ba_rate(float val)
        {
            return SetVariable(eVarNameFlags.BioAccumRate, val);
        }

        public bool diet_of(int iPrey, float val)
        {
            if (this.Group.IsProducer)
                return false;
            return SetVariable(eVarNameFlags.DietComp, iPrey, val);
        }

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
