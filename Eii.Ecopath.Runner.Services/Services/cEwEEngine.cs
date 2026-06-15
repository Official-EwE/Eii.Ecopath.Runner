using Eii.Ecopath.Runner.Datamodel.Automation;
using Eii.Ecopath.Runner.Datamodel.RunInstructions;
using Eii.Ecopath.Runner.Services.Automation;
using EwECore;
using EwECore.SpatialData;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    /// -----------------------------------------------------------------------
    /// <summary>
    /// Central engine to execute an EwE run as dictated by 
    /// a basic <see cref="cEwEConfiguration"/> and <see cref="cEwERunInstructions"/> 
    /// for the different EwE models.
    /// </summary>
    /// -----------------------------------------------------------------------
    public class cEwEEngine
    {
        #region Private vars

        private readonly IcCoreService _coreService;
        private readonly ILogger<cEwEEngine> _logger;
        private readonly cEcopathModifierService _copathSvc;
        private readonly cEcosimModifierService _cosimSvc;
        private readonly cEcospaceModifierService _cospaceSvc;

        private cEwERunInstructions Instructions = null!;
        private cEwEConfiguration EwEConfig = null!;

        #endregion // Private vars

        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor — all dependencies are injected.
        /// </summary>
        // --------------------------------------------------------------------
        public cEwEEngine(
            ILogger<cEwEEngine> logger,
            IcCoreService coreService,
            cEcopathModifierService copathSvc,
            cEcosimModifierService cosimSvc,
            cEcospaceModifierService cospaceSvc)
        {
            _logger = logger;
            _coreService = coreService;
            _copathSvc = copathSvc;
            _cosimSvc = cosimSvc;
            _cospaceSvc = cospaceSvc;

            // Disable logging
            //cLog.VerboseLevel = eVerboseLevel.Disabled;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// The one and only way to start a run. Give'er.
        /// </summary>
        /// <param name="instructions">The run instructions to execute.</param>
        /// <returns>True if all went well.</returns>
        /// <remarks>
        /// This method will load the requested models of EwE (Path, Sim, SimTS, Space)
        /// and will trigger the different model runs.
        /// </remarks>
        /// -------------------------------------------------------------------
        public bool Run(cEwERunInstructions instructions)
        {
            Instructions = instructions;
            EwEConfig = instructions.Configuration;

            Console.WriteLine("==== Confguring environment ====");
            _logger.LogInformation("==== Configuring environment ====");

            if (!ConfigureEnvironment())
                return false;

            Console.WriteLine("OK");
            Console.WriteLine();

            Console.WriteLine("==== Ecopath ====");
            _logger.LogInformation("==== Ecopath ====");

            if (!LoadModel())
                return false;

            if (!RunEcopath())
                return false;

            Console.WriteLine("OK");
            Console.WriteLine();

            if ((EwEConfig.EcosimScenario <= 0) & (EwEConfig.EcospaceScenario <= 0))
                return true;

            Console.WriteLine("==== Ecosim ====");
            _logger.LogInformation("==== Ecosim ====");

            if (!LoadEcosim())
                return false;

            if (!RunEcosim())
                return false;

            Console.WriteLine("OK");
            Console.WriteLine();

            if (EwEConfig.EcospaceScenario <= 0)
                return true;

            Console.WriteLine("==== Ecospace ====");
            _logger.LogInformation("==== Ecospace ====");

            if (!LoadEcospace())
                return false;

            if (!RunEcospace())
                return false;

            Console.WriteLine("OK");
            Console.WriteLine();
            return true;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Write the automation capabilities (tree or command paths) to console.
        /// </summary>
        // --------------------------------------------------------------------
        public bool WriteAutomationCapabilities(cEwERunInstructions instructions, bool bTree)
        {
            Instructions = instructions;
            EwEConfig = instructions.Configuration;

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
            cEwERootNode om = new cEwERootNode(_coreService, _logger);
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
            _coreService.OutputPath = Instructions.OutputFolder;
            _coreService.SaveWithFileHeader = EwEConfig.SaveWithHeader;
            Console.WriteLine("Output folder set to '{0}', write with headers {1}", _coreService.OutputPath, _coreService.SaveWithFileHeader);
            _logger.LogInformation("Output folder set to '{OutputFolder}', write with headers {WithHeader}", _coreService.OutputPath, _coreService.SaveWithFileHeader);

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
                _logger.LogWarning("Error setting working directory to '{WorkFolder}': {Message}", workfolder, ex.Message);
                return false;
            }
            Console.WriteLine("Working directory set to '{0}'", workfolder);
            _logger.LogInformation("Working directory set to '{WorkFolder}'", workfolder);

            // Plug-ins
            int n = _coreService.PluginManager.LoadPlugins(".", true);
            Console.WriteLine("Loaded {0} plugin(s):", n);
            _logger.LogInformation("Loaded {PluginCount} plugin(s)", n);
            foreach (var pa in _coreService.PluginManager.PluginAssemblies)
                Console.WriteLine("- {0} v{1}", Path.GetFileNameWithoutExtension(pa.Filename), pa.Version);

            return true;
        }

        private bool LoadModel()
        {
            if (!File.Exists(EwEConfig.ModelFile))
            {
                Console.WriteLine("! Failed to locate model file '{0}'", EwEConfig.ModelFile);
                _logger.LogWarning("Failed to locate model file '{ModelFile}'", EwEConfig.ModelFile);
                return false;
            }

            // Load model (before loading spat temp data)
            if (!_coreService.LoadModel(EwEConfig.ModelFile))
            {
                Console.WriteLine("! Failed to load EwE model '{0}'", EwEConfig.ModelFile);
                _logger.LogWarning("Failed to load EwE model '{ModelFile}'", EwEConfig.ModelFile);
                return false;
            }
            Console.WriteLine("Loaded EwE model '{0}'", _coreService.ModelName);
            _logger.LogInformation("Loaded EwE model '{ModelName}'", _coreService.ModelName);

            // STDF config file
            if (!string.IsNullOrEmpty(EwEConfig.ExtDataConfigFile))
            {
                cSpatialDataConnectionManager man = _coreService.SpatialDataConnectionManager;
                cSpatialDataSetManager dsm = man.DatasetManager();
                if (dsm.Load(EwEConfig.ExtDataConfigFile, true))
                {
                    Console.WriteLine("Loaded STDF data from '{0}', {1} dataset(s)", EwEConfig.ExtDataConfigFile, dsm.Datasets().Length);
                    _logger.LogInformation("Loaded STDF data from '{ConfigFile}', {DatasetCount} dataset(s)", EwEConfig.ExtDataConfigFile, dsm.Datasets().Length);
                }
                else
                {
                    Console.WriteLine("! Failed to load STDF data from '{0}'", EwEConfig.ExtDataConfigFile);
                    _logger.LogWarning("Failed to load STDF data from '{ConfigFile}'", EwEConfig.ExtDataConfigFile);
                    return false;
                }
            }
            return true;
        }

        private bool RunEcopath()
        {
            cEcopathModifier mod = new cEcopathModifier(EwEConfig, Instructions.EcopathRun);
            if (!_copathSvc.Run(mod))
            {
                Console.WriteLine("! Failed to run Ecopath");
                _logger.LogWarning("Failed to run Ecopath");
                return false;
            }
            return true;
        }

        private bool LoadEcosim()
        {
            // Get Ecosim scenario to run
            int iSim = Math.Max(EwEConfig.EcospaceScenario > 0 ? 1 : 0, EwEConfig.EcosimScenario);

            if (iSim > _coreService.nEcosimScenarios)
            {
                Console.WriteLine("! Requested Ecosim scenario #{0} does not exist", iSim);
                _logger.LogWarning("Requested Ecosim scenario #{Scenario} does not exist", iSim);
                return false;
            }

            if (!_coreService.LoadEcosimScenario(iSim))
            {
                Console.WriteLine("! Failed to load Ecosim scenario #{0}", iSim);
                _logger.LogWarning("Failed to load Ecosim scenario #{Scenario}", iSim);
                return false;
            }

            Console.WriteLine("Loaded Ecosim scenario {0}: {1}", iSim, _coreService.GetEcosimScenarioName(iSim));
            _logger.LogInformation("Loaded Ecosim scenario {Scenario}: {Name}", iSim, _coreService.GetEcosimScenarioName(iSim));

            if (EwEConfig.EcosimTimeseries > 0)
            {
                // Log this 
                if (!_coreService.LoadTimeSeries(EwEConfig.EcosimTimeseries))
                {
                    Console.WriteLine("! Failed to load Ecosim timeseries #{0}", EwEConfig.EcosimTimeseries);
                    _logger.LogWarning("Failed to load Ecosim timeseries #{Timeseries}", EwEConfig.EcosimTimeseries);
                    return false;
                }
                Console.WriteLine("Loaded Ecosim timeseries {0}: {1}", EwEConfig.EcosimTimeseries, _coreService.GetTimeSeriesDatasetName(EwEConfig.EcosimTimeseries));
                _logger.LogInformation("Loaded Ecosim timeseries {Timeseries}: {Name}", EwEConfig.EcosimTimeseries, _coreService.GetTimeSeriesDatasetName(EwEConfig.EcosimTimeseries));
            }
            return true;
        }

        private bool RunEcosim()
        {
            Console.WriteLine("Start Ecosim run");
            _logger.LogInformation("Start Ecosim run");

            if (EwEConfig.RunYears > 0)
            {
                cEcoSimModelParameters parms = _coreService.EcosimModelParameters;
                parms.NumberYears = EwEConfig.RunYears;
                if (_coreService.nEcosimYears != EwEConfig.RunYears)
                {
                    Console.WriteLine("! Failed to set Ecosim run years to {0}", EwEConfig.RunYears);
                    _logger.LogWarning("Failed to set Ecosim run years to {RunYears}", EwEConfig.RunYears);
                    return false;
                }
            }
            Console.WriteLine("Ecosim run years = {0}", _coreService.nEcosimYears);
            _logger.LogInformation("Ecosim run years = {RunYears}", _coreService.nEcosimYears);

            cEcosimModifier mod = new cEcosimModifier(EwEConfig, Instructions.EcosimRun);
            if (!_cosimSvc.Run(mod))
            {
                Console.WriteLine("! Failed to run Ecosim");
                _logger.LogWarning("Failed to run Ecosim");
                return false;
            }
            Console.WriteLine("End Ecosim run");
            _logger.LogInformation("End Ecosim run");
            return true;
        }

        private bool LoadEcospace()
        {
            int iSpace = EwEConfig.EcospaceScenario;

            // Fail if the requested Ecospace scenario is not defined
            if (iSpace > _coreService.nEcospaceScenarios)
            {
                Console.WriteLine("! Requested Ecospace scenario #{0} does not exist", iSpace);
                _logger.LogWarning("Requested Ecospace scenario #{Scenario} does not exist", iSpace);
                return false;
            }

            if (!_coreService.LoadEcospaceScenario(iSpace))
            {
                Console.WriteLine("! Failed to load Ecospace scenario #{0}", iSpace);
                _logger.LogWarning("Failed to load Ecospace scenario #{Scenario}", iSpace);
                return false;
            }
            Console.WriteLine("Loaded Ecospace scenario {0}: {1}", iSpace, _coreService.GetEcospaceScenarioName(iSpace));
            _logger.LogInformation("Loaded Ecospace scenario {Scenario}: {Name}", iSpace, _coreService.GetEcospaceScenarioName(iSpace));
            return true;
        }

        private bool RunEcospace()
        {
            Console.WriteLine("Start Ecospace run");
            _logger.LogInformation("Start Ecospace run");

            if (EwEConfig.RunYears > 0 & _coreService.nEcospaceYears != EwEConfig.RunYears)
            {
                cEcospaceModelParameters parms = _coreService.EcospaceModelParameters;
                parms.TotalTime = EwEConfig.RunYears;
                if (_coreService.nEcospaceYears != EwEConfig.RunYears)
                {
                    Console.WriteLine("! Failed to set Ecospace run years to {0}", EwEConfig.RunYears);
                    _logger.LogWarning("Failed to set Ecospace run years to {RunYears}", EwEConfig.RunYears);
                    return false;
                }
            }
            Console.WriteLine("Ecospace run years = {0}", _coreService.nEcospaceYears);
            _logger.LogInformation("Ecospace run years = {RunYears}", _coreService.nEcospaceYears);

            cEcospaceModifier mod = new cEcospaceModifier(EwEConfig, Instructions.EcospaceRun);
            if (!_cospaceSvc.Run(mod))
            {
                Console.WriteLine("! Failed to run Ecospace");
                _logger.LogWarning("Failed to run Ecospace");
                return false;
            }
            Console.WriteLine("End Ecospace run");
            _logger.LogInformation("End Ecospace run");

            return true;
        }

        #endregion // Internal implementation
    }

}

