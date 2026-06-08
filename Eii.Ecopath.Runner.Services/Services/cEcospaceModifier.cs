using Eii.Ecopath.Runner.Datamodel.RunInstructions;
using EwEBridge.Ecospace;
using EwECore;
using EwECore.Common;
using EwECore.Plugins;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    /// <summary>
    /// Modify an Ecospace run on the fly.
    /// </summary>
    internal class cEcospaceModifier : cRuntimeModifier
    {
        #region Construction / destruction

        public cEcospaceModifier(cCore core, cEwEConfiguration config, cEcospaceRunInstructions runmodel) : base(core, "ecospace", config, runmodel)
        {
            // Establish plug-in callbacks to alter ecospace mid-run
            IPlugin? pi = GetPlugin(typeof(cEcospaceBridgePlugin));
            if (pi != null)
            {
                cEcospaceBridgePlugin ppt = (cEcospaceBridgePlugin)pi;
                ppt.BridgeCallback = BridgeCallback;
            }
        }

        #endregion // Construction / destruction

        protected cEcospaceRunInstructions MyRunModel => (cEcospaceRunInstructions)RunModel;
        private int _nts;

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

        public override bool Run()
        {
            RunSuccess = true;

            cEcospaceDataStructures ds = Core.EcospaceDataStructures;

            // Disable unnecessary data caching - Ecospace won't be saved after
            ds.PreserveLayerData = false;
            // Reroute spatial log folder
            Core.SpatialOperationLog.LogFilePath = Core.OutputPath;

            _nts = ds.nTimeStepsPerYear;

            if (MyRunModel.RunSpinupYears > 0)
            {
                ds.UseSpinUp = true;
                ds.SpinUpYears = MyRunModel.RunSpinupYears;
                Console.WriteLine("Ecospace spin-up = {0} years", ds.SpinUpYears);
            }

            if (MyRunModel.MinHabCap == 0)
            {
                Console.WriteLine("Ecospace habitat corrections = off");

            }
            ds.UseHabCapGradientCorrections = false;
            if (MyRunModel.MinHabCap > 0)
            {
                ds.UseHabCapGradientCorrections = true;
                ds.MinHabCap = MyRunModel.MinHabCap;
                Console.WriteLine("Ecospace habitat corrections = {0}", ds.MinHabCap);
            }

            if (MyRunModel.MaxCores > 0)
            {
                ds.nEffortDistThreads = MyRunModel.MaxCores;
                ds.nSpaceSolverThreads = MyRunModel.MaxCores;
                ds.nGridSolverThreads = MyRunModel.MaxCores;
                ds.nIBMMovementSolverThreads = MyRunModel.MaxCores;
                Console.WriteLine("Ecospace #threads = {0}", ds.nEffortDistThreads);
            }

            ds.UseIBM = MyRunModel.UseIBM;
            ds.NewMultiStanza = !ds.UseIBM;
            Console.WriteLine("Ecospace runmode = {0}", ds.UseIBM ? "ibm" : "multi-stanza");

            // Check data connections
            Console.WriteLine("Ecospace no. ext data connections = {0}", Core.SpatialDataConnectionManager.NumConnectedAdapters);
            Console.WriteLine();
            Console.WriteLine("Start run");

            // Capture Ecospace STDF messages
            cMessageHandler mh = new cMessageHandler(CoreMessageHandler, eCoreComponentType.External, eMessageType.GISOperation, new SynchronizationContext());
            Core.Messages.AddMessageHandler(mh);

            // Go for it - added delegate to meet the requirements of the API 
            cCore.EcoSpaceInterfaceDelegate dgt = new cCore.EcoSpaceInterfaceDelegate(EcospaceDummyCallback);
            RunSuccess &= Core.RunEcospace(ref dgt, false);

            Console.WriteLine("End run");

            // Clean up
            Core.Messages.RemoveMessageHandler(mh);

            // Done
            return RunSuccess;
        }

        protected override int DateToTimeStep(DateTime date)
        {
            return Core.AbsoluteTimeToEcospaceTimestep(date);
        }

        #endregion // Runtime overrides

        #region Internals 


        /// -------------------------------------------------------------------
        /// <summary>
        /// Plug-in callback for making runtime modifications. 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="iTime"></param>
        /// -------------------------------------------------------------------
        protected void BridgeCallback(cEcospaceBridgePlugin.EventType e, int iTime)
        {
            if (e == cEcospaceBridgePlugin.EventType.BeginTimeStep)
            {
                // Print out time tracking
                if ((iTime - 1) % cCore.N_MONTHS == 0)
                {
                    cEcospaceDataStructures ds = Core.EcospaceDataStructures;
                    Console.WriteLine("{0}{1}{2}", 
                        Core.EcospaceTimestepToAbsoluteTime(iTime).Year, 
                        ds.bInSpinUp ? " (spin-up)" : "", 
                        (iTime == ds.FirstOutputTimeStep) ? " autosaving starting" : "");
                }

                RunSuccess &= Apply(iTime);
            }
        }

        protected void EcospaceDummyCallback(ref cEcospaceTimestep spaceres)
        {
            // NOP
        }

        protected void CoreMessageHandler(ref cMessage msg)
        {
            if (msg.Importance == eMessageImportance.Information)
            {
                Console.WriteLine("STDF: {0}", msg.Message);
            }
            if (msg.Importance == eMessageImportance.Critical)
            {
                Console.WriteLine("! STDF: {0}", msg.Message);
            }
        }

        protected bool IsASCWriter(string name)
        {
            return name.Contains("ecospaceasc");
        }

        protected bool IsCSVWriter(string name)
        {
            return !IsASCWriter(name);
        }

        #endregion // Internals

    }
}