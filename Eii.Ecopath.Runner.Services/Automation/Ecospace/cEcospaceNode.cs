using EwECore;
using EwERuEii.Ecopath.Runner.ServicesnConsole.Automation;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcospaceNode : cEwECoreNode
    {
        public cEcospaceNode(cCore core, cEcospaceModelParameters parms) : base(core, parms) 
        {
        }

        // Accessor
        public cMPANode? mpa(int iMPA)
        {
            if ((iMPA < 1) | (iMPA > this.Core.nMPAs))
            {
                Console.WriteLine("! Ecospace MPA {0} invalid, must be [1, {1}]", iMPA, this.Core.nMPAs);
                return null;
            }
            return new cMPANode(this.Core, this.Core.get_EcospaceMPAs(iMPA));
        }

        public cHabitatNode? habitat(int iHabitat)
        {
            if ((iHabitat < 1) | (iHabitat > this.Core.nHabitats))
            {
                Console.WriteLine("! Ecospace habitat {0} invalid, must be [1, {1}]", iHabitat, this.Core.nHabitats);
                return null;
            }
            return new cHabitatNode(this.Core, this.Core.get_EcospaceHabitats(iHabitat));
        }

        public cEcospaceEnvDriverNode? envdriver(int iIndex)
        {
            if ((iIndex < 0) | (iIndex > this.Core.nEnvironmentalDriverLayers))
            {
                Console.WriteLine("! Ecospace env driver {0} invalid, must be 0 (depth) or [1, {1}]", iIndex, this.Core.nEnvironmentalDriverLayers);
                return null;
            }
            cEcospaceBasemap bm = this.Core.EcospaceBasemap;
            return new cEcospaceEnvDriverNode(this.Core, iIndex == 0 ? bm.LayerDepth : bm.get_LayerDriver(iIndex));
        }

        public cEcospaceGroupNode? group(int iGroup)
        {
            if ((iGroup <= 0) | (iGroup > this.Core.nGroups))
            {
                Console.WriteLine("! Ecospace group {0} invalid, must be [1, {1}]", iGroup, this.Core.nGroups);
                return null;
            }
            return new cEcospaceGroupNode(this.Core, this.Core.get_EcospaceGroupInputs(iGroup));
        }

        public cEcospaceFleetNode? fleet(int iFleet)
        {
            if ((iFleet < 0) | (iFleet > this.Core.nFleets))
            { 
                Console.WriteLine("! Ecospace fleet {0} invalid, must be [1, {1}]", iFleet, this.Core.nFleets);
                return null;
            }
            return new cEcospaceFleetNode(this.Core, this.Core.get_EcospaceFleetInputs(iFleet));
        }

        public cMapNode? regions()
        {
            cEcospaceBasemap bm = this.Core.EcospaceBasemap;
            return new cMapNode(this.Core, bm.LayerRegion);
        }
    }
}
