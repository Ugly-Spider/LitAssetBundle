#if UNITY_EDITOR
using Object = UnityEngine.Object;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LitAssetBundle
{
    
    public static class AssetBundleBuilder
    {
        
        private const string AB_CONFIG_PATH = "Assets/Resources";
        public const string ASSET_STREAMING_PATH = "Assets/StreamingAssets/";
        private const string DEFAULT_OUTPUT_PATH = "AssetBundle";


        [MenuItem("Window/LitAssetBundle/Open Build Windows")]
        private static void ShowWindows_Menu()
        {
            BuildAssetBundleWindows.ShowWindowsFromSelection();
        }
        
        [MenuItem("Assets/LitAssetBundle/Open Build Windows")]
        private static void ShowWindows_Assets()
        {
            BuildAssetBundleWindows.ShowWindowsFromSelection();
        }

        [MenuItem("Assets/LitAssetBundle/Clear Marks")]
        private static void ClearMark()
        {
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);
                
                if (Directory.Exists(path)) continue;

                var importer = AssetImporter.GetAtPath(path);
                importer.assetBundleName = null;
            }

            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        [MenuItem("Assets/LitAssetBundle/Mark As File(Name)")]
        private static void MarkAsFile_Name()
        {
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);
                
                if (Directory.Exists(path)) continue;

                var importer = AssetImporter.GetAtPath(path);
                var arr = asset.name.Split('.');
                importer.assetBundleName = arr[0];
            }
        }

        [MenuItem("Assets/LitAssetBundle/Mark As File(Path)")]
        private static void MarkAsFile_Path()
        {
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);

                if (Directory.Exists(path)) continue;

                var importer = AssetImporter.GetAtPath(path);
                var arr = path.Split('.');
                importer.assetBundleName = arr[0];
            }
        }

        [MenuItem("Assets/LitAssetBundle/Mark As Package(Name)")]
        private static void MarkAsPackage_Name()
        {
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);

                if (Directory.Exists(path))
                {
                    var name = asset.name;

                    var assetsInFolder = UnityPathFileUtil.GetAllFiles(path);
                    foreach (var v in assetsInFolder)
                    {
                        var importer = AssetImporter.GetAtPath(v);
                        importer.assetBundleName = name;
                    }
                }
            }
        }
        
        [MenuItem("Assets/LitAssetBundle/Mark As Package(Path)")]
        private static void MarkAsPackage_Path()
        {
            var assets = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            for (var i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                var path = AssetDatabase.GetAssetPath(asset);

                if (Directory.Exists(path))
                {
                    var name = path;

                    var assetsInFolder = UnityPathFileUtil.GetAllFiles(path);
                    foreach (var v in assetsInFolder)
                    {
                        var importer = AssetImporter.GetAtPath(v);
                        importer.assetBundleName = name;
                    }
                }
            }
        }

        public static void Build(AssetBundleConfig abConfig)
        {
            if (!Directory.Exists(abConfig.outputPath))
            {
                Directory.CreateDirectory(abConfig.outputPath);
            }
            
//            CheckAndClearOldRepeatAssets();
            ClearUselessAssetBundleFiles(abConfig.outputPath);
            
            var assets = new List<string>();
            foreach (var abName in AssetDatabase.GetAllAssetBundleNames())
            {
                foreach (var assetPath in AssetDatabase.GetAssetPathsFromAssetBundle(abName))
                {
                    assets.Add(assetPath);
                }
            }
            
            AddAssetBundleNameForRepeatDependencies(assets);
            InternalBuildAssetBundle(abConfig, null);
        }
        
        public static AssetBundleConfig GetOrCreateConfig()
        {
            var abConfigPath = AB_CONFIG_PATH;
            var abConfigFullPath = Path.Combine(abConfigPath, nameof(AssetBundleConfig));
            var abConfig = AssetDatabase.LoadAssetAtPath<AssetBundleConfig>(abConfigFullPath + ".asset");

            if (abConfig == null)
            {
                abConfig = ScriptableObject.CreateInstance<AssetBundleConfig>();
                abConfig.rootPath = DEFAULT_OUTPUT_PATH;
                if (!AssetDatabase.IsValidFolder(abConfigPath))
                {
                    AssetDatabase.CreateFolder("Assets", abConfigPath.Substring(abConfigPath.IndexOf('/') + 1));
                }

                AssetDatabase.CreateAsset(abConfig, abConfigFullPath + ".asset");
                AssetDatabase.SaveAssets();
            }
            
            return abConfig;
        }

        public static void ClearUselessAssetBundleFiles(string path)
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();

            bool IsAssetBundleFile(string s)
            {
                if (s.Contains('.')) return false;
                var ab = AssetDatabase.LoadAssetAtPath<AssetBundle>(s);
                return ab == null;
            }

            var list = AssetDatabase.GetAllAssetBundleNames().Select(x => path + "/" + x).ToList();
            var abFiles = UnityPathFileUtil.GetAllFilesInProject(path);
            var assetBundleName = path + "/AssetBundle";

            foreach (var v in abFiles)
            {
                if (v == assetBundleName) continue;
                if (!IsAssetBundleFile(v)) continue;

                if (!list.Contains(v))
                {
                    var _path = Application.dataPath.Replace("Assets", "") + v;
                    if (File.Exists(_path)) File.Delete(_path);
                    var _manifest = _path + ".manifest";
                    if (File.Exists(_manifest)) File.Delete(_manifest);
                }
            }
            
            AssetDatabase.Refresh();
        }

        private static void SetAssetBundleName(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath);

            if (ReferenceEquals(importer, null)) return;

            var pointIndex = assetPath.LastIndexOf('.');
            if (pointIndex == -1)
            {
                throw new Exception("Unexpected file path:" + assetPath);
            }

            importer.assetBundleName = assetPath.Substring(0, pointIndex);
        }

        //检查需要打assetbundle的资源，如果有多个assetbundle引用同一个资源，则将改资源也标记为一个单独的assetbundle
        private static void AddAssetBundleNameForRepeatDependencies(List<string> assets)
        {
            if (assets != null && assets.Count != 0)
            {
                var dict = new Dictionary<string, int>();
                foreach (var asset in assets)
                {
                    var deps = AssetDatabase.GetDependencies(asset);
                    foreach (var dep in deps)
                    {
                        if (dict.ContainsKey(dep))
                        {
                            dict[dep]++;
                        }
                        else
                        {
                            dict.Add(dep, 1);
                        }
                    }
                }

                var repeatAssets = new List<string>();
                foreach (var v in dict)
                {
                    if (v.Value > 1)
                    {
                        SetAssetBundleName(v.Key);
                        repeatAssets.Add(v.Key);
                    }
                }

//                SaveRepeatAssets(repeatAssets);
            }
        }

//        private const char SEPERATOR = ',';

//        private static void SaveRepeatAssets(List<string> assets)
//        {
//            var sb = new StringBuilder(assets.Count * 10);
//            bool first = true;
//            foreach (var asset in assets)
//            {
//                if (!first)
//                {
//                    sb.Append(SEPERATOR);
//                }
//                else
//                {
//                    first = false;
//                }
//
//                sb.Append(asset);
//            }
//
//            var key = $"{UnityPathFileUtil.GetProjectName()}_TAB_REPEAT_ASSETS";
//            EditorPrefs.SetString(key, sb.ToString());
//        }
//
//        private static void CheckAndClearOldRepeatAssets()
//        {
//            var key = $"{UnityPathFileUtil.GetProjectName()}_TAB_REPEAT_ASSETS";
//            var value = EditorPrefs.GetString(key, String.Empty);
//
//            if (String.IsNullOrEmpty(value)) return;
//
//            var assets = value.Split(SEPERATOR);
//            foreach (var asset in assets)
//            {
//                var importer = AssetImporter.GetAtPath(asset);
//                if (importer != null)
//                {
//                    importer.assetBundleName = String.Empty;
//                }
//            }
//
//            EditorPrefs.SetString(key, String.Empty);
//        }

        private static void InternalBuildAssetBundle(AssetBundleConfig abConfig, AssetBundleBuild[] builds)
        {
            if (builds == null)
            {
                BuildPipeline.BuildAssetBundles(abConfig.outputPath, abConfig.buildAssetBundleOptions,
                    EditorUserBuildSettings.activeBuildTarget);
            }
            else
            {
                BuildPipeline.BuildAssetBundles(abConfig.outputPath, builds, abConfig.buildAssetBundleOptions,
                    EditorUserBuildSettings.activeBuildTarget);
            }


            AssetDatabase.Refresh();
        }
    }
}
#endif