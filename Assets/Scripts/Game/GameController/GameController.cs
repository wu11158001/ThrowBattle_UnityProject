using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 遊戲控制器
/// </summary>
public class GameController : MonoBehaviour
{
    private GameplayContext _context;

    // 子控制器
    private CharacterMoveController _moveController;
    private CharacterThrowController _throwController;

    /// <summary>
    /// 是否遊戲結束
    /// </summary>
    public ReactiveProperty<bool> IsGameOver = new ReactiveProperty<bool>(false);

    /// <summary>
    /// 移動輸入方向接收
    /// </summary>
    /// <param name="direction"></param>
    public void SetInputDirection(float direction) => _moveController.SetInputDirection(direction);

    /// <summary>
    /// 設置投擲蓄力狀態
    /// </summary>
    /// <param name="isPressing"></param>
    public void SetThrowPressState(bool isPressing) => _throwController.SetThrowPressState(isPressing);

    /// <summary>
    /// 設置下次投擲的類型
    /// </summary>
    /// <param name="type"></param>
    public void SetNextThrowType(THROW_TYPE type) => _throwController.SetNextThrowType(type);

    /// <summary>
    /// 執行投擲
    /// </summary>
    public void ExecuteThrow() => _throwController.ExecuteThrow();

    private void Start()
    {
        _context = GameplayManager.CurrentContext;

        _moveController = new();
        _throwController = new(_moveController);

        Bind();
    }

    private void Bind()
    {
        // 每幀驅動
        this.UpdateAsObservable()
            .Where(_ => _context.CurrentTurnCharacter != null)
            .Subscribe(_ =>
            {
                // 連線權限檢查
                if (StaticDataManager.PlayType == PLAY_TYPE.Match)
                {
                    bool isMyTurn = StaticDataManager.MatchData.isCreator
                        ? _context.CurrentTurnCharacter == _context.P1_CharacterView
                        : _context.CurrentTurnCharacter == _context.P2_CharacterView;

                    if (!isMyTurn) return;
                }

                // 驅動子控制器（投擲優先級高於移動）
                _throwController.Tick();

                // 只有在沒有蓄力時，才驅動移動邏輯
                if (!_throwController.IsCharging)
                {
                    _moveController.Tick();
                }
            })
            .AddTo(this);
    }

    /// <summary>
    /// 遊戲開始
    /// </summary>
    public void StartGameplay()
    {
        SetTurn(_context.P1_CharacterView);
    }

    /// <summary>
    /// 設定新回合
    /// </summary>
    public void SetTurn(CharacterView targetCharacter)
    {
        var config = StaticDataManager.DataConfig;

        // 舊操作者清理
        if (_context.CurrentTurnCharacter != null)
        {
            _context.CurrentTurnCharacter.SetControlTip(false);
        }

        _context.CurrentTurnCharacter = targetCharacter;

        // 新操作者初始化
        if (_context.CurrentTurnCharacter != null)
        {
            // 本地顯示操作提示
            _context.CurrentTurnCharacter.SetControlTip(_context.CurrentTurnCharacter.IsLocalPlayer);

            // 本地玩家的回合設置
            SetIsLocalTurn(_context.CurrentTurnCharacter.IsLocalPlayer);

            // 如果是 AI 的回合，觸發 AI 的大腦驅動
            if (!_context.CurrentTurnCharacter.IsLocalPlayer)
            {
                
            }
        }
        else
        {
            SetIsLocalTurn(false);
        }

        // 通知子控制器重置當前狀態
        _moveController.ResetState();
        _throwController.ResetState();

        // 設置風力強度與方向
        float windStrength = UnityEngine.Random.Range(-config.WindMaxStrength, config.WindMaxStrength);
        _throwController.SetWindStrength(windStrength);
        _context.GameView.SetWindStrength(windStrength);
    }

    /// <summary>
    /// 設置是否是本地回合
    /// </summary>
    /// <param name="isLocalTurn"></param>
    public void SetIsLocalTurn(bool isLocalTurn)
    {
        _context.GameView.SetIsLocalTurn(isLocalTurn);
    }

    /// <summary>
    /// 切換回合
    /// </summary>
    public void SwitchTurn()
    {
        if (StaticDataManager.PlayType == PLAY_TYPE.TwoPlayer)
        {
            var nextCharacter = (_context.CurrentTurnCharacter == _context.P1_CharacterView)
                ? _context.P2_CharacterView
                : _context.P1_CharacterView;
            SetTurn(nextCharacter);
        }
        else if (StaticDataManager.PlayType == PLAY_TYPE.WithAi)
        {
            SetTurn(_context.P2_CharacterView);
        }
        else if (StaticDataManager.PlayType == PLAY_TYPE.Match)
        {
            SetTurn(null);
        }
    }
}