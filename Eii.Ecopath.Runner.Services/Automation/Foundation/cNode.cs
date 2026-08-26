using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Reflection;

namespace Eii.Ecopath.Runner.Services.Automation
{
    /// -----------------------------------------------------------------------
    /// <summary>
    /// A single node in the automation tree. This class supports nested
    /// invocation of functions by reading a string, to find and invoke 
    /// a final end point function.
    /// </summary>
    /// -----------------------------------------------------------------------
    public class cNode
    {
        #region Private vars

        protected readonly ICoreService CoreService;
        protected readonly ILogger Logger;
        /// <summary>Escape hatch for the rare EwECore constructors that require a raw cCore.</summary>
        protected cCore Core => CoreService.Core;

        #endregion

        /// -------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="cNode"/> class.
        /// </summary>
        /// <param name="coreService">The core service providing access to the EwE model.</param>
        /// <param name="logger">The logger for diagnostic messages.</param>
        /// -------------------------------------------------------------------
        public cNode(ICoreService coreService, ILogger logger)
        {
            CoreService = coreService;
            Logger = logger;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Try to invoke an automation command in the context of the current 
        /// executing model.
        /// </summary>
        /// <param name="context">The context of the current running model.</param>
        /// <param name="methodPath">The complete path to execute.</param>
        /// <param name="fnparms">The parameters to pass to the final function to execute.</param>
        /// <returns>True if the command was successfully invoked; otherwise, false.</returns>
        /// -------------------------------------------------------------------
        public bool Invoke(string context, string methodPath, object fnparms)
        {
            bool bIncompatible = false;

            // Check compatibility
            switch (context.ToLower())
            {
                case "ecospace":
                    // All good
                    break;

                case "ecosim":
                    bIncompatible = methodPath.StartsWith("ecospace");
                    break;

                case "ecopath":
                    bIncompatible = methodPath.StartsWith("ecosim") | methodPath.StartsWith("ecospace");
                    break;
            }
            if (bIncompatible)
            {
                Logger.LogError("Change '{MethodPath}' cannot be executed under '{Context}'", methodPath, context);
                return false;
            }

            // Go for it
            return CrawlAutomationTree(methodPath, fnparms);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Recursively crawl an object chain for a given function end point to call.
        /// Properties and indexed properties are not yet supported.
        /// </summary>
        /// <param name="methodPath"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// -------------------------------------------------------------------
        protected bool CrawlAutomationTree(string methodPath, object fnparms)
        {
            try
            {
                string[] parts = methodPath.Split('.', 2);
                object[]? parms = null;
                string parm = "";

                int iBracket = parts[0].IndexOf('[');
                if (iBracket >= 0)
                {
                    parm = parts[0].Substring(iBracket + 1).Replace("]", "");
                    parts[0] = parts[0].Substring(0, iBracket);
                    parms = new object[] { Convert.ToInt32(parm) };
                }

                // Done?
                if (parts.Length > 1)
                {
                    // Iterate on — resolve by parameter types to avoid AmbiguousMatchException on overloaded methods
                    Type[] paramTypes = parms?.Select(p => p.GetType()).ToArray() ?? Type.EmptyTypes;
                    MethodInfo? method = GetType().GetMethod(parts[0], paramTypes);
                    if (method == null)
                    {
                        Logger.LogError("Automation entry '{Entry}' in '{MethodPath}' cannot be resolved", parts[0], methodPath);
                        return false;
                    }
                    var result = method.Invoke(this, parms);
                    if (result == null)
                    {
                        Logger.LogError("Automation invocation {Entry}({Parm}) caused an error", parts[0], parm);
                        return false;
                    }
                    if (result is cNode)
                    {
                        return ((cNode)result).CrawlAutomationTree(parts[1], fnparms);
                    }

                    // Catch all - the returned method is of the wrong class
                    Logger.LogError("Automation entry '{Entry}' in '{MethodPath}' bug! cNode expected", parts[0], methodPath);
                    return false;
                }
                else
                {
                    // Execute final method
                    MethodInfo? method = GetType().GetMethod(parts[0]);
                    if (method == null)
                    {
                        Logger.LogError("Automation endpoint '{Entry}' in '{MethodPath}' cannot be resolved", parts[0], methodPath);
                        return false;
                    }

                    try
                    {
                        var result = method.Invoke(this, [fnparms]);
                        return Convert.ToBoolean(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Automation endpoint {Entry}({Parms}) threw error", parts[0], fnparms);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Automation {MethodPath} threw error", methodPath);
            }
            return false;
        }

        public List<string> ListAutomationTree(string prefix = "", string indent = "", int padSize = 50)
        {
            List<string> paths = new();
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<AutomationIgnoreAttribute>() != null)
                    continue;

                // Explore method properties
                string methodName = method.Name;
                string fullPath = methodName; // Ignore prefix: string.IsNullOrEmpty(prefix) ? methodName : $"{prefix}{methodName}";
                bool returnsNode = typeof(cNode).IsAssignableFrom(method.ReturnType);

                // Format parameter list
                var parameters = method.GetParameters();
                bool hasParams = parameters.Length > 0;

                string paramList = string.Join(", ", parameters.Select(p => $"{p.Name}: {p.ParameterType.Name}"));
                string signature = methodName;

                if (hasParams)
                    if (returnsNode)
                        signature += "[" + paramList + "]";
                    else
                        signature += " = " + (parameters.Length == 1 ? paramList : "(" + paramList + ")");

                // Get method description
                var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
                string description = descAttr?.Description ?? "";

                // Pad for description alignment
                string paddedSig = fullPath.Replace(methodName, signature).PadRight(padSize);
                paths.Add(string.Format($"{indent}- {paddedSig} // {description}"));

                // Show parameter descriptions
                foreach (var p in parameters)
                {
                    string pDesc = p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";
                    paths.Add(string.Format($"{indent}    └─ {p.Name}: {pDesc}"));
                }

                // Only recurse into safe methods
                if (returnsNode)
                {
                    try
                    {
                        object[] args;

                        // Support single integer parameter (like indexed nodes)
                        if (hasParams && parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
                            args = new object[] { 1 };
                        else if (!hasParams)
                            args = Array.Empty<object>();
                        else
                            continue; // skip other parameter configs for now

                        var result = method.Invoke(this, args);
                        if (result is cNode subNode)
                        {
                            paths.AddRange(subNode.ListAutomationTree(".", indent + "  ", padSize));
                        }
                    }
                    catch
                    {
                        paths.Add(string.Format($"{indent}  [!] Could not invoke method '{method.Name}' safely."));
                    }
                }
            }
            return paths;
        }

        /// -----------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------
        public List<string> ListAutomationPaths(string prefix = "")
        {
            List<string> paths = new();

            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<AutomationIgnoreAttribute>() != null)
                    continue;

                object[] parms = { };

                // Skip methods with parameters
                if (method.GetParameters().Length > 0)
                {
                    // Skip string-parameter overloads (name-based aliases); use int-indexed overloads only
                    if (method.GetParameters()[0].ParameterType == typeof(string))
                        continue;
                    parms = [1];
                }

                // Check if return type is cNode (or subclass)
                if (typeof(cNode).IsAssignableFrom(method.ReturnType))
                {
                    // Safe to invoke
                    var result = method.Invoke(this, parms);

                    if (result is cNode subNode)
                    {
                        string fullPath = string.IsNullOrEmpty(prefix) ? method.Name : $"{prefix}.{method.Name}";
                        if (parms.Count() > 0) fullPath += "[#]";

                        // Recurse into child node
                        paths.AddRange(subNode.ListAutomationPaths(fullPath));
                    }
                }
                else
                {
                    // Not a cNode — treat as endpoint
                    string fullPath = string.IsNullOrEmpty(prefix) ? method.Name : $"{prefix}.{method.Name}";
                    paths.Add(fullPath);
                }
            }

            return paths;
        }

        #region Ecpoath-wide accessors

        /// -------------------------------------------------------------------
        /// <summary>
        /// Find the index of a group by name.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>The group index, or <see cref="cCore.NULL_VALUE"/> if no match 
        /// was found.</returns>        
        /// -------------------------------------------------------------------
        protected int FindGroup(string groupName)
        {
            cEcopathDataStructures ds = this.Core.EcopathDataStructures;
            return FindItem(groupName, ds.GroupName);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Find the index of a fleet by name. This function cannot be used to 
        /// find the "all" fleet that is used in some specific EwE logic.
        /// </summary>
        /// <param name="fleetName"></param>
        /// <returns>The fleet index, or <see cref="cCore.NULL_VALUE"/> if no match 
        /// was found.</returns>        
        /// -------------------------------------------------------------------
        protected int FindFleet(string fleetName)
        {
            cEcopathDataStructures ds = this.Core.EcopathDataStructures;
            return FindItem(fleetName, ds.FleetName);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Find the index of a named item by string comparison. All comparisons 
        /// ignore casing.
        /// </summary>
        /// <param name="name">The name of the item to find.</param>
        /// <param name="names">The array of names to search within.</param>
        /// <returns>The index, or <see cref="cCore.NULL_VALUE"/> if no match 
        /// was found.</returns>
        /// -------------------------------------------------------------------
        protected int FindItem(string name, string[] names)
        {
            name = name.Trim();
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Compare(name, names[i], StringComparison.OrdinalIgnoreCase) == 0)
                    return i;
            }
            return cCore.NULL_VALUE;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Find the index of a shape by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shapes"></param>
        /// <returns></returns>
        /// -------------------------------------------------------------------
        protected int FindShape(string name, IEnumerable<cShapeData> shapes)
        {
            if (shapes == null) return cCore.NULL_VALUE;

            name = name.ToLowerInvariant();
            foreach (cShapeData shp in shapes)
            {
                if (string.Compare(name, shp.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    return shp.Index;
            }

            return cCore.NULL_VALUE;
        }

        #endregion // Ecopath-wide accessors
    }
}
