using UnityEngine;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 角色
/// </summary>
public class CharacterView : BaseObject
{
    [SerializeField] private CharacterAnimControl _characterAnimControl;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private TextMeshPro _text_Nickname;
    [SerializeField] private Canvas _uiCanvas;

    [Header("控制圖示")]
    [SerializeField] private GameObject _controlTip;

    [Header("投擲力道")]
    [SerializeField] private GameObject _chargingObj;
    [SerializeField] private Image _img_Charging;

    [Header("文字泡泡")]
    [SerializeField] private GameObject _textBubbleObj;
    [SerializeField] private CanvasGroup _textBubbleCanvasGroup;
    [SerializeField] private TextMeshProUGUI _text_Bubble;

    [Header("貼圖")]
    [SerializeField] private GameObject _stickObj;
    [SerializeField] private CanvasGroup _stickCanvasGroup;
    [SerializeField] private Image _img_Stick;

    // 初始面向方向
    private bool _initFillX;

    // 移動屬性
    private float _moveSpeed ;
    private Vector2 _moveRange;

    // Hp屬性
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }

    /// <summary>
    /// 是否為本地玩家可控制的角色
    /// </summary>
    public bool IsLocalPlayer { get; set; }

    // 紀錄強化攻擊瞬移初始位置
    private Vector3 _teleportInitPos;
    // 紀錄強化攻擊瞬移位置
    private Vector3 _teleportTargetPos;

    // 移動輸入值
    private float _inputDir = 0;

    private GameplayContext _context;
    private DataConfig _dataConfig;

    public override void OnDestroy()
    {
        _textBubbleCanvasGroup.DOKill();
        _textBubbleCanvasGroup.DOKill();
        transform.DOKill();

        base.OnDestroy();
    }

    private void Start()
    {
        _context = GameplayManager.CurrentContext;
        _dataConfig = StaticDataManager.DataConfig;

        _moveSpeed = _dataConfig.CharacterMoveSpeed;
        _moveRange = _dataConfig.CharacterMoveRange;

        _uiCanvas.worldCamera = Camera.main;
        CloseThrowStrength();

        _textBubbleObj.SetActive(false);
        _stickObj.SetActive(false);

        _controlTip.SetActive(false);
        _controlTip.transform.DOLocalMoveY(_controlTip.transform.localPosition.y + 0.2f, 0.5f)
            .SetEase(ease: Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject)
            .SetTarget(_controlTip);

        Bind();
    }

    private void Bind()
    {
        // 每幀驅動
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                UpdateMovement();
            })
            .AddTo(this); ;
    }

    /// <summary>
    /// 角色設定
    /// </summary>
    /// <param name="isPlayer1"></param>
    public void SetCharacter(bool isPlayer1)
    {
        // 設置初始位置(預設在最遠處, player1在右側)
        transform.position = isPlayer1 ?
            new(_dataConfig.CharacterMoveRange.y, _dataConfig.CharacterPosY, 0) :
            new(-_dataConfig.CharacterMoveRange.y, _dataConfig.CharacterPosY, 0);

        // 設置角色暱稱與 IsLocalPlayer
        switch (StaticDataManager.PlayType)
        {
            // 連線配對
            case PLAY_TYPE.Match:
                MatchSuccessData matchData = StaticDataManager.MatchData;
                string localPlayerNickname = StaticDataManager.RegisterPlayerData.Nickname;
                string opponentPlayerNickname = matchData.opponentNickname;

                bool isMine = (isPlayer1 && matchData.mySeat == 0) || (!isPlayer1 && matchData.mySeat == 1);

                _text_Nickname.text = isMine ? localPlayerNickname : opponentPlayerNickname;
                IsLocalPlayer = isMine;
                break;

            // AI對戰
            case PLAY_TYPE.WithAi:
                _text_Nickname.text = isPlayer1 ? $"{StaticDataManager.RegisterPlayerData.Nickname}" : "AI";

                IsLocalPlayer = isPlayer1;
                break;

            // 兩名玩家 (單機雙人)
            case PLAY_TYPE.TwoPlayer:
                _text_Nickname.text = isPlayer1 ? "Player1" : "Player2";

                IsLocalPlayer = true;
                break;
        }

        // 設置角色顏色
        string colorHtml = isPlayer1 ?
            $"#{_dataConfig.CharacterColor_P1}" :
            $"#{_dataConfig.CharacterColor_P2}";

        if (ColorUtility.TryParseHtmlString(colorHtml, out Color customColor))
        {
            _spriteRenderer.color = customColor;
        }

        // 設置角色面向方向
        _initFillX = isPlayer1;
        _spriteRenderer.flipX = _initFillX;

        // Hp
        MaxHp = _dataConfig.CharacterMaxHp;
        CurrentHp = MaxHp;
    }

    /// <summary>
    /// 設置移動
    /// </summary>
    /// <param name="inputDir">0 = 停止, -1 = 向左, 1=向右</param>
    /// <param name="targetPosX">同步使用(停止時的位置)</param>
    public void SetMove(float inputDir, float targetPosX)
    {
        _inputDir = inputDir;

        // 停止移動時更新位置
        if (_inputDir == 0)
        {
            Vector3 pos = transform.position;
            pos.x = targetPosX;
            transform.position = pos;
        }

        // 播放動畫
        _characterAnimControl.MoveAnimationControl(Mathf.Abs(_inputDir) > 0);

        // 角色面向
        if (Mathf.Abs(_inputDir) > 0)
        {
            _spriteRenderer.flipX = _inputDir < 0 ? true : false;
        }
        else
        {
            _spriteRenderer.flipX = _initFillX;
        }
    }

    /// <summary>
    /// 移動更新
    /// </summary>
    private void UpdateMovement()
    {
        if (_inputDir == 0) return;

        // 計算新位置
        Vector3 newPos = transform.position + new Vector3(_inputDir * _moveSpeed * Time.deltaTime, 0, 0);

        // 限制移動範圍
        if (_initFillX)
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
    }

    /// <summary>
    /// 顯示貼圖
    /// </summary>
    /// <param name="stickIndex"></param>
    public void ShowStick(int stickIndex)
    {
        // 關閉文字泡泡
        _textBubbleCanvasGroup.DOKill();
        _textBubbleObj.SetActive(false);

        if (!_stickObj.activeSelf)
        {
            _stickObj.SetActive(true);
        }

        Sprite sprite = StaticDataManager.StickTextures[stickIndex];
        _img_Stick.sprite = sprite;

        _stickCanvasGroup.DOKill();
        _stickCanvasGroup.alpha = 0f;

        // 建立動畫序列
        DOTween.Sequence()
            .Append(_stickCanvasGroup.DOFade(1f, 0.5f)) // 淡入
            .AppendInterval(3f)                              // 停留
            .Append(_stickCanvasGroup.DOFade(0f, 0.5f)) // 淡出 
            .OnComplete(() =>
            {
                _stickObj.SetActive(false);
            })
            .SetLink(gameObject)
            .SetTarget(_stickCanvasGroup);
    }

    /// <summary>
    /// 顯示文字泡泡
    /// </summary>
    /// <param name="message"></param>
    public void ShowTextBubble(string message)
    {
        // 關閉貼圖
        _stickCanvasGroup.DOKill();
        _stickObj.SetActive(false);

        if (!_textBubbleObj.gameObject.activeSelf)
        {
            _textBubbleObj.gameObject.SetActive(true);
        }

        _text_Bubble.text = message;

        _textBubbleCanvasGroup.DOKill();
        _textBubbleCanvasGroup.alpha = 0f;

        // 建立動畫序列
        DOTween.Sequence()
            .Append(_textBubbleCanvasGroup.DOFade(1f, 0.5f)) // 淡入
            .AppendInterval(3f)                              // 停留
            .Append(_textBubbleCanvasGroup.DOFade(0f, 0.5f)) // 淡出 
            .OnComplete(() =>
            {
                _textBubbleObj.SetActive(false);
            })
            .SetLink(gameObject)
            .SetTarget(_textBubbleCanvasGroup);
    }

    /// <summary>
    /// 更新蓄力值
    /// </summary>
    /// <param name="value">投擲力道(0~1)</param>
    public void UpdateCharging(float value)
    {
        if (!_chargingObj.activeSelf)
        {
            _chargingObj.SetActive(true);
        }

        _img_Charging.fillAmount = value;
    }

    /// <summary>
    /// 關閉投擲力道
    /// </summary>
    public void CloseThrowStrength()
    {
        _chargingObj.SetActive(false);
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
    /// 技能_強化攻擊瞬移位置設置
    /// </summary>
    /// <param name="isToAttackPoint">是否瞬移置攻擊點</param>
    public void TeleportToPos(bool isToAttackPoint)
    {
        if(isToAttackPoint)
        {
            float height = _dataConfig.SkillStrengthDamagePosHeight;
            _teleportTargetPos.y = height;
            transform.position = _teleportTargetPos;
        }
        else
        {
            transform.position = _teleportInitPos;
        }
    }

    /// <summary>
    /// 撥放投擲動畫
    /// </summary>
    /// <param name="type"></param>
    public void PlayThrowAnimation(THROW_TYPE type, Vector3 throwTargetPos)
    {
        if(type == THROW_TYPE.StrengthDamage)
        {
            _teleportInitPos = transform.position;
            _teleportTargetPos = throwTargetPos;
        }

        _characterAnimControl.PlayThrowAnimation(type);
    }

    /// <summary>
    /// 撥放蓄力動畫
    /// </summary>
    public void PlayChargingAnimation() => _characterAnimControl.PlayChargingAnimation();

    /// <summary>
    /// 撥放嘲諷動畫
    /// </summary>
    public void PlayDerideAnimation() => _characterAnimControl.PlayDerideAnimation();

    /// <summary>
    /// 撥放受擊動畫
    /// </summary>
    /// <param name="type"></param>
    public void PlayHurtAnimation(THROW_TYPE type) => _characterAnimControl.PlayHurtAnimation(type);

    /// <summary>
    /// 撥放死亡動畫
    /// </summary>
    public void PlayDeathAnimation() => _characterAnimControl.PlayDeathAnimation();

    /// <summary>
    /// 受到傷害
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public int TakeDamage(int damage, THROW_TYPE type)
    {
        CurrentHp = Mathf.Max(0, CurrentHp - damage);

        // 撥放動畫
        if (CurrentHp > 0)
        {
            PlayHurtAnimation(type);
        }
        else
        {
            PlayDeathAnimation();
        }

        return CurrentHp;
    }

    /// <summary>
    /// 伺服器回傳HP
    /// </summary>
    public void SyncHPFromServer(int serverHP)
    {
        CurrentHp = serverHP;

        // 更新血條 UI
        bool isP1 = (this == _context.P1_CharacterView);
        _context.GameView.UpdateHpBar(isP1, CurrentHp);
    }
}
