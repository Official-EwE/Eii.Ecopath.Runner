using EwECore;
using EwERuEii.Ecopath.Runner.ServicesnConsole.Automation;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcospaceNode : cEwECoreNode
    {
        public cEcospaceNode(cCore core, cEcospaceModelParameters parms, ILogger logger) : base(core, parms, logger) 
        {
        }

        // Accessor
        public cMPANode? mpa(int iMPA)
        {
            if ((iMPA < 1) | (iMPA > this.Core.nMPAs))
            {
                Logger.LogError("Ecospace MPA {MPA} invalid, must be [1, {MaxMPA}]", iMPA, Core.nMPAs);
                return null;
            }
            return new cMPANode(this.Core, this.Core.get_EcospaceMPAs(iMPA), Logger);
        }

        public cHabitatNode? habitat(int iHabitat)
        {
            if ((iHabitat < 1) | (iHabitat > this.Core.nHabitats))
            {
                Logger.LogError("Ecospace habitat {Habitat} invalid, must be [1, {MaxHabitat}]", iHabitat, Core.nHabitats);
                return null;
            }
            return new cHabitatNode(this.Core, this.Core.get_EcospaceHabitats(iHabitat), Logger);
        }

        public cEcospaceEnvDriverNode? envdriver(int iIndex)
        {
            if ((iIndex < 0) | (iIndex > this.Core.nEnvironmentalDriverLayers))
            {
                Logger.LogError("Ecospace env driver {Index} invalid, must be 0 (depth) or [1, {MaxIndex}]", iIndex, Core.nEnvironmentalDriverLayers);
                return null;
            }
            cEcospaceBasemap bm = this.Core.EcospaceBasemap;
            return new cEcospaceEnvDriverNode(this.Core, iIndex == 0 ? bm.LayerDepth : bm.get_LayerDriver(iIndex), Logger);
        }

        public cEcospaceGroupNode? group(int iGroup)
        {
            if ((iGroup <= 0) | (iGroup > this.Core.nGroups))
            {
                Logger.LogError("Ecospace group {Group} invalid, must be [1, {MaxGroup}]", iGroup, Core.nGroups);
                return null;
            }
            return new cEcospaceGroupNode(this.Core, this.Core.get_EcospaceGroupInputs(iGroup), Logger);
        }

        public cEcospaceFleetNode? fleet(int iFleet)
        {
            if ((iFleet < 0) | (iFleet > this.Core.nFleets))
            { 
                Logger.LogError("Ecospace fleet {Fleet} invalid, must be [1, {MaxFleet}]", iFleet, Core.nFleets);
                return null;
            }
            return new cEcospaceFleetNode(this.Core, this.Core.get_EcospaceFleetInputs(iFleet), Logger);
        }

        public cMapNode? regions()
        {
            cEcospaceBasemap bm = this.Core.EcospaceBasemap;
            return new cMapNode(this.Core, bm.LayerRegion, Logger);
        }
    }
}
