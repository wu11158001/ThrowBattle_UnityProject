using System;
using UnityEngine;
using SocketIOClient;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

public class SocketManager : SingletonMonoBehaviour<SocketManager>
{
    private SocketIOUnity socket;

    protected override void OnDestroy()
    {
        Disconnect();
        base.OnDestroy();
    }

    protected override void OnApplicationQuit()
    {
        // Socket 斷線
        Disconnect();

        base.OnApplicationQuit();
    }

    /// <summary>
    /// 主動中斷連線
    /// </summary>
    public void Disconnect()
    {
        if (socket != null && socket.Connected)
        {
            socket.DisconnectAsync();
            Debug.Log("[Socket] 已主動中斷連線。");
        }
    }

    /// <summary>
    /// 開始連線至 Socket 伺服器
    /// </summary>
    /// <param name="playerId"></param>
    public void ConnectToServer(string playerId)
    {
        if (socket != null && socket.Connected) return;

        Debug.Log("[Socket] 正在嘗試連線至伺服器...");

        var uri = new Uri(StaticDataManager.DataConfig.BaseUrl);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket // 強制使用 WebSocket 傳輸
        });

        // 系統事件監聽: 連線
        socket.OnConnected += (sender, e) =>
        {
            // 利用 UniTask 異步執行身分綁定，避免阻塞
            SendJoinEventAsync(playerId).Forget();
        };

        // 系統事件監聽: 斷線
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.LogWarning($"[Socket] 與伺服器連線中斷：{e}");
        };

        // 監聽配對成功
        socket.On("match_success", (response) =>
        {
            string jsonText = response.GetValue<string>();
            Debug.Log($"[Socket] 收到配對成功訊號！ 原始 JSON: {jsonText}");

            MatchSuccessData matchData = JsonConvert.DeserializeObject<MatchSuccessData>(jsonText);

            // 使用 UniTask 安全切換回主執行緒並執行後續邏輯
            HandleMatchSuccessAsync(matchData).Forget();
        });

        // 監聽錯誤訊息
        socket.On("error_msg", (response) =>
        {
            string error = response.GetValue<string>();
            Debug.LogError($"[Socket] 伺服器錯誤提示: {error}");
        });

        // 啟動連線
        socket.ConnectAsync();
    }

    /// <summary>
    /// 連線成功後，向伺服器發送 join 驗證
    /// </summary>
    private async UniTaskVoid SendJoinEventAsync(string playerId)
    {
        Debug.Log("[Socket] 連線成功！準備發送 join 驗證身分...");
        var joinData = new { playerId = playerId };
        await socket.EmitAsync("join", joinData);
    }

    /// <summary>
    /// 處理配對成功
    /// </summary>
    private async UniTaskVoid HandleMatchSuccessAsync(MatchSuccessData data)
    {
        // 使用 UniTask 一行指令直接切換回 Unity 主執行緒！
        await UniTask.SwitchToMainThread();

        Debug.Log($"進入房間: {data.roomId} | 對手: {data.opponentNickname} (角色編號: {data.opponentCharacterIndex})");

        // 設置全域配對成功資料
        StaticDataManager.MatchData = data;

        // 進入遊戲場景
        SceneLoader.Instance.LoadSceneAsync(sceneType: SCENE_TYPE.GameScene).Forget();
    }
}