using Eii.Ecopath.Runner.Datamodel.RunInstructions;
using EwEBridge.Ecosim;
using EwECore;
using EwECore.Ecosim;
using EwECore.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    internal class cEcosimModifier : cRuntimeModifier
    {
        private List<cEcosimResultWriter.eResultTypes> m_autosaveresults = [];
        private bool m_bSaveAnnual = false;

        public cEcosimModifier(cCore core, cEwEConfiguration config, cEcosimRunInstructions runmodel, cNodeService nodeService, ILogger<cEcosimModifier> logger)
            : base(core, "ecosim", config, runmodel, nodeService, logger)
        {
        }

        internal cEcosimRunInstructions MyRunModel => (cEcosimRunInstructions)this.RunModel;

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

        internal void DoAutosave()
        {
            cEcosimResultWriter wr = new cEcosimResultWriter(this.Core);
            if (m_autosaveresults.Count > 0)
            {
                string path = this.Core.get_DefaultOutputPath(eAutosaveTypes.EcosimResults);
                wr.WriteResults(path, null, m_bSaveAnnual ? TriState.False : TriState.True, false);
                Console.WriteLine("Ecosim wrote output to {0}", path);
            }
        }

        protected override int DateToTimeStep(DateTime date)
        {
            return Core.AbsoluteTimeToEcosimTimestep(date);
        }
    }
}