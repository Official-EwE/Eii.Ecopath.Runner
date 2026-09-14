using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
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
        /// <param name="methodPath">The complete path to the method to invoke.</param>
        /// <param name="fnparms">The parameters to pass to the final function to execute.</param>
        /// <returns>True if the method was successfully invoked; otherwise, false.</returns>
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

        /// -----------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix">The prefix to prepend to each automation path.</param>
        /// <returns>A list of automation paths.</returns>
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
                    // Skip string-parameter overloads (itemName-based aliases); use int-indexed overloads only
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
        /// Find the index of a group by itemName.
        /// </summary>
        /// <param name="groupName">The name of the group to find.</param>
        /// <param name="logger">The logger to log to.</param>
        /// <returns>The group index, or <see cref="cCore.NULL_VALUE"/> if no match 
        /// was found.</returns>        
        /// -------------------------------------------------------------------
        protected int FindGroup(string groupName, ILogger logger)
        {
            cEcopathDataStructures ds = this.Core.EcopathDataStructures;
            return FindItem(groupName, ds.GroupName, logger, "group");
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Find the index of a fleet by itemName. This function cannot be used to 
        /// find the "all" fleet that is used in some specific EwE logic.
        /// </summary>
        /// <param name="fleetName">The name of the fleet to find.</param>
        /// <param name="logger">The logger to log to.</param>
        /// <returns>The fleet index, or <see cref="cCore.NULL_VALUE"/> if no match 
        /// was found.</returns>        
        /// -------------------------------------------------------------------
        protected int FindFleet(string fleetName, ILogger logger)
        {
            cEcopathDataStructures ds = this.Core.EcopathDataStructures;
            return FindItem(fleetName, ds.FleetName, logger, "fleet");
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Find the index of a named item by string comparison. All comparisons 
        /// ignore casing.
        /// </summary>
        /// <param name="itemName">The name of the item to find.</param>
        /// <param name="names">The array of names to search within.</param>
        /// <param name="logger">The logger to log to.</param>
        /// <param name="itemType">The type of item to find, for logging purposes.</param>
        /// <returns>The index, or <see cref="cCore.NULL_VALUE"/> if no match 
        /// was found.</returns>
        /// -------------------------------------------------------------------
        protected int FindItem(string itemName, string[] names, ILogger logger, string itemType)
        {
            itemName = itemName.Trim();
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Compare(itemName, names[i], StringComparison.OrdinalIgnoreCase) == 0)
                    return i;
            }
            logger.LogWarning("Unable to find {ItemType} '{ItemName}'", itemType, itemName);
            return cCore.NULL_VALUE;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Find the index of a shape by itemName
        /// </summary>
        /// <param name="shapeName">The name of the shape to find.</param>
        /// <param name="shapes">The collection of shapes to search within.</param>
        /// <param name="logger">The logger to log to.</param>
        /// <param name="shapeType">The type of shape to find, for logging purposes.</param>
        /// <returns>The index of the shape, or <see cref="cCore.NULL_VALUE"/> if no match was found.</returns>
        /// -------------------------------------------------------------------
        protected int FindShape(string shapeName, IEnumerable<cShapeData> shapes, ILogger logger, string shapeType)
        {
            if (shapes == null) return cCore.NULL_VALUE;

            shapeName = shapeName.ToLowerInvariant();
            foreach (cShapeData shp in shapes)
            {
                if (string.Compare(shapeName, shp.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    return shp.Index;
            }
            logger.LogWarning("Unable to find {shapeType} '{shapeName}'", shapeType, shapeName);
            return cCore.NULL_VALUE;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Find the index of a shape by its database ID.
        /// </summary>
        /// <param name="IDBID">The database ID of the shape to find.</param>
        /// <param name="shapes">The collection of shapes to search within.</param>
        /// <param name="logger">The logger to log to.</param>
        /// <param name="shapeType">The type of shape to find, for logging purposes.</param>
        /// <returns>The index of the shape, or <see cref="cCore.NULL_VALUE"/> if no match was found.</returns>
        /// -------------------------------------------------------------------
        protected int FindShape(int IDBID, IEnumerable<cShapeData> shapes, ILogger logger, string shapeType)
        {
            if (shapes == null) return cCore.NULL_VALUE;

            foreach (cShapeData shp in shapes)
            {
                if (shp.DBID == IDBID)
                    return shp.Index;
            }
            logger.LogWarning("Unable to find {ShapeType} with DBID '{IDBID}'", shapeType, IDBID);
            return cCore.NULL_VALUE;
        }
        #endregion // Ecopath-wide accessors
    }
}
