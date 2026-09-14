using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcospaceNode : cEwECoreNode
    {
        public cEcospaceNode(ICoreService coreService, cEcospaceModelParameters parms, ILogger logger) : base(coreService, parms, logger)
        {
        }

        #region Groups and fleets

        [Description("Select an Ecospace group by name")]
        public cEcospaceGroupNode? group(string name)
        {
            int iGroup = FindGroup(name, Logger);
            if (iGroup <= 0)
            {
                Logger.LogError("Ecospace group {Group} invalid, must be [1, {MaxGroup}]", iGroup, CoreService.nGroups);
                return null;
            }
            return new cEcospaceGroupNode(CoreService, CoreService.get_EcospaceGroupInputs(iGroup), Logger);
        }

        [Description("Select an Ecospace fleet by name")]
        public cEcospaceFleetNode? fleet(string name)
        {
            int iFleet = FindFleet(name, Logger);
            if (iFleet <= 0)
            {
                Logger.LogError("Ecospace fleet {Fleet} invalid, must be [1, {MaxFleet}]", iFleet, CoreService.nFleets);
                return null;
            }
            return new cEcospaceFleetNode(CoreService, CoreService.get_EcospaceFleetInputs(iFleet), Logger);
        }

        #endregion // Groups and fleets

        #region RelPP layer

        public cMapNode relpp()
        {
            var bm = Core.EcospaceBasemap;
            return new cMapNode(CoreService, bm.LayerRelPP, Logger);
        }
        #endregion // RelPP layer

        #region MPA


        [Description("Select an MPA by name")]
        public cMPANode? mpa(string name)
        {
            var ds = this.Core.EcospaceDataStructures;
            int iMPA = FindItem(name, ds.MPAname, Logger, "MPA");
            if (iMPA == cCore.NULL_VALUE) return null;
            return new cMPANode(CoreService, CoreService.get_EcospaceMPAs(iMPA), Logger);
        }

        #endregion // MPA

        #region Habitat

        [Description("Select a habitat by name")]
        public cHabitatNode? habitat(string name)
        {
            var ds = this.Core.EcospaceDataStructures;
            int iHabitat = FindItem(name, ds.HabitatText, Logger, "Habitat");
            return new cHabitatNode(CoreService, CoreService.get_EcospaceHabitats(iHabitat), Logger);
        }

        #endregion // Habitat

        #region Environmental drivers

        [Description("Select an environmental driver layer by name")]
        public cEcospaceEnvDriverNode? envdriver(string name)
        {
            var ds = this.Core.EcospaceDataStructures;
            int iDriver = 0;
            cEcospaceBasemap bm = CoreService.EcospaceBasemap;

            if (!string.Equals(name, "depth", StringComparison.OrdinalIgnoreCase))
                iDriver = FindItem(name, ds.EnvironmentalLayerName, Logger, "Environmental Driver");
            if (iDriver < 0) return null;
            return new cEcospaceEnvDriverNode(CoreService, iDriver == 0 ? bm.LayerDepth : bm.get_LayerDriver(iDriver), Logger);
        }

        #endregion // Environmental drivers

        #region Regions

        [Description("Access the region map layer")]
        public cMapNode? regions()
        {
            cEcospaceBasemap bm = CoreService.EcospaceBasemap;
            return new cMapNode(CoreService, bm.LayerRegion, Logger);
        }

        #endregion // Regions
    }
}
