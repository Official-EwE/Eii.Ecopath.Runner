using EwECore;
using EwECore.Common;
using EwECore.Plugins;
using EwECore.SpatialData;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Singleton service that owns the <see cref="cCore"/> instance and its
    /// <see cref="cPluginManager"/> for the lifetime of the application.
    /// </summary>
    // ------------------------------------------------------------------------
    public class cCoreService : ICoreService
    {
        #region Private vars

        private bool _disposed = false;

        #endregion // Private vars

        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor. Creates and wires up <see cref="cCore"/> and its
        /// <see cref="cPluginManager"/>.
        /// </summary>
        // --------------------------------------------------------------------
        public cCoreService()
        {
            Core = new cCore();
            Core.PluginManager = new cPluginManager();
        }

        // --------------------------------------------------------------------
        /// <inheritdoc/>
        // --------------------------------------------------------------------
        public cCore Core { get; }

        // --------------------------------------------------------------------
        /// <inheritdoc/>
        // --------------------------------------------------------------------
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Core.CloseModel();
            Core.Dispose();
        }

        #region Model metadata

        public string ModelName => Core.EwEModel.Name;
        public string OutputPath { get => Core.OutputPath; set => Core.OutputPath = value; }
        public bool SaveWithFileHeader { get => Core.SaveWithFileHeader; set => Core.SaveWithFileHeader = value; }

        #endregion // Model metadata

        #region Counts

        public int nGroups => Core.nGroups;
        public int nFleets => Core.nFleets;
        public int nMPAs => Core.nMPAs;
        public int nHabitats => Core.nHabitats;
        public int nEnvironmentalDriverLayers => Core.nEnvironmentalDriverLayers;
        public int nEcosimScenarios => Core.nEcosimScenarios;
        public int nEcospaceScenarios => Core.nEcospaceScenarios;
        public int nEcosimYears => Core.nEcosimYears;
        public int nEcospaceYears => Core.nEcospaceYears;

        #endregion // Counts

        #region Model parameters and structures

        public cPluginManager PluginManager => Core.PluginManager;
        public cEcoSimModelParameters EcosimModelParameters => Core.EcosimModelParameters;
        public cEcospaceModelParameters EcospaceModelParameters => Core.EcospaceModelParameters;
        public cEcospaceBasemap EcospaceBasemap => Core.EcospaceBasemap;
        public cEcospaceDataStructures EcospaceDataStructures => Core.EcospaceDataStructures;
        public cEcosimDatastructures EcosimDataStructures => Core.EcosimDataStructures;
        public cSpatialDataConnectionManager SpatialDataConnectionManager => Core.SpatialDataConnectionManager;

        #endregion // Model parameters and structures

        #region Shape managers

        public cFishingEffortShapeManger FishingEffortShapeManager => Core.FishingEffortShapeManager;
        public cFishingMortalityShapeManger FishMortShapeManager => Core.FishMortShapeManager;
        public cForcingFunctionShapeManager ForcingShapeManager => Core.ForcingShapeManager;

        #endregion // Shape managers

        #region Input accessors

        public cEcoPathGroupInput get_EcopathGroupInputs(int iGroup) => Core.get_EcopathGroupInputs(iGroup);
        public cEcopathFleetInput get_EcopathFleetInputs(int iFleet) => Core.get_EcopathFleetInputs(iFleet);
        public cEcospaceMPA get_EcospaceMPAs(int iMPA) => Core.get_EcospaceMPAs(iMPA);
        public cEcospaceHabitat get_EcospaceHabitats(int iHabitat) => Core.get_EcospaceHabitats(iHabitat);
        public cEcospaceGroupInput get_EcospaceGroupInputs(int iGroup) => Core.get_EcospaceGroupInputs(iGroup);
        public cEcospaceFleetInput get_EcospaceFleetInputs(int iFleet) => Core.get_EcospaceFleetInputs(iFleet);

        #endregion // Input accessors

        #region Scenario / dataset name accessors

        public string GetEcosimScenarioName(int iScenario) => Core.get_EcosimScenarios(iScenario).Name;
        public string GetEcospaceScenarioName(int iScenario) => Core.get_EcospaceScenarios(iScenario).Name;
        public string GetTimeSeriesDatasetName(int iDataset) => Core.TimeSeriesDataset(iDataset).Name;

        #endregion // Scenario / dataset name accessors

        #region Spatial log

        public void SetSpatialLogFilePath(string path) => Core.SpatialOperationLog.LogFilePath = path;

        #endregion // Spatial log

        #region Message handlers

        public void AddMessageHandler(cMessageHandler handler) => Core.Messages.AddMessageHandler(handler);
        public void RemoveMessageHandler(cMessageHandler handler) => Core.Messages.RemoveMessageHandler(handler);

        #endregion // Message handlers

        #region Load operations

        public bool LoadModel(string modelFile) => Core.LoadModel(modelFile);
        public bool LoadEcosimScenario(int iScenario) => Core.LoadEcosimScenario(iScenario);
        public bool LoadEcospaceScenario(int iScenario) => Core.LoadEcospaceScenario(iScenario);
        public bool LoadTimeSeries(int iDataset) => Core.LoadTimeSeries(iDataset);

        #endregion // Load operations

        #region Run operations

        public bool RunEcopath() => Core.RunEcopath();
        public bool RunEcosim() => Core.RunEcosim();
        public bool RunEcospace(ref cCore.EcoSpaceInterfaceDelegate dgt, bool bSave) => Core.RunEcospace(ref dgt, bSave);

        #endregion // Run operations

        #region Time conversion

        public float EcosimFirstYear() => Core.EcosimFirstYear();
        public int AbsoluteTimeToEcosimTimestep(DateTime date) => Core.AbsoluteTimeToEcosimTimestep(date);
        public int AbsoluteTimeToEcospaceTimestep(DateTime date) => Core.AbsoluteTimeToEcospaceTimestep(date);
        public DateTime EcospaceTimestepToAbsoluteTime(int iTimeStep) => Core.EcospaceTimestepToAbsoluteTime(iTimeStep);
        public string get_DefaultOutputPath(eAutosaveTypes autosaveType) => Core.get_DefaultOutputPath(autosaveType);

        #endregion // Time conversion
    }
}
