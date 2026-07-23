using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class SocketManager : SingletonMonoBehaviour<SocketManager>
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void ConnectToSocketJS(string url);
    [DllImport("__Internal")] private static extern void EmitJoinEventJS(string playerId);
    [DllImport("__Internal")] private static extern void EmitJoinBattleRoomJS(string jsonStr);
    [DllImport("__Internal")] private static extern void EmitSyncMoveJS(string jsonStr);
    [DllImport("__Internal")] private static extern void EmitSyncChargingJS(string jsonStr);
    [DllImport("__Internal")] private static extern void EmitExecuteThrowJS(string jsonStr);
    [DllImport("__Internal")] private static extern void EmitExecuteHitJS(string jsonStr);
    [DllImport("__Internal")] private static extern void EmitTurnEndJS(string jsonStr);
    [DllImport("__Internal")] private static extern void EmitSendChatJS(string jsonStr);
    [DllImport("__Internal")] private static extern void EmitSendStickJS(string jsonStr);
#endif

    private SocketIOUnity socket;
    private DataConfig _dataConfig;
    private string savedPlayerId;

    /// <summary> 接收事件:角色位置 </summary>
    public Action<MoveData> OnPeerMoveReceived;
    /// <summary> 接收事件:對手畜力狀態 </summary>
    public Action<ChargingData> OnPeerChargingReceived;
    /// <summary> 接收事件:投擲 </summary>
    public Action<ThrowData> OnPeerThrowReceived;
    /// <summary> 接收事件:擊中 </summary>
    public Action<HitData> OnPeerHitReceived;
    /// <summary> 接收事件:新回合 </summary>
    public Action<NewTurnData> OnNewTurnReceived;
    /// <summary> 接收事件:聊天訊息 </summary>
    public Action<ReciveChatData> OnReciveChatReceived;
    /// <summary> 接收事件:貼圖訊息 </summary>
    public Action<ReciveStickData> OnReciveStickReceived;
    /// <summary> 接收事件:回合倒數 </summary>
    public Action<ReciveTurnCountDownData> OnReciveTurnCountDownReceived;
    /// <summary> 接收事件:遊戲結束 </summary>
    public Action<GameOverData> OnGameOverReceived;

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
    /// Server連線
    /// </summary>
    /// <param name="playerId"></param>
    public void ConnectToServer(string playerId)
    {
        savedPlayerId = playerId;
        string cleanUrl = _dataConfig.ServerApiUrl;

#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("[Socket] WebGL 環境：啟動瀏覽器原生 WebSocket 連線...");
        ConnectToSocketJS(cleanUrl);
#else
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

        // 指定全域序列化器為 Newtonsoft
        socket.JsonSerializer = new SocketIOClient.Newtonsoft.Json.NewtonsoftJsonSerializer();

        // 連線成功
        socket.OnConnected += (sender, e) => { SendJoinEventAsync().Forget(); };
        // 連線中斷
        socket.OnDisconnected += (sender, e) => { Debug.LogWarning($"[Socket] 連線中斷：{e}"); };
        // 連線錯誤
        socket.OnError += (sender, e) => { Debug.LogError($"[Socket] 連線錯誤: {e}"); };

        // 監聽:配對成功
        socket.On("match_success", (res) =>
        {
            var data = JsonConvertData<MatchSuccessData>(res);
            HandleMatchSuccessAsync(data).Forget();
        });

        // 監聽:角色移動
        socket.On("on_peer_move_synced", (res) => {
            var data = JsonConvertData<MoveData>(res);
            UniTask.Void(async () => { await UniTask.SwitchToMainThread(); OnPeerMoveReceived?.Invoke(data); });
        });

        // 監聽:蓄力狀態
        socket.On("on_peer_charging_synced", (res) => {
            var data = JsonConvertData<ChargingData>(res);
            UniTask.Void(async () => { await UniTask.SwitchToMainThread(); OnPeerChargingReceived?.Invoke(data); });
        });

        // 監聽:執行投擲
        socket.On("on_peer_execute_throw", (res) => {
            var data = JsonConvertData<ThrowData>(res);
            UniTask.Void(async () => { await UniTask.SwitchToMainThread(); OnPeerThrowReceived?.Invoke(data); });
        });

        // 監聽:擊中
        socket.On("on_peer_hit_synced", (res) => {
            var data = JsonConvertData<HitData>(res);
            UniTask.Void(async () => { await UniTask.SwitchToMainThread(); OnPeerHitReceived?.Invoke(data); });
        });

        // 監聽:回合切換
        socket.On("new_turn", (res) => {
            var data = JsonConvertData<NewTurnData>(res);
            UniTask.Void(async () => { await UniTask.SwitchToMainThread(); OnNewTurnReceived?.Invoke(data); });
        });

        // 監聽:聊天訊息
        socket.On("on_receive_chat", (res) => {
            var data = JsonConvertData<ReciveChatData>(res);
            UniTask.Void(async () => { await UniTask.SwitchToMainThread(); OnReciveChatReceived?.Invoke(data); });
        });

        // 監聽:貼圖訊息
        socket.On("on_receive_stick", (res) => {
            var data = JsonConvertData<ReciveStickData>(res);
            UniTask.Void(async () => { await UniTask.SwitchToMainThread(); OnReciveStickReceived?.Invoke(data); });
        });

        // 監聽:回合倒數
        socket.On("on_turn_countdown", (res) => {
            var data = JsonConvertData<ReciveTurnCountDownData>(res);
            UniTask.Void(async () => { await UniTask.SwitchToMainThread(); OnReciveTurnCountDownReceived?.Invoke(data); });
        });

        // 監聽:遊戲結束
        socket.On("game_over", (res) => {
            var data = JsonConvertData<GameOverData>(res);
            UniTask.Void(async () => { await UniTask.SwitchToMainThread(); OnGameOverReceived?.Invoke(data); });
        });

        socket.ConnectAsync();
#endif
    }

    /// <summary>
    /// 解析資料
    /// <returns>
    private T JsonConvertData<T>(SocketIOResponse response) where T : class
    {
        try
        {
            var jArray = JArray.Parse(response.ToString());

            if (jArray == null || jArray.Count == 0) return null;

            var firstElement = jArray[0];
            string rawJson = firstElement.Type == JTokenType.String
                ? firstElement.Value<string>()
                : firstElement.ToString();

            return JsonConvert.DeserializeObject<T>(rawJson);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Socket] 解析失敗: {ex.Message} | 原始回應: {response}");
            return null;
        }
    }

    /// <summary>
    /// 發送join 驗證
    /// </summary>
    private async UniTaskVoid SendJoinEventAsync()
    {
        Debug.Log("[Socket] 編輯器連線成功！準備發送 join 驗證...");
        await socket.EmitAsync("join", new { playerId = savedPlayerId });
    }

    /// <summary>
    /// 網頁端連線成功
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
    /// <param name="jsonText"></param>
    public void OnMatchSuccessJS(string jsonText)
    {
        Debug.Log($"[Socket] 網頁端收到配對成功！ 原始 JSON: {jsonText}");
        MatchSuccessData matchData = JsonConvert.DeserializeObject<MatchSuccessData>(jsonText);
        HandleMatchSuccessAsync(matchData).Forget();
    }

    /// <summary>
    /// 處理配對成功
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private async UniTaskVoid HandleMatchSuccessAsync(MatchSuccessData data)
    {
        await UniTask.SwitchToMainThread();
        Debug.Log($"進入房間: {data.roomId} | 對手: {data.opponentNickname}");
        StaticDataManager.MatchData = data;

        // 先切換場景，並 await 等待場景完全載入完成
        await SceneLoader.Instance.LoadSceneAsync(sceneType: SCENE_TYPE.GameScene);

        // 確定新場景的物件、Socket 監聽都到位了，再加入戰鬥房間
        Debug.Log($"[Socket] 場景載入完成，正式加入戰鬥房間: {data.roomId}");

#if UNITY_WEBGL && !UNITY_EDITOR
        string json = JsonConvert.SerializeObject(data);
        EmitJoinBattleRoomJS(json);
#else
        if (socket != null && socket.Connected)
        {
            await socket.EmitAsync("join_battle_room", new { roomId = data.roomId });
        }
#endif
    }

    #region WebGL 橋接器回傳資料
    /// <summary>
    /// 接收:角色移動
    /// </summary>
    public void OnPeerMoveSyncedJS(string jsonText) => OnPeerMoveReceived?.Invoke(JsonConvert.DeserializeObject<MoveData>(jsonText));

    /// <summary>
    /// 接收:畜力狀態
    /// </summary>
    public void OnPeerChargingSyncedJS(string jsonText) => OnPeerChargingReceived?.Invoke(JsonConvert.DeserializeObject<ChargingData>(jsonText));

    /// <summary>
    /// 接收:執行投擲
    /// </summary>
    public void OnPeerExecuteThrowJS(string jsonText) => OnPeerThrowReceived?.Invoke(JsonConvert.DeserializeObject<ThrowData>(jsonText));

    /// <summary>
    /// 接收:執行擊中
    /// </summary>
    public void OnPeerExecuteHitJS(string jsonText) => OnPeerHitReceived?.Invoke(JsonConvert.DeserializeObject<HitData>(jsonText));

    /// <summary>
    /// 接收:回合切換
    /// </summary>
    public void OnNewTurnJS(string jsonText) => OnNewTurnReceived?.Invoke(JsonConvert.DeserializeObject<NewTurnData>(jsonText));

    /// <summary>
    /// 接收:聊天訊息
    /// </summary>
    public void OnReceiveChatJS(string jsonText) => OnReciveChatReceived?.Invoke(JsonConvert.DeserializeObject<ReciveChatData>(jsonText));

    /// <summary>
    /// 接收:貼圖訊息
    /// </summary>
    public void OnReciveStickJS(string jsonText) => OnReciveStickReceived?.Invoke(JsonConvert.DeserializeObject<ReciveStickData>(jsonText));

    /// <summary>
    /// 接收:回合倒數
    /// </summary>
    public void OnReciveTurnCountDownJS(string jsonText) => OnReciveTurnCountDownReceived?.Invoke(JsonConvert.DeserializeObject<ReciveTurnCountDownData>(jsonText));

    /// <summary>
    /// 接收:遊戲結束
    /// </summary>
    public void OnGameOverJS(string jsonText) => OnGameOverReceived?.Invoke(JsonConvert.DeserializeObject<GameOverData>(jsonText));
    #endregion

    #region 發送資料
    /// <summary>
    /// 發送:角色移動
    /// </summary>
    public void SendSyncMove(MoveData data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string json = JsonConvert.SerializeObject(data);
        EmitSyncMoveJS(json);
#else
        if (socket != null && socket.Connected) socket.EmitAsync("sync_move", data);
#endif
    }

    /// <summary>
    /// 發送:畜力狀態
    /// </summary>
    public void SendSyncCharging(ChargingData data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string json = JsonConvert.SerializeObject(data);
        EmitSyncChargingJS(json);
#else
        if (socket != null && socket.Connected) socket.EmitAsync("sync_charging", data);
#endif
    }

    /// <summary>
    /// 發送:執行投擲
    /// </summary>
    public void SendExecuteThrow(ThrowData data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string json = JsonConvert.SerializeObject(data);
        EmitExecuteThrowJS(json);
#else
        if (socket != null && socket.Connected) socket.EmitAsync("execute_throw", data);
#endif
    }

    /// <summary>
    /// 發送:擊中
    /// </summary>
    public void SendExecuteHit(HitData data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string json = JsonConvert.SerializeObject(data);
        EmitExecuteHitJS(json);
#else
        if (socket != null && socket.Connected) socket.EmitAsync("execute_hit", data);
#endif
    }

    /// <summary>
    /// 發送:回合結束
    /// </summary>
    public void SendTurnEnd(TurnEndData data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string json = JsonConvert.SerializeObject(data);
        EmitTurnEndJS(json);
#else
        if (socket != null && socket.Connected) socket.EmitAsync("turn_end", data);
#endif
    }

    /// <summary>
    /// 發送:聊天訊息
    /// </summary>
    public void SendChat(SendChatData data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string json = JsonConvert.SerializeObject(data);
        EmitSendChatJS(json);
#else
        if (socket != null && socket.Connected) socket.EmitAsync("send_chat", data);
#endif
    }

    /// <summary>
    /// 發送:貼圖訊息
    /// </summary>
    public void SendStick(SendStickData data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string json = JsonConvert.SerializeObject(data);
        EmitSendStickJS(json);
#else
        if (socket != null && socket.Connected) socket.EmitAsync("send_stick", data);
#endif
    }
    #endregion
}