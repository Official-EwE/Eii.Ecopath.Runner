using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    // --------------------------------------------------------------------
    /// <summary>
    /// A node in the automation tree that provides a number of operations
    /// on internal variables.
    /// </summary>
    // --------------------------------------------------------------------
    public abstract class cEwECoreNode : cNode
    {
        public cEwECoreNode(IcCoreService coreService, cCoreInputOutputBase coreobj, ILogger logger) : base(coreService, logger)
        {
            this.CoreObj = coreobj;
        }

        protected readonly cCoreInputOutputBase CoreObj;

        protected bool SetVariable(eVarNameFlags var, object val)
        {
            try
            {
                cCoreInputOutputBase obj = this.CoreObj;
                obj?.SetVariable(var, val);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Automation error setting EwE variable {Var} to {Val}. {Message}", var.ToString(), val, ex.Message);
                return false;
            }
            return true;
        }

        protected bool SetVariable(eVarNameFlags var, int iIndex, object val)
        {
            try
            {
                cCoreInputOutputBase obj = this.CoreObj;
                obj?.SetVariable(var, val, iIndex);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Automation error setting EwE variable {Var}({Index}) to {Val}. {Message}", var.ToString(), iIndex, val, ex.Message);
                return false;
            }
            return true;
        }

    }
}
