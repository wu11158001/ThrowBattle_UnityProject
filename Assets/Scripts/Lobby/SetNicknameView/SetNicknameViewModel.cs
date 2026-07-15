using System;
using UniRx;

/// <summary>
/// 設置暱稱介面 ViewModel 
/// </summary>
public class SetNicknameViewModel
{
    /// <summary>
    /// 發送註冊請求
    /// </summary>
    /// <param name="nickname"></param>
    /// <param name="successCallback"></param>
    /// <param name="failCallback"></param>
    public void SendRegisterRequest(string nickname, Action successCallback, Action<long> failCallback)
    {
        RegisterRequest req = new()
        {
            nickname = nickname
        };

        _ = HttpManager.Instance.SendPostAsync<RegisterRequest, RegisterResponse>(
                subUrl: StaticDataManager.RegisterSubUrl,
                requestData: req,
                onSuccess: (res) =>
                {
                    PlayerData playerData = new()
                    {
                        Nickname = res.nickname,
                        PlayerId = res.playerId,
                    };

                    // 全域資料設置
                    StaticDataManager.RegisterPlayerData = playerData;

                    // 發送廣播
                    RegisterSuccessMessage registerSuccessMessage = new() { PlayerData = playerData };
                    MessageBroker.Default.Publish(registerSuccessMessage);

                    successCallback?.Invoke();
                },
                onFailure: (errorCode, err) =>
                {
                    failCallback?.Invoke(errorCode);
                }
            );
    }
}
