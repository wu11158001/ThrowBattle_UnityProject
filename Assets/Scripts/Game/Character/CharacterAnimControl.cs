using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 角色動畫控制
/// </summary>
public class CharacterAnimControl : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    private readonly int _isMovingParamId = Animator.StringToHash("IsMoving");
    private readonly int _dodgeParamId = Animator.StringToHash("Dodge");
    private readonly int _hurtParamId = Animator.StringToHash("Hurt");
    private readonly int _hurt_GiantParamId = Animator.StringToHash("Hurt_Giant");
    private readonly int _hurt_StrengthDamageParamId = Animator.StringToHash("Hurt_StrengthDamage");
    private readonly int _normalAttackParamId = Animator.StringToHash("NormalAttack");
    private readonly int _skill_StrengthDamageParamId = Animator.StringToHash("Skill_StrengthDamage");
    private readonly int _skill_GiantParamId = Animator.StringToHash("Skill_Giant");
    private readonly int _derideParamId = Animator.StringToHash("Deride");
    private readonly int _deathParamId = Animator.StringToHash("Death");
    private readonly int _chargingParamId = Animator.StringToHash("Charging");

    private GameplayContext _context;

    private void Start()
    {
        _context = GameplayManager.CurrentContext;
    }

    /// <summary>
    /// 執行投擲(動畫影格觸發)
    /// </summary>
    public void ExecuteThrow()
    {
        _context.GameController.ExecuteThrow();
    }

    /// <summary>
    /// 這回合結束(影格觸發)
    /// </summary>
    public void OnThisTurnFinish()
    {
        if (StaticDataManager.PlayType == PLAY_TYPE.Match)
        {
            if(_context.CurrentTurnCharacter.IsLocalPlayer)
            {
                TurnEndData data = new()
                {
                    roomId = StaticDataManager.MatchData.roomId
                };

                SocketManager.Instance.SendTurnEnd(data);
            }
        }
        else
        {
            SwitchTurnAsync().Forget();
        }        
    }

    /// <summary>
    /// 切換回合(本地遊玩)
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid SwitchTurnAsync()
    {
        bool isCanceled = await UniTask.Delay(1000, 
            cancellationToken: this.GetCancellationTokenOnDestroy())
            .SuppressCancellationThrow();

        if (isCanceled) return;

        _context.GameController.SwitchTurn();
    }

    /// <summary>
    /// 移動動畫控制
    /// </summary>
    /// <param name="isMove"></param>
    public void MoveAnimationControl(bool isMove)
    {
        _anim.SetBool(_isMovingParamId, isMove);
    }

    /// <summary>
    /// 撥放蓄力動畫
    /// </summary>
    public void PlayChargingAnimation()
    {
        _anim.SetTrigger(_chargingParamId);
    }

    /// <summary>
    /// 撥放嘲諷動畫
    /// </summary>
    public void PlayDerideAnimation()
    {
        _anim.SetTrigger(_derideParamId);
    }

    /// <summary>
    /// 撥放閃避動畫
    /// </summary>
    public void PlayDodgeAnimation()
    {
        _anim.SetTrigger(_dodgeParamId);
    }

    /// <summary>
    /// 撥放投擲動畫
    /// </summary>
    /// <param name="type"></param>
    public void PlayThrowAnimation(THROW_TYPE type)
    {
        switch (type)
        {
            case THROW_TYPE.Normal:
                _anim.SetTrigger(_normalAttackParamId);
                break;

            case THROW_TYPE.Giant:
                _anim.SetTrigger(_skill_GiantParamId);
                break;

            case THROW_TYPE.StrengthDamage:
                _anim.SetTrigger(_skill_StrengthDamageParamId);
                break;
        }
    }

    /// <summary>
    /// 撥放受擊動畫
    /// </summary>
    /// <param name="type"></param>
    public void PlayHurtAnimation(THROW_TYPE type)
    {
        switch (type)
        {
            case THROW_TYPE.Normal:
                _anim.SetTrigger(_hurtParamId);
                break;

            case THROW_TYPE.Giant:
                _anim.SetTrigger(_hurt_GiantParamId);
                break;

            case THROW_TYPE.StrengthDamage:
                _anim.SetTrigger(_hurt_StrengthDamageParamId);
                break;
        }
    }

    /// <summary>
    /// 撥放死亡動畫
    /// </summary>
    public void PlayDeathAnimation()
    {
        _anim.SetTrigger(_deathParamId);
    }

    /// <summary>
    /// 開啟遊戲結束介面(影格觸發)
    /// </summary>
    public void OpenGameOverView()
    {
        string winner = "";

        bool isP1 = (_context.CurrentTurnCharacter == _context.P1_CharacterView);
        string localPlayer = StaticDataManager.RegisterPlayerData.Nickname;

        switch (StaticDataManager.PlayType)
        {
            case PLAY_TYPE.Match:
                // 發送:回合結束判斷勝負
                OnThisTurnFinish();
                return;

            case PLAY_TYPE.WithAi:
                winner = isP1 ? localPlayer : "AI";
                break;

            case PLAY_TYPE.TwoPlayer:
                
                winner = isP1 ? "Player1" : "Player2";
                break;
        }

        ViewManager.Instance.OpenView<GameOverView>(
            viewType: VIEW_TYPE.GameOverView,
            canvasType: CANVAS_TYPE.Canvas_Highest,
            callback: (view) =>
            {
                view.SetResult($"Winner: {winner}");
            }).Forget();
    }
}
