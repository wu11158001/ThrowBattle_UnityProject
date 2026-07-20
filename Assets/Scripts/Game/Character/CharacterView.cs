using UnityEngine;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;

/// <summary>
/// 角色
/// </summary>
public class CharacterView : BaseObject
{
    [SerializeField] private CharacterAnimControl _characterAnimControl;
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

    // 同步移動屬性
    private float _peerTargetX;
    private float _peerInputDir;
    private bool _isPeerSyncing = false;

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

    private GameplayContext _context;
    private DataConfig _dataConfig;

    private void Start()
    {
        _context = GameplayManager.CurrentContext;
        _dataConfig = StaticDataManager.DataConfig;

        _throwStrengthCanvas.worldCamera = Camera.main;
        CloseThrowStrength();

        _moveSpeed = _dataConfig.CharacterMoveSpeed;
        _moveRange = _dataConfig.CharacterMoveRange;

        _controlTip.SetActive(false);

        Bind();
    }

    private void Bind()
    {
        // 每幀驅動
        this.UpdateAsObservable()
            .Where(_ => IsLocalPlayer == false)
            .Subscribe(_ =>
            {
                UpdatePeerMovement();
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
    /// 顯示投擲力道
    /// </summary>
    /// <param name="value">投擲力道(0~1)</param>
    public void ShowThrowStrength(float value)
    {
        if (!_throwStrengthCanvas.gameObject.activeSelf) _throwStrengthCanvas.gameObject.SetActive(true);

        _img_ThrowStrength.fillAmount = value;
        _context.CurrentTurnCharacter.PlayAimAnimation();
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
    /// 控制角色移動、動畫與面向
    /// </summary>
    /// <param name="direction">移動方向</param>
    /// <param name="isPeerSync">是否為對手連線同步</param>
    public void Move(float direction, bool isPeerSync = false)
    {
        if (!isPeerSync)
        {
            // 計算新位置
            Vector3 newPos = transform.position + new Vector3(direction * _moveSpeed * Time.deltaTime, 0, 0);

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

        // 播放動畫
        _characterAnimControl.MoveAnimationControl(Mathf.Abs(direction) > 0);

        // 角色面向
        if (Mathf.Abs(direction) > 0)
        {
            _spriteRenderer.flipX = direction < 0 ? true : false;
        }
        else
        {
            _spriteRenderer.flipX = _initFillX;
        }
    }

    /// <summary>
    /// 收到伺服器對手位移封包時呼叫
    /// </summary>
    public void OnReceivePeerMove(MoveData data)
    {
        _peerTargetX = data.posX;
        _peerInputDir = data.inputDir;
        _isPeerSyncing = true;
    }

    /// <summary>
    /// 理對手平滑移動
    /// </summary>
    private void UpdatePeerMovement()
    {
        if (!_isPeerSyncing) return;

        // 先播放對手的動畫與轉向
        Move(_peerInputDir, isPeerSync: true);

        // 如果對手停止移動，且位置已經很接近目標，就關閉同步
        if (_peerInputDir == 0f && Mathf.Abs(transform.position.x - _peerTargetX) < 0.01f)
        {
            Vector3 pos = transform.position;
            pos.x = _peerTargetX;
            transform.position = pos;
            _isPeerSyncing = false;
            return;
        }

        // 預測移動：根據對手的 inputDir 持續移動
        Vector3 currentPos = transform.position;
        float predictedX = currentPos.x + (_peerInputDir * _moveSpeed * Time.deltaTime);

        // 使用 Lerp 平滑修正與伺服器封包真實位置
        float smoothedX = Mathf.Lerp(predictedX, _peerTargetX, Time.deltaTime * 15f);

        // 限制移動範圍
        if (_initFillX)
        {
            // 角色在右側
            smoothedX = Mathf.Clamp(smoothedX, _moveRange.x, _moveRange.y);
        }
        else
        {
            // 角色在左側
            smoothedX = Mathf.Clamp(smoothedX, -_moveRange.y, -_moveRange.x);
        }

        currentPos.x = smoothedX;
        transform.position = currentPos;
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
    public void PlayAimAnimation() => _characterAnimControl.PlayAimAnimation();

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
