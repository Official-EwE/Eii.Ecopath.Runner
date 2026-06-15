using EwECore;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcosimNode : cEwECoreNode
    {

        public cEcosimNode(cCore core, cEcoSimModelParameters parms) : base(core, parms)
        {
        }

        #region Fishing effort 

        /// <summary>
        /// Accessor; get a Fishing Effort shape by fleet index.
        /// </summary>
        /// <param name="iFleet">1-based fleet index. Supports fleet zero (all fleets)</param>
        /// <returns></returns>
        public cForcingFunctionNode? effort(int iFleet)
        {
            cFishingEffortShapeManger man = this.Core.FishingEffortShapeManager;
            if ((iFleet < 0) | (iFleet > man.Count))
            {
                Console.WriteLine("! Ecosim Effort function {0} invalid, must be fleet [1, {1}]", iFleet, this.Core.nFleets);
                return null;
            }
            // Allow for the using the 0 fleet too
            return new cForcingFunctionNode(this.Core, man.get_CoreItem(iFleet));
        }

        /// <summary>
        /// Accessor alias; get a Fishing Effort shape by fleet name.
        /// </summary>
        /// <param name="name">Fleet name.</param>
        /// <returns></returns>
        public cForcingFunctionNode? effort(string name)
        {
            cFishingEffortShapeManger man = this.Core.FishingEffortShapeManager;
            return effort(FindShape(name, man.Shapes));
        }

        #endregion // Fishing effort

        #region Fishing mortality

        /// <summary>
        /// Accessor; get a Fishing Mortality shape by group index.
        /// </summary>
        /// <param name="iGroup">1-based group index.</param>
        /// <returns></returns>
        public cForcingFunctionNode? fishingmortality(int iGroup)
        {
            cFishingMortalityShapeManger man = this.Core.FishMortShapeManager;
            if ((iGroup < 1) | (iGroup > man.Count))
            {
                Console.WriteLine("! Ecosim F function {0} invalid, must be group [1, {1}]", iGroup, this.Core.nGroups);
                return null;
            }
            return new cForcingFunctionNode(this.Core, man.get_CoreItem(iGroup));
        }

        /// <summary>
        /// Accessor alias; get a Fishing Mortality shape by group index.
        /// </summary>
        /// <param name="iFleet">1-based fleet index. Supports fleet zero (all fleets)</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(iGroup)"/>
        public cForcingFunctionNode? f(int iGroup)
        {
            return fishingmortality(iGroup);
        }

        /// <summary>
        /// Accessor alias; get a Fishing Mortality shape by group name.
        /// </summary>
        /// <param name="iFleet">1-based fleet index. Supports fleet zero (all fleets)</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(int)"/>
        public cForcingFunctionNode? f(string groupname)
        {
            return fishingmortality(groupname);
        }

        /// <summary>
        /// Accessor alias; get a Fishing Mortality shape by group name.
        /// </summary>
        /// <param name="iFleet">1-based fleet index. Supports fleet zero (all fleets)</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(int)"/>
        public cForcingFunctionNode? fishingmortality(string groupname)
        {
            cFishingMortalityShapeManger man = this.Core.FishMortShapeManager;
            return fishingmortality(FindShape(groupname, man.Shapes));
        }

        #endregion // Fishing mortality

        #region Forcing function

        /// <summary>
        /// Accessor; get a Forcing Function shape by function index.
        /// </summary>
        /// <param name="iIndex">1-based shape index.</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(int)"/>
        public cForcingFunctionNode? forcingfunction(int iIndex)
        {
            cForcingFunctionShapeManager man = this.Core.ForcingShapeManager;
            if ((iIndex < 1) | (iIndex > man.Count))
            {
                Console.WriteLine("! Ecosim forcing function {0} invalid, must be [1, {1}]", iIndex, man.Count);
                return null;
            }

            return new cForcingFunctionNode(this.Core, man.get_CoreItem(iIndex));
        }

        /// <summary>
        /// Accessor alias; get a Forcing Function shape by name.
        /// </summary>
        /// <param name="name">function name.</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(int)"/>
        public cForcingFunctionNode? forcingfunction(string name)
        {
            cForcingFunctionShapeManager man = this.Core.ForcingShapeManager;
            return forcingfunction(FindShape(name, man.Shapes));
        }


        /// <summary>
        /// Accessor alias; get a Forcing Function shape by index.
        /// </summary>
        /// <param name="iIndex">one-based function index.</param>
        /// <returns></returns>
        /// <see cref="forcingfunction(int)"/>
        public cForcingFunctionNode? ff(int iIndex)
        {
            return forcingfunction(iIndex);
        }

        /// <summary>
        /// Accessor alias; get a Forcing Function shape by name.
        /// </summary>
        /// <param name="name">function name.</param>
        /// <returns></returns>
        /// <see cref="fishingmortality(int)"/>
        public cForcingFunctionNode? ff(string name)
        {
            return forcingfunction(name);
        }

        #endregion Forcing function

        //#region Environmental responses

        //public cEnvResponseFunctionNode? envresponse(int iIndex)
        //{
        //    cEnviroResponseShapeManager man = this.Core.EnviroResponseShapeManager;
        //    if ((iIndex < 1) | (iIndex > man.Count))
        //    {
        //        Console.WriteLine("! Ecosim env response function {0} invalid, must be [1, {1}]", iIndex, man.Count);
        //        return null;
        //    }
        //    return new cEnvResponseFunctionNode(this.Core, man.get_CoreItem(iIndex));
        //}

        //#endregion // Environmental responses

        //public MortResponseFunction? mortalityresponse(int iIndex)
        //{
        //    cEcosimMortalityResponseManager man = this.Core.EcosimMortalityResponseManager;

        //    if (iIndex < 0) return null;
        //    if (iIndex > man.nInputData) return null;

        //    return new MortResponseFunction(man.get_InputData(iIndex));
        //}

        #region Utility

        /// <summary>
        /// Retrieve a shape by name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shapes"></param>
        /// <returns></returns>
        int FindShape(string name, IEnumerable<cShapeData> shapes)
        {
            if (shapes == null) return cCore.NULL_VALUE;

            name = name.ToLowerInvariant();
            foreach (cShapeData shp in shapes)
            {
                if (string.Compare(name, shp.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return shp.Index;
            }

            return cCore.NULL_VALUE;
        }

        #endregion
    }
}