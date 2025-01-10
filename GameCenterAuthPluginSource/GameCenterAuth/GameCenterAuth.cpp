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

#include <GameKit/GameKit.h>

#include "GameCenterAuth.h"

void GenerateIdentityVerificationSignature(GenerateSucceeded OnSucceeded, GenerateFailed OnFailed)
{
    __weak GKLocalPlayer *localPlayer = [GKLocalPlayer localPlayer];
    
    [localPlayer fetchItemsForIdentityVerificationSignature:
         ^(NSURL *publicKeyUrl, NSData *signature, NSData *salt, uint64_t timestamp, NSError *error)
     {
     if (error)
     {
     NSLog(@"ERROR: %@", error);
     OnFailed([[error localizedDescription] UTF8String]);
     }
     else
     {
     NSString *signatureb64 = [signature base64EncodedStringWithOptions:0];
     NSString *saltb64 = [salt base64EncodedStringWithOptions:0];
     NSString *teamPlayerId = localPlayer.teamPlayerID;
     NSString *gamePlayerId = localPlayer.gamePlayerID;
     NSString *displayName = localPlayer.displayName;
     NSString *alias = localPlayer.alias;
     NSString *bundleId = [[NSBundle mainBundle] bundleIdentifier];
     
     OnSucceeded(
                 [[publicKeyUrl absoluteString] UTF8String],
                 timestamp,
                 [signatureb64 UTF8String],
                 [saltb64 UTF8String],
                 [gamePlayerId UTF8String],
                 [teamPlayerId UTF8String],
                 [displayName UTF8String],
                 [alias UTF8String],
                 [bundleId UTF8String]
                 );
     }
     }
    ];
}
