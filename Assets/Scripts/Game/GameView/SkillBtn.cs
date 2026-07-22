using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

/// <summary>
/// 技能按鈕
/// </summary>
public class SkillBtn : MonoBehaviour
{
    [SerializeField] private Button _mainBtn;
    [SerializeField] private Image _cdMask;

    private Action _clickEvent;

    // 所需CD回合
    private int _maxCDTurns;
    // 當前剩餘CD回合
    private int _currentCDTurns = 0;

    /// <summary>
    /// 技能類型
    /// </summary>
    public THROW_TYPE SkillType { get; private set; }

    /// <summary>
    /// 是否正在CD
    /// </summary>
    public bool IsInCD => _currentCDTurns > 0;

    /// <summary>
    /// 歸屬角色(0 = P1, 1 = P2)
    /// </summary>
    public int BelongIndex { get; private set; }

    public void SetData(THROW_TYPE skillType, int maxCDTurns, Action clickEvent, Sprite skillIcon, int belongIndex)
    {
        SkillType = skillType;
        _maxCDTurns = maxCDTurns;
        _clickEvent = clickEvent;
        BelongIndex = belongIndex;

        _mainBtn.image.sprite = skillIcon;
        _cdMask.fillAmount = 0;

        Bind();

        _currentCDTurns = 0;
    }

    private void Bind()
    {
        _mainBtn.OnClickAsObservable()
            .Subscribe(_ =>
            {
                _clickEvent?.Invoke();
                StartCD();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 設置按鈕可互動狀態
    /// </summary>
    public void SetInteractable(bool isInteractable)
    {
        if (IsInCD)
        {
            _mainBtn.transition = Selectable.Transition.None;
            _mainBtn.interactable = false;
        }
        else
        {
            _mainBtn.transition = isInteractable ? Selectable.Transition.ColorTint : Selectable.Transition.None;
            _mainBtn.interactable = isInteractable;
        }
    }

    /// <summary>
    /// 開始CD
    /// </summary>
    private void StartCD()
    {
        _currentCDTurns = _maxCDTurns;
        UpdateMask();
    }

    /// <summary>
    /// 扣除CD
    /// </summary>
    public void ReduceCD()
    {
        if (_currentCDTurns > 0)
        {
            _currentCDTurns--;
            UpdateMask();
        }
    }

    /// <summary>
    /// 重製CD
    /// </summary>
    public void ResetCD()
    {
        _currentCDTurns = 0;
        UpdateMask();
    }

    /// <summary>
    /// 更新遮罩圖片的填滿度
    /// </summary>
    private void UpdateMask()
    {
        if (_maxCDTurns <= 0) return;
        _cdMask.fillAmount = (float)_currentCDTurns / _maxCDTurns;
    }
}
