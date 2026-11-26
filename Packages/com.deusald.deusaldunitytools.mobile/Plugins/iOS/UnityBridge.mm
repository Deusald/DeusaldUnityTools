#import <UIKit/UIKit.h>
#import "UnityInterface.h"

__attribute__((used))
__attribute__((visibility("default")))
extern "C" UIViewController* GetUnityViewController() {
    return UnityGetGLViewController();
}