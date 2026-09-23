#import <AppMetricaCore/AppMetricaCore.h>
#import <AppMetricaCoreExtension/AMAAppMetricaExtended.h>
#import "AMAUARAAdRevenueUtils.h"

void amauara_reportAdRevenue(char *adRevenueJson) {
    AMAAdRevenueInfo *adRevenue = amauara_deserializeAdRevenueInfoFromJson(adRevenueJson);
    if (adRevenue != nil) {
        [AMAAppMetrica reportAdRevenue:adRevenue
                       isAutocollected:YES
                             onFailure:^(NSError *error) {
            NSLog(@"Failed to report AdRevenue to AppMetrica: %@", [error localizedDescription]);
        }];
    } else {
        NSLog(@"Failed to deserialize AppMetrica AdRevenue: %s", adRevenueJson);
    }
}
