package com.deusald.deusaldjavatools;

import android.app.Activity;
import android.app.Fragment;
import android.content.Intent;
import android.net.Uri;
import androidx.core.content.FileProvider;

import java.io.File;
import java.net.URLConnection;
import java.util.ArrayList;

public class Share extends Fragment {

    private static final String TAG = "DeusaldShareFragment";
    private static final int SHARE_REQUEST_CODE = 1000;

    private String providerName;

    public void init(Activity activity, String providerName) {
        this.providerName = providerName;
        activity.getFragmentManager().beginTransaction().add(this, TAG).commit();
    }

    public void shareText(String message) {
        Intent shareIntent = new Intent(Intent.ACTION_SEND);
        shareIntent.setType("text/plain");
        shareIntent.putExtra(Intent.EXTRA_TEXT, message);
        startActivityForResult(Intent.createChooser(shareIntent, "Share via"), SHARE_REQUEST_CODE);
    }

    public void shareSingleFile(Activity activity, String filePath, String message) {
        File file = new File(filePath);
        Uri uri = FileProvider.getUriForFile(activity, providerName, file);

        String mimeType = URLConnection.guessContentTypeFromName(file.getName());
        if (mimeType == null) mimeType = "file/*";

        Intent shareIntent = new Intent(Intent.ACTION_SEND);
        shareIntent.setType(mimeType);
        shareIntent.putExtra(Intent.EXTRA_TEXT, message);
        shareIntent.putExtra(Intent.EXTRA_STREAM, uri);
        shareIntent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);

        startActivityForResult(Intent.createChooser(shareIntent, "Share via"), SHARE_REQUEST_CODE);
    }

    public void shareMultipleFiles(Activity activity, String[] filePaths, String message) {
        ArrayList<Uri> uris = new ArrayList<>();
        String mimeType = "*/*";

        for (String path : filePaths) {
            File file = new File(path);
            Uri uri = FileProvider.getUriForFile(activity, providerName, file);
            uris.add(uri);

            if (mimeType.equals("*/*")) {
                String guessed = URLConnection.guessContentTypeFromName(file.getName());
                if (guessed != null) {
                    mimeType = guessed;
                }
            }
        }

        Intent shareIntent = new Intent(Intent.ACTION_SEND_MULTIPLE);
        shareIntent.setType(mimeType);
        shareIntent.putExtra(Intent.EXTRA_TEXT, message);
        shareIntent.putParcelableArrayListExtra(Intent.EXTRA_STREAM, uris);
        shareIntent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);

        startActivityForResult(Intent.createChooser(shareIntent, "Share via"), SHARE_REQUEST_CODE);
    }
}
