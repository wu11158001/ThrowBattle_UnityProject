using System;

#region 錯誤訊息
/// <summary>
/// 錯誤訊息
/// </summary>
[System.Serializable]
public class ErrorResponse
{
    public string error;
}
#endregion

#region 註冊
// 註冊請求
[Serializable]
public class RegisterRequest
{
    /// <summary> 暱稱 </summary>
    public string nickname;
}

// 註冊回傳
[Serializable]
public class RegisterResponse
{
    /// <summary> 註冊回傳訊息 </summary>
    public string message;
    /// <summary> 暱稱 </summary>
    public string nickname;
    /// <summary> 玩家專屬ID </summary>
    public string playerId;
}
#endregion

#region 配對
// 配對請求
[Serializable]
public class MatchRequest
{
    /// <summary> 玩家專屬ID </summary>
    public string playerId;
    /// <summary> 所選角色編號 </summary>
    public string characterIndex;
}

// 配對回傳
[Serializable]
public class MatchResponse
{
    /// <summary> 配對回傳訊息 </summary>
    public string message;
    /// <summary> 當前狀態(在大廳[Lobby]/配對中[Matching]) </summary>
    public string currentStatus;
}
#endregion