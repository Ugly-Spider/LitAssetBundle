using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitAssetBundle;
using UniRx.Async;
using UnityEngine;

public class Demo : MonoBehaviour
{
    async void Start()
    {
        //先根据AssetBundle的名字加载agent，会自动加载依赖
        var abAgent = await AssetBundleLoader.Instance.LoadAsync("Cube1");
        //再在AssetBundle加载指定的资源
        var prefab = abAgent.LoadAsset<GameObject>("Cube1");
        var go = Instantiate(prefab);
        await UniTask.DelayFrame(60);
        //通过agent释放AssetBundle，会自动卸载依赖
        AssetBundleLoader.Instance.Unload(abAgent);
        Destroy(go);
        
        //对于AssetBundle中只有单个资源，AssetBundle名和资源名相同，可以简写为：
        var go2 = await AssetBundleLoader.Instance.LoadAndInsAsync<GameObject>("Cube1");
        await UniTask.DelayFrame(60);
        Destroy(go2);
        AssetBundleLoader.Instance.Unload("Cube1");
    }
}