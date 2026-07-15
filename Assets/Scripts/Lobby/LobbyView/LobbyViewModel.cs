using System;

/// <summary>
/// 大廳介面 ViewModel
/// </summary>
public class LobbyViewModel
{
    /// <summary>
    /// 發送配對請求
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="failCallback"></param>
    public void SendMatchRequest(Action successCallback, Action<long> failCallback)
    {
        MatchRequest req = new()
        {
            playerId = StaticDataManager.RegisterPlayerData.PlayerId,
            characterIndex = StaticDataManager.RegisterPlayerData.CharacterIndex,
        };

        _ = HttpManager.Instance.SendPostAsync<MatchRequest, MatchResponse>(
                subUrl: StaticDataManager.MatchSubUrl,
                requestData: req,
                onSuccess: (res) =>
                {
                    SocketManager.Instance.ConnectToServer(StaticDataManager.RegisterPlayerData.PlayerId);
                    successCallback?.Invoke();         
                },
                onFailure: (code, err) =>
                {
                    failCallback?.Invoke(code);                    
                }
            );
    }
}
