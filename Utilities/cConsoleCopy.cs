using System.Text;

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

    private class DoubleWriter : TextWriter
    {
        private readonly TextWriter m_one;
        private readonly TextWriter m_two;

        public DoubleWriter(TextWriter one, TextWriter two)
        {
            m_one = one;
            m_two = two;
        }

        public override Encoding Encoding => m_one.Encoding;

        public override void Flush()
        {
            m_one.Flush();
            m_two.Flush();
        }

        public override void Write(char value)
        {
            m_one.Write(value);
            m_two.Write(value);
        }
    }

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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}