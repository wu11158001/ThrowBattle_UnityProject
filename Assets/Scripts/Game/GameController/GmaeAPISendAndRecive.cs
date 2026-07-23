using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 遊戲API發送與接
/// </summary>
public class GmaeAPISendAndRecive
{
    private GameplayContext _context;
    private CharacterThrowController _throwController;

    public GmaeAPISendAndRecive()
    {
        _context = GameplayManager.CurrentContext;
        _throwController = _context.GameController.ThrowController;

        ListenServerEvent();
    }

    /// <summary>
    /// 移除監聽Server事件
    /// </summary>
    public void RemoveListenServerEvent()
    {
        if (StaticDataManager.PlayType == PLAY_TYPE.Match && SocketManager.Instance != null)
        {
            SocketManager.Instance.OnPeerMoveReceived -= OnPeerMove;
            SocketManager.Instance.OnNewTurnReceived -= OnServerNewTurn;
            SocketManager.Instance.OnPeerChargingReceived -= OnPeerCharging;
            SocketManager.Instance.OnPeerThrowReceived -= OnPeerThrow;
            SocketManager.Instance.OnPeerHitReceived -= OnPeerHit;
            SocketManager.Instance.OnGameOverReceived -= OnGameOver;
            SocketManager.Instance.OnReciveChatReceived -= OnReciveChar;
            SocketManager.Instance.OnReciveStickReceived -= OnReciveStick;
            SocketManager.Instance.OnReciveTurnCountDownReceived -= OnReciveTurnCountDown;
        }
    }

    /// <summary>
    /// 監聽Server事件
    /// </summary>
    private void ListenServerEvent()
    {
        if (StaticDataManager.PlayType == PLAY_TYPE.Match)
        {
            SocketManager.Instance.OnPeerMoveReceived += OnPeerMove;
            SocketManager.Instance.OnNewTurnReceived += OnServerNewTurn;
            SocketManager.Instance.OnPeerChargingReceived += OnPeerCharging;
            SocketManager.Instance.OnPeerThrowReceived += OnPeerThrow;
            SocketManager.Instance.OnPeerHitReceived += OnPeerHit;
            SocketManager.Instance.OnGameOverReceived += OnGameOver;
            SocketManager.Instance.OnReciveChatReceived += OnReciveChar;
            SocketManager.Instance.OnReciveStickReceived += OnReciveStick;
            SocketManager.Instance.OnReciveTurnCountDownReceived += OnReciveTurnCountDown;
        }
    }

    /// <summary>
    /// 接收:新回合通知
    /// </summary>
    private void OnServerNewTurn(NewTurnData data)
    {
        string json = JsonUtility.ToJson(data);
        Debug.Log($"接收:新回合通知: {json}");

        // 關閉回合倒數
        _context.GameView.CloseCountDown();

        // 同步Hp
        _context.GameView.UpdateHpBar(true, data.p1Hp);
        _context.GameView.UpdateHpBar(false, data.p2Hp);

        // 判斷當前回合行動玩家(0 = Player1, 1 = Player2)
        CharacterView targetCharacter = (data.currentTurnSeat == 0)
            ? _context.P1_CharacterView
            : _context.P2_CharacterView;

        _context.GameController.SetTurn(targetCharacter);

        // 當前回合風力
        float windStrength = data.windStrength;
        _throwController.WindStrength = windStrength;
        _context.GameView.SetWindStrength(windStrength);

        // 強制角色停止移動
        _context.GameController.AllCharacterStop();
    }

    /// <summary>
    /// 接收:角色移動
    /// </summary>
    public void OnPeerMove(MoveData data)
    {
        string json = JsonUtility.ToJson(data);
        Debug.Log($"接收:角色移動: {json}");

        if (_context.CurrentTurnCharacter == null) return;
        _context.CurrentTurnCharacter.SetMove(data.inputDir, data.posX);
    }

    /// <summary>
    /// 接收:畜力狀態
    /// </summary>
    private void OnPeerCharging(ChargingData data)
    {
        string json = JsonUtility.ToJson(data);
        Debug.Log($"接收:畜力狀態: {json}");

        // 關閉回合倒數
        _context.GameView.CloseCountDown();

        if (data.isCharging)
        {
            _throwController.SetChargingState();
        }
        else
        {
            _throwController.StartThrow();
        }

        // 強制角色停止移動
        _context.GameController.AllCharacterStop();
    }

    /// <summary>
    /// 接收:投擲
    /// </summary>
    private void OnPeerThrow(ThrowData data)
    {
        string json = JsonUtility.ToJson(data);
        Debug.Log($"接收:投擲: {json}");

        // 同步參數狀態
        _throwController.ThrowType = (THROW_TYPE)data.throwType;
        _throwController.ThrowStrength = data.force;
        // 關閉投擲力道
        _context.CurrentTurnCharacter.CloseThrowStrength();
        // 計算投擲位置
        Vector3 throwTargetPos = _throwController.GetThrowTargetPos(data.force);
        _throwController.ThrowTargetPos = throwTargetPos;
        // 撥放投擲動畫
        _context.CurrentTurnCharacter.PlayThrowAnimation((THROW_TYPE)data.throwType, throwTargetPos);
        // 關閉回合倒數
        _context.GameView.CloseCountDown();
    }

    /// <summary>
    /// 接收:擊中
    /// </summary>
    private void OnPeerHit(HitData data)
    {
        string json = JsonUtility.ToJson(data);
        Debug.Log($"接收:擊中: {json}");

        // 判斷擊中玩家(0 = Player1, 1 = Player2)
        CharacterView hitCharacter = (data.targetSeat == 0)
            ? _context.P1_CharacterView
            : _context.P2_CharacterView;

        var throwView = _context.ThrowObjectView;
        throwView.OnHit();

        if (hitCharacter == null)
        {
            Debug.LogError("找不到防守方角色");
            return;
        }

        if (data.damage > 0)
        {
            // 判斷被擊中對象
            bool isP1 = data.targetSeat == 0;
            int getHitTargetHp = isP1 ? data.p1Hp : data.p2Hp;

            // 撥放動畫
            if (getHitTargetHp > 0)
            {
                hitCharacter.PlayHurtAnimation((THROW_TYPE)data.throwType);
            }
            else
            {
                hitCharacter.PlayDeathAnimation();
            }

            // 同步Hp
            _context.GameView.UpdateHpBar(true, data.p1Hp);
            _context.GameView.UpdateHpBar(false, data.p2Hp);
        }
        else
        {
            // 閃避
            if (data.damage == -1) hitCharacter.PlayDodgeAnimation();
            // 未擊中
            else hitCharacter.PlayDerideAnimation();
        }
    }

    /// <summary>
    /// 接收:遊戲結束
    /// </summary>
    private void OnGameOver(GameOverData data)
    {
        _context.GameController.IsGameOver.Value = true;

        // 關閉回合倒數
        _context.GameView.CloseCountDown();

        string winMessage = "";
        if (string.IsNullOrEmpty(data.message))
        {
            winMessage = $"Winner: {data.winnerNickname}";
        }
        else
        {
            // 有訊息代表對手中途斷線
            winMessage = data.message;
        }

        ViewManager.Instance.OpenView<GameOverView>(
            viewType: VIEW_TYPE.GameOverView,
            canvasType: CANVAS_TYPE.Canvas_Highest,
            callback: (view) =>
            {
                view.SetResult(winMessage);
            }).Forget();
    }

    /// <summary>
    /// 接收:聊天訊息
    /// </summary>
    private void OnReciveChar(ReciveChatData data)
    {
        CharacterView sendPlayer = data.senderSeat == 0 ? 
            _context.P1_CharacterView : 
            _context.P2_CharacterView;

        sendPlayer.ShowTextBubble(data.chatMessage);
    }

    /// <summary>
    /// 接收:貼圖訊息
    /// </summary>
    public void OnReciveStick(ReciveStickData data)
    {
        CharacterView sendPlayer = data.senderSeat == 0 ?
            _context.P1_CharacterView :
            _context.P2_CharacterView;

        sendPlayer.ShowStick(data.stickIndex);
    }

    /// <summary>
    /// 接收:回合倒數
    /// </summary>
    public void OnReciveTurnCountDown(ReciveTurnCountDownData data)
    {
        _context.GameView.ShowCountDown(data.secondsLeft);
    }
}
