#import "AMAUARAAdRevenueUtils.h"

NSString *amauara_stringFromCString(const char *string) {
    return string == nil ? nil : [NSString stringWithUTF8String:string];
}

bool amauara_isDictionaryOrNil(NSDictionary *dictionary) {
    return dictionary == nil || [dictionary isKindOfClass:[NSDictionary class]];
}

NSDictionary *amauara_dictionaryFromJSONString(NSString *json) {
    NSError *error = nil;
    NSDictionary *dict = json == nil ? nil : [NSJSONSerialization JSONObjectWithData:[json dataUsingEncoding:NSUTF8StringEncoding]
                                                                             options:0
                                                                               error:&error];
    if (error == nil && amauara_isDictionaryOrNil(dict)) {
        return dict;
    } else {
        [NSException raise:@"Failed parse json" format:@"%@ for json %@", error, json];
    }
}

NSDictionary *amauara_dictionaryFromCString(const char *json) {
    return amauara_dictionaryFromJSONString(amauara_stringFromCString(json));
}

NSDecimalNumber *amauara_decimalFromString(NSString *str) {
    NSDictionary *locale = [NSDictionary dictionaryWithObject:@"." forKey:NSLocaleDecimalSeparator];
    return [NSDecimalNumber decimalNumberWithString:str locale:locale];
}

AMAAdType amauara_adTypeFromString(NSString *adType)
{
    if (adType == nil) {
        return AMAAdTypeUnknown;
    }

    if ([adType isEqualToString:@"Native"]) {
        return AMAAdTypeNative;
    }
    if ([adType isEqualToString:@"Banner"]) {
        return AMAAdTypeBanner;
    }
    if ([adType isEqualToString:@"Rewarded"]) {
        return AMAAdTypeRewarded;
    }
    if ([adType isEqualToString:@"Interstitial"]) {
        return AMAAdTypeInterstitial;
    }
    if ([adType isEqualToString:@"Mrec"]) {
        return AMAAdTypeMrec;
    }
    if ([adType isEqualToString:@"Other"]) {
        return AMAAdTypeOther;
    }
    if ([adType isEqualToString:@"AppOpen"]) {
        return AMAAdTypeAppOpen;
    }

    return AMAAdTypeUnknown;
}

AMAAdRevenueInfo *amauara_deserializeAdRevenueInfoFromJson(const char *jsonString) {
    if (jsonString == nil) {
        return nil;
    }
    
    NSDictionary *dict = amauara_dictionaryFromCString(jsonString);
    if (dict == nil) {
        return nil;
    }
    
    NSDecimalNumber *adRevenueValue = amauara_decimalFromString(dict[@"AdRevenue"]);
    AMAMutableAdRevenueInfo *adRevenue = [[AMAMutableAdRevenueInfo alloc] initWithAdRevenue:adRevenueValue
                                                                                   currency:dict[@"Currency"]];
    
    if (dict[@"AdNetwork"] != nil) {
        adRevenue.adNetwork = dict[@"AdNetwork"];
    }
    if (dict[@"AdPlacementId"] != nil) {
        adRevenue.adPlacementID = dict[@"AdPlacementId"];
    }
    if (dict[@"AdPlacementName"] != nil) {
        adRevenue.adPlacementName = dict[@"AdPlacementName"];
    }
    if (dict[@"AdType"] != nil) {
        adRevenue.adType = amauara_adTypeFromString(dict[@"AdType"]);
    }
    if (dict[@"AdUnitId"] != nil) {
        adRevenue.adUnitID = dict[@"AdUnitId"];
    }
    if (dict[@"AdUnitName"] != nil) {
        adRevenue.adUnitName = dict[@"AdUnitName"];
    }
    if (dict[@"Payload"] != nil && amauara_isDictionaryOrNil(dict[@"Payload"])) {
        adRevenue.payload = dict[@"Payload"];
    }
    if (dict[@"Precision"] != nil) {
        adRevenue.precision = dict[@"Precision"];
    }
    
    return adRevenue;
}
