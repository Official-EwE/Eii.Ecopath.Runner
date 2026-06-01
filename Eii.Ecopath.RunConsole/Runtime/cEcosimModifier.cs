using EwEBridge.Ecosim;
using EwECore;
using EwECore.Plugins;
using EwERunConsole.Instructions;
using System;

namespace EwERunConsole.Runtime
{ 
    internal class cEcosimModifier : cRuntimeModifier
    {
        public cEcosimModifier(cCore core, cEwEConfiguration config, cEcosimRunInstructions runmodel) : base(core, "ecosim", config, runmodel)
        {
            // Create a plug-in bridge to be able to intervene into the
            // running Ecosim model during tmie stepping
            IPlugin? pi = GetPlugin(typeof(EwEBridge.Ecosim.cEcosimBridgePlugin));
            if (pi != null)
            {
                cEcosimBridgePlugin ppt = (cEcosimBridgePlugin)pi;
                ppt.BridgeCallback = BridgeCallback;
            }
        }

        public override void ConfigureAutosave()
        {
            // ToDo
        }

        public override bool Run()
        {
            RunSuccess = true;
            // Go for it
            RunSuccess &= Core.RunEcosim();
            // Done
            return RunSuccess;
        }

        // Plug-in callback for making specific modifications.
        protected void BridgeCallback (cEcosimBridgePlugin.EventType e, int iTime)
        {
            if (e== cEcosimBridgePlugin.EventType.BeginTimeStep)
            {
                cEcosimDatastructures ds = Core.EcosimDataStructures;

                // Print out time tracking
                if ((iTime - 1) % ds.NumStepsPerYear == 0)
                {
                    Console.WriteLine("{0}",
                        (int) Core.EcosimFirstYear() + ((iTime - 1) / ds.NumStepsPerYear));
                }
                RunSuccess &= Apply(iTime);
            }
        }

        protected override int DateToTimeStep(DateTime date)
        {
            return Core.AbsoluteTimeToEcosimTimestep(date);
        }
    }
}