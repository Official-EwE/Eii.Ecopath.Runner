using EwECore;
using EwEPlugin;
using EwERunConsole.Instructions;
using EwERunConsoleBridge;

namespace EwERunConsole.Runtime
{ 
    internal class cEcosimModifier : cRuntimeModifier
    {
        public cEcosimModifier(cCore core, cEwEConfiguration config, cEcosimRunInstructions runmodel) : base(core, "ecosim", config, runmodel)
        {
            // Create a plug-in bridge to be able to intervene into the
            // running Ecosim model during tmie stepping
            IPlugin? pi = GetPlugin(typeof(cEcosimCallbackPluginPoint));
            if (pi != null)
            {
                cEcosimCallbackPluginPoint ppt = (cEcosimCallbackPluginPoint)pi;
                ppt.BridgeCallback = BridgeCallback;
            }
        }

        public override void ConfigureAutosave()
        {
            // ToDo
        }

        public override bool Run()
        {
            this.RunSuccess = true;
            // Go for it
            this.RunSuccess &= this.Core.RunEcosim();
            // Done
            return RunSuccess;
        }

        // Plug-in callback for making specific modifications.
        protected void BridgeCallback (cEcosimCallbackPluginPoint.EventType e, int iTime)
        {
            if (e== cEcosimCallbackPluginPoint.EventType.BeginTimeStep)
            {
                cEcosimDatastructures ds = this.Core.EcosimDataStructures;

                // Print out time tracking
                if ((iTime - 1) % ds.NumStepsPerYear == 0)
                {
                    Console.WriteLine("{0}",
                        (int) this.Core.EcosimFirstYear() + ((iTime - 1) / ds.NumStepsPerYear));
                }
                this.RunSuccess &= this.Apply(iTime);
            }
        }

        protected override int DateToTimeStep(DateTime date)
        {
            return this.Core.AbsoluteTimeToEcosimTimestep(date);
        }
    }
}