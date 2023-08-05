using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace LizardOfTheCoast.ProjectSettings
{
    public class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => -200;

        public void OnPreprocessBuild(BuildReport report)
        {
            SettingsLoaderResources.CreateResources();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            SettingsLoaderResources.DestroyResources();
        }
    }
}