using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcopathFleetNode : cEwECoreNode
    {
        public cEcopathFleetNode(cCore core, cEcopathFleetInput fleet, ILogger logger) : base(core, fleet, logger)
        { 
        }

        protected cEcopathFleetInput Group => (cEcopathFleetInput)CoreObj;

        public bool value_of(float val, int iGroup)
        {
            return SetVariable(eVarNameFlags.OffVesselPrice, iGroup, val);
        }

        public bool values(int[] value_of)
        {
            bool bOK = true;
            for (int i = 0; i < Math.Min(value_of.Length, this.Core.nGroups); i++)
                bOK &= this.value_of(value_of[i], i);
            return bOK;
        }

        public bool landings_of(float val, int iGroup)
        {
            return SetVariable(eVarNameFlags.Landings, iGroup, val);
        }

        public bool landings(int[] landings_of)
        {
            bool bOK = true;
            for (int i = 0; i < Math.Min(landings_of.Length, this.Core.nGroups); i++)
                bOK &= this.landings_of(landings_of[i], i);
            return bOK;
        }

        public bool discards_of(float val, int iGroup)
        {
            return SetVariable(eVarNameFlags.Discards, iGroup, val);
        }

        public bool discards(int[] discards_of)
        {
            bool bOK = true;
            for (int i = 0; i < Math.Min(discards_of.Length, this.Core.nGroups); i++)
                bOK &= this.discards_of(discards_of[i], i);
            return bOK;
        }

    }
}
