using Eii.Ecopath.Runner.Datamodel.RunInstructions;

namespace Eii.Ecopath.Runner.Datamodel.Automation
{
    /// <summary>
    /// Data container for an Ecospace run.
    /// </summary>
    public class cEcospaceModifier : cRuntimeModifier
    {
        #region Construction / destruction

        public cEcospaceModifier(cEwEConfiguration config, cEcospaceRunInstructions runmodel)
            : base("ecospace", config, runmodel)
        {
        }

        #endregion // Construction / destruction

        public cEcospaceRunInstructions MyRunModel => (cEcospaceRunInstructions)RunModel;

        #region Internals 

        public bool IsASCWriter(string name)
        {
            return name.Contains("ecospaceasc");
        }

        public bool IsCSVWriter(string name)
        {
            return !IsASCWriter(name);
        }

        #endregion // Internals

    }
}
