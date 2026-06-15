using Eii.Ecopath.Runner.Datamodel.RunInstructions;
using Eii.Ecopath.Runner.Services.Automation;
using EwECore;
using EwECore.Plugins;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Base class for modifying EwE values over time in one of the EwE core
    /// models (and possibly searches?).
    /// </summary>
    // ------------------------------------------------------------------------
    internal abstract class cRuntimeModifier
    {
        #region Internal vars 

        protected internal readonly cCore Core;
        protected readonly string Root;
        protected readonly IModelRunInstructions RunModel;
        protected readonly cEwEConfiguration Configuration;
        protected readonly Dictionary<int, cModificationsAtT> Changes;
        protected bool RunSuccess = true;

        private readonly cNodeService _nodeService;
        private readonly ILogger _logger;

        public const int FirstTimeStep = 1;
        public const int NoTimeStep = -1;

        #endregion // Internal vars 

        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="core">The <see cref="cCore"/> to operate onto.</param>
        /// <param name="root">The root name of the runtime modifier, used to 
        /// determine if code modifications are allowed in the context of the 
        /// running model.</param>
        /// <param name="config">The EwE-wide configuration as defined by the user.</param>
        /// <param name="runmodel">The model-specific configuration and changes as 
        /// defined by the user.</param>
        // --------------------------------------------------------------------
        public cRuntimeModifier(cCore core, string root, cEwEConfiguration config, IModelRunInstructions runmodel, cNodeService nodeService, ILogger logger)
        {
            Core = core;
            Root = root;
            Configuration = config;
            RunModel = runmodel;
            Changes = new Dictionary<int, cModificationsAtT>();
            _nodeService = nodeService;
            _logger = logger;

            // Parse and complete information in the requrest changes, and organize
            // them in an internal dictionary for ease of processing.
            CompleteAndPrepareChanges();
            // Generic configuration of autosaving
            ConfigureAutosave();
        }

        #region Execution 

        // --------------------------------------------------------------------
        /// <summary>
        /// Implement to configure model-specific auto-running of plug-ins and 
        /// auto-saving of data
        /// </summary>
        // --------------------------------------------------------------------
        public abstract void ConfigureAutosave();

        #endregion // Execution

        #region Applying the changes

        // --------------------------------------------------------------------
        /// <summary>
        /// Complete the <see cref="IModelRunInstructions.Changes">incoming changes
        /// </see> by converting free date texts into time steps, and place the 
        /// time steps into <see cref="Changes">dictionary</see> for swift 
        /// processing.
        /// </summary>
        // --------------------------------------------------------------------
        private void CompleteAndPrepareChanges()
        {
            foreach (cModificationsAtT c in RunModel.Changes)
            {
                int iTimeStep = c.TimeStep;
                if (iTimeStep == 0) iTimeStep = ParseDate(c.Date);
                Changes[iTimeStep] = c;
            }
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Apply any changes at a given simulation time step.
        /// </summary>
        /// <param name="iTime">The time step to process</param>
        /// <returns></returns>
        // --------------------------------------------------------------------
        internal bool Apply(int iTime)
        {
            bool bSucces = true;

            // Are there changes for this specific time step?
            if (Changes.ContainsKey(iTime))
            {
                // #Yes: grab it and pop it off the list.
                cModificationsAtT c = Changes[iTime];
                Changes.Remove(iTime);

                Debug.Assert(c != null);

                // Process all changes via the injectable node service
                foreach (string key in c.modifications.Keys)
                {
                    string lowerKey = key.ToLower();
                    object val = c.modifications[key];
                    // Apply change at the current Root. Keys are processed in lower case
                    if (_nodeService.Invoke(Core, Root, lowerKey, val))
                    {
                        string msg = $"TS {iTime,4}: Applied {key}({val})";
                        Console.WriteLine(msg);
                        _logger.LogInformation(msg);
                    }
                    else
                        bSucces = false;
                }
            }
            return bSucces;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Parse an incoming textual date to a time step for the uderlying EwE model.
        /// </summary>
        /// <param name="date">The date, which can be "start".</param>
        /// <returns>A one-based timestep, starting at <see cref="FirstTimeStep"/>
        /// or <see cref="NoTimeStep"> if an error occurred.</returns>
        // --------------------------------------------------------------------
        public int ParseDate(string date)
        {
            if (string.IsNullOrEmpty(date))
                return FirstTimeStep;
            if (string.Compare(date, "start", true) == 0) return FirstTimeStep;

            DateTime dt = DateTime.MinValue;
            if (DateTime.TryParse(date, out dt))
                return DateToTimeStep(dt);

            return NoTimeStep;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Translate an incoming date to a model-specific time step for the 
        /// underlying EwE model.
        /// </summary>
        /// <param name="date"></param>
        /// <returns>A time step.</returns>
        // --------------------------------------------------------------------
        abstract protected int DateToTimeStep(DateTime date);

        #endregion // Date parsing

        // --------------------------------------------------------------------
        /// <summary>
        /// Helper method to obtain a specific instance of a loaded plug-in.
        /// </summary>
        /// <param name="t">The Type of the plug-in class to obtain.</param>
        /// <returns>A plug-in class instance, or null if not found.</returns>
        // --------------------------------------------------------------------
        public IPlugin? GetPlugin(Type t)
        {
            cPluginManager pm = Core.PluginManager;
            List<IPlugin> plugins = (List<IPlugin>)pm.GetPlugins(t);
            if (plugins.Count > 0)
                return plugins[0];
            return null;
        }
    }
}