using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcopathFleetNode : cEwECoreNode
    {
        public cEcopathFleetNode(ICoreService coreService, cEcopathFleetInput fleet, ILogger logger) : base(coreService, fleet, logger)
        { 
        }

        protected cEcopathFleetInput Group => (cEcopathFleetInput)CoreObj;

        [Description("Set the ex-vessel price for a specific group")]
        public bool value_of(float val, int iGroup)
        {
            return SetVariable(eVarNameFlags.OffVesselPrice, iGroup, val);
        }

        [Description("Set all ex-vessel prices")]
        public bool values(int[] value_of)
        {
            bool bOK = true;
            for (int i = 0; i < Math.Min(value_of.Length, CoreService.nGroups); i++)
                bOK &= this.value_of(value_of[i], i);
            return bOK;
        }

        [Description("Set landings for a specific group")]
        public bool landings_of(float val, int iGroup)
        {
            return SetVariable(eVarNameFlags.Landings, iGroup, val);
        }

        [Description("Set all landings")]
        public bool landings(int[] landings_of)
        {
            bool bOK = true;
            for (int i = 0; i < Math.Min(landings_of.Length, CoreService.nGroups); i++)
                bOK &= this.landings_of(landings_of[i], i);
            return bOK;
        }

        [Description("Set discards for a specific group")]
        public bool discards_of(float val, int iGroup)
        {
            return SetVariable(eVarNameFlags.Discards, iGroup, val);
        }

        [Description("Set all discards")]
        public bool discards(int[] discards_of)
        {
            bool bOK = true;
            for (int i = 0; i < Math.Min(discards_of.Length, CoreService.nGroups); i++)
                bOK &= this.discards_of(discards_of[i], i);
            return bOK;
        }

    }
}
