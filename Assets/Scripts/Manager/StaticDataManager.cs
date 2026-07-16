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

    #region API URL
    /// <summary> 註冊 URL </summary>
    public static readonly string RegisterSubUrl = "/api/lobby/register";
    /// <summary> 玩家配對 URL </summary>
    public static readonly string MatchSubUrl = "/api/lobby/match";
    /// <summary> 取消玩家配對 URL </summary>
    public static readonly string CancelMatchSubUrl = "/api/lobby/cancel-match";
    #endregion

    #region 資料
    /// <summary> 註冊的玩家資料 </summary>
    public static PlayerData RegisterPlayerData;
    /// <summary> 配對成功資料 </summary>
    public static MatchSuccessData MatchData;
    /// <summary> 對戰模式 </summary>
    public static PLAY_TYPE PlayType;
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