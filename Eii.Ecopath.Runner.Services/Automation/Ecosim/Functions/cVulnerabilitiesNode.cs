using Eii.Ecopath.Runner.Datamodel.Utilities;
using Eii.Ecopath.Runner.Services.Runtime;
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
        /// <param name="csvpath">The path th the CSV file, in us-EN number format.</param>
        /// <returns>True if successful.</returns>
        // --------------------------------------------------------------------
        [Description("Load the vulnerabilities matrix from a standard EwE CSV file")]
        public bool load(string filename)
        {
            DataTable? dt;
            try
            {
                dt = cEwECSVReader.ReadDataTable(filename);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Unable to load vulnerabilities file '{0}', {1}", filename, ex.Message);
                return false;
            }

            try
            {
                // ViulMult[prey,pred],
                cEwEArrayReader.ReadArray(dt, this.Core.EcosimDataStructures.VulMult, cEwEArrayReader.RowColMapping2D.RowCol);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Error applying vulnerabilities file '{0}' to Ecosim, {1}", filename, ex.Message);
                return false;
            }
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
        public void fill(double value)
        {
            var data = this.Core.EcosimDataStructures.VulMult;
            EwEUtils.Extensions.Fill(ref data, (float)value);
        }
    }
}
