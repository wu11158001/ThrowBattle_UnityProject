
/// <summary>
/// 配置檔中心,啟動時用來設置各項SO資料
/// </summary>
public static class ConfigManager
{
    /// <summary>
    /// 設置各項設定檔資料
    /// </summary>
    public static void SetConfigDataAsync(AllConfig allConfig)
    {
        StaticDataManager.ViewConfig = allConfig.ViewConfig;
        StaticDataManager.AudioConfig = allConfig.AudioConfig;
        StaticDataManager.DataConfig = allConfig.DataConfig;
        StaticDataManager.ObjectPrefabConfig = allConfig.ObjectPrefabConfig;
    }
}
