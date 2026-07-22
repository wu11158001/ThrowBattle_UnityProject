using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全域遊戲資料
/// </summary>
public static class StaticDataManager
{
    #region 唯獨配置檔
    /// <summary> 介面配置檔 </summary>
    public static ViewConfig ViewConfig { get; set; }
    /// <summary> 音訊配置檔 </summary>
    public static AudioConfig AudioConfig { get; set; }
    /// <summary> 資料配置檔 </summary>
    public static DataConfig DataConfig { get; set; }
    /// <summary> 遊戲物件配置檔 </summary>
    public static ObjectPrefabConfig ObjectPrefabConfig { get; set; }
    #endregion

    #region 圖片
    /// <summary> 貼圖 </summary>
    public static List<Sprite> StickTextures { get; set; } = new();
    #endregion

    #region API URL
    /// <summary> 註冊 API </summary>
    public static readonly string RegisterSubUrl = "/api/lobby/register";
    /// <summary> 玩家配對 API </summary>
    public static readonly string MatchSubUrl = "/api/lobby/match";
    /// <summary> 取消玩家配對 API </summary>
    public static readonly string CancelMatchSubUrl = "/api/lobby/cancel-match";
    /// <summary> 主動退出遊戲 API </summary>
    public static readonly string LeaveBattleSubUrl = "/api/lobby/leave-battle";
    #endregion

    #region 遊戲資料
    /// <summary> 註冊的玩家資料 </summary>
    public static PlayerData RegisterPlayerData;
    /// <summary> 配對成功資料 </summary>
    public static MatchSuccessData MatchData;
    /// <summary> 選擇的對戰模式 </summary>
    public static PLAY_TYPE PlayType;
    /// <summary> 選擇的AI困難度資料 </summary>
    public static AIDifficultyData AIDifficultyData;
    /// <summary> 選擇的AI風格資料 </summary>
    public static AIStyleData AIStyleData;
    #endregion
}

/// <summary>
/// 玩家資料
/// </summary>
public class PlayerData
{
    public string Nickname;
    public string PlayerId;
}