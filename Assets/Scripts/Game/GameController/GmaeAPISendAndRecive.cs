using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 遊戲API發送與接
/// </summary>
public class GmaeAPISendAndRecive
{
    private GameplayContext _context;
    private CharacterThrowController _throwController;

    public GmaeAPISendAndRecive(CharacterThrowController throwController)
    {
        _throwController = throwController;

        _context = GameplayManager.CurrentContext;

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
            SocketManager.Instance.OnPeerAimReceived -= OnPeerAim;
            SocketManager.Instance.OnPeerThrowReceived -= OnPeerThrow;
            SocketManager.Instance.OnPeerHitReceived -= OnPeerHit;
            SocketManager.Instance.OnGameOverReceived -= OnGameOver;
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
            SocketManager.Instance.OnPeerAimReceived += OnPeerAim;
            SocketManager.Instance.OnPeerThrowReceived += OnPeerThrow;
            SocketManager.Instance.OnPeerHitReceived += OnPeerHit;
            SocketManager.Instance.OnGameOverReceived += OnGameOver;
        }
    }

    #region 接收 Server 事件
    /// <summary>
    /// 接收:對手的位置同步
    /// </summary>
    public void OnPeerMove(MoveData data)
    {
        if (_context.CurrentTurnCharacter == null) return;
        _context.CurrentTurnCharacter.OnReceivePeerMove(data);
    }

    /// <summary>
    /// 接收:新回合通知
    /// </summary>
    private void OnServerNewTurn(NewTurnData data)
    {
        // 同步Hp
        _context.GameView.UpdateHpBar(true, data.p1Hp);
        _context.GameView.UpdateHpBar(false, data.p2Hp);

        // 判斷當前回合行動玩家(0 = Player1, 1 = Player2)
        CharacterView targetCharacter = (data.currentTurnSeat == 0)
            ? _context.P1_CharacterView
            : _context.P2_CharacterView;

        Debug.Log($"收到 Server 回合切換通知！當前玩家編號(0 = P1, 1 = P2): {data.currentTurnSeat}");

        _context.GameController.SetTurn(targetCharacter);

        // 當前回合風力
        float windStrength = data.windStrength;
        _throwController.WindStrength = windStrength;
        _context.GameView.SetWindStrength(windStrength);
    }

    /// <summary>
    /// 接收:對手畜力狀態
    /// </summary>
    private void OnPeerAim(AimData data)
    {
        _context.CurrentTurnCharacter.ShowThrowStrength(data.force);
    }

    /// <summary>
    /// 接收:投擲
    /// </summary>
    private void OnPeerThrow(ThrowData data)
    {
        // 同步參數狀態
        _throwController.ThrowType = (THROW_TYPE)data.throwType;
        _throwController.ThrowStrength = data.force;

        // 計算投擲位置
        Vector3 throwTargetPos = _throwController.GetNextThrowTargetPos(data.force);
        _throwController.ThrowTargetPos = throwTargetPos;

        // 關閉投擲力道顯示
        _context.CurrentTurnCharacter.CloseThrowStrength();

        // 撥放投擲動畫
        _context.CurrentTurnCharacter.PlayThrowAnimation((THROW_TYPE)data.throwType, throwTargetPos);
    }

    /// <summary>
    /// 接收:擊中
    /// </summary>
    private void OnPeerHit(HitData data)
    {
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
            hitCharacter.PlayDerideAnimation();
        }
    }

    /// <summary>
    /// 接收:遊戲結束
    /// </summary>
    private void OnGameOver(GameOverData data)
    {
        _context.GameController.IsGameOver.Value = true;

        string winMessage = "";
        if (string.IsNullOrEmpty(data.message))
        {
            string localPlayer = StaticDataManager.RegisterPlayerData.Nickname;
            string winner = data.winnerSeat == 0 ?
                StaticDataManager.MatchData.opponentNickname :
                localPlayer;

            winMessage = $"Winner: {winner}";
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
    #endregion
}
