package io.appmetrica.adrevenueadapter.plugin.unity;

import android.util.Log;
import androidx.annotation.NonNull;
import org.json.JSONException;
import io.appmetrica.analytics.ModulesFacade;

public class AdRevenueAdapterProxy {

    public static void reportAdRevenue(@NonNull String adRevenue) {
        try {
            ModulesFacade.reportAdRevenue(AdRevenueDeserializer.fromJsonString(adRevenue), true);
        } catch (JSONException e) {
            Log.e("AdRevenueAdapterUnity", "Failed to report AdRevenue. Data was parsed with error", e);
        }
    }

}
