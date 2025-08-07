using UnityEngine;
using ModLoader;
using ModLoader.IO;
using SFS.IO;
using System;
using Object = UnityEngine.Object;
using Console = ModLoader.IO.Console;

namespace FrameRateLockMod
{
    public class Main : Mod
    {
        public static Main main;

        public override string ModNameID => "FrameRateLockMod";
        public override string DisplayName => "Frame Rate Lock";
        public override string Author => "DarkSpace";
        public override string MinimumGameVersionNecessary => "1.5.9.8";
        public override string ModVersion => "v1.0.1";
        public override string Description => "Locks the application frame rate at configured FPS and sets physics fixed time step accordingly.";

        private GameObject _enforcer;

        public override void Early_Load()
        {
            Main.main = this;
        }

        public override void Load()
        {
            // Initialize settings
            Settings.Init(new FolderPath(this.ModFolder).ExtendToFile("settings.txt"));

            // Create frame rate enforcer
            _enforcer = new GameObject("FrameRateEnforcer");
            Object.DontDestroyOnLoad(_enforcer);
            _enforcer.AddComponent<FrameRateEnforcer>();

            ModLoader.IO.Console.main.WriteText("[FrameRateLockMod] Initialized frame rate lock.");
        }
    }

    public static class Settings
    {
        private static int _targetFPS = 60;
        private static FilePath _settingsFile;

        public static int TargetFPS => _targetFPS;
        public static float FixedDelta => 1f / _targetFPS;

        public static void Init(FilePath settingsFile)
        {
            _settingsFile = settingsFile;
            LoadSettings();
        }

        private static void LoadSettings()
        {
            try
            {
                if (!_settingsFile.FileExists())
                {
                    // Create default settings
                    _settingsFile.WriteText("60");
                    return;
                }

                string text = _settingsFile.ReadText();
                if (int.TryParse(text, out int fps))
                {
                    _targetFPS = Mathf.Clamp(fps, 30, 360);
                }
            }
            catch (Exception ex)
            {
                Console.main.WriteText($"[FrameRateLockMod] Settings error: {ex.Message}");
            }
        }
    }

    public class FrameRateEnforcer : MonoBehaviour
    {
        void Update()
        {
            ApplySettings();
        }

        private void ApplySettings()
        {
            if (Application.targetFrameRate != Settings.TargetFPS)
                Application.targetFrameRate = Settings.TargetFPS;

            if (QualitySettings.vSyncCount != 1)
                QualitySettings.vSyncCount = 1;

            if (Mathf.Abs(Time.fixedDeltaTime - Settings.FixedDelta) > 0.00001f)
                Time.fixedDeltaTime = Settings.FixedDelta;
        }
    }
}