using EwECore;

namespace Eii.Ecopath.Runner.Services.Tests.Automation.Foundation
{
    /// <summary>
    /// Minimal, fully functional <see cref="cShapeData"/> test double.
    /// Only the abstract <see cref="Update"/> member is overridden; all other
    /// behavior (point storage, locking, etc.) is the real base implementation.
    /// </summary>
    internal sealed class cTestShapeData : cShapeData
    {
        public int UpdateCallCount { get; private set; }

        public cTestShapeData(int numberOfPoints) : base(numberOfPoints)
        {
        }

        public override bool Update()
        {
            UpdateCallCount++;
            return true;
        }
    }
}
