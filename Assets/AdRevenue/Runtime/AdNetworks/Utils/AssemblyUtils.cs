using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Io.AppMetrica.AdRevenueAdapter.AdNetworks.Utils {
    internal static class AssemblyUtils {
        private static readonly string[] RequiredAssemblies = {
            Constants.TopOnAssemblyName,
            Constants.IronSourceV8AssemblyName,
            Constants.IronSourceV9AssemblyName,
            Constants.AppLovinAssemblyName
        };

        private static Dictionary<string, Assembly> _assemblyCache;

        private static bool _isInitialized;

        private static readonly object LockObject = new object();

        public static Assembly GetAssembly(string name) {
            if (!_isInitialized) {
                lock (LockObject) {
                    if (!_isInitialized) {
                        InitializeAssemblyCache();
                        _isInitialized = true;
                    }
                }
            }

            return _assemblyCache.TryGetValue(name, out var assembly) ? assembly : null;
        }

        private static void InitializeAssemblyCache() {
            _assemblyCache = new Dictionary<string, Assembly>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                var fullName = assembly.GetName().Name;
                
                if (RequiredAssemblies.Contains(fullName)) {
                    _assemblyCache[fullName] = assembly;
                }
            }
        }
    }
}
