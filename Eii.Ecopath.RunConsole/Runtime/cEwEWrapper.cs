using EwECore;
using EwECore.SpatialData;
using EwERunConsole.Automation;
using EwERunConsole.Instructions;
using System;
using System.IO;
using System.Linq;

#pragma warning disable CS8618

namespace EwERunConsole.Runtime
{
    /// -----------------------------------------------------------------------
    /// <summary>
    /// Central engine to execute an Ecospace run as dictated by 
    /// a basic <see cref="cEwEConfiguration"/> and <see cref="cEwERunInstructions"/> 
    /// for the different EwE models.
    /// </summary>
    /// -----------------------------------------------------------------------
    public class cEwEEngine
    {
        #region Private vars

        private readonly cCore Core;
        
        private cEwERunInstructions Instructions;
        private cEwEConfiguration EwEConfig;

        #endregion // Private vars

        public cEwEEngine(cEwERunInstructions instructions)
        {
            Instructions = instructions;
            EwEConfig = instructions.Configuration;

            // Instantiate the EwE model and load plug-ins
            Core = new cCore();
            cPluginManager pi = new cPluginManager();
            Core.PluginManager = pi;

            // Disable logging
            //cLog.VerboseLevel = eVerboseLevel.Disabled;
        }

        ~cEwEEngine() 
        {
            if (Core != null)
            {
                Core.CloseModel();
                Core.Dispose();
            }
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// The one and only way to start a run. Give'er.
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns>True if all went well.</returns>
        /// <remarks>
        /// This method will load the requested models of EwE (Path, Sim, SimTS, Space)
        /// and will trigger the different model runs.
        /// </remarks>
        /// -------------------------------------------------------------------
        public bool Run()
        {
            Console.WriteLine("==== Confguring environment ====");

            if (!ConfigureEnvironment())
                return false;

            Console.WriteLine("OK");
            Console.WriteLine();

            Console.WriteLine("==== Ecopath ====");

            if (!LoadModel()) 
                return false;

            if (!RunEcopath())
                return false;

            Console.WriteLine("OK");
            Console.WriteLine();

            if ((EwEConfig.EcosimScenario <= 0) & (EwEConfig.EcospaceScenario <= 0))
                return true;

            Console.WriteLine("==== Ecosim ====");

            if (!LoadEcosim())
                return false;

            if (!RunEcosim())
                return false;

            Console.WriteLine("OK");
            Console.WriteLine();

            if (EwEConfig.EcospaceScenario <= 0)
                return true;

            Console.WriteLine("==== Ecospace ====");

            if (!LoadEcospace())
                return false;

            if (!RunEcospace())
                return false;

            Console.WriteLine("OK");
            Console.WriteLine();
            return true;
        }

        public bool WriteAutomationCapabilities(bool bTree)
        {
            if (!ConfigureEnvironment())
                return false;

            if (!LoadModel())
                return false;

            if ((EwEConfig.EcosimScenario > 0) | (EwEConfig.EcospaceScenario > 0))
            {
                if (!LoadEcosim())
                    return false;

                if (EwEConfig.EcospaceScenario > 0)
                {
                    if (!LoadEcospace())
                        return false;
                }
            }
            cEwERootNode om = new cEwERootNode(Core);
            string[] info = { };
            if (bTree)
                info = om.AutomationTree();
            else
                info = om.AutomationPaths();
            for (int i = 0; i < info.Count(); i++)
            {
                Console.WriteLine(info[i]);
            }
                return true;
        }

        #region Internal implementation

        private bool ConfigureEnvironment()
        {

            // Set save options
            Core.OutputPath = Instructions.OutputFolder;
            Core.SaveWithFileHeader = EwEConfig.SaveWithHeader;
            Console.WriteLine("Output folder set to '{0}', write with headers {1}", Core.OutputPath, Core.SaveWithFileHeader);

            // Set current directory
            string? workfolder = EwEConfig.WorkFolder;
            try
            {
                if (string.IsNullOrWhiteSpace(workfolder))
                    workfolder = Path.GetDirectoryName(Instructions.RunConfigFile);
                if (!string.IsNullOrWhiteSpace(workfolder))
                    Directory.SetCurrentDirectory(workfolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine("! Error setting working directory to '{0}': {1}", workfolder, ex.Message);
                return false;
            }
            Console.WriteLine("Working directory set to '{0}'", workfolder);

            // Plug-ins
            int n = Core.PluginManager.LoadPlugins(".", true);
            Console.WriteLine("Loaded {0} plugin(s):", n);
            foreach (var pa in Core.PluginManager.PluginAssemblies)
                Console.WriteLine("- {0} v{1}", Path.GetFileNameWithoutExtension(pa.Filename), pa.Version);

            return true;
        }

        private bool LoadModel()
        {
            if (!File.Exists(EwEConfig.ModelFile)) 
            {
                Console.WriteLine("! Failed to locate model file '{0}'", EwEConfig.ModelFile);
                return false; 
            }

            // Load model (before loading spat temp data)
            if (!Core.LoadModel(EwEConfig.ModelFile))
            {
                Console.WriteLine("! Failed to load EwE model '{0}'", EwEConfig.ModelFile);
                return false;
            }
            Console.WriteLine("Loaded EwE model '{0}'", Core.EwEModel.Name);

            // STDF config file
            if (!string.IsNullOrEmpty(EwEConfig.ExtDataConfigFile))
            {
                cSpatialDataConnectionManager man = Core.SpatialDataConnectionManager;
                cSpatialDataSetManager dsm = man.DatasetManager();
                if (dsm.Load(EwEConfig.ExtDataConfigFile, true))
                    Console.WriteLine("Loaded STDF data from '{0}', {1} dataset(s)", EwEConfig.ExtDataConfigFile, dsm.Datasets().Length);
                else
                {
                    Console.WriteLine("! Failed to load STDF data from '{0}'", EwEConfig.ExtDataConfigFile);
                    return false;
                }
            }
            return true;
        }

        private bool RunEcopath()
        {
            cRuntimeModifier mod = new cEcopathModifier(Core, EwEConfig, Instructions.EcopathRun);
            if (!mod.Run())
            {
                Console.WriteLine("! Failed to run Ecopath");
                return false;
            }
            return true;
        }

        private bool LoadEcosim()
        {
            // Get Ecosim scenario to run
            int iSim = Math.Max(EwEConfig.EcospaceScenario > 0 ? 1 : 0, EwEConfig.EcosimScenario);

            if (iSim > Core.nEcosimScenarios)
            {
                Console.WriteLine("! Requested Ecosim scenario #{0} does not exist", iSim);
                return false;
            }

            if (!Core.LoadEcosimScenario(iSim))
            {
                Console.WriteLine("! Failed to load Ecosim scenario #{0}", iSim);
                return false;
            }

            Console.WriteLine("Loaded Ecosim scenario {0}: {1}", iSim, Core.get_EcosimScenarios(iSim).Name);

            if (EwEConfig.EcosimTimeseries > 0)
            {
                // Log this 
                if (!Core.LoadTimeSeries(EwEConfig.EcosimTimeseries))
                {
                    Console.WriteLine("! Failed to load Ecosim timeseries #{0}", EwEConfig.EcosimTimeseries);
                    return false;
                }
                Console.WriteLine("Loaded Ecosim timeseries {0}: {1}", EwEConfig.EcosimTimeseries, Core.TimeSeriesDataset(EwEConfig.EcosimTimeseries).Name);
            }
            return true;
        }

        private bool RunEcosim()
        {
            Console.WriteLine("Start Ecosim run");

            if (EwEConfig.RunYears > 0)
            {
                cEcoSimModelParameters parms = Core.EcosimModelParameters;
                parms.NumberYears = EwEConfig.RunYears;
                if (Core.nEcosimYears != EwEConfig.RunYears)
                {
                    Console.WriteLine("! Failed to set Ecosim run years to {0}", EwEConfig.RunYears);
                    return false;
                }
            }
            Console.WriteLine("Ecosim run years = {0}", Core.nEcosimYears);

            cRuntimeModifier mod = new cEcosimModifier(Core, EwEConfig, Instructions.EcosimRun);
            if (!mod.Run())
            {
                Console.WriteLine("! Failed to run Ecosim");
                return false;
            }
            Console.WriteLine("End Ecosim run");
            return true;
        }

        private bool LoadEcospace()
        {
            int iSpace = EwEConfig.EcospaceScenario;

            // Fail if the requested Ecospace scenario is not defined
            if (iSpace > Core.nEcospaceScenarios)
            {
                Console.WriteLine("! Requested Ecospace scenario #{0} does not exist", iSpace);
                return false;
            }

            if (!Core.LoadEcospaceScenario(iSpace))
            {
                Console.WriteLine("! Failed to load Ecospace scenario #{0}", iSpace);
                return false;
            }
            Console.WriteLine("Loaded Ecospace scenario {0}: {1}", iSpace, Core.get_EcospaceScenarios(iSpace).Name);
            return true;    
        }

        private bool RunEcospace()
        {
            Console.WriteLine("Start Ecospace run");

            if (EwEConfig.RunYears > 0 & Core.nEcospaceYears != EwEConfig.RunYears)
            {
                cEcospaceModelParameters parms = Core.EcospaceModelParameters;
                parms.TotalTime = EwEConfig.RunYears;
                if (Core.nEcospaceYears != EwEConfig.RunYears)
                {
                    Console.WriteLine("! Failed to set Ecospace run years to {0}", EwEConfig.RunYears);
                    return false;
                }
            }
            Console.WriteLine("Ecospace run years = {0}", Core.nEcospaceYears);

            cRuntimeModifier mod = new cEcospaceModifier(Core, EwEConfig, Instructions.EcospaceRun);
            if (!mod.Run())
            {
                Console.WriteLine("! Failed to run Ecospace");
                return false;
            }
            Console.WriteLine("End Ecospace run");

            return true;
        }

        #endregion // Internal implementation
    }

}
#pragma warning restore CS8618
