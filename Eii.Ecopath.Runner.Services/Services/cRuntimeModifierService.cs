using Eii.Ecopath.Runner.Datamodel.Automation;
using Eii.Ecopath.Runner.Datamodel.RunInstructions;
using EwECore;
using EwECore.Plugins;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Abstract base for the per-model modifier services. Holds the shared
    /// <see cref="cCore"/> reference (via <see cref="cCoreService"/>) and
    /// provides common helpers for applying timed changes and date parsing.
    /// </summary>
    // ------------------------------------------------------------------------
    public abstract class cRuntimeModifierService
    {
        #region Private vars

        protected readonly ICoreService _coreService;
        private readonly cNodeService _nodeService;
        protected readonly ILogger _logger;

        #endregion // Private vars

        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="coreService">Singleton owner of the EwE core.</param>
        /// <param name="nodeService">Automation-tree invocation service.</param>
        /// <param name="logger">Logger for this service.</param>
        // --------------------------------------------------------------------
        protected cRuntimeModifierService(ICoreService coreService, cNodeService nodeService, ILogger logger)
        {
            _coreService = coreService;
            _nodeService = nodeService;
            _logger = logger;
        }

        #region Change processing

        // --------------------------------------------------------------------
        /// <summary>
        /// Parse the incoming changes on <paramref name="mod"/> — converting
        /// free date texts into time steps — and populate
        /// <see cref="cRuntimeModifier.Changes"/> for swift processing.
        /// </summary>
        // --------------------------------------------------------------------
        protected void CompleteAndPrepareChanges(cRuntimeModifier mod)
        {
            foreach (cModificationsAtT c in mod.RunModel.Changes)
            {
                int iTimeStep = c.TimeStep;
                if (iTimeStep == 0) iTimeStep = ParseDate(c.Date);
                mod.Changes[iTimeStep] = c;
            }
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Apply any changes at a given simulation time step.
        /// </summary>
        /// <param name="mod">The modifier whose change schedule to process.</param>
        /// <param name="iTime">The time step to process.</param>
        /// <returns>True if all changes were applied successfully.</returns>
        // --------------------------------------------------------------------
        protected bool Apply(cRuntimeModifier mod, int iTime)
        {
            bool bSuccess = true;

            // Are there changes for this specific time step?
            if (mod.Changes.ContainsKey(iTime))
            {
                // #Yes: grab it and pop it off the list.
                cModificationsAtT c = mod.Changes[iTime];
                mod.Changes.Remove(iTime);

                Debug.Assert(c != null);

                // Process all changes via the injectable node service
                foreach (string key in c.modifications.Keys)
                {
                    string lowerKey = key.ToLower();
                    object val = c.modifications[key];
                    // Apply change at the current Root. Keys are processed in lower case
                    if (_nodeService.Invoke(_coreService, mod.Root, lowerKey, val))
                    {
                        string msg = $"TS {iTime,4}: Applied {key}({val})";
                        Console.WriteLine(msg);
                        _logger.LogInformation(msg);
                    }
                    else
                        bSuccess = false;
                }
            }
            return bSuccess;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Parse an incoming textual date to a time step for the underlying
        /// EwE model.
        /// </summary>
        /// <param name="date">The date, which can be "start".</param>
        /// <returns>A one-based timestep, starting at
        /// <see cref="cRuntimeModifier.FirstTimeStep"/> or
        /// <see cref="cRuntimeModifier.NoTimeStep"/> if an error occurred.
        /// </returns>
        // --------------------------------------------------------------------
        public int ParseDate(string date)
        {
            if (string.IsNullOrEmpty(date))
                return cRuntimeModifier.FirstTimeStep;
            if (string.Compare(date, "start", true) == 0)
                return cRuntimeModifier.FirstTimeStep;

            if (DateTime.TryParse(date, out DateTime dt))
                return DateToTimeStep(dt);

            return cRuntimeModifier.NoTimeStep;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Translate an incoming date to a model-specific time step.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>A time step.</returns>
        // --------------------------------------------------------------------
        protected abstract int DateToTimeStep(DateTime date);

        #endregion // Change processing

        #region Plug-in helpers

        // --------------------------------------------------------------------
        /// <summary>
        /// Helper method to obtain a specific instance of a loaded plug-in.
        /// </summary>
        /// <param name="t">The Type of the plug-in class to obtain.</param>
        /// <returns>A plug-in class instance, or null if not found.</returns>
        // --------------------------------------------------------------------
        protected IPlugin? GetPlugin(Type t)
        {
            cPluginManager pm = _coreService.PluginManager;
            List<IPlugin> plugins = (List<IPlugin>)pm.GetPlugins(t);
            if (plugins.Count > 0)
                return plugins[0];
            return null;
        }

        #endregion // Plug-in helpers
    }
}
