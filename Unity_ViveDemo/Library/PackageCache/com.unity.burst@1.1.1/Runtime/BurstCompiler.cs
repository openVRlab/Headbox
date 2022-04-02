// For some reasons Unity.Burst.LowLevel is not part of UnityEngine in 2018.2 but only in UnityEditor
// In 2018.3 It should be fine
#if !UNITY_ZEROPLAYER && !UNITY_CSHARP_TINY && ((UNITY_2018_2_OR_NEWER && UNITY_EDITOR) || UNITY_2018_3_OR_NEWER)
using System.Text;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Unity.Burst
{
    /// <summary>
    /// The burst compiler runtime frontend.
    /// </summary>
    public static class BurstCompiler
    {
        private static readonly object GlobalLock = new object();
        private static BurstCompilerOptions _global = null;

        /// <summary>
        /// Gets the global options for the burst compiler.
        /// </summary>
        public static BurstCompilerOptions Options
        {
            get
            {
                // We only create it when it is used, not when this class is initialized
                lock (GlobalLock)
                {
                    // Make sure to late initialize the settings
                    return _global ?? (_global = new BurstCompilerOptions(BurstCompilerOptions.GlobalSettingsName)); // naming used only for debugging
                }
            }
        }

#if UNITY_2019_3_OR_NEWER && UNITY_BURST_PENDING_FEATURE_FOR_2019_3
        /// <summary>
        /// Sets the execution mode for all jobs spawned from now on.
        /// </summary>
        /// <param name="mode">Specifiy the required execution mode</param>
        public static void SetExecutionMode(BurstExecutionEnvironment mode)
        {
            Burst.LowLevel.BurstCompilerService.SetCurrentExecutionMode((uint)mode);
        }
        /// <summary>
        /// Retrieve the current execution mode that is configured.
        /// </summary>
        /// <returns>Currently configured execution mode</returns>
        public static BurstExecutionEnvironment GetExecutionMode()
        {
            return (BurstExecutionEnvironment)Burst.LowLevel.BurstCompilerService.GetCurrentExecutionMode();
        }
#endif

        /// <summary>
        /// Compile the following delegate with burst and return a new delegate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegateMethod"></param>
        /// <returns></returns>
        /// <remarks>NOT AVAILABLE, unsafe to use</remarks>
        internal static unsafe T CompileDelegate<T>(T delegateMethod) where T : class
        {
            // We have added support for runtime CompileDelegate in 2018.2+
            void* function = Compile(delegateMethod);
            object res = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer((IntPtr)function, delegateMethod.GetType());
            return (T)res;
        }

        /// <summary>
        /// Compile the following delegate into a function pointer with burst, only invokable from a burst jobs.
        /// </summary>
        /// <typeparam name="T">Type of the delegate of the function pointer</typeparam>
        /// <param name="delegateMethod">The delegate to compile</param>
        /// <returns>A function pointer invokable from a burst jobs</returns>
        public static unsafe FunctionPointer<T> CompileFunctionPointer<T>(T delegateMethod) where T : class
        {
            // We have added support for runtime CompileDelegate in 2018.2+
            void* function = Compile(delegateMethod);
            return new FunctionPointer<T>(new IntPtr(function));
        }

        private static unsafe void* Compile<T>(T delegateObj) where T : class
        {
            if (delegateObj == null) throw new ArgumentNullException(nameof(delegateObj));
            if (!(delegateObj is Delegate)) throw new ArgumentException("object instance must be a System.Delegate", nameof(delegateObj));

            var delegateMethod = (Delegate)(object)delegateObj;
            if (!delegateMethod.Method.IsStatic)
            {
                throw new InvalidOperationException($"The method `{delegateMethod.Method}` must be static. Instance methods are not supported");
            }

            string defaultOptions = "--enable-synchronous-compilation";
            // TODO: Disable this part as it is using Editor code that is not accessible from the runtime. We will have to move the editor code to here
            string extraOptions;
            bool debug;

            void* function = null;

            // The attribute is directly on the method, so we recover the underlying method here
            if (Options.TryGetOptions(delegateMethod.Method, false, out extraOptions, out debug))
            {
                if (!string.IsNullOrWhiteSpace(extraOptions))
                {
                    defaultOptions += "\n" + extraOptions;
                }

                int delegateMethodID = Unity.Burst.LowLevel.BurstCompilerService.CompileAsyncDelegateMethod(delegateObj, defaultOptions);
                function = Unity.Burst.LowLevel.BurstCompilerService.GetAsyncCompiledAsyncDelegateMethod(delegateMethodID);
            }

            if (!debug && Options.IsEnabled && function == null)
            {
                var builder = new StringBuilder();
                builder.AppendLine(delegateMethod.Method.ToString());
                foreach (var attribute in delegateMethod.Method.GetCustomAttributes())
                {
                    builder.AppendLine("   attribute: " + attribute.GetType().FullName);
                }
                throw new InvalidOperationException("Burst failed to compile the given delegate: " + builder.ToString());
            }

            // When burst compilation is disabled, we are still returning a valid function pointer (the a pointer to the managed function)
            // so that CompileFunctionPointer actually returns a delegate in all cases
            return function == null ? (void*)Marshal.GetFunctionPointerForDelegate(delegateMethod) : function;
        }
        /// <summary>
        /// Lets the compiler service know we are shutting down, called by the event EditorApplication.quitting
        /// </summary>
        internal static void Shutdown()
        {
            Unity.Burst.LowLevel.BurstCompilerService.GetDisassembly(typeof(BurstCompiler).GetMethod("ShutdownMethod",BindingFlags.Static|BindingFlags.NonPublic), "$shutdown");
        }

        /// <summary>
        /// Dummy empty method for Shutdown purposes
        /// </summary>
        private static void ShutdownMethod() { }
    }
}
#endif
