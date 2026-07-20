using System;

/// <summary>
/// 配對中介面 ViewModel
/// </summary>
public class MatchingViewModel
{
    /// <summary>
    /// 發送:取消配對
    /// </summary>
    /// <param name="successCallback"></param>
    /// <param name="failCallback"></param>
    public void SendCancelMatchRequest(Action successCallback, Action<long> failCallback)
    {
        CancelMatchRequest req = new()
        {
            playerId = StaticDataManager.RegisterPlayerData.PlayerId
        };

        _ = HttpManager.Instance.SendPostAsync<CancelMatchRequest, CancelMatchResponse>(
                subUrl: "/api/lobby/cancel-match",
                requestData: req,
                onSuccess: (res) =>
                {
                    successCallback?.Invoke();
                },
                onFailure: (errorCode, err) =>
                {
                    failCallback?.Invoke(errorCode);
                }
            );
    }
}
