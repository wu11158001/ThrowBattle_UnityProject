using UnityEngine;

/// <summary>
/// 全域遊戲資料
/// </summary>
public static class StaticDataManager
{
    [Header("唯獨配置檔")]
    /// <summary> 介面配置檔 </summary>
    public static ViewConfig ViewConfig { get; set; }
    /// <summary> 音訊配置檔 </summary>
    public static AudioConfig AudioConfig { get; set; }
}
