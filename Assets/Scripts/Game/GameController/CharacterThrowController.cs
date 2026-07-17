using UnityEngine;
using DG.Tweening;

/// <summary>
/// 處理投擲
/// </summary>
public class CharacterThrowController
{
    private GameplayContext _context;
    private CharacterMoveController _characterMoveController1;

    // 是否已開始畜力
    private bool _isPressingThrow = false;
    // 畜力程度(0~1)
    private float _throwStrength = 0f;
    // 是否需要先放開按鍵才能進行下一次蓄力
    private bool _needRelease = false;

    // 風力強度
    private float _windStrength = 0f;

    /// <summary>
    /// 提供外部查詢目前是否正處於蓄力狀態
    /// </summary>
    public bool IsCharging { get; private set; } = false;

    public CharacterThrowController(CharacterMoveController characterMoveController)
    {
        _context = GameplayManager.CurrentContext; ;
        _characterMoveController1 = characterMoveController;
    }

    /// <summary>
    /// 重製狀態
    /// </summary>
    public void ResetState()
    {
        IsCharging = false;
        _throwStrength = 0f;
    }

    /// <summary>
    /// 每幀驅動
    /// </summary>
    public void Tick()
    {
        if (_context.CurrentTurnCharacter == null) return;

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
                _context.GameController.UpdateMoveControlPanelVisibility(false);
            }

            // 蓄力數值計算
            float chargeSpeed = 1f / StaticDataManager.DataConfig.ThrowChargeSpeed;
            _throwStrength += chargeSpeed * Time.deltaTime;
            _throwStrength = Mathf.Clamp01(_throwStrength);

            // 顯示蓄力條
            _context.CurrentTurnCharacter.ShowThrowStrength(_throwStrength);

            // 滿力判定
            if (_throwStrength >= 1f)
            {
                _needRelease = true;
                ExecuteParabolaThrow();
            }
        }
        else
        {
            // 蓄力中途放開按鍵
            if (IsCharging)
            {
                ExecuteParabolaThrow();
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
    /// 設置風力強度
    /// </summary>
    /// <param name="value"></param>
    public void SetWindStrength(float value)
    {
        _windStrength = value;
    }

    /// <summary>
    /// 執行拋物線投擲
    /// </summary>
    private void ExecuteParabolaThrow()
    {
        // 進入投擲飛行階段，解除按壓與蓄力標記，防止 Tick 重複觸發
        IsCharging = false;

        _context.CurrentTurnCharacter.CloseThrowStrength();

        var config = StaticDataManager.DataConfig;
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
        float maxDistance = config.ThrowMaxDistance + windStrength;
        float actualDistance = maxDistance * _throwStrength;

        // 目標位置
        Vector3 targetPos = new Vector3(
            startPos.x + (throwDirection * actualDistance),
            config.ThrowGroundJudgeY,
            0
        );

        // 投擲高度
        float peakHeight = config.ThrowMaxHeight * _throwStrength;

        // 啟動投擲物件
        throwView.UpdatePosition(startPos);
        throwView.OnThrow();

        // 記錄上一幀的座標，用來計算射線方向與距離
        Vector3 lastPos = startPos;
        bool hasHit = false;
        // 移動時間
        float duration = config.ThrowMoveDuration;
        // 圓形射線參數設定
        float castRadius = config.RaycastRadius;

        // 拋物線模擬
        Tween throwTween = null;
        throwTween = DOVirtual.Float(0f, 1f, duration, (t) =>
        {
            if (hasHit) return;

            // 計算這一幀的新座標
            float x = Mathf.Lerp(startPos.x, targetPos.x, t);
            float y = Mathf.Lerp(startPos.y, targetPos.y, t) + (4f * peakHeight * t * (1f - t));
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
            // 正常落地（未擊中角色）
            if (!hasHit)
            {
                HandleHit(throwView, defenseCharacter, isHitCharacter: false);
            }
        });
    }

    /// <summary>
    /// 處理擊中
    /// </summary>
    private void HandleHit(ThrowObjectView throwView, CharacterView defenseCharacter, bool isHitCharacter)
    {
        throwView.OnHit();

        if (isHitCharacter)
        {
            // 擊中
            if(defenseCharacter != null)
            {
                Debug.Log($"[圓形射線偵測] 成功擊中角色: {defenseCharacter.name}！");
            }
        }
        else
        {
            // 未擊中
            if (defenseCharacter != null)
            {
                Debug.Log("[圓形射線偵測] 未擊中角色，正常落到地面。");
            }
        }

        // 通知回合結束
        _context.GameController.OnThrowComplete();
    }
}