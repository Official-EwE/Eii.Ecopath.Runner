using EwECore;

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
        public cEwECoreNode(cCore core, cCoreInputOutputBase coreobj) : base(core)
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
                Console.WriteLine("! Automation error setting EwE variable {0} to {1}. {2}", var.ToString(), val, ex.Message);
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
                Console.WriteLine("! Automation error setting EwE variable {0}({1}) to {2}. {3}", var.ToString(), iIndex, val, ex.Message);
                return false;
            }
            return true;
        }

    }
}
