using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;

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
    public AIBrain _aIBrain;


    private void OnDestroy()
    {
        GmaeAPISendAndRecive.RemoveListenServerEvent();
    }

    private void Start()
    {
        _context = GameplayManager.CurrentContext;
        _dataConfig = StaticDataManager.DataConfig;

        MoveController = new();
        ThrowController = new();
        GmaeAPISendAndRecive = new();

        GameObject obj = new("AIBrain");
        obj.transform.SetParent(transform);
        AIBrain aIBrain = obj.AddComponent<AIBrain>();
        _aIBrain = aIBrain;

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
    public void StartGamePlay()
    {
        if (StaticDataManager.PlayType == PLAY_TYPE.Match)
        {
            Debug.Log("[GameController] 連線模式開場動畫結束，等待 Server 驅動第一回合...");
        }
        else
        {
            // 單機模式始終由Player1開始
            SetTurn(_context.P1_CharacterView);
        }
    }

    /// <summary>
    /// 設定新回合
    /// </summary>
    public void SetTurn(CharacterView targetCharacter)
    {
        ThrowController.ResetState();

        if(StaticDataManager.PlayType != PLAY_TYPE.Match)
        {
            // 設置風力強度與方向
            float windStrength = UnityEngine.Random.Range(-_dataConfig.WindMaxStrength, _dataConfig.WindMaxStrength);
            ThrowController.WindStrength = windStrength;
            _context.GameView.SetWindStrength(windStrength);
        }

        // 舊操作者清理
        if (_context.CurrentTurnCharacter != null)
        {
            _context.CurrentTurnCharacter.SetControlTip(false);
        }

        _context.CurrentTurnCharacter = targetCharacter;

        // 新操作者初始化
        if (_context.CurrentTurnCharacter != null)
        {
            bool isLocalPlayer = _context.CurrentTurnCharacter.IsLocalPlayer;

            // 本地顯示操作提示
            _context.CurrentTurnCharacter.SetControlTip(true);
            // 本地玩家的回合設置
            _context.GameView.SetIsLocalTurn(isLocalPlayer);

            // 如果是 AI 的回合，觸發 AI 的大腦驅動
            if (StaticDataManager.PlayType == PLAY_TYPE.WithAi && !isLocalPlayer)
            {
                _aIBrain.RequestAIDecision();
            }
        }
        else
        {
            _context.GameView.SetIsLocalTurn(false);
        }
    }

    /// <summary>
    /// 切換回合(本地遊玩)
    /// </summary>
    public void SwitchTurn()
    {
        if (_context.GameController.IsGameOver.Value) return;

        var nextCharacter = (_context.CurrentTurnCharacter == _context.P1_CharacterView)
                    ? _context.P2_CharacterView
                    : _context.P1_CharacterView;

        SetTurn(nextCharacter);
    }

    /// <summary>
    /// 強制角色停止移動
    /// </summary>
    public void AllCharacterStop()
    {
        _context.P1_CharacterView.SetMove(0, _context.P1_CharacterView.transform.position.x);
        _context.P2_CharacterView.SetMove(0, _context.P2_CharacterView.transform.position.x);
        SetInputDirection(0);
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