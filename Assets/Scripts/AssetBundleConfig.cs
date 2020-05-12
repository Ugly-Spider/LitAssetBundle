using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LitAssetBundle
{
    [CreateAssetMenu(fileName = "AssetBundleConfig", menuName = "AssetBundle/AssetBundleConfig")]
    public class AssetBundleConfig : ScriptableObject
    {
        public string rootPath;

#if UNITY_EDITOR
        public string outputPath => "Assets/StreamingAssets/" + rootPath;
        [HideInInspector] public bool autoRebuildWhenBuildPlayer;
        [HideInInspector] public BuildAssetBundleOptions buildAssetBundleOptions;
#endif
    }
}