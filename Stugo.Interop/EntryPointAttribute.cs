using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stugo.Interop
{
    /// <summary>
    /// Allows the specification of the unmanaged entry point.
    /// </summary>
    [AttributeUsage(AttributeTargets.Delegate)]
    public class EntryPointAttribute : Attribute
    {
        public string EntryPoint { get; set; }
    }
}
