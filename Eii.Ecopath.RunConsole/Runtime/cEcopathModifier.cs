using EwECore;
using EwERunConsole.Instructions;
using System;

namespace EwERunConsole.Runtime
{
    internal class cEcopathModifier : cRuntimeModifier
    {
        public cEcopathModifier(cCore core, cEwEConfiguration config, IModelRunInstructions runmodel) 
            : base(core, "ecopath", config, runmodel)
        {
            // ToDo: configure Ecopath settings
        }

        protected override int DateToTimeStep(DateTime date)
        {
            return FirstTimeStep; // Bwahah!
        }

        public override void ConfigureAutosave()
        {
            // ToDo
        }

        public override bool Run()
        {
            RunSuccess = Apply(FirstTimeStep);
            RunSuccess &= Core.RunEcopath();
            return RunSuccess;
        }
    }
}