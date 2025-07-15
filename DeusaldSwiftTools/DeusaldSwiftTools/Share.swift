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
import UIKit

// MARK: - Static Sharing Functions

fileprivate func presentActivityController(with items: [Any], in viewController: UIViewController) {
    let activityVC = UIActivityViewController(activityItems: items, applicationActivities: nil)

    if let popover = activityVC.popoverPresentationController {
        popover.sourceView = viewController.view
        popover.sourceRect = CGRect(
            x: UIScreen.main.bounds.midX,
            y: UIScreen.main.bounds.midY,
            width: 0,
            height: 0
        )
        popover.permittedArrowDirections = []
    }

    DispatchQueue.main.async {
        viewController.present(activityVC, animated: true, completion: nil)
    }
}

typealias UnityGetVCFunc = @convention(c) () -> UIViewController?

func GetUnityViewController() -> UIViewController? {
    guard let sym = dlsym(UnsafeMutableRawPointer(bitPattern: -2), "GetUnityViewController") else {
        print("⚠️ Unity function not found at runtime.")
        return nil
    }
    
    let fn = unsafeBitCast(sym, to: UnityGetVCFunc.self)
    return fn()
}

// MARK: - C-callable Functions for Unity

@_cdecl("_ShareText")
public func ShareText(_ cText: UnsafePointer<CChar>) {
    let text = String(cString: cText)
    guard let vc = GetUnityViewController() else { return }
    presentActivityController(with: [text], in: vc)
}

@_cdecl("_ShareFile")
public func ShareFile(_ cPaths: UnsafePointer<CChar>, _ cMessage: UnsafePointer<CChar>) {
    let paths = String(cString: cPaths)
    let message = String(cString: cMessage)

    let filePaths = paths.components(separatedBy: "<deusald_sep>")
    var items: [Any] = filePaths.map { URL(fileURLWithPath: $0) }

    if !message.isEmpty {
        items.append(message)
    }

    guard let vc = GetUnityViewController() else { return }
    presentActivityController(with: items, in: vc)
}
