using Eii.Ecopath.Runner.Services.Automation;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Injectable service that executes automation-tree invocations against a
    /// running EwE core.  Wraps <see cref="cEwERootNode"/> so callers never
    /// need to instantiate it directly.
    /// </summary>
    // ------------------------------------------------------------------------
    public class cNodeService
    {
        #region Private vars

        private readonly ILogger<cNodeService> _logger;

        #endregion // Private vars

        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        // --------------------------------------------------------------------
        public cNodeService(ILogger<cNodeService> logger)
        {
            _logger = logger;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Invoke a single automation command on the given core in the given
        /// model context.
        /// </summary>
        /// <param name="core">The <see cref="cCore"/> instance to operate on.</param>
        /// <param name="root">The context root (e.g. "ecosim", "ecospace").</param>
        /// <param name="key">The dot-separated automation path (lower-case).</param>
        /// <param name="value">The parameter value to pass to the end-point.</param>
        /// <returns>True if the invocation succeeded.</returns>
        // --------------------------------------------------------------------
        internal bool Invoke(IcCoreService coreService, string root, string key, object value)
        {
            cEwERootNode om = new cEwERootNode(coreService, _logger);
            bool bSuccess = om.Invoke(root, key, value);
            if (!bSuccess)
            {
                string msg = $"! Automation invoke failed: {root}.{key}({value})";
                Console.WriteLine(msg);
                _logger.LogWarning(msg);
            }
            return bSuccess;
        }
    }
}
