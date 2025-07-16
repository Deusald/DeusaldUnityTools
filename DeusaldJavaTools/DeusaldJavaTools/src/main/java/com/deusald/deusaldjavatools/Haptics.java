package com.deusald.deusaldjavatools;

import android.annotation.SuppressLint;
import android.content.Context;
import android.os.Build;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.os.VibratorManager;

import java.util.HashMap;

public class Haptics {

    private static final long LIGHT_DURATION = 20;
    private static final long MEDIUM_DURATION = 40;
    private static final long HEAVY_DURATION = 80;

    private static final int LIGHT_AMPLITUDE = 40;
    private static final int MEDIUM_AMPLITUDE = 120;
    private static final int HEAVY_AMPLITUDE = 255;

    private static final HashMap<String, long[]> patterns = new HashMap<>() {{
        put("Light", new long[]{LIGHT_DURATION});
        put("Medium", new long[]{MEDIUM_DURATION});
        put("Heavy", new long[]{HEAVY_DURATION});
        put("Selection", new long[]{LIGHT_DURATION});
        put("Success", new long[]{0, LIGHT_DURATION, LIGHT_DURATION, HEAVY_DURATION});
        put("Warning", new long[]{0, HEAVY_DURATION, LIGHT_DURATION, MEDIUM_DURATION});
        put("Error", new long[]{0, MEDIUM_DURATION, LIGHT_DURATION, MEDIUM_DURATION, LIGHT_DURATION, HEAVY_DURATION, LIGHT_DURATION, LIGHT_DURATION});
    }};

    private static final HashMap<String, int[]> amplitudes = new HashMap<>() {{
        put("Light", new int[]{LIGHT_AMPLITUDE});
        put("Medium", new int[]{MEDIUM_AMPLITUDE});
        put("Heavy", new int[]{HEAVY_AMPLITUDE});
        put("Selection", new int[]{LIGHT_AMPLITUDE});
        put("Success", new int[]{0, LIGHT_AMPLITUDE, 0, HEAVY_AMPLITUDE});
        put("Warning", new int[]{0, HEAVY_AMPLITUDE, 0, MEDIUM_AMPLITUDE});
        put("Error", new int[]{0, MEDIUM_AMPLITUDE, 0, MEDIUM_AMPLITUDE, 0, HEAVY_AMPLITUDE, 0, LIGHT_AMPLITUDE});
    }};

    private static Vibrator vibrator;

    public static void baseVibrate(String hapticId)
    {
        vibrator.vibrate(patterns.get(hapticId), -1);
    }

    @SuppressLint("NewApi")
    public static void advancedVibrate(String hapticId)
    {
        VibrationEffect effect;
        long[] pattern = patterns.get(hapticId);
        int[] amplitude = amplitudes.get(hapticId);

        assert pattern != null;
        if (pattern.length == 1) {
            assert amplitude != null;
            effect = VibrationEffect.createOneShot(pattern[0], amplitude[0]);
        } else {
            effect = VibrationEffect.createWaveform(pattern, amplitude, -1);
        }
        vibrator.vibrate(effect);
    }

    // 0 - not checked
    // 1 - not supported
    // 2 - vibration class can't be called (Base)
    // 3 - vibration class can be called (Advanced)
    public static int isHapticSupported(Context context) {
        vibrator = getVibrator(context);
        return android.os.Build.VERSION.SDK_INT >= 26 ? 3 : 2;
    }

    private static Vibrator getVibrator(Context context) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) { // API 31
            VibratorManager vibratorManager = (VibratorManager) context.getSystemService(Context.VIBRATOR_MANAGER_SERVICE);
            return vibratorManager.getDefaultVibrator();
        } else {
            return (Vibrator) context.getSystemService(Context.VIBRATOR_SERVICE);
        }
    }
}
