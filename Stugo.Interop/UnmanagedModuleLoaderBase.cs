using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Stugo.Interop.Linux;
using Stugo.Interop.Windows;

namespace Stugo.Interop
{
    /// <summary>
    /// Represents a class capable of returning delegates to unmanaged functions.
    /// </summary>
    public abstract class UnmanagedModuleLoaderBase
    {
        /// <summary>
        /// Gets a value indicating if the current platform is Linunx.
        /// </summary>
        public static bool IsLinux
        {
            get
            {
                // http://stackoverflow.com/a/5117005/358336
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }


        /// <summary>
        /// Gets a loader for the current platform.
        /// </summary>
        /// <param name="modulePath">The path to the module.</param>
        /// <returns>An UnmanagedModuleLoaderBase implementation.</returns>
        public static UnmanagedModuleLoaderBase GetLoader(string modulePath)
        {
            if (IsLinux)
                return new LinuxUnmanagedModuleLoader(modulePath);
            else
                return new WindowsUnmanagedModuleLoader(modulePath);
        }


        /// <summary>
        /// Gets the path to the unmanaged module.
        /// </summary>
        public string ModulePath { get; private set; }


        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="modulePath">The path to the module.</param>
        protected UnmanagedModuleLoaderBase(string modulePath)
        {
            this.ModulePath = modulePath;
        }


        /// <summary>
        /// Gets a delegate to a method in an unmanaged module.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="delegateType">The type of the delegate to return.</param>
        /// <returns>A delegate to the method.</returns>
        public virtual Delegate GetDelegate(string methodName, Type delegateType)
        {
            IntPtr procaddress = getUnmanagedMethodPointer(methodName);
            return Marshal.GetDelegateForFunctionPointer(procaddress, delegateType);
        }


        /// <summary>
        /// Gets a delegate to a method in an unmanaged module.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate to return.</typeparam>
        /// <param name="methodName">The name of the method.</param>
        /// <returns>A delegate to the method.</returns>
        public virtual TDelegate GetDelegate<TDelegate>(string methodName)
            where TDelegate : class
        {
            IntPtr procaddress = getUnmanagedMethodPointer(methodName);
            return Marshal.GetDelegateForFunctionPointer(procaddress, typeof(TDelegate)) as TDelegate;
        }


        /// <summary>
        /// When overriden in a derived class, gets a pointer to an unmanaged method.
        /// </summary>
        /// <param name="methodName">The name of the method to get a pointer for.</param>
        /// <returns>The method pointer.</returns>
        protected abstract IntPtr getUnmanagedMethodPointer(string methodName);
    }
}
