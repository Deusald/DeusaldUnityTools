package com.deusald.deusaldjavatools;

import android.app.Activity;
import android.graphics.Rect;
import android.view.View;
import android.view.Window;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.util.Log;

public class Tools {

    public static int getKeyboardHeight(Activity activity) {

        Window window = activity.getWindow();
        View decorView = window.getDecorView();
        Rect rect = new Rect();
        decorView.getWindowVisibleDisplayFrame(rect);
        int visibleHeight = rect.height();

        View rootView = decorView.getRootView();
        int screenHeight = rootView.getHeight();

        int keyboardHeight = screenHeight - visibleHeight;

        if (keyboardHeight > screenHeight / 5) {
            return keyboardHeight;
        }

        return 0;
    }

    public static void launchOrOpenPlayStore(Context context, String packageName) {
        try {
            PackageManager packageManager = context.getPackageManager();
            Intent launchIntent = packageManager.getLaunchIntentForPackage(packageName);

            if (launchIntent != null) {
                // App is installed
                context.startActivity(launchIntent);
            } else {
                // App is not installed, open Play Store
                tryOpenPlayStorePage(context, packageName);
            }
        } catch (Exception e) {
            Log.e("AppLauncher", "Error launching app or Play Store", e);
        }
    }

    public static void tryOpenPlayStorePage(Context context, String packageName) {
        String marketUri = "market://details?id=" + packageName;
        String webUri = "https://play.google.com/store/apps/details?id=" + packageName;

        Intent marketIntent = new Intent(Intent.ACTION_VIEW, Uri.parse(marketUri));
        PackageManager packageManager = context.getPackageManager();

        if (marketIntent.resolveActivity(packageManager) != null) {
            context.startActivity(marketIntent);
        } else {
            Intent webIntent = new Intent(Intent.ACTION_VIEW, Uri.parse(webUri));
            context.startActivity(webIntent);
        }
    }

    public static void openAppSettings(Context context) {
        String packageName = context.getPackageName();

        Intent intent = new Intent(android.provider.Settings.ACTION_APPLICATION_DETAILS_SETTINGS);
        Uri uri = Uri.fromParts("package", packageName, null);
        intent.setData(uri);

        context.startActivity(intent);
    }
}
