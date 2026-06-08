using EwECore;
using System;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcosimNode : cEwECoreNode
    {

        public cEcosimNode(cCore core, cEcoSimModelParameters parms) : base(core, parms)
        {
        }

        protected cEcoSimModelParameters parms => (cEcoSimModelParameters)this.CoreObj;

        // Accessor
        public cForcingFunctionNode? effort(int iFleet)
        {
            if ((iFleet < 1) | (iFleet > this.Core.nFleets))
            {
                Console.WriteLine("! Ecosim Effort function {0} invalid, must be fleet [1, {1}]", iFleet, this.Core.nFleets);
                return null;
            }

            cFishingEffortShapeManger man = this.Core.FishingEffortShapeManager;
            cShapeData[] shapes = (cShapeData[])man.Shapes;
            return new cForcingFunctionNode(this.Core, shapes[iFleet]);
        }

        public cForcingFunctionNode? fishingmortality(int iGroup)
        {
            if ((iGroup < 1) | (iGroup > this.Core.nGroups))
            {
                Console.WriteLine("! Ecosim F function {0} invalid, must be group [1, {1}]", iGroup, this.Core.nGroups);
                return null;
            }

            cFishingMortalityShapeManger man = this.Core.FishMortShapeManager;
            cShapeData[] shapes = (cShapeData[])man.Shapes;
            return new cForcingFunctionNode(this.Core, shapes[iGroup]);
        }

        public cEnvResponseFunctionNode? envresponse(int iIndex)
        {
            cEnviroResponseShapeManager man = this.Core.EnviroResponseShapeManager;
            if ((iIndex < 1) | (iIndex > man.Count))
            {
                Console.WriteLine("! Ecosim env response function {0} invalid, must be [1, {1}]", iIndex, man.Count);
                return null;
            }

            cShapeData[] shapes = (cShapeData[])man.Shapes;
            return new cEnvResponseFunctionNode(this.Core, (cEnviroResponseFunction)shapes[iIndex - 1]);
        }

        //public MortResponseFunction? mortalityresponse(int iIndex)
        //{
        //    cEcosimMortalityResponseManager man = this.Core.EcosimMortalityResponseManager;

        //    if (iIndex < 0) return null;
        //    if (iIndex > man.nInputData) return null;

        //    return new MortResponseFunction(man.get_InputData(iIndex));
        //}

    }
}