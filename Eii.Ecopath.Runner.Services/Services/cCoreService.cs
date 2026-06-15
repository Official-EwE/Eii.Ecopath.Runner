using EwECore;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Singleton service that owns the <see cref="cCore"/> instance and its
    /// <see cref="cPluginManager"/> for the lifetime of the application.
    /// </summary>
    // ------------------------------------------------------------------------
    public class cCoreService : IDisposable
    {
        #region Private vars

        private bool _disposed = false;

        #endregion // Private vars

        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor. Creates and wires up <see cref="cCore"/> and its
        /// <see cref="cPluginManager"/>.
        /// </summary>
        // --------------------------------------------------------------------
        public cCoreService()
        {
            Core = new cCore();
            Core.PluginManager = new cPluginManager();
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// The single <see cref="cCore"/> instance shared across the application.
        /// </summary>
        // --------------------------------------------------------------------
        public cCore Core { get; }

        // --------------------------------------------------------------------
        /// <inheritdoc/>
        // --------------------------------------------------------------------
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Core.CloseModel();
            Core.Dispose();
        }
    }
}
