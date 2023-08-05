#if UNITY_EDITOR // Defined in Runtime Assembly for accessibility from generated classes
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LizardOfTheCoast.ProjectSettings
{
    public static class SettingsProviderHelper
    {
        public static SettingsProvider Create<T>() where T : ScriptableObject
        {
            var settings = SettingsLoader.LoadSettings<T>();
            var editor = Editor.CreateEditor(settings);
            var attribute = typeof(T).GetCustomAttribute<ProjectSettingsAttribute>();
            var menuPath = attribute.SettingsPath ?? typeof(T).Name;
            var label = Path.GetFileName(menuPath);
            var scopes = attribute.SettingsScope;
            var keywords = attribute.SettingsKeywords.Length > 0
                ? attribute.SettingsKeywords
                : new[] {typeof(T).Name};

            void EnsureEditor()
            {
                if (settings == null)
                    settings = SettingsLoader.LoadSettings<T>();
                if (editor == null || editor.target != settings)
                    editor = Editor.CreateEditor(settings);
            }

            void OnChange()
            {
                editor.serializedObject.ApplyModifiedProperties();
                SettingsLoader.SaveSettings(settings);
            }

            var provider = new SettingsProvider(menuPath, scopes)
            {
                label = label,
                guiHandler = (searchContext) =>
                {
                    EnsureEditor();

                    using var changeCheck = new EditorGUI.ChangeCheckScope();

                    editor.OnInspectorGUI();

                    if (changeCheck.changed)
                    {
                        OnChange();
                    }
                },

                deactivateHandler = OnChange,

                keywords = new HashSet<string>(keywords)
            };

            return provider;
        }
    }
}
#endif // UNITY_EDITOR