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

import UIKit
import AudioToolbox

import Foundation

// 0 - not checked
// 1 - not supported
// 2 - taptic engine supported
// 3 - haptic supported
@_cdecl("_IsHapticSupported")
public func IsHapticSupported() -> Int32 {
    var size: size_t = 0
    sysctlbyname("hw.machine", nil, &size, nil, 0)

    var machine = [CChar](repeating: 0, count: size)
    sysctlbyname("hw.machine", &machine, &size, nil, 0)
    let platform = String(cString: machine)

    if platform.hasPrefix("iPad") || platform.hasPrefix("iPod") {
        return 1
    }

    let olderDevices: Set<String> = [
        "iPhone1,1", // iPhone
        "iPhone1,2", // iPhone 3G
        "iPhone2,1", // iPhone 3GS
        "iPhone3,1", "iPhone3,2", "iPhone3,3", // iPhone 4
        "iPhone4,1", // iPhone 4s
        "iPhone5,1", "iPhone5,2", // iPhone 5
        "iPhone5,3", "iPhone5,4", // iPhone 5c
        "iPhone6,1", "iPhone6,2", // iPhone 5s
        "iPhone7,1", // iPhone 6 Plus
        "iPhone7,2", // iPhone 5
        "iPhone8,4"  // iPhone SE
    ]

    if olderDevices.contains(platform) {
        return 1
    }

    if platform == "iPhone8,1" || platform == "iPhone8,2" {
        return 2 // Taptic only (iPhone 6S / 6S Plus)
    }

    return 3 // Full haptic support
}

@_cdecl("_TapticEngine")
public func TapticEngine(_ type: Int32) {
    let systemSoundID: SystemSoundID

    switch type {
    case 1, 2, 4: systemSoundID = 1519 // Light / Medium / Selection
    case 3: systemSoundID = 1520       // Heavy
    case 5, 6, 7: systemSoundID = 1521 // Success / Warning / Error
    default: systemSoundID = 1519 // Like Light
    }

    AudioServicesPlaySystemSound(systemSoundID)
}

@_cdecl("_Haptic")
public func Haptic(_ type: Int32) {
    DispatchQueue.main.async {
        switch type {
        case 1:
            let generator = UIImpactFeedbackGenerator(style: .light)
            generator.impactOccurred()
            generator.prepare()
        case 2:
            let generator = UIImpactFeedbackGenerator(style: .medium)
            generator.impactOccurred()
            generator.prepare()
        case 3:
            let generator = UIImpactFeedbackGenerator(style: .heavy)
            generator.impactOccurred()
            generator.prepare()
        case 4:
            let generator = UISelectionFeedbackGenerator()
            generator.selectionChanged()
        case 5:
            let generator = UINotificationFeedbackGenerator()
            generator.notificationOccurred(.success)
        case 6:
            let generator = UINotificationFeedbackGenerator()
            generator.notificationOccurred(.warning)
        case 7:
            let generator = UINotificationFeedbackGenerator()
            generator.notificationOccurred(.error)
        default:
            let generator = UIImpactFeedbackGenerator(style: .medium)
            generator.impactOccurred()
            generator.prepare()
        }
    }
}
