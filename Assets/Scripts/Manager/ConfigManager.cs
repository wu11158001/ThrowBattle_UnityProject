using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine;

/// <summary>
/// 配置檔中心,啟動時用來設置各項SO資料
/// </summary>
public static class ConfigManager
{
    /// <summary>
    /// 設置各項設定檔資料
    /// </summary>
    public static async UniTask SetConfigDataAsync(AllConfig allConfig)
    {
        StaticDataManager.ViewConfig = allConfig.ViewConfig;
        StaticDataManager.AudioConfig = allConfig.AudioConfig;
        StaticDataManager.DataConfig = allConfig.DataConfig;
        StaticDataManager.ObjectPrefabConfig = allConfig.ObjectPrefabConfig;

        await LoadStickTexture();
    }

    /// <summary>
    /// 載入所有貼圖
    /// </summary>
    /// <returns></returns>
    private static async UniTask LoadStickTexture()
    {
        string address = "Sticks/Sticks.png";

        var handle = Addressables.LoadAssetAsync<IList<Sprite>>(address);
        var result = await handle.ToUniTask();

        if (result != null)
        {
            StaticDataManager.StickTextures = new List<Sprite>(result);

            Debug.Log($"成功載入 {StaticDataManager.StickTextures.Count} 張貼圖！");
        }
    }
}
