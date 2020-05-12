using System;
using System.Collections.Generic;
using System.IO;
using UniRx.Async;
using UnityEngine;
using AssetBundle = UnityEngine.AssetBundle;

namespace LitAssetBundle
{

    public class AssetBundleLoader : MonoBehaviour
    {
        private const string MANIFEST_NAME = nameof(AssetBundleManifest);

        private static AssetBundleLoader _Instance;

        public static AssetBundleLoader Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<AssetBundleLoader>();
                    if (_Instance == null)
                    {
                        var go = new GameObject($"[{nameof(AssetBundleLoader)}]");
                        DontDestroyOnLoad(go);
                        _Instance = go.AddComponent<AssetBundleLoader>();
                    }
                    
                    _Instance.Initialize();
                }

                return _Instance;
            }
        }

        private AssetBundleManifest _abManifest;

        private readonly Dictionary<string, (IAssetBundleAgent, int)> _bundleDict =
            new Dictionary<string, (IAssetBundleAgent, int)>();
        
        private readonly Dictionary<string, Action> _loadTaskDict = new Dictionary<string, Action>();

        private string _assetPath;

        #region public
        public T Load<T>(string abName) where T : UnityEngine.Object
        {
            var agent = Load(abName);
            return agent.LoadAsset<T>(abName);
        }

        public T LoadAndIns<T>(string abName) where T : UnityEngine.Object
        {
            var agent = Load(abName);
            return agent.LoadAssetAndInstantiate<T>(abName);
        }
        
        public async UniTask<T> LoadAsync<T>(string abName) where T : UnityEngine.Object
        {
            var agent = await LoadAsync(abName);
            return await agent.LoadAssetAsync<T>(abName);
        }

        public async UniTask<T> LoadAndInsAsync<T>(string abName) where T : UnityEngine.Object
        {
            var agent = await LoadAsync(abName);
            return await agent.LoadAssetAndInstantiateAsync<T>(abName);
        }

        public IAssetBundleAgent Load(string abName)
        {
            var abPath = AbNameToAbPath(abName);
            var deps = _abManifest.GetAllDependencies(abName);
            foreach (var dep in deps)
            {
                var _abPath = AbNameToAbPath(dep);
                Load(_abPath);
            }

            return InternalLoad(abPath);
        }

        public async UniTask<IAssetBundleAgent> LoadAsync(string abName)
        {
            var abPath = AbNameToAbPath(abName);
            var deps = _abManifest.GetAllDependencies(abName);
            foreach (var dep in deps)
            {
                var _abPath = AbNameToAbPath(dep);
                await LoadAsync(_abPath);
            }

            return await InternalLoadAsync(abPath);
        }
        #endregion

        private async UniTask<IAssetBundleAgent> InternalLoadAsync(string abPath)
        {
            if (!_bundleDict.ContainsKey(abPath))
            {
                if (!_loadTaskDict.ContainsKey(abPath))
                {
                    _loadTaskDict.Add(abPath, null);
                    var req = AssetBundle.LoadFromFileAsync(abPath);
                    await UniTask.WaitUntil(() => req.isDone);
                    _loadTaskDict[abPath]?.Invoke();
                    _loadTaskDict.Remove(abPath);
                    var agent = new AssetBundleAgent(req.assetBundle, abPath);
                    _bundleDict[abPath] = (agent, 1);
                    
                    return agent;
                }

                var loadFinish = false;
                _loadTaskDict[abPath] += () => { loadFinish = true; };
                await UniTask.WaitUntil(() => loadFinish);
                
            }

            var temp = _bundleDict[abPath];
            _bundleDict[abPath] = (temp.Item1, temp.Item2 + 1);

            return temp.Item1;
        }

        private string AbPathToAbName(string abPath)
        {
            return abPath.Substring(abPath.LastIndexOf("/") + 1);
        }

        private string AbNameToAbPath(string abName)
        {
            return Path.Combine(_assetPath, abName);
        }

        private IAssetBundleAgent InternalLoad(string abPath)
        {
            if (_bundleDict.ContainsKey(abPath))
            {
                _bundleDict[abPath] = (_bundleDict[abPath].Item1, _bundleDict[abPath].Item2 + 1);
                return _bundleDict[abPath].Item1;
            }

            var bundle = AssetBundle.LoadFromFile(abPath);
            var agent = new AssetBundleAgent(bundle, abPath);
            _bundleDict.Add(abPath, (agent, 1));
            return agent;
        }

        public void Unload(string abName)
        {
            var abPath = AbNameToAbPath(abName);
            if (!_bundleDict.ContainsKey(abPath)) return;

            var deps = _abManifest.GetAllDependencies(abName);
            foreach (var dep in deps)
            {
                var _abPath = AbNameToAbPath(dep);
                Unload(_abPath);
            }

            InternalUnload(abPath);
        }

        public void Unload(IAssetBundleAgent abAgent)
        {
            var abPath = abAgent.abPath;
            var abName = AbPathToAbName(abPath);
            var deps = _abManifest.GetAllDependencies(abName);
            foreach (var dep in deps)
            {
                var _abPath = AbNameToAbPath(dep);
                Unload(_abPath);
            }

            InternalUnload(abPath);
        }

        private void InternalUnload(string abPath)
        {
            var abAgent = _bundleDict[abPath].Item1;
            if (_bundleDict[abPath].Item2 > 1)
            {
                _bundleDict[abPath] = (_bundleDict[abPath].Item1, _bundleDict[abPath].Item2 - 1);
            }
            else
            {
                _bundleDict.Remove(abPath);
                abAgent.Unload();
            }
        }

        public IReadOnlyList<string> GetDependences(string abName)
        {
            return _abManifest.GetAllDependencies(abName);
        }

        private static bool _Initialized;
        void Awake()
        {
            _Instance = this;
            Initialize();
        }

        private void Initialize()
        {
            if(_Initialized) return;
            
            AssetBundleConfig abConfig = Resources.Load<AssetBundleConfig>(nameof(AssetBundleConfig));
            _assetPath = Path.Combine(Application.streamingAssetsPath, abConfig.rootPath);
            var index = abConfig.rootPath.LastIndexOf('/');
            var manifestName = (index == -1) ? abConfig.rootPath : abConfig.rootPath.Substring(index);

            var path = AbNameToAbPath(manifestName);
            AssetBundle manifestAb = AssetBundle.LoadFromFile(path);
            _abManifest = manifestAb.LoadAsset<AssetBundleManifest>(MANIFEST_NAME);
            manifestAb.Unload(false);

            _Initialized = true;
        }

    }
}