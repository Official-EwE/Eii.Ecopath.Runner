using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Eii.Ecopath.Runner.Services.Automation
{
    // --------------------------------------------------------------------
    /// <summary>
    /// Generates a Markdown reference document for all automation commands
    /// that can be used in the <c>Modifications</c> dictionary of a
    /// <c>Changes</c> entry in a run-info JSON file.
    /// No model file or EwE core instance is required.
    /// </summary>
    // --------------------------------------------------------------------
    public static class cAutomationDocumentation
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// Generate a Markdown string documenting all automation commands
        /// reachable from the root node.
        /// </summary>
        // --------------------------------------------------------------------
        public static string GenerateMarkdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# EwE Runner \u2014 Automation Commands");
            sb.AppendLine();
            sb.AppendLine("Commands are placed in the `Modifications` dictionary of a `Changes` entry in a run-info JSON file:");
            sb.AppendLine();
            sb.AppendLine("```json");
            sb.AppendLine("{");
            sb.AppendLine("  \"Date\": \"1-1-2025\",");
            sb.AppendLine("  \"Modifications\": {");
            sb.AppendLine("    \"ecosim.effort[1].fill\": [ 0.5 ]");
            sb.AppendLine("  }");
            sb.AppendLine("}");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("> **`[#]`** = 1-based integer index. String-name aliases work at runtime but are not listed here.");
            sb.AppendLine();
            sb.AppendLine("## Commands");
            sb.AppendLine();
            AppendNodeDocs(sb, typeof(cEwERootNode), "", 3, []);
            return sb.ToString();
        }

        // Walk declared methods on the type AND its base chain up to (but not
        // including) cNode, so inherited methods such as cFunctionNode.set on
        // cForcingFunctionNode are included. The most-derived declaration wins.
        static IEnumerable<MethodInfo> GetAutomationMethods(Type t)
        {
            var seen = new HashSet<string>();
            Type? cur = t;
            while (cur != null && cur != typeof(cNode) && cur != typeof(object))
            {
                foreach (MethodInfo m in cur.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                    if (seen.Add(m.Name))
                        yield return m;
                cur = cur.BaseType;
            }
        }

        static void AppendNodeDocs(StringBuilder sb, Type nodeType, string prefix, int depth, HashSet<string> visited)
        {
            string heading = new string('#', Math.Min(depth, 6));

            foreach (MethodInfo method in GetAutomationMethods(nodeType))
            {
                if (method.GetCustomAttribute<AutomationIgnoreAttribute>() != null) continue;

                ParameterInfo[] parameters = method.GetParameters();

                // Skip string-parameter (name-based) aliases — same rule as ListAutomationPaths
                if (parameters.Length > 0 && parameters[0].ParameterType == typeof(string)) continue;

                bool returnsNode = typeof(cNode).IsAssignableFrom(method.ReturnType);
                bool hasIntParam = parameters.Length == 1 && parameters[0].ParameterType == typeof(int);

                string segment = method.Name + (hasIntParam ? "[#]" : "");
                string fullPath = string.IsNullOrEmpty(prefix) ? segment : $"{prefix}.{segment}";

                // Cycle guard: same concrete return type at the same path prefix
                string visitKey = $"{method.ReturnType.FullName}@{fullPath}";
                if (!visited.Add(visitKey)) continue;

                string desc = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";

                if (returnsNode)
                {
                    sb.AppendLine($"{heading} `{fullPath}`");
                    if (!string.IsNullOrEmpty(desc))
                        sb.AppendLine($"> {desc}");
                    sb.AppendLine();
                    AppendNodeDocs(sb, method.ReturnType, fullPath, depth + 1, visited);
                }
                else
                {
                    string paramStr = string.Join(", ", parameters.Select(p =>
                        $"`{p.ParameterType.Name}` {p.GetCustomAttribute<DescriptionAttribute>()?.Description ?? p.Name ?? ""}"));
                    sb.Append($"- **`{fullPath}`**");
                    if (!string.IsNullOrEmpty(paramStr)) sb.Append($" = {paramStr}");
                    if (!string.IsNullOrEmpty(desc)) sb.Append($" \u2014 {desc}");
                    sb.AppendLine();
                }
            }
        }
    }
}
