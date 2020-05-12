using System;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LitAssetBundle
{

    public class AssetBundleAgent : IAssetBundleAgent
    {
        private readonly AssetBundle _bundle;
        
        public AssetBundleAgent(AssetBundle bundle, string abPath)
        {
            _bundle = bundle ?? throw new ArgumentNullException();
            _abPath = abPath;
        }

        private readonly string _abPath;
        public string abPath => _abPath;

        public IReadOnlyList<string> GetAllAssetNames()
        {
            return _bundle.GetAllAssetNames();
        }

        public IReadOnlyList<string> GetAllScenePaths()
        {
            return _bundle.GetAllScenePaths();
        }

        public T LoadAsset<T>(string name) where T : Object
        {
            return (T)_bundle.LoadAsset(name, typeof(T));
        }

        public T LoadAssetAndInstantiate<T>(string name) where T : Object
        {
            var asset = LoadAsset<T>(name);
            return ReferenceEquals(asset, null) ? null : Object.Instantiate(asset);
        }

        public Object LoadAsset(string name, Type t)
        {
            return _bundle.LoadAsset(name, t);
        }

        public async UniTask<T> LoadAssetAsync<T>(string name) where T : Object
        {
            var req = _bundle.LoadAssetAsync(name, typeof(T));
            await UniTask.WaitUntil(() => req.isDone);
            return (T)req.asset;
        }

        public async UniTask<T> LoadAssetAndInstantiateAsync<T>(string name) where T : Object
        {
            var req = _bundle.LoadAssetAsync(name, typeof(T));
            await UniTask.WaitUntil(() => req.isDone);
            var asset = (T)req.asset;
            return ReferenceEquals(asset, null) ? null : Object.Instantiate(asset);
        }

        public void Unload()
        {
            _bundle.Unload(true);
        }

        public async UniTask<Object> LoadAssetAsync(string name, Type t)
        {
            var req = _bundle.LoadAssetAsync(name, t);
            await UniTask.WaitUntil(() => req.isDone);
            return req.asset;
        }
    }

}
