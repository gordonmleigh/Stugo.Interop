using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Stugo.Interop
{
    /// <summary>
    /// Represents a collection of unmanaged modules.
    /// </summary>
    public class UnmanagedModuleCollection
    {
        /// <summary>
        /// Gets the current instance.  Creates a new instance if none has been created.
        /// </summary>
        public static UnmanagedModuleCollection Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new UnmanagedModuleCollection();
                return _Instance;
            }
        } static UnmanagedModuleCollection _Instance = null;


        /// <summary>
        /// The currently loaded modules.
        /// </summary>
        private readonly Dictionary<Type, object> modules;


        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public UnmanagedModuleCollection()
        {
            this.modules = new Dictionary<Type, object>();
        }


        /// <summary>
        /// Loads the module at the given path and produces a 
        /// wrapper instance for it.
        /// </summary>
        public TWrapper LoadModule<TWrapper>(string modulePath)
        {
            return (TWrapper)LoadModule(modulePath, typeof(TWrapper));
        }


        /// <summary>
        /// Loads the module at the given path and produces a
        /// wrapper instance for it.
        /// </summary>
        public object LoadModule(string modulePath, Type wrapperType)
        {
            if (this.modules.ContainsKey(wrapperType))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "The type \"{0}\" has already been loaded",
                        wrapperType.FullName));
            }

            object instance = LoadUnmanagedModule(modulePath, wrapperType);
            this.modules.Add(wrapperType, instance);
            return instance;
        }


        /// <summary>
        /// Loads an unmanaged module from an embedded resource stream.
        /// </summary>
        public TWrapper LoadModuleFromEmbeddedResource<TWrapper>(Assembly resourceAssembly, string resourceId)
        {
            return (TWrapper)LoadModuleFromEmbeddedResource(resourceAssembly, resourceId, typeof(TWrapper));
        }


        /// <summary>
        /// Loads an unmanaged module from an embedded resource stream.
        /// </summary>
        public object LoadModuleFromEmbeddedResource(Assembly resourceAssembly, string resourceId, Type wrapperType)
        {
            if (this.modules.ContainsKey(wrapperType))
            {
                throw new InvalidOperationException(
                    string.Format(
                        "The type \"{0}\" has already been loaded",
                        wrapperType.FullName));
            }

            string modulepath = Path.Combine(
                Path.GetTempPath(),
                resourceAssembly.GetName().Name + "." + resourceAssembly.GetName().Version.ToString());

            Directory.CreateDirectory(modulepath);

            modulepath = Path.Combine(modulepath,
                Path.ChangeExtension(resourceId, Path.GetExtension(resourceId)));

            // store the module in a temporary file
            using (Stream resourceStream = resourceAssembly.GetManifestResourceStream(resourceId))
            {
                const int bufferSize = 4096;

                using (Stream outfile = File.Create(modulepath))
                {
                    byte[] buffer = new byte[bufferSize];

                    // write out the module file
                    while (true)
                    {
                        int count = resourceStream.Read(buffer, 0, bufferSize);

                        if (count < 1)
                            break;

                        outfile.Write(buffer, 0, count);
                    }
                }
            }

            return LoadModule(modulepath, wrapperType);
        }


        /// <summary>
        /// Returns the implementation of the given wrapper type.
        /// </summary>
        public TWrapper GetModule<TWrapper>()
        {
            return (TWrapper)this.modules[typeof(TWrapper)];
        }


        /// <summary>
        /// Returns the implementation of the given wrapper type.
        /// </summary>
        public object GetModule(Type wrapperType)
        {
            return this.modules[wrapperType];
        }


        /// <summary>
        /// Loads a module and constructs a wrapper for it.
        /// </summary>
        protected virtual object LoadUnmanagedModule(string path, Type wrapperType)
        {
            UnmanagedModuleLoaderBase loader = UnmanagedModuleLoaderBase.GetLoader(path);
            object wrapper = Activator.CreateInstance(wrapperType);

            foreach (FieldInfo field in wrapperType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                Type delegateType = field.FieldType;

                if (!typeof(Delegate).IsAssignableFrom(delegateType.BaseType))
                    continue;

                EntryPointAttribute entryPoint = (EntryPointAttribute)field.GetCustomAttributes(
                    typeof(EntryPointAttribute), true).SingleOrDefault();

                string methodName = entryPoint != null ? entryPoint.EntryPoint : field.Name;

                object methodDelegate = loader.GetDelegate(methodName, delegateType);
                field.SetValue(wrapper, methodDelegate);
            }

            return wrapper;
        }
    }
}
