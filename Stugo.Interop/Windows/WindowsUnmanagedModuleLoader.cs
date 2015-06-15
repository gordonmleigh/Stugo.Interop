using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Stugo.Interop.Windows
{
    public class WindowsUnmanagedModuleLoader : UnmanagedModuleLoaderBase
    {
        [DllImport("kernel32.dll")]
        protected static extern IntPtr LoadLibrary(string filename);

        [DllImport("kernel32.dll")]
        protected static extern IntPtr GetProcAddress(IntPtr hModule, string procname);


        /// <summary>
        /// Gets the handle for the unmanaged library.
        /// </summary>
        protected IntPtr ModuleHandle { get; private set; }


        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="modulePath">The path to the module.</param>
        public WindowsUnmanagedModuleLoader(string modulePath)
            : base(modulePath)
        {
            this.ModuleHandle = LoadLibrary(modulePath);

            // give a meaningful error if the library cannot be loaded.
            if (this.ModuleHandle == IntPtr.Zero)
            {
                Win32Exception win32exception = new Win32Exception(Marshal.GetLastWin32Error());

                throw new ArgumentException(
                    string.Format(
                        "Unable to load unmanaged module \"{0}\": {1}",
                        modulePath,
                        win32exception.Message),
                    win32exception);
            }
        }


        /// <summary>
        /// When overriden in a derived class, gets a pointer to an unmanaged method.
        /// </summary>
        /// <param name="methodName">The name of the method to get a pointer for.</param>
        /// <returns>The method pointer.</returns>
        protected override IntPtr getUnmanagedMethodPointer(string methodName)
        {
            IntPtr ptr = GetProcAddress(this.ModuleHandle, methodName);

            if (ptr == IntPtr.Zero)
                throw new MissingMethodException(
                    string.Format(
                        "The unmanaged method \"{0}\" does not exist",
                        methodName));

            return ptr;
        }
    }
}
