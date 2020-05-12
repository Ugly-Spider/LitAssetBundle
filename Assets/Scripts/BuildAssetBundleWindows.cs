#if UNITY_EDITOR
using System.Linq;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LitAssetBundle
{
    public class BuildAssetBundleWindows : EditorWindow
    {
        private AssetBundleConfig _abConfig;

        public static void ShowWindowsFromSelection()
        {
            var windows = EditorWindow.GetWindow<BuildAssetBundleWindows>();
            windows.Initial();
            windows.Show();
        }

        private void Initial()
        {
            _abConfig = AssetBundleBuilder.GetOrCreateConfig();
        }

        void OnGUI()
        {
            if (ReferenceEquals(_abConfig, null))
            {
                _abConfig = AssetBundleBuilder.GetOrCreateConfig();
            }

            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.Label("Path");
                GUILayout.BeginHorizontal();
                GUILayout.Label(AssetBundleBuilder.ASSET_STREAMING_PATH, GUILayout.Width(145));
                _abConfig.rootPath = GUILayout.TextField(_abConfig.rootPath);
                if (GUILayout.Button("Select", GUILayout.Width(45)))
                {
                    string path = EditorUtility.OpenFolderPanel("Output path", _abConfig.outputPath, "");
                    if (!String.IsNullOrEmpty(path))
                    {
                        if (path.Contains(AssetBundleBuilder.ASSET_STREAMING_PATH))
                        {
                            _abConfig.rootPath = path.Replace(Application.dataPath, "Assets")
                                .Replace(AssetBundleBuilder.ASSET_STREAMING_PATH, "");
                        }
                        else
                        {
                            Debug.LogError("Invalid path, please select path in StreamingAssets.");
                        }
                    }
                }

                GUILayout.EndHorizontal();

                GUILayout.Label("BuildAssetBundleOption:");
                _abConfig.buildAssetBundleOptions =
                    (BuildAssetBundleOptions) EditorGUILayout.EnumPopup(_abConfig.buildAssetBundleOptions);

                GUILayout.BeginHorizontal();
                _abConfig.autoRebuildWhenBuildPlayer = GUILayout.Toggle(_abConfig.autoRebuildWhenBuildPlayer,
                    "autoRebuildWhenBuildPlayer");
                GUILayout.EndHorizontal();

                if (cc.changed)
                {
                    EditorUtility.SetDirty(_abConfig);
                    AssetDatabase.SaveAssets();
                }
            }

            bool empty = true;
            if (Directory.Exists(_abConfig.outputPath))
            {
                empty = !Directory.EnumerateFileSystemEntries(_abConfig.outputPath).Any();
            }
            EditorGUI.BeginDisabledGroup(empty);
            if (GUILayout.Button("Clear All"))
            {
                bool clearFolder = EditorUtility.DisplayDialog("Alert",
                    $"Delete all file in folder:{UnityPathFileUtil.AssetPathToPath(_abConfig.outputPath)}?", "Yes", "No");
                if (clearFolder)
                {
                    UnityPathFileUtil.ClearFolder(_abConfig.outputPath);
                    AssetDatabase.Refresh();
                }
            }

            if (GUILayout.Button("Clear Useless"))
            {
                AssetBundleBuilder.ClearUselessAssetBundleFiles(_abConfig.outputPath);
            }
            
            EditorGUI.EndDisabledGroup();
            
            if (GUILayout.Button("Build"))
            {
                AssetBundleBuilder.Build(_abConfig);
            }
        }
    }
}
#endif