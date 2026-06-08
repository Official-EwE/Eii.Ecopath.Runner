using System;

namespace Eii.Ecopath.Runner.Services.Automation
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AutomationIgnoreAttribute : Attribute
    {
    }
}
