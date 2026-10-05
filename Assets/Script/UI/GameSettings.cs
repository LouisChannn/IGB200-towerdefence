using UnityEngine;

public static class GameSettings
{
    private const string VolumeKey = "Settings.MasterVolume";
    private const string FullscreenKey = "Settings.Fullscreen";
    private static bool initialized;

    public static float MasterVolume { get; private set; } = 1f;
    public static bool Fullscreen { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInitialization()
    {
        initialized = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));
        Fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) != 0;
        AudioListener.volume = MasterVolume;
        ApplyFullscreen();
    }

    public static void SetVolume(float volume)
    {
        Initialize();
        MasterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = MasterVolume;
        PlayerPrefs.SetFloat(VolumeKey, MasterVolume);
    }

    public static void SetFullscreen(bool fullscreen)
    {
        Initialize();
        Fullscreen = fullscreen;
        ApplyFullscreen();
        PlayerPrefs.SetInt(FullscreenKey, Fullscreen ? 1 : 0);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    private static void ApplyFullscreen()
    {
        // The Editor cannot switch the player's desktop window mode.
        if (!Application.isEditor)
        {
            Screen.fullScreenMode = Fullscreen
                ? FullScreenMode.FullScreenWindow
                : FullScreenMode.Windowed;
        }
    }
}
