using CsvHelper;
using System.Data;
using System.Globalization;

namespace Eii.Ecopath.Runner.Datamodel.Utilities
{
    /// <summary>
    /// Helper class for reading CSV files into DataTable objects using the en-US culture.
    /// </summary>
    public class cEwECSVReader
    {
        /// <summary>
        /// Reads a CSV file and loads its contents into a DataTable.
        /// </summary>
        /// <param name="csvpath">The path to the CSV file to read.</param>
        /// <returns>A DataTable containing the CSV data.</returns>
        public static DataTable ReadDataTable(string csvpath)
        {
            var dt = new DataTable();

            using (var reader = new StreamReader(csvpath))
            using (var csv = new CsvReader(reader, CultureInfo.GetCultureInfo("en-US")))
            using (var dr = new CsvDataReader(csv))
                dt.Load(dr);
            return dt;
        }
    }
}
