using System.Text;

namespace Eii.Ecopath.Runner.Datamodel.Utilities
{
    /// <summary>
    /// Credits to Skyblade
    /// https://stackoverflow.com/questions/420429/mirroring-console-output-to-a-file
    /// </summary>
    /// <remarks>
    /// Usage:
    /// <code>
    /// using (var cc = new ConsoleCopy("filename.txt"))
    /// {
    ///     Console.WriteLine("Tada!");
    /// }
    /// </code>
    /// </remarks>
    public class cConsoleCopy : IDisposable
    {
        private FileStream? m_fileStream;
        private StreamWriter? m_fileWriter;
        private TextWriter? m_doubleWriter;
        private TextWriter m_oldOut;
        private bool m_bDisposed = false;

        /// <summary>
        /// Internal helper class that writes output to two TextWriter streams simultaneously.
        /// </summary>
        private class DoubleWriter : TextWriter
        {
            private readonly TextWriter m_one;
            private readonly TextWriter m_two;

            /// <summary>
            /// Initializes a new instance of the <see cref="DoubleWriter"/> class.
            /// </summary>
            /// <param name="one">The first TextWriter to write to.</param>
            /// <param name="two">The second TextWriter to write to.</param>
            public DoubleWriter(TextWriter one, TextWriter two)
            {
                m_one = one;
                m_two = two;
            }

            /// <summary>
            /// Gets the encoding of the first writer.
            /// </summary>
            public override Encoding Encoding => m_one.Encoding;

            /// <summary>
            /// Flushes both output streams.
            /// </summary>
            public override void Flush()
            {
                m_one.Flush();
                m_two.Flush();
            }

            /// <summary>
            /// Writes a character to both output streams.
            /// </summary>
            /// <param name="value">The character to write.</param>
            public override void Write(char value)
            {
                m_one.Write(value);
                m_two.Write(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="cConsoleCopy"/> class and starts copying console output to a file.
        /// </summary>
        /// <param name="fn">The filename to write console output to.</param>
        public cConsoleCopy(string fn)
        {
            m_oldOut = Console.Out;

            try
            {
                m_fileStream = File.Open(fn, FileMode.Create, FileAccess.Write, FileShare.Read);
                m_fileWriter = new StreamWriter(m_fileStream) { AutoFlush = true };
                m_doubleWriter = new DoubleWriter(m_fileWriter, m_oldOut);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open file for writing");
                Console.WriteLine(e.Message);
                return;
            }

            Console.SetOut(m_doubleWriter);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="cConsoleCopy"/> class with a formatted header containing run information.
        /// </summary>
        /// <param name="fn">The filename to write console output to.</param>
        /// <param name="script">The script identifier (unused in current implementation).</param>
        /// <param name="rev">The revision identifier.</param>
        /// <param name="description">The description of the run.</param>
        /// <param name="model">Optional path to the model file.</param>
        /// <param name="pout">Optional output path.</param>
        /// <param name="entries">Optional additional key-value pairs to include in the header.</param>
        public cConsoleCopy(string fn, string script, string rev, string description,
                            string model = "", string pout = "",
                            Dictionary<string, string>? entries = null)
            : this(fn)
        {
            int maxLen = 8;
            if (entries != null)
            {
                foreach (var entry in entries.Keys)
                {
                    maxLen = Math.Max(maxLen, entry.Length);
                }
            }

            Console.WriteLine("*******************************************************");
            Console.WriteLine("{0," + maxLen + "} : {1}", "Date", DateTime.Now.ToShortDateString());
            Console.WriteLine("{0," + maxLen + "} : {1}", "Revision", rev);
            Console.WriteLine("{0}", description);
            if (!string.IsNullOrEmpty(model))
                Console.WriteLine("{0," + maxLen + "} : {1}", "Model", Path.GetFileName(model));
            if (entries != null)
                foreach (var entry in entries.Keys)
                    Console.WriteLine("{0," + maxLen + "} : {1}", entry, entries[entry]);
            Console.WriteLine();
            if (!string.IsNullOrEmpty(pout))
                Console.WriteLine("{0," + maxLen + "} : {1}", "Output", pout);

            Console.WriteLine("*******************************************************");
            Console.WriteLine();
        }

        /// <summary>
        /// Releases unmanaged and optionally managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_bDisposed)
            {
                if (disposing)
                {
                    Console.SetOut(m_oldOut);

                    m_fileWriter?.Flush();
                    m_fileWriter?.Close();
                    m_fileWriter = null;

                    m_fileStream?.Close();
                    m_fileStream = null;
                }
                m_bDisposed = true;
            }
        }

        /// <summary>
        /// Restores the original console output and closes the file stream.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}