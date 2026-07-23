using System.Data;

namespace Eii.Ecopath.Runner.Datamodel.Utilities
{
    /// <summary>
    /// Helper class to read DataTable content into EwE arrays, interpreting numerical row- and column headers 
    /// as row and column indices. The code assumes that the CSV files are formatted as exported by the EwE
    /// user interface.
    /// </summary>
    public class cEwEArrayReader
    {
        public enum RowColMapping2D
        {
            RowCol,
            ColRow
        }

        public static void ReadArray<T>(DataTable table, T[,] target, RowColMapping2D mapping = RowColMapping2D.RowCol)
        {
            if (table == null) return;
            if (target == null) return;

            foreach (DataRow row in table.Rows)
            {
                if (!int.TryParse(row[0]?.ToString(), out int rowIndex) ||
                    (uint)rowIndex >= (uint)target.GetLength(mapping == RowColMapping2D.RowCol ? 0 : 1))
                    continue;

                for (int c = 1; c < table.Columns.Count; c++)
                {
                    if (!int.TryParse(table.Columns[c].ColumnName, out int columnIndex) ||
                        (uint)columnIndex >= (uint)target.GetLength(mapping == RowColMapping2D.RowCol ? 1 : 0))
                        continue;

                    int i1 = mapping == RowColMapping2D.RowCol ? rowIndex : columnIndex;
                    int i2 = mapping == RowColMapping2D.RowCol ? columnIndex : rowIndex;

                    target[i1, i2] = (T)row[c];
                }
            }
        }
    }
}
