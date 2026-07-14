using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using Cysharp.Threading.Tasks;

public static class HttpManager
{
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
    public static async UniTask<TResponse> SendPostAsync<TRequest, TResponse>(
        string subUrl,
        TRequest requestData,
        Action<TResponse> onSuccess = null,
        Action<long, string> onFailure = null)
    {
        // 檢查網址
        if (StaticDataManager.DataConfig == null || string.IsNullOrEmpty(StaticDataManager.DataConfig.HttpBaseUrl))
        {
            string urlError = "請求 URL 錯誤：HttpBaseUrl 未初始化！";
            Debug.LogError(urlError);
            onFailure?.Invoke(-1, urlError); // -1 = 未知錯誤
            return default;
        }

        string url = StaticDataManager.DataConfig.HttpBaseUrl + subUrl;
        string jsonPayload = JsonUtility.ToJson(requestData);

        // 發送請求
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await webRequest.SendWebRequest();
            }
            catch (Exception ex)
            {
                // 處理物理性斷網或超時
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

                    // 觸發成功 Action
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

                onFailure?.Invoke(webRequest.responseCode, friendlyMessage);
                return default;
            }
        }
    }
}