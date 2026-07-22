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

    // 是否正處於蓄力狀態
    private bool _isCharging = false;

    /// <summary>
    ///  畜力程度(0~1)
    /// </summary>
    public float ThrowStrength { get; set; }

    /// <summary>
    /// 風力強度
    /// </summary>
    public float WindStrength { get; set; }

    /// <summary>
    /// 投擲類型
    /// </summary>
    public THROW_TYPE ThrowType { get; set; }

    /// <summary>
    /// 投擲目標位置
    /// </summary>
    public Vector3 ThrowTargetPos { get; set; }

    public CharacterThrowController()
    {
        _context = GameplayManager.CurrentContext;
        _dataConfig = StaticDataManager.DataConfig;
        _characterMoveController1 = _context.GameController.MoveController;
    }

    /// <summary>
    /// 重製狀態
    /// </summary>
    public void ResetState()
    {
        _isThrowed = false;
        _isCharging = false;
        ThrowStrength = 0;
    }

    public void Tick()
    {
        if (_context.CurrentTurnCharacter == null) return;

        if (!_isThrowed)
        {
            // 開始蓄力
            if (_isCharging)
            {
                float chargeSpeed = 1f / _dataConfig.ThrowChargeSpeed;
                ThrowStrength += chargeSpeed * Time.deltaTime;
                ThrowStrength = Mathf.Clamp01(ThrowStrength);

                _context.CurrentTurnCharacter.UpdateCharging(ThrowStrength);

                // 蓄力值滿直接投擲
                if (ThrowStrength >= 1f)
                {
                    StartThrow();
                }
            }
        }
    }

    /// <summary>
    /// 設置輸入蓄力狀態
    /// </summary>
    /// <param name="isCharging"></param>
    public void SetInputCharging(bool isCharging)
    {
        if (_isThrowed) return;

        if (StaticDataManager.PlayType == PLAY_TYPE.Match)
        {
            bool isMyTurn = _context.CurrentTurnCharacter.IsLocalPlayer;
            if (!isMyTurn) return;

            ChargingData data = new()
            {
                roomId = StaticDataManager.MatchData.roomId,
                isCharging = isCharging,
                force = ThrowStrength
            };

            SocketManager.Instance.SendSyncCharging(data);
        }
        else
        {
            if (isCharging) SetChargingState();
            else StartThrow();
        }  
    }

    /// <summary>
    /// 設置蓄力狀態
    /// </summary>
    public void SetChargingState()
    {
        _isCharging = true;
        _context.CurrentTurnCharacter.PlayChargingAnimation();
        _context.GameView.SetControlPanelActive(false);
    }

    /// <summary>
    /// 開始投擲
    /// </summary>
    public void StartThrow()
    {
        _isThrowed = true;
        _isCharging = false;

        if (StaticDataManager.PlayType == PLAY_TYPE.Match)
        {
            bool isMyTurn = _context.CurrentTurnCharacter.IsLocalPlayer;
            if (isMyTurn)
            {
                ThrowData data = new()
                {
                    roomId = StaticDataManager.MatchData.roomId,
                    throwType = (int)ThrowType,
                    force = ThrowStrength
                };

                SocketManager.Instance.SendExecuteThrow(data);
            }
        }
        else
        {
            // 關閉投擲力道
            _context.CurrentTurnCharacter.CloseThrowStrength();
            // 獲取投擲位置
            ThrowTargetPos = GetThrowTargetPos(ThrowStrength);
            // 撥放投擲動畫
            _context.CurrentTurnCharacter.PlayThrowAnimation(ThrowType, ThrowTargetPos);
        }
    }

    /// <summary>
    /// 獲取投擲位置
    /// </summary>
    public Vector3 GetThrowTargetPos(float throwStrength)
    {
        var throwView = _context.ThrowObjectView;
        var attacker = _context.CurrentTurnCharacter;

        // 找出防守方
        var defenseCharacter = (attacker == _context.P1_CharacterView)
            ? _context.P2_CharacterView
            : _context.P1_CharacterView;

        if (throwView == null || attacker == null || defenseCharacter == null) return Vector3.zero;

        // 計算拋物線軌跡參數
        Vector3 startPos = attacker.transform.position + new Vector3(0, 0.5f, 0);
        float throwDirection = (attacker == _context.P1_CharacterView) ? -1f : 1f;

        // 風向與風力判斷
        float windStrength = (attacker == _context.P1_CharacterView) ? -WindStrength : WindStrength;

        // 最大投擲距離
        float maxDistance = Mathf.Max(1, _dataConfig.ThrowMaxDistance + windStrength);
        float actualDistance = maxDistance * throwStrength;

        // 目標位置
        return new Vector3(
            startPos.x + (throwDirection * actualDistance),
            _dataConfig.ThrowGroundJudgeY,
            0
        );
    }

    /// <summary>
    /// 執行投擲
    /// </summary>
    public void ExecuteThrow()
    {
        switch (ThrowType)
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
        float peakHeight = _dataConfig.ThrowMaxHeight * ThrowStrength;
        // 投擲物件大小
        float size = ThrowType == THROW_TYPE.Giant ? _dataConfig.SkillGiantSize : 1;

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
            float x = Mathf.Lerp(startPos.x, ThrowTargetPos.x, t);
            float y = Mathf.Lerp(startPos.y, ThrowTargetPos.y, t) + (4f * peakHeight * t * (1f - t));
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

                    HandleHit(defenseCharacter, isHitCharacter: true);
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
                HandleHit(defenseCharacter, isHitCharacter: false);
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

                    HandleHit(defenseCharacter, isHitCharacter: true);
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
                HandleHit(defenseCharacter, isHitCharacter: false);
            }
        });
    }

    /// <summary>
    /// 處理擊中
    /// </summary>
    private void HandleHit(CharacterView hitCharacter, bool isHitCharacter)
    {
        // 傷害
        int damage = ThrowType == THROW_TYPE.StrengthDamage ?
            Mathf.CeilToInt(_dataConfig.ThrowDamage * _dataConfig.SkillStrengthDamageMultiplier) :
            _dataConfig.ThrowDamage;

        bool isMyTurn = _context.CurrentTurnCharacter.IsLocalPlayer;
        if (StaticDataManager.PlayType == PLAY_TYPE.Match && isMyTurn)
        {
            bool isP1 = (hitCharacter == _context.P1_CharacterView);

            HitData data = new()
            {
                roomId = StaticDataManager.MatchData.roomId,
                targetSeat = isP1 ? 0 : 1,
                throwType = (int)ThrowType,
                damage = isHitCharacter ? damage : 0
            };

            SocketManager.Instance.SendExecuteHit(data);
        }
        else
        {
            ExecuteHit(hitCharacter, damage, isHitCharacter);
        }
    }

    /// <summary>
    /// 執行擊中
    /// </summary>
    /// <param name="hitCharacter"></param>
    /// <param name="damage"></param>
    /// <param name="isHitCharacter"></param>
    public void ExecuteHit( CharacterView hitCharacter, int damage, bool isHitCharacter)
    {
        var throwView = _context.ThrowObjectView;

        throwView.OnHit();

        if (hitCharacter == null)
        {
            Debug.LogError("找不到防守方角色");
            return;
        }

        if (isHitCharacter)
        {
            int remainingHp = hitCharacter.TakeDamage(damage, ThrowType);

            // 判斷被擊中的是 P1 還是 P2，並更新對應的血條 UI
            bool isP1 = (hitCharacter == _context.P1_CharacterView);
            _context.GameView.UpdateHpBar(isP1, remainingHp);

            // 檢查遊戲是否結束
            if (remainingHp <= 0)
            {
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