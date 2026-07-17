using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

/// <summary>
/// 角色
/// </summary>
public class CharacterView : BaseObject
{
    [SerializeField] private Animator _p1_Anim;
    [SerializeField] private TextMeshPro _text_Nickname;
    [SerializeField] private GameObject _controlTip;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("投擲力道")]
    [SerializeField] private Canvas _throwStrengthCanvas;
    [SerializeField] private Image _img_ThrowStrength;

    // 初始面向方向
    private bool _initFillX;

    // 移動屬性
    private float _moveSpeed ;
    private Vector2 _moveRange;

    // 動畫
    private readonly int _isMovingParamId = Animator.StringToHash("IsMoving");
    private readonly int _hurtParamId = Animator.StringToHash("Hurt");
    private readonly int _hurt_GiantParamId = Animator.StringToHash("Hurt_Giant");
    private readonly int _hurt_DoubleDamageParamId = Animator.StringToHash("Hurt_DoubleDamage");
    private readonly int _normalAttackParamId = Animator.StringToHash("NormalAttack");
    private readonly int _skill_DoubleDamageParamId = Animator.StringToHash("Skill_DoubleDamage");
    private readonly int _skill_GiantParamId = Animator.StringToHash("Skill_Giant");
    private readonly int _derideParamId = Animator.StringToHash("Deride");

    private void Start()
    {
        _throwStrengthCanvas.worldCamera = Camera.main;
        CloseThrowStrength();

        var config = StaticDataManager.DataConfig;
        _moveSpeed = config.CharacterMoveSpeed;
        _moveRange = config.CharacterMoveRange;
    }

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        _controlTip.SetActive(false);
    }

    /// <summary>
    /// 角色設定
    /// </summary>
    /// <param name="isPlayer1"></param>
    public void SetCharacter(bool isPlayer1)
    {
        // 設置初始位置(預設在最遠處, player1在右側)
        transform.position =
            isPlayer1 ?
            new(StaticDataManager.DataConfig.CharacterMoveRange.y, StaticDataManager.DataConfig.CharacterPosY, 0) :
            new(-StaticDataManager.DataConfig.CharacterMoveRange.y, StaticDataManager.DataConfig.CharacterPosY, 0);

        // 設置角色暱稱
        switch (StaticDataManager.PlayType)
        {
            // 連線配對
            case PLAY_TYPE.Match:
                MatchSuccessData matchData = StaticDataManager.MatchData;
                string localPlayerNickname = StaticDataManager.RegisterPlayerData.Nickname;
                string opponentPlayerNickname = matchData.opponentNickname;

                _text_Nickname.text = isPlayer1 && matchData.isCreator ? 
                    localPlayerNickname : 
                    opponentPlayerNickname;
                break;

            // AI對戰
            case PLAY_TYPE.WithAi:
                _text_Nickname.text = isPlayer1 ? $"{StaticDataManager.RegisterPlayerData.Nickname}" : "AI";
                break;

            // 兩名玩家
            case PLAY_TYPE.TwoPlayer:
                _text_Nickname.text = isPlayer1 ? "Player1" : "Player2";
                break;
        }

        // 設置角色顏色
        string colorHtml = isPlayer1 ?
            $"#{StaticDataManager.DataConfig.CharacterColor_P1}" :
            $"#{StaticDataManager.DataConfig.CharacterColor_P2}";

        if (ColorUtility.TryParseHtmlString(colorHtml, out Color customColor))
        {
            _spriteRenderer.color = customColor;
        }

        // 設置角色面向方向
        _initFillX = isPlayer1 ? true : false;
        _spriteRenderer.flipX = _initFillX;
    }

    /// <summary>
    /// 顯示投擲力道
    /// </summary>
    /// <param name="value">投擲力道(0~1)</param>
    public void ShowThrowStrength(float value)
    {
        if (!_throwStrengthCanvas.gameObject.activeSelf) _throwStrengthCanvas.gameObject.SetActive(true);

        _img_ThrowStrength.fillAmount = value;
    }

    /// <summary>
    /// 關閉投擲力道
    /// </summary>
    public void CloseThrowStrength()
    {
        _throwStrengthCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// 設定控制提示顯示狀態
    /// </summary>
    /// <param name="isActive"></param>
    public void SetControlTip(bool isActive)
    {
        if (_controlTip != null)
        {
            _controlTip.SetActive(isActive);
        }
    }

    /// <summary>
    /// 角色移動
    /// </summary>
    /// <param name="direction">-1 表示向左，1 表示向右</param>
    public void Move(float direction)
    {
        // 計算新位置
        Vector3 newPos = transform.position + new Vector3(direction * _moveSpeed * Time.deltaTime, 0, 0);

        // 限制移動範圍
        if(_initFillX)
        {
            // 角色在右側
            newPos.x = Mathf.Clamp(newPos.x, _moveRange.x, _moveRange.y);
        }
        else
        {
            // 角色在左側
            newPos.x = Mathf.Clamp(newPos.x, -_moveRange.y, -_moveRange.x);
        }

        transform.position = newPos;

        // 播放動畫
        if (_p1_Anim != null)
        {
            _p1_Anim.SetBool(_isMovingParamId, Mathf.Abs(direction) > 0);

            // 角色面向
            if(Mathf.Abs(direction) > 0)
            {
                _spriteRenderer.flipX =
                    direction < 0 ?
                    true :
                    false;
            }
            else
            {
                _spriteRenderer.flipX = _initFillX;
            }
        }
    }
}
