using Eii.Ecopath.Runner.Datamodel.Utilities;
using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Data;

namespace Eii.Ecopath.Runner.Services.Automation.Ecosim.Functions
{
    public class cVulnerabilitiesNode : cNode
    {
        public cVulnerabilitiesNode(ICoreService coreService, ILogger logger) : base(coreService, logger)
        {
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Load an EwE vulnerabilities CSV file
        /// </summary>
        /// <param name="csvpath">The path to the CSV file, in us-EN number format.</param>
        /// <returns>True if successful.</returns>
        // --------------------------------------------------------------------
        [Description("Load the vulnerabilities matrix from a standard EwE CSV file")]
        public bool load(string filename)
        {
            // Cannot do this while running
            if (Core.StateMonitor.IsBusy()) return false;

            this.Core.SetBatchLock(cCore.eBatchLockType.Update);
            DataTable? dt;
            try
            {
                dt = cEwECSVReader.ReadDataTable(filename);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Unable to read V file '{0}', {1}", filename, ex.Message);
                return false;
            }

            try
            {
                // VulMult[prey,pred],
                cEwEArrayReader.ReadArray(dt, this.Core.EcosimDataStructures.VulMult, cEwEArrayReader.RowColMapping2D.RowCol);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Unable to load V from file '{0}', {1}", filename, ex.Message);
                return false;
            }

            this.Core.ReleaseBatchLock(cCore.eBatchChangeLevelFlags.Ecosim);
            this.Core.EcosimArenaManager.ResetArenas(0);
            return true;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Set the vulnerabilities to a given value
        /// </summary>
        /// <param name="value">The value to set the vulnerabilities to.</param>
        /// <returns>True if successful.</returns>
        // --------------------------------------------------------------------
        [Description("Set the vulnerabilities to a given value")]
        public bool fill(double value)
        {
            // Cannot do this while running
            if (Core.StateMonitor.IsBusy()) return false;

            this.Core.SetVToDefault((float)value);
            this.Core.EcosimArenaManager.ResetArenas(0);
            return true;
        }
    }
}
