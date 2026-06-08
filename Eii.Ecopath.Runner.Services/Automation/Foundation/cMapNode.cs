using EwECore;
using EwECore.Common;

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
        // --------------------------------------------------------------------
        public cMapNode(cCore core, cEcospaceLayer layer) : base(core) 
        {
            this.Layer = layer;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Method to fill the map withe a single value.
        /// </summary>
        /// <param name="value">The value to set to every cell.</param>
        /// <returns>True if successful.</returns>
        // --------------------------------------------------------------------
        public bool fill(object value)
        {
            cEcospaceBasemap bm = this.Core.EcospaceBasemap;
            for (int ir = 1; ir <= bm.InRow; ir++)
                for (int ic = 1; ic <= bm.InCol; ic++)
                {
                    this.Layer.set_Cell(ir, ic, Value: value);
                }
            return true;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Load the map from a file.
        /// </summary>
        /// <param name="filename">The file to load the map from.</param>
        /// <returns>True if successful.</returns>
        // --------------------------------------------------------------------
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
                    // Unable to read file: log this
                    bSuccess = false;
                }
            }
            else
            {
                // Unable to read file: log this
                bSuccess = false;
            }
            return bSuccess;
        }
    }
}