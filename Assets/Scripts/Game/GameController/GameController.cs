using UnityEngine;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 遊戲控制器
/// </summary>
public class GameController : MonoBehaviour
{
    private GameplayContext _context;
    public DataConfig _dataConfig;
    // 子控制器
    public CharacterMoveController MoveController { get; private set; }
    public CharacterThrowController ThrowController { get; private set; }
    public GmaeAPISendAndRecive GmaeAPISendAndRecive { get; private set; }

    private void OnDestroy()
    {
        GmaeAPISendAndRecive.RemoveListenServerEvent();
    }

    private void Start()
    {
        _context = GameplayManager.CurrentContext;
        _dataConfig = StaticDataManager.DataConfig;

        MoveController = new();
        ThrowController = new(MoveController);
        GmaeAPISendAndRecive = new(ThrowController);

        Bind();
    }

    private void Bind()
    {
        // 每幀驅動
        this.UpdateAsObservable()
            .Where(_ => _context.CurrentTurnCharacter != null)
            .Subscribe(_ =>
            {                
                ThrowController.Tick();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 遊戲開始
    /// </summary>
    public void StartGameplay()
    {
        if (StaticDataManager.PlayType == PLAY_TYPE.Match)
        {
            Debug.Log("[GameController] 連線模式開場動畫結束，等待 Server 驅動第一回合...");
        }
        else
        {
            SetTurn(_context.P1_CharacterView);
        }
    }

    /// <summary>
    /// 設定新回合
    /// </summary>
    public void SetTurn(CharacterView targetCharacter)
    {
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
            _context.CurrentTurnCharacter.SetControlTip(true);

            // 本地玩家的回合設置
            _context.GameView.SetIsLocalTurn(_context.CurrentTurnCharacter.IsLocalPlayer);

            // 如果是 AI 的回合，觸發 AI 的大腦驅動
            if (!_context.CurrentTurnCharacter.IsLocalPlayer)
            {
                
            }
        }
        else
        {
            _context.GameView.SetIsLocalTurn(false);
        }

        ThrowController.ResetState();

        // 設置風力強度與方向
        float windStrength = UnityEngine.Random.Range(-_dataConfig.WindMaxStrength, _dataConfig.WindMaxStrength);
        ThrowController.WindStrength = windStrength;
        _context.GameView.SetWindStrength(windStrength);
    }

    /// <summary>
    /// 切換回合
    /// </summary>
    public void SwitchTurn()
    {
        if (_context.GameController.IsGameOver.Value) return;

        switch (StaticDataManager.PlayType)
        {
            case PLAY_TYPE.WithAi:
                SetTurn(_context.P2_CharacterView);
                break;

            case PLAY_TYPE.TwoPlayer:
                var nextCharacter = (_context.CurrentTurnCharacter == _context.P1_CharacterView)
                    ? _context.P2_CharacterView
                    : _context.P1_CharacterView;

                SetTurn(nextCharacter);
                break;
        }
    }

    /// <summary>
    /// 移動輸入方向接收
    /// </summary>
    /// <param name="direction"></param>
    public void SetInputDirection(float direction) => MoveController.SetInputDirection(direction);

    /// <summary>
    /// 設置投擲蓄力狀態
    /// </summary>
    /// <param name="isPressing"></param>
    public void SetChargingState(bool isCharging) => ThrowController.SetInputCharging(isCharging);

    /// <summary>
    /// 設置投擲的類型
    /// </summary>
    /// <param name="type"></param>
    public void SetThrowType(THROW_TYPE type) => ThrowController.ThrowType = type;

    /// <summary>
    /// 執行投擲
    /// </summary>
    public void ExecuteThrow() => ThrowController.ExecuteThrow();

    /// <summary>
    /// 是否遊戲結束
    /// </summary>
    public ReactiveProperty<bool> IsGameOver = new ReactiveProperty<bool>(false);
}