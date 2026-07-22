using CsvHelper;
using System.Data;
using System.Globalization;

namespace Eii.Ecopath.Runner.Datamodel.Utilities
{
    public class cEwECSVReader
    {
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
