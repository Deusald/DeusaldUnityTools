// MIT License

// DeusaldUnityTools:
// Copyright (c) 2020 Adam "Deusald" Orliński

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#import "AudioToolBox/AudioServices.h"
#import <Foundation/Foundation.h>
#import <sys/sysctl.h>

extern "C"
{
    // 0 - not checked
    // 1 - not supported
    // 2 - taptic engine supported
    // 3 - haptic supported
    int _IsHapticSupported() 
    {
        size_t size;
        sysctlbyname("hw.machine", NULL, &size, NULL, 0);
        char *machine = (char *)malloc(size);
        sysctlbyname("hw.machine", machine, &size, NULL, 0);
        NSString *platform = [NSString stringWithUTF8String:machine];
        free(machine);
    
        // Check if the platform string starts with "iPad" or "iPod"
        if ([platform hasPrefix:@"iPad"] || [platform hasPrefix:@"iPod"]) 
        {
            return 1;
        }
        
        // https://gist.github.com/adamawolf/3048717
        NSArray<NSString *> *olderDevices = @[
                    @"iPhone1,1", // iPhone
                    @"iPhone1,2", // iPhone 3G
                    @"iPhone2,1", // iPhone 3GS
                    @"iPhone3,1", @"iPhone3,2", @"iPhone3,3", // iPhone 4
                    @"iPhone4,1", // iPhone 4s
                    @"iPhone5,1", @"iPhone5,2", // iPhone 5
                    @"iPhone5,3", @"iPhone5,4", // iPhone 5c
                    @"iPhone6,1", @"iPhone6,2", // iPhone 5s
                    @"iPhone7,1", // iPhone 6 Plus
                    @"iPhone7,2", // iPhone 6
                    @"iPhone8,4"  // iPhone SE
                ];
                
        if ([olderDevices containsObject:platform])
        {
            return 1;
        }
        
        if ([platform isEqualToString:@"iPhone8,1"] || [platform isEqualToString:@"iPhone8,2"]) // iPhone 6S i iPhone 6S Plus
        {
            return 2;
        }
    
        return 3;
    }
    
    void _TapticEngine(int type)
    {
        switch (type) 
        {
            case 1: // Light
            {
                AudioServicesPlaySystemSound(1519);
                break;
            }
            case 2: // Medium
            {
                AudioServicesPlaySystemSound(1519);
                break;
            }
            case 3: // Heavy
            {
                AudioServicesPlaySystemSound(1520);
                break;
            }
            case 4: // Selection
            {
                AudioServicesPlaySystemSound(1519);
                break;
            }
            case 5: // Success
            {
                AudioServicesPlaySystemSound(1521);
                break;
            }
            case 6: // Warning
            {
                AudioServicesPlaySystemSound(1521);
                break;
            }
            case 7: // Error
            {
                AudioServicesPlaySystemSound(1521);
                break;
            }
            default: // Default
                AudioServicesPlaySystemSound(1519);
                break;
        }
    }
    
    void _Haptic(int type)
    {
        switch (type) 
        {
            case 1: // Light
            {
                UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
                [generator impactOccurred];
                [generator prepare];
                break;
            }
            case 2: // Medium
            {
                UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
                [generator impactOccurred];
                [generator prepare];
                break;
            }
            case 3: // Heavy
            {
                UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
                [generator impactOccurred];
                [generator prepare];
                break;
            }
            case 4: // Selection
            {
                UISelectionFeedbackGenerator *generator = [[UISelectionFeedbackGenerator alloc] init];
                [generator selectionChanged];
                break;
            }
            case 5: // Success
            {
                UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
                [generator notificationOccurred:UINotificationFeedbackTypeSuccess];
                break;
            }
            case 6: // Warning
            {
                UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
                [generator notificationOccurred:UINotificationFeedbackTypeWarning];
                break;
            }
            case 7: // Error
            {
                UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
                [generator notificationOccurred:UINotificationFeedbackTypeError];
                break;
            }
            default: // Default
                UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
                [generator impactOccurred];
                [generator prepare];
                break;
        }
    }
}