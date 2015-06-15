# Stugo.Interop

Helper library for dynamic P/Invoke on Windows and Linux.  Developed for my company, 
[Stugo Ltd](http://stugo.co.uk/).

This library allows you to load the correct unmanaged module for the current platform at runtime,
and P/Invoke to methods in that library.


## Licence

Copyright (c) 2012-2015 Stugo Ltd

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.


## Sample usage

Create the library wrapper:

    public class MyUnmanagedWrapper
    {
        public Func<IntPtr, string> Foo;

		[EntryPoint("nameOfMethodInUnmanagedLib")]
		public Func<IntPtr, int, int> Bar;
    }

Load the library from the assembly's embedded resources at program initialisation:

	string unmanagedLib = "My.Project.Namespace.";
            
    if (UnmanagedModuleLoaderBase.IsLinux)
        unmanagedLib += "libunmanaged.so";
    else
        unmanagedLib += "unmanaged.dll";
            
    UnmanagedModuleCollection.Instance.LoadModuleFromEmbeddedResource<MyUnmanagedWrapper>(
        typeof(IndexFile).Assembly, unmanagedLib);

Use the library anywhere it is required:

	var unmanaged = UnmanagedModuleCollection.Instance.GetModule<MyUnmanagedWrapper>();

Alternatively, you can access the loader instance directly:

	var loader = UnmanagedModuleLoaderBase.GetLoader("path/to/library");
	var func = loader.GetDelegate("foo", typeof(Func<IntPtr, string>));
	func("hello");