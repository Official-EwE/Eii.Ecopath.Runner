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
        /// Accessor; get a Fishing Effort shape by fleet index.
        /// </summary>
        /// <param name="iFleet">1-based fleet index. Supports fleet zero (all fleets)</param>
        /// <returns></returns>
        /// -------------------------------------------------------------------
        [Description("Get the fishing effort shape for a fleet by 1-based index")]
        public cForcingFunctionNode? effort(int iFleet)
        {
            cFishingEffortShapeManger man = CoreService.FishingEffortShapeManager;
            if ((iFleet < 0) | (iFleet > man.Count))
            {
                Logger.LogError("Ecosim Effort function {Fleet} invalid, must be fleet [1, {MaxFleet}]", iFleet, CoreService.nFleets);
                return null;
            }
            // Allow for the using the 0 fleet too
            return new cForcingFunctionNode(CoreService, man.get_CoreItem(iFleet), Logger);
        }

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
            return effort(FindShape(name, man.Shapes));
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor; get a Fishing Mortality shape by group index.
        /// </summary>
        /// <param name="iGroup">1-based group index.</param>
        /// <returns></returns>
        /// -------------------------------------------------------------------
        [Description("Get the fishing mortality shape for a group by 1-based index")]
        public cForcingFunctionNode? fishingmortality(int iGroup)
        {
            cFishingMortalityShapeManger man = CoreService.FishMortShapeManager;
            if ((iGroup < 1) | (iGroup > man.Count))
            {
                Logger.LogError("Ecosim F function {Group} invalid, must be group [1, {MaxGroup}]", iGroup, CoreService.nGroups);
                return null;
            }
            return new cForcingFunctionNode(CoreService, man.get_CoreItem(iGroup), Logger);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor alias; get a Fishing Mortality shape by group index.
        /// </summary>
        /// <param name="iFleet">1-based fleet index. Supports fleet zero (all fleets)</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(iGroup)"/>
        /// -------------------------------------------------------------------
        [Description("Alias for fishingmortality(int)")]
        public cForcingFunctionNode? f(int iGroup)
        {
            return fishingmortality(iGroup);
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
            return fishingmortality(FindShape(groupname, man.Shapes));
        }

        #endregion // Fisheries

        #region Forcing function

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor; get a Forcing Function shape by function index.
        /// </summary>
        /// <param name="iIndex">1-based shape index.</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(int)"/>
        /// -------------------------------------------------------------------
        [Description("Get a forcing function shape by 1-based index")]
        public cForcingFunctionNode? forcingfunction(int iIndex)
        {
            cForcingFunctionShapeManager man = CoreService.ForcingShapeManager;
            if ((iIndex < 1) | (iIndex > man.Count))
            {
                Logger.LogError("Ecosim forcing function {Index} invalid, must be [1, {MaxIndex}]", iIndex, man.Count);
                return null;
            }

            return new cForcingFunctionNode(CoreService, man.get_CoreItem(iIndex), Logger);
        }

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
            return forcingfunction(FindShape(name, man.Shapes));
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor alias; get a Forcing Function shape by index.
        /// </summary>
        /// <param name="iIndex">one-based function index.</param>
        /// <returns></returns>
        /// <see cref="forcingfunction(int)"/>
        /// -------------------------------------------------------------------
        [Description("Alias for forcingfunction(int)")]
        public cForcingFunctionNode? ff(int iIndex)
        {
            return forcingfunction(iIndex);
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

        #region Environmental responses

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor; get an Environmental Response functiopn by index.
        /// </summary>
        /// <param name="iIndex">1-based shape index.</param>
        /// <returns></returns>
        /// -------------------------------------------------------------------
        [Description("Get an environmental response function by 1-based index")]
        public cEnvResponseFunctionNode? envresponse(int iIndex)
        {
            cEnviroResponseShapeManager man = this.Core.EnviroResponseShapeManager;
            if ((iIndex < 1) | (iIndex > man.Count))
            {
                Logger.LogError("Ecosim env response function {Index} invalid, must be [1, {MaxIndex}]", iIndex, man.Count);
                return null;
            }
            return new cEnvResponseFunctionNode(CoreService, (cEnviroResponseFunction)man.get_CoreItem(iIndex), Logger);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Accessor alias; get an Environmental Response functiopn by name.
        /// </summary>
        /// <param name="name">The environmental response function name.</param>
        /// <returns></returns>
        /// -------------------------------------------------------------------
        [Description("Get an environmental response function by name")]
        public cEnvResponseFunctionNode? envresponse(string name)
        {
            cEnviroResponseShapeManager man = this.Core.EnviroResponseShapeManager;
            return envresponse(FindShape(name, man.Shapes));
        }

        #endregion // Environmental responses

        #region // Other mortality

        //public MortResponseFunction? mortalityresponse(int iIndex)
        //{
        //    cEcosimMortalityResponseManager man = this.Core.EcosimMortalityResponseManager;

        //    if (iIndex < 0) return null;
        //    if (iIndex > man.nInputData) return null;

        //    return new MortResponseFunction(CoreService, man.get_InputData(iIndex), Logger);
        //}

        #endregion // Other mortality

        #region Vulnerabilities

        public cVulnerabilitiesNode? vulnerabilities()
        {
            return new cVulnerabilitiesNode(CoreService, Logger);
        }

        #endregion // Vulnerabilities
    }
}