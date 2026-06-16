using EwECore;
using EwECore.SpatialData;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Abstraction for the singleton service that owns the <see cref="cCore"/>
    /// instance for the lifetime of the application.
    /// </summary>
    // ------------------------------------------------------------------------
    public interface ICoreService : IDisposable
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// Escape hatch: the raw <see cref="cCore"/> for the rare call sites
        /// that must pass it directly into EwECore constructors.
        /// </summary>
        // --------------------------------------------------------------------
        cCore Core { get; }

        #region Model metadata

        string ModelName { get; }
        string OutputPath { get; set; }
        bool SaveWithFileHeader { get; set; }

        #endregion // Model metadata

        #region Counts

        int nGroups { get; }
        int nFleets { get; }
        int nMPAs { get; }
        int nHabitats { get; }
        int nEnvironmentalDriverLayers { get; }
        int nEcosimScenarios { get; }
        int nEcospaceScenarios { get; }
        int nEcosimYears { get; }
        int nEcospaceYears { get; }

        #endregion // Counts

        #region Model parameters and structures

        cPluginManager PluginManager { get; }
        cEcoSimModelParameters EcosimModelParameters { get; }
        cEcospaceModelParameters EcospaceModelParameters { get; }
        cEcospaceBasemap EcospaceBasemap { get; }
        cEcospaceDataStructures EcospaceDataStructures { get; }
        cEcosimDatastructures EcosimDataStructures { get; }
        cSpatialDataConnectionManager SpatialDataConnectionManager { get; }

        #endregion // Model parameters and structures

        #region Shape managers

        cFishingEffortShapeManger FishingEffortShapeManager { get; }
        cFishingMortalityShapeManger FishMortShapeManager { get; }
        cForcingFunctionShapeManager ForcingShapeManager { get; }

        #endregion // Shape managers

        #region Input accessors

        cEcoPathGroupInput get_EcopathGroupInputs(int iGroup);
        cEcopathFleetInput get_EcopathFleetInputs(int iFleet);
        cEcospaceMPA get_EcospaceMPAs(int iMPA);
        cEcospaceHabitat get_EcospaceHabitats(int iHabitat);
        cEcospaceGroupInput get_EcospaceGroupInputs(int iGroup);
        cEcospaceFleetInput get_EcospaceFleetInputs(int iFleet);

        #endregion // Input accessors

        #region Scenario / dataset name accessors

        string GetEcosimScenarioName(int iScenario);
        string GetEcospaceScenarioName(int iScenario);
        string GetTimeSeriesDatasetName(int iDataset);

        #endregion // Scenario / dataset name accessors

        #region Spatial log

        void SetSpatialLogFilePath(string path);

        #endregion // Spatial log

        #region Message handlers

        void AddMessageHandler(cMessageHandler handler);
        void RemoveMessageHandler(cMessageHandler handler);

        #endregion // Message handlers

        #region Load operations

        bool LoadModel(string modelFile);
        bool LoadEcosimScenario(int iScenario);
        bool LoadEcospaceScenario(int iScenario);
        bool LoadTimeSeries(int iDataset);

        #endregion // Load operations

        #region Run operations

        bool RunEcopath();
        bool RunEcosim();
        bool RunEcospace(ref cCore.EcoSpaceInterfaceDelegate dgt, bool bSave);

        #endregion // Run operations

        #region Time conversion

        float EcosimFirstYear();
        int AbsoluteTimeToEcosimTimestep(DateTime date);
        int AbsoluteTimeToEcospaceTimestep(DateTime date);
        DateTime EcospaceTimestepToAbsoluteTime(int iTimeStep);
        string get_DefaultOutputPath(eAutosaveTypes autosaveType);

        #endregion // Time conversion
    }
}
