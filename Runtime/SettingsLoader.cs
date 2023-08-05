using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LizardOfTheCoast.ProjectSettings
{
    public static class SettingsLoader
    {
        public static Func<string, ScriptableObject> LoadFromProjectSettingsFunc = default;
        public static Action<ScriptableObject> SaveToProjectSettingsAction = default;
        public static Action<string, ScriptableObject> RegisterFileAction = default;

        private static T GetOrCreateSettings<T>(string path) where T : ScriptableObject
        {
            var settings = LoadFromProjectSettingsFunc?.Invoke(path) as T;
            if (settings == null)
            {
                settings = Resources.Load<T>(path);

                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<T>();
                }
            }

            RegisterFileAction?.Invoke(path, settings);

            return settings;
        }

        public static T LoadSettings<T>() where T : ScriptableObject
        {
            var projectSettingsAttribute = typeof(T).GetCustomAttributes<ProjectSettingsAttribute>().FirstOrDefault();
            Debug.Assert(projectSettingsAttribute != null);
            return GetOrCreateSettings<T>(projectSettingsAttribute.FilePath);
        }

        public static ScriptableObject LoadSettings(string path)
        {
            return GetOrCreateSettings<ScriptableObject>(path);
        }

        public static void SaveSettings<T>(T settings) where T : ScriptableObject
        {
            SaveToProjectSettingsAction?.Invoke(settings);
        }
    }
}