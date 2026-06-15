using Eii.Ecopath.Runner.Datamodel.RunInstructions;
using EwEBridge.Ecospace;
using EwECore;
using EwECore.Common;
using EwECore.Plugins;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    /// <summary>
    /// Modify an Ecospace run on the fly.
    /// </summary>
    internal class cEcospaceModifier : cRuntimeModifier
    {
        #region Construction / destruction

        public cEcospaceModifier(cCore core, cEwEConfiguration config, cEcospaceRunInstructions runmodel, cNodeService nodeService, ILogger<cEcospaceModifier> logger)
            : base(core, "ecospace", config, runmodel, nodeService, logger)
        {
        }

        #endregion // Construction / destruction

        internal cEcospaceRunInstructions MyRunModel => (cEcospaceRunInstructions)RunModel;

        #region Runtime overrides 

        public override void ConfigureAutosave()
        {
            cEcospaceModelParameters parms = Core.EcospaceModelParameters;
            cEcospaceDataStructures ds = Core.EcospaceDataStructures;

            Console.WriteLine("Ecospace {0} result writer(s)", parms.nResultWriters);
            for (int i = 0; i < parms.nResultWriters; i++)
            {
                bool bEnable = false;

                IEcospaceResultsWriter wr = parms.ResultWriter(i + 1);
                string n = wr.GetType().Name.ToLower();
                if (IsASCWriter(n))
                {
                    foreach (var vn in MyRunModel.SaveContentASC)
                    {
                        if (n.Contains(vn.ToLower()))
                            bEnable = true;
                    }
                }
                else if (IsCSVWriter(n))
                {
                    foreach (var vn in MyRunModel.SaveContentCSV)
                    {
                        if (n.Contains(vn.ToLower()))
                            bEnable = true;
                    }
                }
                else
                {
                    // NOP
                }
                wr.Enabled = bEnable;
                Console.WriteLine("  {0,2}: {1} {2}", i, wr.Enabled ? "V" : "-", wr.GetType().ToString());
            }

            // Set autosave properties
            ds.FirstOutputTimeStep = DateToTimeStep(new DateTime(Math.Max(Core.EcosimFirstYear(), MyRunModel.SaveFirstYear), 1, 1));
            ds.SaveAnnual = MyRunModel.SaveAnnual;
            Console.WriteLine("Ecospace writing output at time step {0}, {1}", ds.FirstOutputTimeStep, ds.SaveAnnual ? "annual" : "monthly");
         }

        protected override int DateToTimeStep(DateTime date)
        {
            return Core.AbsoluteTimeToEcospaceTimestep(date);
        }

        #endregion // Runtime overrides

        #region Internals 

        internal bool IsASCWriter(string name)
        {
            return name.Contains("ecospaceasc");
        }

        internal bool IsCSVWriter(string name)
        {
            return !IsASCWriter(name);
        }

        #endregion // Internals

    }
}