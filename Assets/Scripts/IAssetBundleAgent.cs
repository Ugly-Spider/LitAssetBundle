using System;
using System.Collections.Generic;
using UniRx.Async;

namespace LitAssetBundle
{
    
    public interface IAssetBundleAgent
    {
        string abPath { get; }
        
        IReadOnlyList<string> GetAllAssetNames();

        IReadOnlyList<string> GetAllScenePaths();

        T LoadAsset<T>(string name) where T : UnityEngine.Object;

        T LoadAssetAndInstantiate<T>(string name) where T : UnityEngine.Object;

        UniTask<T> LoadAssetAsync<T>(string name) where T : UnityEngine.Object;
        
        UniTask<T> LoadAssetAndInstantiateAsync<T>(string name) where T : UnityEngine.Object;

        void Unload();
    }


}

