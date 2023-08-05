using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LizardOfTheCoast.ProjectSettings
{
    public static class SettingsLoaderProjectSettings
    {
        private static readonly Dictionary<string, ScriptableObject> OpenSettings = new();

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            RegisterFunctions();
            SaveSettingsOnExit();
        }

        internal static ScriptableObject LoadFromProjectSettings(string path)
        {
            var projectSettingsPath = ResourcesPathToProjectSettingsPath(path);
            if (OpenSettings.TryGetValue(projectSettingsPath, out var settings))
            {
                if (settings != null)
                {
                    return settings;
                }

                OpenSettings.Remove(projectSettingsPath);
            }

            settings =
                InternalEditorUtility.LoadSerializedFileAndForget(projectSettingsPath).FirstOrDefault() as
                    ScriptableObject;
            OpenSettings.Add(projectSettingsPath, settings);

            return settings;
        }

        private static void RegisterFunctions()
        {
            SettingsLoader.LoadFromProjectSettingsFunc = LoadFromProjectSettings;

            SettingsLoader.SaveToProjectSettingsAction = settings =>
            {
                var path = OpenSettings.First(x => x.Value == settings).Key;
                SaveSettings(path, settings);
            };

            SettingsLoader.RegisterFileAction = (path, settings) =>
            {
                var projectSettingsPath = ResourcesPathToProjectSettingsPath(path);
                OpenSettings.Remove(projectSettingsPath);
                OpenSettings.Add(projectSettingsPath, settings);
            };
        }

        private static void SaveSettingsOnExit()
        {
            EditorApplication.quitting += SaveSettings;
        }

        private static void SaveSettings()
        {
            foreach (var (path, settings) in OpenSettings)
            {
                SaveSettings(path, settings);
            }
        }

        private static void SaveSettings(string path, Object settings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] {settings}, path,
                allowTextSerialization: true);
        }

        public static string ResourcesPathToProjectSettingsPath(string resourcesPath) =>
            $"ProjectSettings/{resourcesPath}.asset";
    }
}