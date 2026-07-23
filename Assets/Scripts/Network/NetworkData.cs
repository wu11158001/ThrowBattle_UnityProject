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
}

/// <summary> 配對回傳 </summary>
[Serializable]
public class MatchResponse
{
    /// <summary> 配對回傳訊息 </summary>
    public string message;
    /// <summary> 當前狀態(在大廳[Lobby]/配對中[Matching]/遊戲中[InGame]) </summary>
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

#region 主動退出遊戲
/// <summary>
/// 主動退出遊戲請求
/// </summary>
[Serializable]
public class LeaveBattleRequest
{
    /// <summary> 玩家專屬ID </summary>
    public string playerId;
}
/// <summary>
/// 主動退出遊戲回傳
/// </summary>
[Serializable]
public class LeaveBattleResponse
{

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
    /// <summary> 本地玩家編號(0 = Player1, 1 = Player2) </summary>
    public int mySeat;
    /// <summary> 對手暱稱 </summary>
    public string opponentNickname;
}
#endregion

#region 遊戲
/// <summary>
/// 角色位置同步資料
/// </summary>
[Serializable]
public class MoveData
{
    /// <summary> 房間ID </summary>
    public string roomId;
    /// <summary> 角色當前最新的 X 軸位置 </summary>
    public float posX;
    /// <summary> 移動方向（1、-1 或 0） </summary>
    public float inputDir;
}

/// <summary>
/// 閃避資料
/// </summary>
[Serializable]
public class DodgeData
{
    /// <summary> 房間ID </summary>
    public string roomId;
    /// <summary> 閃避角色(0 = P1, 1 = P2) </summary>
    public int targetSeat;
}

/// <summary>
/// 畜力狀態資料
/// </summary>
[Serializable]
public class ChargingData
{
    /// <summary> 房間ID </summary>
    public string roomId;
    /// <summary> 是否開始蓄力 </summary>
    public bool isCharging;
    /// <summary> 畜力程度(0~1) </summary>
    public float force;
}

/// <summary>
/// 投擲資料
/// </summary>
[Serializable]
public class ThrowData
{
    /// <summary> 房間ID </summary>
    public string roomId;
    /// <summary> 投擲類型 </summary>
    public int throwType;
    /// <summary> 力道 </summary>
    public float force;
}

/// <summary>
/// 擊中資料
/// </summary>
[Serializable]
public class HitData
{
    /// <summary> 房間ID </summary>
    public string roomId;
    /// <summary> 擊中對象(0 = Player1, 1 = Player2) </summary>
    public int targetSeat;
    /// <summary> 投擲類型 </summary>
    public int throwType;
    /// <summary> 造成傷害(0 = 未擊中, -1 = 閃避) </summary>
    public int damage;
    /// <summary> 玩家1Hp </summary>
    public int p1Hp;
    /// <summary> 玩家2Hp </summary>
    public int p2Hp;
}

/// <summary>
/// 回合結束資料
/// </summary>
[Serializable]
public class TurnEndData
{
    /// <summary> 房間ID </summary>
    public string roomId;
}

/// <summary>
/// 回合切換資料
/// </summary>
[Serializable]
public class NewTurnData
{
    /// <summary> 當前回合操作玩家(0 = Player1, 1= Player2) </summary>
    public int currentTurnSeat;
    /// <summary> 玩家1Hp </summary>
    public int p1Hp;
    /// <summary> 玩家2Hp </summary>
    public int p2Hp;
    /// <summary> 當前回合風力 </summary>
    public float windStrength;
    /// <summary> 是否是超時而更換回合 </summary>
    public bool isTimeout;
}

/// <summary>
/// 發送聊天資料
/// </summary>
[Serializable]
public class SendChatData
{
    /// <summary> 房間ID </summary>
    public string roomId;
    /// <summary> 聊天內容 </summary>
    public string chatMessage;
}

/// <summary>
/// 接收聊天資料
/// </summary>
[Serializable]
public class ReciveChatData
{
    /// <summary> 發送者(0 = Player1, 1= Player2) </summary>
    public int senderSeat;
    /// <summary> 發送者暱稱 </summary>
    public string senderNickname;
    /// <summary> 聊天內容 </summary>
    public string chatMessage;
    /// <summary> 發送時間 </summary>
    public string timestamp;

    /// <summary> 自動轉換後的當地時間 (唯讀屬性) </summary>
    public DateTime LocalTime
    {
        get
        {
            if (DateTime.TryParse(timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime utcTime))
            {
                return utcTime.ToLocalTime();
            }
            return DateTime.Now;
        }
    }

    /// <summary> 發送顯示時間字串 </summary>
    public string DisplayTime => LocalTime.ToString("HH:mm");
}

/// <summary>
/// 發送貼圖資料
/// </summary>
[Serializable]
public class SendStickData
{
    /// <summary> 房間ID </summary>
    public string roomId;
    /// <summary> 貼圖Index </summary>
    public int stickIndex;
}

/// <summary>
/// 接收貼圖資料
/// </summary>
[Serializable]
public class ReciveStickData
{
    /// <summary> 發送者(0 = Player1, 1= Player2) </summary>
    public int senderSeat;
    /// <summary> 發送者暱稱 </summary>
    public string senderNickname;
    /// <summary> 貼圖Index </summary>
    public int stickIndex;
    /// <summary> 發送時間 </summary>
    public string timestamp;

    /// <summary> 自動轉換後的當地時間 (唯讀屬性) </summary>
    public DateTime LocalTime
    {
        get
        {
            if (DateTime.TryParse(timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime utcTime))
            {
                return utcTime.ToLocalTime();
            }
            return DateTime.Now;
        }
    }

    /// <summary> 發送顯示時間字串 </summary>
    public string DisplayTime => LocalTime.ToString("HH:mm");
}

/// <summary>
/// 回合倒數資料
/// </summary>
[Serializable]
public class ReciveTurnCountDownData
{
    /// <summary> 當前回合甚餘秒數 </summary>
    public int secondsLeft;
}


/// <summary>
/// 遊戲結束
/// </summary>
[Serializable]
public class GameOverData
{
    /// <summary> 獲勝玩家(0 = Player1, 1= Player2) </summary>
    public int winnerSeat;
    /// <summary> 獲勝暱稱 </summary>
    public int winnerNickname;
    /// <summary> 若對手中斷連線這會收到內容 </summary>
    public string message;
}
#endregion