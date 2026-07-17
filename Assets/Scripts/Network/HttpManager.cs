using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public class HttpManager : SingletonMonoBehaviour<HttpManager>
{
    private DataConfig _dataConfig;

    protected override void OnApplicationQuit()
    {
        // 移除大廳玩家
        if (StaticDataManager.RegisterPlayerData != null && !string.IsNullOrEmpty(StaticDataManager.RegisterPlayerData.PlayerId))
        {
            string pId = StaticDataManager.RegisterPlayerData.PlayerId;
            LogoutRequest req = new() { playerId = pId };

            _ = SendPostAsync<LogoutRequest, RegisterResponse>(
                subUrl: "/api/lobby/logout",
                requestData: req,
                onSuccess: (res) =>
                {
                    Debug.Log($"[API 成功回傳][Url = /api/lobby/logout]: 離線通知已發送: {pId}");
                },
                onFailure: (code, err) => 
                {
                    Debug.Log($"[API 回傳失敗][Url = /api/lobby/logout]: 離線通知未處理（可能已被 Socket 清除）: {err}");
                }
            );
        }

        base.OnApplicationQuit();
    }

    protected override void Awake()
    {
        base.Awake();

        _dataConfig = StaticDataManager.DataConfig;
    }

    /// <summary>
    /// POST 請求
    /// </summary>
    /// <typeparam name="TRequest">傳送的資料型別</typeparam>
    /// <typeparam name="TResponse">預期接收的回傳資料型別</typeparam>
    /// <param name="subUrl">API 子路徑 (例如 "/api/lobby/register")</param>
    /// <param name="requestData">要傳送的資料物件</param>
    /// <param name="onSuccess">成功時的回呼 (傳入解析後的 Response 物件)</param>
    /// <param name="onFailure">失敗時的回呼 (傳入錯誤訊息字串)</param>
    /// <returns>回傳 UniTask<TResponse>，若失敗則回傳預設值</returns>
    public async UniTask<TResponse> SendPostAsync<TRequest, TResponse>(
        string subUrl,
        TRequest requestData,
        Action<TResponse> onSuccess = null,
        Action<long, string> onFailure = null)
    {
        if (_dataConfig == null || string.IsNullOrEmpty(_dataConfig.BaseUrl))
        {
            string urlError = "請求 URL 錯誤：HttpBaseUrl 未初始化！";
            Debug.LogError(urlError);
            onFailure?.Invoke(-1, urlError);
            return default;
        }

        string url = _dataConfig.BaseUrl + subUrl;
        string jsonPayload = JsonUtility.ToJson(requestData);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            try
            {
                AsyncOperation asyncOp = webRequest.SendWebRequest();
                await asyncOp.ToUniTask();
            }
            catch (Exception ex)
            {
                // 捕捉真斷網（ DNS 失敗、連接失敗、連線超時等物理性異常）
                string netError = $"網路連線失敗: {ex.Message}";
                Debug.LogError($"網路連線失敗: {netError}");
                onFailure?.Invoke(0, netError);
                return default;
            }

            // 解析結果
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    TResponse response = JsonUtility.FromJson<TResponse>(webRequest.downloadHandler.text);
                    Debug.Log($"[API 成功回傳] [Url = {subUrl}]: {JsonUtility.ToJson(response, true)}");

                    onSuccess?.Invoke(response);
                    return response;
                }
                catch (Exception ex)
                {
                    long statusCode = webRequest.responseCode;
                    string jsonError = $"JSON 解析失敗: {ex.Message}";
                    Debug.LogError($"{jsonError}. 原始資料: {webRequest.downloadHandler.text}");
                    onFailure?.Invoke(statusCode, jsonError);
                    return default;
                }
            }
            else
            {
                string serverRawError = webRequest.downloadHandler?.text;
                string friendlyMessage = "網路請求失敗";

                if (!string.IsNullOrEmpty(serverRawError))
                {
                    try
                    {
                        ErrorResponse errorObj = JsonUtility.FromJson<ErrorResponse>(serverRawError);
                        friendlyMessage = errorObj.error;
                    }
                    catch
                    {
                        friendlyMessage = webRequest.error;
                    }
                }

                Debug.LogWarning($"[API 回傳失敗] [Url = {subUrl}]: {friendlyMessage}");
                onFailure?.Invoke(webRequest.responseCode, friendlyMessage);
                return default;
            }
        }
    }
}