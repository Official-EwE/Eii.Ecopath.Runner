using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using EwERuEii.Ecopath.Runner.ServicesnConsole.Automation;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcospaceNode : cEwECoreNode
    {
        public cEcospaceNode(IcCoreService coreService, cEcospaceModelParameters parms, ILogger logger) : base(coreService, parms, logger) 
        {
        }

        // Accessor
        public cMPANode? mpa(int iMPA)
        {
            if ((iMPA < 1) | (iMPA > CoreService.nMPAs))
            {
                Logger.LogError("Ecospace MPA {MPA} invalid, must be [1, {MaxMPA}]", iMPA, CoreService.nMPAs);
                return null;
            }
            return new cMPANode(CoreService, CoreService.get_EcospaceMPAs(iMPA), Logger);
        }

        public cHabitatNode? habitat(int iHabitat)
        {
            if ((iHabitat < 1) | (iHabitat > CoreService.nHabitats))
            {
                Logger.LogError("Ecospace habitat {Habitat} invalid, must be [1, {MaxHabitat}]", iHabitat, CoreService.nHabitats);
                return null;
            }
            return new cHabitatNode(CoreService, CoreService.get_EcospaceHabitats(iHabitat), Logger);
        }

        public cEcospaceEnvDriverNode? envdriver(int iIndex)
        {
            if ((iIndex < 0) | (iIndex > CoreService.nEnvironmentalDriverLayers))
            {
                Logger.LogError("Ecospace env driver {Index} invalid, must be 0 (depth) or [1, {MaxIndex}]", iIndex, CoreService.nEnvironmentalDriverLayers);
                return null;
            }
            cEcospaceBasemap bm = CoreService.EcospaceBasemap;
            return new cEcospaceEnvDriverNode(CoreService, iIndex == 0 ? bm.LayerDepth : bm.get_LayerDriver(iIndex), Logger);
        }

        public cEcospaceGroupNode? group(int iGroup)
        {
            if ((iGroup <= 0) | (iGroup > CoreService.nGroups))
            {
                Logger.LogError("Ecospace group {Group} invalid, must be [1, {MaxGroup}]", iGroup, CoreService.nGroups);
                return null;
            }
            return new cEcospaceGroupNode(CoreService, CoreService.get_EcospaceGroupInputs(iGroup), Logger);
        }

        public cEcospaceFleetNode? fleet(int iFleet)
        {
            if ((iFleet < 0) | (iFleet > CoreService.nFleets))
            { 
                Logger.LogError("Ecospace fleet {Fleet} invalid, must be [1, {MaxFleet}]", iFleet, CoreService.nFleets);
                return null;
            }
            return new cEcospaceFleetNode(CoreService, CoreService.get_EcospaceFleetInputs(iFleet), Logger);
        }

        public cMapNode? regions()
        {
            cEcospaceBasemap bm = CoreService.EcospaceBasemap;
            return new cMapNode(CoreService, bm.LayerRegion, Logger);
        }
    }
}
