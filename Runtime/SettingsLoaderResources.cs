#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace LizardOfTheCoast.ProjectSettings
{
    public static class SettingsLoaderResources
    {
        [MenuItem("Assets/ProjectSettings/CreateResources")]
        public static void CreateResources()
        {
            foreach (var settingsType in TypeCache.GetTypesWithAttribute<ProjectSettingsAttribute>())
            {
                var projectSettingsAttribute =
                    settingsType.GetCustomAttributes<ProjectSettingsAttribute>().First();

                var projectSettingsPath = projectSettingsAttribute.FilePath;
                if (!File.Exists(projectSettingsPath))
                    continue;

                var resourcesPath = $"Assets/Settings/Resources/{projectSettingsAttribute.FilePath}.asset";
                File.Copy(projectSettingsPath, resourcesPath, overwrite: true);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/ProjectSettings/DestroyResources")]
        public static void DestroyResources()
        {
            foreach (var settingsType in TypeCache.GetTypesWithAttribute<ProjectSettingsAttribute>())
            {
                var projectSettingsAttribute =
                    settingsType.GetCustomAttributes<ProjectSettingsAttribute>().First();
                var resourcesPath = $"Assets/Settings/Resources/{projectSettingsAttribute.FilePath}.asset";
                AssetDatabase.DeleteAsset(resourcesPath);
            }

            const string resourcesDirectory = "Assets/Settings/Resources";
            DeleteEmptyDirectory("Assets/Settings");
            if (!Directory.Exists(resourcesDirectory))
            {
                File.Delete($"{resourcesDirectory}.meta");
            }

            AssetDatabase.Refresh();
        }

        private static void DeleteEmptyDirectory(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                DeleteEmptyDirectory(directory);
                if (Directory.GetFiles(directory).All(f => f.EndsWith(".meta")) &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, true);
                    File.Delete($"{directory}.meta");
                }
            }
        }
    }
}
#endif // UNITY_EDITOR
