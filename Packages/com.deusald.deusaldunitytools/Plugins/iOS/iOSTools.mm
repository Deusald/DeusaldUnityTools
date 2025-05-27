#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

extern "C" void _OpenAppStorePage(const char* appIdCStr)
{
    NSString *appId = [NSString stringWithUTF8String:appIdCStr];
    NSString *urlString = [NSString stringWithFormat:@"https://apps.apple.com/app/id%@", appId];
    NSURL *url = [NSURL URLWithString:urlString];

    if ([[UIApplication sharedApplication] canOpenURL:url]) {
        [[UIApplication sharedApplication] openURL:url options:@{} completionHandler:nil];
    }
}

extern "C" void _OpenAppSettings()
{
    if (@available(iOS 8.0, *)) {
        NSURL *settingsURL = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
        if ([[UIApplication sharedApplication] canOpenURL:settingsURL]) {
            [[UIApplication sharedApplication] openURL:settingsURL options:@{} completionHandler:nil];
        }
    }
}