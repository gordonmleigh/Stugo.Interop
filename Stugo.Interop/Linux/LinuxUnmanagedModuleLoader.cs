using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Stugo.Interop.Linux
{
    public class LinuxUnmanagedModuleLoader : UnmanagedModuleLoaderBase
    {
        const int RTLD_NOW = 2; // for dlopen's flags 

        [DllImport("libdl.so")]
        protected static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl.so")]
        protected static extern IntPtr dlsym(IntPtr handle, string symbol);


        /// <summary>
        /// Gets the handle for the unmanaged library.
        /// </summary>
        protected IntPtr ModuleHandle { get; private set; }


        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="modulePath">The path to the module.</param>
        public LinuxUnmanagedModuleLoader(string modulePath)
            : base(modulePath)
        {
            this.ModuleHandle = dlopen(modulePath, RTLD_NOW);

            // give a meaningful error if the library cannot be loaded.
            if (this.ModuleHandle == IntPtr.Zero)
            {
                throw new ArgumentException(
                    string.Format(
                        "Unable to load unmanaged module \"{0}\"",
                        modulePath));
            }
        }


        /// <summary>
        /// When overriden in a derived class, gets a pointer to an unmanaged method.
        /// </summary>
        /// <param name="methodName">The name of the method to get a pointer for.</param>
        /// <returns>The method pointer.</returns>
        protected override IntPtr getUnmanagedMethodPointer(string methodName)
        {
            IntPtr ptr = dlsym(this.ModuleHandle, methodName);

            if (ptr == IntPtr.Zero)
                throw new MissingMethodException(
                    string.Format(
                        "The unmanaged method \"{0}\" does not exist",
                        methodName));

            return ptr;
        }
    }
}
