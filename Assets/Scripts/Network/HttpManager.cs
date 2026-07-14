using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using Cysharp.Threading.Tasks;

public static class HttpManager
{
    private static string _baseUrl = "http://localhost:3000";

    public static void SetUrl(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// Post請求
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="subUrl"></param>
    /// <param name="requestData"></param>
    /// <returns></returns>
    public static async UniTask<TResponse> SendPostAsync<TRequest, TResponse>(string subUrl, TRequest requestData)
    {
        string url = _baseUrl + subUrl;
        string jsonPayload = JsonUtility.ToJson(requestData);

        // 建立 UnityWebRequest
        UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        // 等待網路下載
        await webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            return JsonUtility.FromJson<TResponse>(webRequest.downloadHandler.text);
        }
        else
        {
            throw new Exception(webRequest.error);
        }
    }
}