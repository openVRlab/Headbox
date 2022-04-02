using System;
using System.IO;
using System.Reflection;
using Unity.Burst.LowLevel;
using UnityEditor;

namespace Unity.Burst.Editor
{
    /// <summary>
    /// Main entry point for initializing the burst compiler service for both JIT and AOT
    /// </summary>
    [InitializeOnLoad]
    internal class BurstLoader
    {
        /// <summary>
        /// Gets the location to the runtime path of burst.
        /// </summary>
        public static string RuntimePath { get; private set; }

        public static bool IsDebugging { get; private set; }

        static BurstLoader()
        {
            // This can be setup to get more diagnostics
            IsDebugging = Environment.GetEnvironmentVariable("UNITY_BURST_DEBUG") != null;

            // Try to load the runtime through an environment variable
            RuntimePath = Environment.GetEnvironmentVariable("UNITY_BURST_RUNTIME_PATH");

            // Otherwise try to load it from the package itself
            if (!Directory.Exists(RuntimePath))
            {
                RuntimePath = Path.GetFullPath("Packages/com.unity.burst/.Runtime");
            }

            BurstEditorOptions.EnsureSynchronized();

            BurstCompilerService.Initialize(RuntimePath, TryGetOptionsFromMember);

            EditorApplication.quitting += BurstCompiler.Shutdown;
        }

        private static bool TryGetOptionsFromMember(MemberInfo member, out string flagsOut)
        {
            bool debug;
            var result = BurstCompiler.Options.TryGetOptions(member, true, out flagsOut, out debug);
            return result && !debug;
        }
    }
}
