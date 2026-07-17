using UnityEngine;
using DG.Tweening;

/// <summary>
/// 投擲類型
/// </summary>
public enum THROW_TYPE
{
    /// <summary> 一般投擲 </summary>
    Normal,
    /// <summary> 巨大化 </summary>
    Giant,
    /// <summary> 強化傷害 </summary>
    StrengthDamage,
}

/// <summary>
/// 處理投擲
/// </summary>
public class CharacterThrowController
{
    private GameplayContext _context;
    private DataConfig _dataConfig;
    private CharacterMoveController _characterMoveController1;

    // 本回合是否已投擲
    private bool _isThrowed;
    // 是否已開始畜力
    private bool _isPressingThrow = false;
    // 畜力程度(0~1)
    private float _throwStrength = 0f;
    // 是否需要先放開按鍵才能進行下一次蓄力
    private bool _needRelease = false;

    // 風力強度
    private float _windStrength = 0f;
    // 下次投擲類型
    private THROW_TYPE _throwType;
    // 下次投擲目標位置
    private Vector3 _throwTargetPos;

    /// <summary>
    ///是否正處於蓄力狀態
    /// </summary>
    public bool IsCharging { get; private set; } = false;

    public CharacterThrowController(CharacterMoveController characterMoveController)
    {
        _context = GameplayManager.CurrentContext;
        _dataConfig = StaticDataManager.DataConfig;
        _characterMoveController1 = characterMoveController;
    }

    /// <summary>
    /// 重製狀態
    /// </summary>
    public void ResetState()
    {
        _isThrowed = false;
        IsCharging = false;
        _throwStrength = 0f;
    }

    /// <summary>
    /// 每幀驅動
    /// </summary>
    public void Tick()
    {
        if (_context.CurrentTurnCharacter == null) return;

        if(!_isThrowed)
        {
            if (_isPressingThrow)
            {
                // 如果安全鎖啟動中（上一次投擲完還沒放開手），直接攔截不處理
                if (_needRelease) return;

                // 剛按下的第一幀
                if (!IsCharging)
                {
                    IsCharging = true;

                    // 強制控制角色停止移動
                    _characterMoveController1.ForceStop();

                    // 移動面板關閉
                    _context.GameView.SetControlPanelActive(false);
                }

                // 蓄力數值計算
                float chargeSpeed = 1f / _dataConfig.ThrowChargeSpeed;
                _throwStrength += chargeSpeed * Time.deltaTime;
                _throwStrength = Mathf.Clamp01(_throwStrength);

                // 顯示蓄力條
                _context.CurrentTurnCharacter.ShowThrowStrength(_throwStrength);

                // 滿力判定
                if (_throwStrength >= 1f)
                {
                    _needRelease = true;
                    StartThrow();
                }
            }
            else
            {
                // 蓄力中途放開按鍵
                if (IsCharging)
                {
                    StartThrow();
                }
            }
        }
    }

    /// <summary>
    /// 設置投擲蓄力狀態
    /// </summary>
    /// <param name="isPressing"></param>
    public void SetThrowPressState(bool isPressing)
    {
        _isPressingThrow = isPressing;

        // 如果放開了按鍵，解除安全鎖
        if (!_isPressingThrow)
        {
            _needRelease = false;
        }
    }

    /// <summary>
    /// 設置下次投擲的類型
    /// </summary>
    /// <param name="type"></param>
    public void SetNextThrowType(THROW_TYPE type)
    {
        _throwType = type;
    }

    /// <summary>
    /// 設置風力強度
    /// </summary>
    /// <param name="value"></param>
    public void SetWindStrength(float value)
    {
        _windStrength = value;
    }

    /// <summary>
    /// 設置下一次投擲位置
    /// </summary>
    private void SetNextThrowTargetPos()
    {
        var throwView = _context.ThrowObjectView;
        var attacker = _context.CurrentTurnCharacter;

        // 找出防守方
        var defenseCharacter = (attacker == _context.P1_CharacterView)
            ? _context.P2_CharacterView
            : _context.P1_CharacterView;

        if (throwView == null || attacker == null || defenseCharacter == null) return;

        // 計算拋物線軌跡參數
        Vector3 startPos = attacker.transform.position + new Vector3(0, 0.5f, 0);
        float throwDirection = (attacker == _context.P1_CharacterView) ? -1f : 1f;

        // 風向與風力判斷
        float windStrength = (attacker == _context.P1_CharacterView) ? -_windStrength : _windStrength;

        // 最大投擲距離
        float maxDistance = _dataConfig.ThrowMaxDistance + windStrength;
        float actualDistance = maxDistance * _throwStrength;

        // 目標位置
        _throwTargetPos = new Vector3(
            startPos.x + (throwDirection * actualDistance),
            _dataConfig.ThrowGroundJudgeY,
            0
        );
    }

    /// <summary>
    /// 開始投擲
    /// </summary>
    private void StartThrow()
    {
        _isThrowed = true;
        // 進入投擲飛行階段，解除按壓與蓄力標記，防止 Tick 重複觸發
        IsCharging = false;

        // 設置下一次投擲位置
        SetNextThrowTargetPos();

        // 關閉投擲力道
        _context.CurrentTurnCharacter.CloseThrowStrength();
        // 撥放投擲動畫
        _context.CurrentTurnCharacter.PlayThrowAnimation(_throwType, _throwTargetPos);
    }

    /// <summary>
    /// 執行投擲
    /// </summary>
    public void ExecuteThrow()
    {
        switch (_throwType)
        {
            case THROW_TYPE.Normal:
                ExecuteParabolaThrow();
                break;

            case THROW_TYPE.Giant:
                ExecuteParabolaThrow();
                break;

            case THROW_TYPE.StrengthDamage:
                ExecuteDownStraightThrow();
                break;
        }
    }

    /// <summary>
    /// 執行拋物線投擲
    /// </summary>
    private void ExecuteParabolaThrow()
    {
        var throwView = _context.ThrowObjectView;
        var attacker = _context.CurrentTurnCharacter;

        // 找出防守方
        var defenseCharacter = (attacker == _context.P1_CharacterView)
            ? _context.P2_CharacterView
            : _context.P1_CharacterView;

        if (throwView == null || attacker == null || defenseCharacter == null) return;

        // 起始位置
        Vector3 startPos = attacker.transform.position + new Vector3(0, 0.5f, 0);

        // 投擲高度
        float peakHeight = _dataConfig.ThrowMaxHeight * _throwStrength;
        // 投擲物件大小
        float size = _throwType == THROW_TYPE.Giant ? _dataConfig.SkillGiantSize : 1;

        // 啟動投擲物件
        throwView.UpdatePosition(startPos);
        throwView.SetSize(size);
        throwView.StartThrow();

        // 記錄上一幀的座標，用來計算射線方向與距離
        Vector3 lastPos = startPos;
        bool hasHit = false;
        // 移動時間
        float duration = _dataConfig.ThrowMoveDuration;
        // 圓形射線參數設定
        float castRadius = _dataConfig.ThrowRaycastRadius * size;

        // 拋物線模擬
        Tween throwTween = null;
        throwTween = DOVirtual.Float(0f, 1f, duration, (t) =>
        {
            if (hasHit) return;

            // 計算這一幀的新座標
            float x = Mathf.Lerp(startPos.x, _throwTargetPos.x, t);
            float y = Mathf.Lerp(startPos.y, _throwTargetPos.y, t) + (4f * peakHeight * t * (1f - t));
            Vector3 nextPos = new(x, y, 0);

            // 更新 Debug 用的射線資訊
            throwView.UpdateDebugCastInfo(lastPos, nextPos, castRadius);

            // 計算移動向量
            Vector3 moveDirection = nextPos - lastPos;
            float moveDistance = moveDirection.magnitude;

            // 避免第一幀或未移動時（距離太小）做無效偵測
            if (moveDistance > 0.001f)
            {
                // 發射圓形射線：從「上一幀位置」往「移動方向」發射，距離為「這幀移動的長度」
                RaycastHit2D hit = Physics2D.CircleCast(lastPos, castRadius, moveDirection.normalized, moveDistance);

                // 檢查是否有打中防守方角色
                if (hit.collider != null && hit.collider.gameObject == defenseCharacter.gameObject)
                {
                    hasHit = true;
                    throwTween.Kill();
                    throwView.UpdatePosition(hit.point);

                    HandleHit(throwView, defenseCharacter, isHitCharacter: true);
                    return;
                }
            }

            throwView.UpdatePosition(nextPos);
            lastPos = nextPos;
        })
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            // 正常落地(未擊中角色)
            if (!hasHit)
            {
                HandleHit(throwView, defenseCharacter, isHitCharacter: false);
            }
        });
    }

    /// <summary>
    /// 執行向下直線投擲
    /// </summary>
    private void ExecuteDownStraightThrow()
    {
        var throwView = _context.ThrowObjectView;
        var attacker = _context.CurrentTurnCharacter;

        // 找出防守方
        var defenseCharacter = (attacker == _context.P1_CharacterView)
            ? _context.P2_CharacterView
            : _context.P1_CharacterView;

        if (throwView == null || attacker == null || defenseCharacter == null) return;

        // 起始位置
        Vector3 startPos = attacker.transform.position + new Vector3(0, 0.5f, 0);

        // 直線投擲的終點
        Vector3 straightTargetPos = new(startPos.x, _dataConfig.ThrowGroundJudgeY, 0);

        // 投擲物件大小
        float size = 1f;

        // 啟動投擲物件
        throwView.UpdatePosition(startPos);
        throwView.SetSize(size);
        throwView.StartThrow();

        // 記錄上一幀的座標，用來計算射線方向與距離
        Vector3 lastPos = startPos;
        bool hasHit = false;

        // 移動時間
        float duration = _dataConfig.ThrowMoveDuration * 0.25f;

        // 圓形射線參數設定
        float castRadius = _dataConfig.ThrowRaycastRadius * size;

        // 直線下砸模擬
        Tween throwTween = null;
        throwTween = DOVirtual.Float(0f, 1f, duration, (t) =>
        {
            if (hasHit) return;

            // 線性插值：從起點直接往正下方的終點移動
            float x = startPos.x;
            float y = Mathf.Lerp(startPos.y, straightTargetPos.y, t);
            Vector3 nextPos = new(x, y, 0);

            // 更新 Debug 用的射線資訊
            throwView.UpdateDebugCastInfo(lastPos, nextPos, castRadius);

            // 計算移動向量
            Vector3 moveDirection = nextPos - lastPos;
            float moveDistance = moveDirection.magnitude;

            // 避免微小位移做無效偵測
            if (moveDistance > 0.001f)
            {
                // 發射圓形射線
                RaycastHit2D hit = Physics2D.CircleCast(lastPos, castRadius, moveDirection.normalized, moveDistance);

                // 檢查是否有打中防守方角色
                if (hit.collider != null && hit.collider.gameObject == defenseCharacter.gameObject)
                {
                    hasHit = true;
                    throwTween.Kill();
                    throwView.UpdatePosition(hit.point);

                    HandleHit(throwView, defenseCharacter, isHitCharacter: true);
                    return;
                }
            }

            throwView.UpdatePosition(nextPos);
            lastPos = nextPos;
        })
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            // 正常落地(未擊中角色)
            if (!hasHit)
            {
                HandleHit(throwView, defenseCharacter, isHitCharacter: false);
            }
        });
    }

    /// <summary>
    /// 處理擊中
    /// </summary>
    private void HandleHit(ThrowObjectView throwView, CharacterView hitCharacter, bool isHitCharacter)
    {
        throwView.OnHit();

        if(hitCharacter == null)
        {
            Debug.LogError("找不到防守方角色");
            return;
        }

        if (isHitCharacter)
        {
            // 傷害
            int damage = _throwType == THROW_TYPE.StrengthDamage ?
                Mathf.CeilToInt(_dataConfig.ThrowDamage * _dataConfig.SkillStrengthDamageMultiplier) :
                _dataConfig.ThrowDamage;

            int remainingHp = hitCharacter.TakeDamage(damage, _throwType);

            // 判斷被擊中的是 P1 還是 P2，並更新對應的血條 UI
            bool isP1 = (hitCharacter == _context.P1_CharacterView);
            _context.GameView.UpdateHpBar(isP1, remainingHp, hitCharacter.MaxHp);

            // 檢查遊戲是否結束
            if (remainingHp <= 0)
            {
                Debug.Log($"【遊戲結束】 {hitCharacter.name} 倒下了！");
                _context.GameController.IsGameOver.Value = true;
                return;
            }
        }
        else
        {
            hitCharacter.PlayDerideAnimation();
        }
    }
}