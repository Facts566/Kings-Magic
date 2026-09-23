using System;
using UnityEditor;

namespace Io.AppMetrica.AdRevenueAdapter.Editor.AdNetworks.Utils {
    internal abstract class AssetsUtils {
        internal static bool IsAssetInProject(string assetName) {
            try {
                var assets = AssetDatabase.FindAssets(assetName);
                if (assets.Length != 0) {
                    return true;
                }
            } catch (Exception) {
                return false;
            }

            return false;
        }
    }
}
