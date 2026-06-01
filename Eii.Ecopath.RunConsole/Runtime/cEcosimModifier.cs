using EwEBridge.Ecosim;
using EwECore;
using EwECore.Ecosim;
using EwECore.Plugins;
using EwERunConsole.Instructions;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;

namespace EwERunConsole.Runtime
{
    internal class cEcosimModifier : cRuntimeModifier
    {
        private List<cEcosimResultWriter.eResultTypes> m_autosaveresults = [];
        private bool m_bSaveAnnual = false;

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

        protected cEcosimRunInstructions MyRunModel => (cEcosimRunInstructions)this.RunModel;

        public override void ConfigureAutosave()
        {
            m_autosaveresults.Clear();

            if (this.MyRunModel.SaveContentCSV != null)
            {
                string requests = string.Join(" ", this.MyRunModel.SaveContentCSV.ToArray()).ToLower();
                foreach (cEcosimResultWriter.eResultTypes result in (cEcosimResultWriter.eResultTypes[])Enum.GetValues(typeof(cEcosimResultWriter.eResultTypes)))
                {
                    if (requests.Contains(result.ToString().ToLower()))
                    {
                        m_autosaveresults.Add(result);
                    }
                }
                m_bSaveAnnual = MyRunModel.SaveAnnual;
                Console.WriteLine("Ecosim writing output {0}", m_bSaveAnnual ? "annual" : "monthly");
            }
        }

        private void DoAutosave()
        {
            cEcosimResultWriter wr = new cEcosimResultWriter(this.Core);
            if (m_autosaveresults.Count > 0)
            {
                wr.WriteResults("", m_autosaveresults.ToArray(), m_bSaveAnnual ? TriState.False : TriState.True, true);
            }
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
        protected void BridgeCallback(cEcosimBridgePlugin.EventType e, int iTime)
        {
            cEcosimDatastructures ds = this.Core.EcosimDataStructures;

            if (e == cEcosimBridgePlugin.EventType.BeginTimeStep)
            {

                // Print out time tracking
                if ((iTime - 1) % ds.NumStepsPerYear == 0)
                {
                    Console.WriteLine("{0}",
                        (int)Core.EcosimFirstYear() + ((iTime - 1) / ds.NumStepsPerYear));
                }
                RunSuccess &= Apply(iTime);
            }

            if (e == cEcosimBridgePlugin.EventType.EndTimeStepPost)
            {
                if (iTime == ds.NTimes)
                    DoAutosave();
            }
        }

        protected override int DateToTimeStep(DateTime date)
        {
            return Core.AbsoluteTimeToEcosimTimestep(date);
        }
    }
}