using System;

#region 註冊
// 註冊請求
[Serializable]
public class RegisterRequest
{
    /// <summary> 暱稱 </summary>
    public string Nickname;
}

// 註冊回傳
[Serializable]
public class RegisterResponse
{
    /// <summary> 註冊回傳訊息 </summary>
    public string Message;
    /// <summary> 玩家專屬ID </summary>
    public string PlayerId;
}
#endregion

#region 配對
// 配對請求
[Serializable]
public class MatchRequest
{
    /// <summary> 玩家專屬ID </summary>
    public string PlayerId;
    /// <summary> 所選角色編號 </summary>
    public string CharacterIndex;
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