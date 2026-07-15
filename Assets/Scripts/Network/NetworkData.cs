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

#region 登出離線
/// <summary> 登出離線請求 </summary>
[Serializable]
public class LogoutRequest
{
    public string playerId;
}
#endregion

#region 註冊
/// <summary> 註冊請求 </summary>
[Serializable]
public class RegisterRequest
{
    /// <summary> 暱稱 </summary>
    public string nickname;
}

/// <summary> 註冊回傳 </summary>
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
/// <summary> 配對請求 </summary>
[Serializable]
public class MatchRequest
{
    /// <summary> 玩家專屬ID </summary>
    public string playerId;
    /// <summary> 所選角色編號 </summary>
    public int characterIndex;
}

/// <summary> 配對回傳 </summary>
[Serializable]
public class MatchResponse
{
    /// <summary> 配對回傳訊息 </summary>
    public string message;
    /// <summary> 當前狀態(在大廳[Lobby]/配對中[Matching]) </summary>
    public string currentStatus;
}
#endregion

#region 取消配對
/// <summary> 取消配對請求 </summary>
[Serializable]
public class CancelMatchRequest
{
    /// <summary> 玩家專屬ID </summary>
    public string playerId;
}
/// <summary> 取消配對回傳 </summary>
[Serializable]
public class CancelMatchResponse
{
    /// <summary> 回傳訊息 </summary>
    public string message;
    /// <summary> 當前狀態 </summary>
    public string currentStatus;
}
#endregion

#region 配對成功
/// <summary>
/// 配對成功資料
/// </summary>
[Serializable]
public class MatchSuccessData
{
    /// <summary> 房間ID </summary>
    public string roomId;
    /// <summary> 是否為房主(Player1) </summary>
    public bool isCreator;
    /// <summary> 對手暱稱 </summary>
    public string opponentNickname;
    /// <summary> 對手角色編號 </summary>
    public int opponentCharacterIndex;
}
#endregion