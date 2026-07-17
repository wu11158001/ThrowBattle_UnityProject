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
            _context.CurrentTurnCharacter.SetControlTip(true);
            UpdateMoveControlPanelVisibility(true);
        }
        else
        {
            UpdateMoveControlPanelVisibility(false);
        }

        // 通知子控制器重置當前狀態
        _moveController.ResetState();
        _throwController.ResetState();

        // 設置風力強度與方向
        float windStrength = UnityEngine.Random.Range(-config.WindMaxStrength, config.WindMaxStrength);
        _throwController.SetWindStrength(windStrength);
        _context.GameView.SetWindStrength(windStrength);
    }

    // 接收來自 GameView 的原生輸入訊號並轉發給子控制器
    public void SetInputDirection(float direction) => _moveController.SetInputDirection(direction);
    public void SetThrowPressState(bool isPressing) => _throwController.SetThrowPressState(isPressing);

    /// <summary>
    /// 投擲結束
    /// </summary>
    public void OnThrowComplete()
    {
        SwitchTurn();
    }

    /// <summary>
    /// 切換操作面板顯示狀態
    /// </summary>
    /// <param name="isActive"></param>
    public void UpdateMoveControlPanelVisibility(bool isActive)
    {
        _context.GameView.SetMoveControlActive(isActive);
    }

    /// <summary>
    /// 切換回合
    /// </summary>
    private void SwitchTurn()
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