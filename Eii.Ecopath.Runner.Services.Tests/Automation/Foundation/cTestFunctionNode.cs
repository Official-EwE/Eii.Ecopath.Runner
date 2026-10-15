using Eii.Ecopath.Runner.Services.Automation;
using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Tests.Automation.Foundation
{
    /// <summary>
    /// Minimal concrete subclass allowing the abstract <see cref="cFunctionNode"/>
    /// to be instantiated for testing.
    /// </summary>
    internal sealed class cTestFunctionNode : cFunctionNode
    {
        public cTestFunctionNode(ICoreService coreService, cShapeData shape, ILogger logger)
            : base(coreService, shape, logger)
        {
        }
    }
}
