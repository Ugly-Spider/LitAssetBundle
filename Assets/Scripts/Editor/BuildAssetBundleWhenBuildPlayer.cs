using UnityEditor;

namespace LitAssetBundle
{
    [InitializeOnLoad]
    class BuildAssetBundleWhenBuildPlayer
    {
        static BuildAssetBundleWhenBuildPlayer()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(
                new System.Action<BuildPlayerOptions>(buildPlayerOptions =>
                {
                    CheckAndBuildAssetBundle();
                    BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
                }));
        }

        private static void CheckAndBuildAssetBundle()
        {
            var abConfig = AssetBundleBuilder.GetOrCreateConfig();
            if (abConfig.autoRebuildWhenBuildPlayer)
            {
                AssetBundleBuilder.Build(abConfig);
            }
        }
    }
}