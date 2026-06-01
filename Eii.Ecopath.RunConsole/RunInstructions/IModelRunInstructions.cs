
using System.Collections.Generic;

namespace EwERunConsole.Instructions
{
    // --------------------------------------------------------------------
    /// <summary>
    /// Interface for describing the changes that a user may want to make to
    /// a running EwE model or search routine.
    /// </summary>
    // --------------------------------------------------------------------
    internal interface IModelRunInstructions
    {
        List<string> SaveContentCSV { get; set; }
        List<cModificationsAtT> Changes { get; set; }
    }
}