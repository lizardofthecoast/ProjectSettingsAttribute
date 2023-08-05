using System;
using UnityEditor;

namespace LizardOfTheCoast.ProjectSettings
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ProjectSettingsAttribute : Attribute
    {
        public enum SettingsProviderMode { DefaultProvider, CustomProvider };

        public SettingsProviderMode ProviderMode = SettingsProviderMode.DefaultProvider;
        public string FilePath = default;
        public string SettingsPath = default;
#if UNITY_EDITOR
        public SettingsScope SettingsScope = SettingsScope.Project;
#endif
        public string[] SettingsKeywords = Array.Empty<string>();
    }
}