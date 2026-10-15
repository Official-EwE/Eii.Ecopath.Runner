using Eii.Ecopath.Runner.Services.Automation.Ecosim.Functions;
using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcosimNode : cEwECoreNode
    {

        public cEcosimNode(ICoreService coreService, cEcoSimModelParameters parms, ILogger logger) : base(coreService, parms, logger)
        {
        }

        #region Fisheries 

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor alias; get a Fishing Effort shape by fleet name.
        /// </summary>
        /// <param name="name">Fleet name.</param>
        /// <returns></returns>
        /// -------------------------------------------------------------------
        [Description("Get the fishing effort shape for a fleet by name")]
        public cForcingFunctionNode? effort(string name)
        {
            cFishingEffortShapeManger man = CoreService.FishingEffortShapeManager;
            cShapeData? shape = FindShape(name, man.Shapes, Logger, "Fishing Effort Shape");
            if (shape == null) return null;
            return new cForcingFunctionNode(CoreService, shape, Logger);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor alias; get a Fishing Mortality shape by group name.
        /// </summary>
        /// <param name="iFleet">1-based fleet index. Supports fleet zero (all fleets)</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(int)"/>
        /// -------------------------------------------------------------------
        [Description("Alias for fishingmortality(string)")]
        public cForcingFunctionNode? f(string groupname)
        {
            return fishingmortality(groupname);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor alias; get a Fishing Mortality shape by group name.
        /// </summary>
        /// <param name="iFleet">1-based fleet index. Supports fleet zero (all fleets)</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(int)"/>
        /// -------------------------------------------------------------------
        [Description("Get the fishing mortality shape for a group by name")]
        public cForcingFunctionNode? fishingmortality(string groupname)
        {
            cFishingMortalityShapeManger man = CoreService.FishMortShapeManager;
            cShapeData? shape = FindShape(groupname, man.Shapes, Logger, "Fishing Mortality Shape");
            if (shape == null) return null;
            return new cForcingFunctionNode(CoreService, shape, Logger);
        }

        #endregion // Fisheries

        #region Forcing function

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor alias; get a Forcing Function shape by name.
        /// </summary>
        /// <param name="name">function name.</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(int)"/>
        /// -------------------------------------------------------------------
        [Description("Get a forcing function shape by name")]
        public cForcingFunctionNode? forcingfunction(string name)
        {
            cForcingFunctionShapeManager man = CoreService.ForcingShapeManager;
            cShapeData? shape = FindShape(name, man.Shapes, Logger, "Forcing Function Shape");
            if (shape == null) return null;
            return new cForcingFunctionNode(CoreService, shape, Logger);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor alias; get a Forcing Function shape by name.
        /// </summary>
        /// <param name="name">function name.</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(int)"/>
        /// -------------------------------------------------------------------
        [Description("Alias for forcingfunction(string)")]
        public cForcingFunctionNode? ff(string name)
        {
            return forcingfunction(name);
        }

        #endregion // Forcing function

        #region Mediation functions

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor alias; get a mediation function by name.
        /// </summary>
        /// <param name="name">The function name.</param>
        /// <returns></returns>
        /// -------------------------------------------------------------------
        [Description("Get an mediation function by name")]
        public cMediationFunctionNode? mediationfunction(string name)
        {
            var man = this.Core.MediationShapeManager;
            cShapeData? shape = FindShape(name, man.Shapes, Logger, "Mediation Shape");
            if (shape == null) return null;
            return new cMediationFunctionNode(CoreService, (cMediationFunction)shape, Logger);
        }

        #endregion // Mediation functions

        #region Environmental responses

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor; get an Environmental Response functiopn by name.
        /// </summary>
        /// <param name="name">The environmental response function name.</param>
        /// <returns></returns>
        /// -------------------------------------------------------------------
        [Description("Get an environmental response function by name")]
        public cEnvResponseFunctionNode? envresponsefunction(string name)
        {
            cEnviroResponseShapeManager man = this.Core.EnviroResponseShapeManager;
            cShapeData? shape = FindShape(name, man.Shapes, Logger, "Environmental Response Shape");
            if (shape == null) return null;
            return new cEnvResponseFunctionNode(CoreService, (cEnviroResponseFunction)shape, Logger);
        }

        #endregion // Environmental responses

        //#region // Other mortality

        //public MortResponseFunction? mortalityresponse(int iIndex)
        //{
        //    cEcosimMortalityResponseManager man = this.Core.EcosimMortalityResponseManager;

        //    if (iIndex < 0) return null;
        //    if (iIndex > man.nInputData) return null;

        //    return new MortResponseFunction(CoreService, man.get_InputData(iIndex), Logger);
        //}

        //#endregion // Other mortality

        #region Vulnerabilities

        public cVulnerabilitiesNode? vulnerabilities()
        {
            return new cVulnerabilitiesNode(CoreService, Logger);
        }

        #endregion // Vulnerabilities
    }
}