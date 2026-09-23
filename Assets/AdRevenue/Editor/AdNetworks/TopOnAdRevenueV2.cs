using System;
using System.IO;
using System.Linq;
using Io.AppMetrica.AdRevenueAdapter.Editor.AdNetworks.Utils;
using UnityEngine;

namespace Io.AppMetrica.AdRevenueAdapter.Editor.AdNetworks {
    internal class TopOnAdRevenueV2 : AdNetwork {
        protected override string DefineName => "APPMETRICA_ADREVENUE_ADAPTER_TOPON_V2";

        internal override void OnChangedAction() {

            var tpnPluginAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == "TpnPlugin.AnyThinkAds");
            
            var filePath = $"{Application.dataPath}/TpnPlugin/AnyThinkAds/AppMetrica.TpnPlugin.asmdef";

            if (!File.Exists(filePath) && tpnPluginAssembly != null) {
                return; // do nothing if TpnPlugin assembly is already added by TopOn
            }

            switch (IsAvailable) {
                case true when !File.Exists(filePath):
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
                    File.WriteAllText(filePath, "{\n  \"name\": \"TpnPlugin.AnyThinkAds\",\n  \"autoReferenced\": true\n}\n");
                    break;
                case false when File.Exists(filePath):
                    File.Delete(filePath);
                    break;
            }
        }
        
        internal override bool IsAvailable => AssetsUtils.IsAssetInProject("ATSDKAPI");
    }
}
