#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LitAssetBundle
{
    public static class UnityPathFileUtil
    {
        public enum PathReturnType
        {
            AssetsPath,
            FullPath,
            OnlyName,
        }

        public static string GetProjectName()
        {
            var array = Application.dataPath.Split('/');
            return array[array.Length - 2];
        }
        
        public static string[] GetAllFiles(string path, bool includeMetaFile = false,
            PathReturnType pathReturnType = PathReturnType.AssetsPath)
        {
            List<string> result = new List<string>();
            GetFilesRecursive(path, result, includeMetaFile, pathReturnType);
            return result.ToArray();
        }

        //"Assets/Scenes"
        public static string[] GetAllFilesInProject(string assetPath,
            PathReturnType pathReturnType = PathReturnType.AssetsPath)
        {
            List<string> result = new List<string>();
            string path = Application.dataPath.Replace("Assets", "") + assetPath;
            GetFilesRecursive(path, result, false, pathReturnType);
            return result.ToArray();
        }

        private static void GetFilesRecursive(string path, List<string> result, bool includeMetaFile,
            PathReturnType pathReturnType)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles();
            DirectoryInfo[] dirs = info.GetDirectories();
            foreach (var v in files)
            {
                if (!includeMetaFile && v.Name.EndsWith(".meta"))
                {
                    continue;
                }

                if (pathReturnType == PathReturnType.OnlyName)
                {
                    result.Add(v.Name);
                }
                else if (pathReturnType == PathReturnType.FullPath)
                {
                    result.Add(v.FullName);
                }
                else
                {
                    string assetPath;
#if UNITY_EDITOR_OSX
                    assetPath = v.FullName.Replace(Application.dataPath, "Assets");
#else
                assetPath = v.FullName.Replace(Application.dataPath.Replace("/", "\\"), "Assets");
#endif
                    result.Add(assetPath);
                }
            }

            foreach (var dir in dirs)
            {
                GetFilesRecursive(dir.FullName, result, includeMetaFile, pathReturnType);
            }
        }

        public static string AssetPathToPath(string assetPath)
        {
            return Application.dataPath + assetPath.Substring(6);
        }

        public static string PathToAssetPath(string path)
        {
            return path.Replace(Application.dataPath, "Assets");
        }

        public static string GetFileNameByPath(string path)
        {
            if (String.IsNullOrEmpty(path)) throw new ArgumentNullException();
            
            string[] array = path.Split('/');
            if (array == null || array.Length == 0)
            {
                array = path.Split('\\');
            }

            return array[array.Length - 1];
        }

        public static void ClearFolder(string path)
        {
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                Directory.Delete(dir, true);
            }

            string[] files = Directory.GetFiles(path);
            foreach (var f in files)
            {
                File.Delete(f);
            }
        }
    }
}
#endif