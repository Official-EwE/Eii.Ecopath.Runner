using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using EwECore.Common;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    // --------------------------------------------------------------------
    /// <summary>
    /// A node in the automation tree that provides a number of operations
    /// on maps.
    /// </summary>
    // --------------------------------------------------------------------
    public class cMapNode : cNode
    {
        #region Private vars

        protected cEcospaceLayer Layer;

        #endregion // Private vars

        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="core">The <see cref="cCore"/> to operate on.</param>
        /// <param name="layer">the <see cref="cEcospaceLayer"/> to operate on.</param>
        /// <param name="logger">Logger for this node.</param>
        // --------------------------------------------------------------------
        public cMapNode(ICoreService coreService, cEcospaceLayer layer, ILogger logger) : base(coreService, logger)
        {
            this.Layer = layer;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Load the map from a file.
        /// </summary>
        /// <param name="filename">The file to load the map from.</param>
        /// <returns>True if successful.</returns>
        // --------------------------------------------------------------------
        [Description("Load the map from an ASCII grid file")]
        public bool load(string filename)
        {
            bool bSuccess = true;
            if (!string.IsNullOrWhiteSpace(filename))
            {
                // ToDo: support other file types such as CSV, geotiff, etc.
                cEcospaceImportExportASCIIData imp = new cEcospaceImportExportASCIIData(this.Core);
                if (imp.Read(filename))
                {
                    ISpatialRaster rs = imp.ToRaster();
                    for (int ir = 1; ir <= rs.NumRows(); ir++)
                        for (int ic = 1; ic <= rs.NumCols(); ic++)
                        {
                            double val = rs.Cell(ir, ic);
                            if (val != rs.NoData())
                                this.Layer.set_Cell(ir, ic, Value: val);
                        }
                }
                else
                {
                    Logger.LogWarning("Unable to read map file '{Filename}'", filename);
                    bSuccess = false;
                }
            }
            else
            {
                Logger.LogWarning("Unable to read map file: filename is null or empty");
                bSuccess = false;
            }
            return bSuccess;
        }
    }
}