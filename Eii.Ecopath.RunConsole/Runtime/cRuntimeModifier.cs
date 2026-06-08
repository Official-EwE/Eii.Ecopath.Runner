using EwECore;
using EwECore.Plugins;
using EwERunConsole.Automation;
using EwERunConsole.Instructions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EwERunConsole.Runtime
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

        protected readonly cCore Core;
        protected readonly string Root;
        protected readonly IModelRunInstructions RunModel;
        protected readonly cEwEConfiguration Configuration;
        protected readonly Dictionary<int, cModificationsAtT> Changes;
        protected bool RunSuccess = true;

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
        public cRuntimeModifier(cCore core, string root, cEwEConfiguration config, IModelRunInstructions runmodel)
        {
            Core = core;
            Root = root;
            Configuration = config;
            RunModel = runmodel;
            Changes = new Dictionary<int, cModificationsAtT>();

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

        // --------------------------------------------------------------------
        /// <summary>
        /// Perform the actual run of one of the EwE models (and possibly one of
        /// the EwE searches too?)
        /// </summary>
        /// <returns>True if successful.</returns>
        // --------------------------------------------------------------------
        public abstract bool Run();

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
        protected bool Apply(int iTime)
        {
            bool bSucces = true;

            // Are there changes for this specific time step?
            if (Changes.ContainsKey(iTime))
            {
                // #Yes: grab it and pop it off the list.
                cModificationsAtT c = Changes[iTime];
                Changes.Remove(iTime);

                Debug.Assert(c != null);

                // Instantiate code automation tree
                cEwERootNode om = new cEwERootNode(Core);
                // Process all changes
                foreach (string key in c.modifications.Keys)
                {
                    // Apply change at the current Root. Keys are processed in lower case
                    if (om.Invoke(Root, key.ToLower(), c.modifications[key]))
                        Console.WriteLine("TS {0,4}: Applied {1}({2})", iTime, key, FormatModificationValue(c.modifications[key]));
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


        private static string FormatModificationValue(object value)
        {
            if (value == null)
                return "";

            if (value is string text)
                return "\"" + text + "\"";

            if (value is System.Collections.IEnumerable values && !(value is string))
            {
                List<string> parts = new List<string>();

                foreach (object item in values)
                    parts.Add(FormatModificationValue(item));

                return "[" + string.Join(", ", parts) + "]";
            }

            return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}