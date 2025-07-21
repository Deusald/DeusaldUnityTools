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

import Foundation
import GameKit

public typealias GenerateSucceeded = @convention(c) (
    UnsafePointer<CChar>,  // publicKeyUrl
    UInt64,                // timestamp
    UnsafePointer<CChar>,  // signature
    UnsafePointer<CChar>,  // salt
    UnsafePointer<CChar>,  // gamePlayerId
    UnsafePointer<CChar>,  // teamPlayerId
    UnsafePointer<CChar>,  // displayName
    UnsafePointer<CChar>,  // alias
    UnsafePointer<CChar>   // bundleId
) -> Void

public typealias GenerateFailed = @convention(c) (
    UnsafePointer<CChar> // reason
) -> Void

@_cdecl("_GenerateIdentityVerificationSignature")
public func GenerateIdentityVerificationSignature(
    _ onSucceeded: @escaping GenerateSucceeded,
    _ onFailed: @escaping GenerateFailed
) {
    let player = GKLocalPlayer.local

    player.fetchItems { url, signature, salt, timestamp, error in
        if let error = error {
            let msg = (error.localizedDescription as NSString).utf8String
            onFailed(msg!)
            return
        }

        guard
            let url = url,
            let signature = signature,
            let salt = salt
        else {
            let msg = ("Invalid data returned" as NSString).utf8String
            onFailed(msg!)
            return
        }

        let signatureB64 = signature.base64EncodedString()
        let saltB64 = salt.base64EncodedString()

        let teamPlayerId = player.teamPlayerID
        let gamePlayerId = player.gamePlayerID
        let displayName = player.displayName
        let alias = player.alias
        let bundleId = Bundle.main.bundleIdentifier ?? ""

        onSucceeded(
            (url.absoluteString as NSString).utf8String!,
            timestamp,
            (signatureB64 as NSString).utf8String!,
            (saltB64 as NSString).utf8String!,
            (gamePlayerId as NSString).utf8String!,
            (teamPlayerId as NSString).utf8String!,
            (displayName as NSString).utf8String!,
            (alias as NSString).utf8String!,
            (bundleId as NSString).utf8String!
        )
    }
}

@_cdecl("_AuthenticateGameCenterPlayer")
public func AuthenticateGameCenterPlayer(
    _ onSuccess: @escaping @convention(c) () -> Void,
    _ onFailure: @escaping @convention(c) (UnsafePointer<CChar>) -> Void
) {
    let player = GKLocalPlayer.local
    player.authenticateHandler = { viewController, error in
        if let error = error {
            let msg = (error.localizedDescription as NSString).utf8String
            onFailure(msg!)
            return
        }

        if let vc = viewController {
            // Unity cannot present the VC, so notify the C# side to handle UI fallback
            let msg = ("Game Center UI needs to be presented." as NSString).utf8String
            onFailure(msg!)
            return
        }

        if player.isAuthenticated {
            onSuccess()
        } else {
            let msg = ("Player is not authenticated." as NSString).utf8String
            onFailure(msg!)
        }
    }
}
