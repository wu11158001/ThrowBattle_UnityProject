using System;
using System.Runtime.InteropServices; // 💡 引入執行 JS 橋接所需的命名空間
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

public class SocketManager : SingletonMonoBehaviour<SocketManager>
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void ConnectToSocketJS(string url);

    [DllImport("__Internal")]
    private static extern void EmitJoinEventJS(string playerId);
#endif

    private SocketIOUnity socket;
    private DataConfig _dataConfig;

    // 暫存 PlayerID，供 JS 連線成功後使用
    private string savedPlayerId;

    protected override void OnDestroy()
    {
        Disconnect();
        base.OnDestroy();
    }

    protected override void OnApplicationQuit()
    {
        Disconnect();
        base.OnApplicationQuit();
    }

    protected override void Awake()
    {
        base.Awake();

        _dataConfig = StaticDataManager.DataConfig;
    }

    public void Disconnect()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL 端由瀏覽器自行處理
#else
        if (socket != null && socket.Connected)
        {
            socket.DisconnectAsync();
            Debug.Log("[Socket] 已主動中斷 C# 連線。");
        }
#endif
    }

    /// <summary>
    /// 開始連線至 Socket 伺服器
    /// </summary>
    public void ConnectToServer(string playerId)
    {
        savedPlayerId = playerId;
        string cleanUrl = _dataConfig.BaseUrl;

        // 判斷運行平台
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL：呼叫 JS 橋接器
            Debug.Log("[Socket] WebGL 環境：啟動瀏覽器原生 WebSocket 連線...");
            ConnectToSocketJS(cleanUrl);
#else
        // 編輯器測試
        if (socket != null && socket.Connected) return;

        Debug.Log("[Socket] 編輯器環境：啟動 C# SocketIOUnity 連線...");
        var uri = new Uri(cleanUrl);
        var options = new SocketIOOptions
        {
            Path = "/socket.io/",
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            EIO = SocketIOClient.EngineIO.V4
        };

        socket = new SocketIOUnity(uri, options);

        socket.OnConnected += (sender, e) => { SendJoinEventAsync(playerId).Forget(); };
        socket.OnDisconnected += (sender, e) => { Debug.LogWarning($"[Socket] 與伺服器連線中斷：{e}"); };
        socket.OnError += (sender, e) => { Debug.LogError($"[Socket] 連線發生錯誤: {e}"); };

        socket.On("match_success", (response) =>
        {
            string jsonText = response.ToString();
            MatchSuccessData matchData = JsonConvert.DeserializeObject<MatchSuccessData>(jsonText);
            HandleMatchSuccessAsync(matchData).Forget();
        });

        socket.ConnectAsync();
#endif
    }

    private async UniTaskVoid SendJoinEventAsync(string playerId)
    {
        Debug.Log("[Socket] 編輯器連線成功！準備發送 join 驗證...");
        var joinData = new Dictionary<string, string> { { "playerId", playerId } };
        await socket.EmitAsync("join", joinData);
    }

    /// <summary>
    ///  Socket 連線成功
    /// </summary>
    public void OnSocketConnectedJS()
    {
        Debug.Log("[Socket] 網頁端連線成功！準備發送 join 驗證身分...");
#if UNITY_WEBGL && !UNITY_EDITOR
            EmitJoinEventJS(savedPlayerId);
#endif
    }

    /// <summary>
    /// 配對成功
    /// </summary>
    public void OnMatchSuccessJS(string jsonText)
    {
        Debug.Log($"[Socket] 網頁端收到配對成功！ 原始 JSON: {jsonText}");
        MatchSuccessData matchData = JsonConvert.DeserializeObject<MatchSuccessData>(jsonText);
        HandleMatchSuccessAsync(matchData).Forget();
    }

    /// <summary>
    /// 處理配對成功
    /// </summary>
    private async UniTaskVoid HandleMatchSuccessAsync(MatchSuccessData data)
    {
        await UniTask.SwitchToMainThread();
        Debug.Log($"進入房間: {data.roomId} | 對手: {data.opponentNickname}");
        StaticDataManager.MatchData = data;
        SceneLoader.Instance.LoadSceneAsync(sceneType: SCENE_TYPE.GameScene).Forget();
    }
}