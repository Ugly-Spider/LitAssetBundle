# LitAssetBundle
Unity AssetBundle打包加载脚本。

## 1.打包
选中需要打包的文件或文件夹，右键，选择Mark As File(把当前资源文件设置为单独的AssetBundleName，设置方式为:1.按name设置，2.按路径设置)或Mark As Package(把多个资源文件设置为一个AssetBundle)添加AssetBundleName，然后Window/LitAssetBundle/Open Build Windows打开打包界面，点击build打包。

打包时会将重复的资源打包为单独的AssetBundle，如Cube1,Cube2同时引用了Cube3，打包时会生成额外生成一个Cube3的AssetBundle，防止打包时资源重复。

## 2.使用
使用时不需要其他设置
```c#
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
    ```
