using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.Globalization;
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

        #region Private automation helpers

        /// <summary>
        /// A path selector exactly as it appeared between square brackets.
        /// The selector remains untyped until a matching automation overload is resolved.
        /// </summary>
        private sealed record AutomationSelector(string Value);

        /// <summary>
        /// A successfully resolved method together with the arguments converted for that method.
        /// Lower scores indicate a better match.
        /// </summary>
        private sealed record MethodBinding(MethodInfo Method, object?[] Arguments, int Score);

        /// <summary>
        /// A parsed element of an automation path.
        /// </summary>
        private sealed record AutomationPathEntry(string MethodName, AutomationSelector? Selector);

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
            if (string.IsNullOrWhiteSpace(methodPath))
            {
                Logger.LogError("Automation method path cannot be empty");
                return false;
            }

            bool bIncompatible = false;

            // Check compatibility. Automation paths are treated case-insensitively.
            switch (context?.ToLowerInvariant())
            {
                case "ecospace":
                    // All good
                    break;

                case "ecosim":
                    bIncompatible = methodPath.StartsWith("ecospace", StringComparison.OrdinalIgnoreCase);
                    break;

                case "ecopath":
                    bIncompatible = methodPath.StartsWith("ecosim", StringComparison.OrdinalIgnoreCase) |
                                    methodPath.StartsWith("ecospace", StringComparison.OrdinalIgnoreCase);
                    break;
            }

            if (bIncompatible)
            {
                Logger.LogError("Change '{MethodPath}' cannot be executed under '{Context}'", methodPath, context);
                return false;
            }

            return CrawlAutomationTree(methodPath, fnparms);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Recursively crawls the automation tree and invokes the final method
        /// endpoint. Intermediate path entries may contain a selector in square
        /// brackets. Selector values are resolved against available overloads
        /// and may therefore represent either names or numeric indexes.
        /// </summary>
        /// <param name="methodPath">The complete path to the method to invoke.</param>
        /// <param name="fnparms">The parameters to pass to the final function to execute.</param>
        /// <returns>True if the method was successfully invoked; otherwise, false.</returns>
        /// -------------------------------------------------------------------
        protected bool CrawlAutomationTree(string methodPath, object fnparms)
        {
            try
            {
                int iDot = methodPath.IndexOf('.');
                string rawEntry = iDot >= 0 ? methodPath[..iDot] : methodPath;
                string? remainder = iDot >= 0 ? methodPath[(iDot + 1)..] : null;

                if (!TryParseAutomationPathEntry(rawEntry, out AutomationPathEntry? entry))
                {
                    Logger.LogError("Automation entry '{Entry}' in '{MethodPath}' is malformed", rawEntry, methodPath);
                    return false;
                }

                // The final path component is the endpoint. Selectors on endpoints are
                // currently not part of the automation grammar; endpoint parameters are
                // supplied through fnparms.
                if (remainder == null)
                {
                    if (entry.Selector != null)
                    {
                        Logger.LogError("Automation endpoint '{Entry}' in '{MethodPath}' cannot contain a selector", rawEntry, methodPath);
                        return false;
                    }

                    object?[] endpointArguments = [fnparms];
                    MethodBinding? binding = ResolveAutomationMethod(GetType(), entry.MethodName, endpointArguments);

                    if (binding == null)
                    {
                        Logger.LogError("Automation endpoint '{Entry}' in '{MethodPath}' cannot be resolved", entry.MethodName, methodPath);
                        return false;
                    }

                    try
                    {
                        object? success = binding.Method.Invoke(this, binding.Arguments);
                        return Convert.ToBoolean(success, CultureInfo.InvariantCulture);
                    }
                    catch (TargetInvocationException ex) when (ex.InnerException != null)
                    {
                        Logger.LogError(ex.InnerException, "Automation endpoint {Entry}({Parms}) threw error", entry.MethodName, fnparms);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Automation endpoint {Entry}({Parms}) threw error", entry.MethodName, fnparms);
                        return false;
                    }
                }

                object?[] arguments = entry.Selector == null
                    ? []
                    : [entry.Selector];

                MethodBinding? intermediateBinding = ResolveAutomationMethod(GetType(), entry.MethodName, arguments);

                if (intermediateBinding == null)
                {
                    Logger.LogError("Automation entry '{Entry}' in '{MethodPath}' cannot be resolved", rawEntry, methodPath);
                    return false;
                }

                object? result;
                try
                {
                    result = intermediateBinding.Method.Invoke(this, intermediateBinding.Arguments);
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    Logger.LogError(ex.InnerException, "Automation invocation '{Entry}' in '{MethodPath}' threw error", rawEntry, methodPath);
                    return false;
                }

                if (result is not cNode subNode)
                {
                    Logger.LogError("Automation entry '{Entry}' in '{MethodPath}' bug! cNode expected", rawEntry, methodPath);
                    return false;
                }

                return subNode.CrawlAutomationTree(remainder, fnparms);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Automation {MethodPath} threw error", methodPath);
                return false;
            }
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Parses one automation path entry into a method name and optional selector.
        /// </summary>
        /// -------------------------------------------------------------------
        private static bool TryParseAutomationPathEntry(string rawEntry, out AutomationPathEntry? entry)
        {
            entry = null;

            if (string.IsNullOrWhiteSpace(rawEntry))
                return false;

            int iOpen = rawEntry.IndexOf('[');
            if (iOpen < 0)
            {
                // A closing bracket without an opening bracket is malformed.
                if (rawEntry.Contains(']'))
                    return false;

                entry = new AutomationPathEntry(rawEntry, null);
                return true;
            }

            int iClose = rawEntry.LastIndexOf(']');

            // For now exactly one selector is supported per path element, and it
            // must occupy the tail of the element: method[selector].
            if (iOpen == 0 ||
                iClose <= iOpen ||
                iClose != rawEntry.Length - 1 ||
                rawEntry.IndexOf('[', iOpen + 1) >= 0 ||
                rawEntry.IndexOf(']', iOpen + 1) != iClose)
            {
                return false;
            }

            string methodName = rawEntry[..iOpen];
            string selector = rawEntry[(iOpen + 1)..iClose].Trim();

            if (string.IsNullOrWhiteSpace(methodName) || string.IsNullOrWhiteSpace(selector))
                return false;

            entry = new AutomationPathEntry(methodName, new AutomationSelector(selector));
            return true;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Resolves the best matching public instance method for an automation
        /// invocation. Method names are matched case-insensitively and arguments
        /// are converted only after a candidate overload has been identified.
        /// </summary>
        /// -------------------------------------------------------------------
        private static MethodBinding? ResolveAutomationMethod(Type type, string methodName, object?[] arguments)
        {
            List<MethodBinding> candidates = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
                .Where(m => m.GetCustomAttribute<AutomationIgnoreAttribute>() == null)
                .Where(m => m.GetParameters().Length == arguments.Length)
                .Select(m => TryBindAutomationMethod(m, arguments))
                .Where(b => b != null)
                .Cast<MethodBinding>()
                .OrderBy(b => b.Score)
                .ThenBy(b => b.Method.MetadataToken)
                .ToList();

            if (candidates.Count == 0)
                return null;

            // Equal-scoring overloads are genuinely ambiguous. Do not depend on
            // reflection order to select one silently.
            if (candidates.Count > 1 && candidates[0].Score == candidates[1].Score)
                return null;

            return candidates[0];
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Attempts to bind supplied arguments to a specific method overload.
        /// </summary>
        /// -------------------------------------------------------------------
        private static MethodBinding? TryBindAutomationMethod(MethodInfo method, object?[] suppliedArguments)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object?[] boundArguments = new object?[suppliedArguments.Length];
            int score = 0;

            for (int i = 0; i < parameters.Length; i++)
            {
                object? supplied = suppliedArguments[i];
                Type targetType = parameters[i].ParameterType;

                if (!TryBindAutomationArgument(supplied, targetType, out object? bound, out int argumentScore))
                    return null;

                boundArguments[i] = bound;
                score += argumentScore;
            }

            return new MethodBinding(method, boundArguments, score);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Attempts to convert one supplied automation argument to the requested
        /// parameter type. Lower scores indicate stronger matches.
        /// </summary>
        /// -------------------------------------------------------------------
        private static bool TryBindAutomationArgument(object? supplied, Type targetType, out object? bound, out int score)
        {
            bound = null;
            score = int.MaxValue;

            Type? nullableType = Nullable.GetUnderlyingType(targetType);
            Type effectiveTargetType = nullableType ?? targetType;

            if (supplied == null)
            {
                if (targetType.IsValueType && nullableType == null)
                    return false;

                score = 0;
                return true;
            }

            // Path selectors are intentionally unresolved lexical tokens. Prefer
            // a meaningful typed interpretation (e.g. int) over string when both
            // overloads are available, while retaining string as the natural
            // fallback for names such as "trawlers".
            if (supplied is AutomationSelector selector)
            {
                string value = selector.Value;

                if (effectiveTargetType != typeof(string) &&
                    TryConvertAutomationValue(value, effectiveTargetType, out object? converted))
                {
                    bound = converted;
                    score = 0;
                    return true;
                }

                if (effectiveTargetType == typeof(string))
                {
                    bound = value;
                    score = 1;
                    return true;
                }

                return false;
            }

            Type suppliedType = supplied.GetType();

            // Exact runtime type.
            if (targetType == suppliedType)
            {
                bound = supplied;
                score = 0;
                return true;
            }

            // Normal reference/interface/base-class compatibility.
            if (targetType.IsAssignableFrom(suppliedType))
            {
                bound = supplied;
                score = 1;
                return true;
            }

            // Last resort for endpoint arguments and other CLR values.
            if (TryConvertAutomationValue(supplied, effectiveTargetType, out object? convertedValue))
            {
                bound = convertedValue;
                score = 2;
                return true;
            }

            return false;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Converts an automation value to a requested CLR type using invariant
        /// culture. Enum and Guid values receive explicit handling.
        /// </summary>
        /// -------------------------------------------------------------------
        private static bool TryConvertAutomationValue(object value, Type targetType, out object? converted)
        {
            converted = null;

            try
            {
                if (targetType.IsEnum)
                {
                    if (value is string enumText)
                    {
                        converted = Enum.Parse(targetType, enumText, ignoreCase: true);
                        return true;
                    }

                    converted = Enum.ToObject(targetType, value);
                    return true;
                }

                if (targetType == typeof(Guid))
                {
                    if (Guid.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out Guid guid))
                    {
                        converted = guid;
                        return true;
                    }

                    return false;
                }

                converted = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// -----------------------------------------------------------------------
        /// <summary>
        /// Lists the paths exposed by the automation tree. Overloads are collapsed
        /// into a single path entry; selector-based navigation is represented by
        /// "[#]". Indexed overloads are used for safe tree discovery where possible.
        /// </summary>
        /// <param name="prefix">The prefix to prepend to each automation path.</param>
        /// <returns>A list of automation paths.</returns>
        /// -----------------------------------------------------------------------
        public List<string> ListAutomationPaths(string prefix = "")
        {
            List<string> paths = new();

            var methodGroups = GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttribute<AutomationIgnoreAttribute>() == null)
                .GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var methodGroup in methodGroups)
            {
                // Endpoints do not need to be invoked to list them.
                MethodInfo? endpoint = methodGroup.FirstOrDefault(
                    m => !typeof(cNode).IsAssignableFrom(m.ReturnType));

                if (endpoint != null)
                {
                    string fullPath = string.IsNullOrEmpty(prefix)
                        ? endpoint.Name
                        : $"{prefix}.{endpoint.Name}";
                    paths.Add(fullPath);
                    continue;
                }

                // Navigation methods return cNode. Prefer a zero-argument overload;
                // otherwise use an integer overload with a conventional valid index
                // for discovery. A string-only navigation overload cannot be invoked
                // generically without inventing a model item name.
                MethodInfo? navigationMethod = methodGroup.FirstOrDefault(m => m.GetParameters().Length == 0);
                object?[] navigationArguments = [];
                bool hasSelector = false;

                if (navigationMethod == null)
                {
                    navigationMethod = methodGroup.FirstOrDefault(m =>
                    {
                        ParameterInfo[] p = m.GetParameters();
                        return p.Length == 1 && p[0].ParameterType == typeof(int);
                    });

                    if (navigationMethod != null)
                    {
                        navigationArguments = [1];
                        hasSelector = true;
                    }
                }

                if (navigationMethod == null)
                    continue;

                object? result;
                try
                {
                    result = navigationMethod.Invoke(this, navigationArguments);
                }
                catch
                {
                    // Listing automation paths should never make the caller fail merely
                    // because a representative child node cannot be constructed.
                    continue;
                }

                if (result is not cNode subNode)
                    continue;

                string navigationPath = string.IsNullOrEmpty(prefix)
                    ? navigationMethod.Name
                    : $"{prefix}.{navigationMethod.Name}";

                if (hasSelector)
                    navigationPath += "[#]";

                paths.AddRange(subNode.ListAutomationPaths(navigationPath));
            }

            return paths
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        #region Ecopath-wide accessors

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
