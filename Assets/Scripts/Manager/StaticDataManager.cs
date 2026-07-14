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
    #endregion

    #region API URL
    /// <summary> 註冊 URL </summary>
    public static readonly string RegisterSubUrl = "/api/lobby/register";
    #endregion

    #region 資料
    /// <summary> 註冊的玩家資料 </summary>
    public static PlayerData RegisterPlayerData;

    /// <summary>
    /// 玩家資料
    /// </summary>
    public class PlayerData
    {
        public string Nickname;
        public string PlayerId;
    }
    #endregion
}
