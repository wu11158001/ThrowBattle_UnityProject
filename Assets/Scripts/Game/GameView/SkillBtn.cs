using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;

/// <summary>
/// 技能按鈕
/// </summary>
public class SkillBtn : MonoBehaviour
{
    [SerializeField] private Button _mainBtn;
    [SerializeField] private Image _cdMask;
    [SerializeField] private UIEventHandler uiEventHandler;

    private Action _clickEvent;

    // 所需CD回合
    private int _maxCDTurns;
    // 當前剩餘CD回合
    private int _currentCDTurns = 0;
    // 技能描述
    private string _describle;

    private bool _isShowDescrible = false;

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

    private void OnDestroy()
    {
        _cdMask.DOKill();
    }

    public void SetData(THROW_TYPE skillType, int maxCDTurns, Action clickEvent, Sprite skillIcon, int belongIndex, string describle)
    {
        SkillType = skillType;
        _maxCDTurns = maxCDTurns;
        _clickEvent = clickEvent;
        _describle = describle;
        BelongIndex = belongIndex;

        _mainBtn.image.sprite = skillIcon;
        _cdMask.sprite = skillIcon;
        _cdMask.fillAmount = 0;

        Bind();

        _currentCDTurns = 0;
    }

    private void Bind()
    {
        // 技能按鈕點擊
        _mainBtn.OnClickAsObservable()
            .Subscribe(_ =>
            {
                _clickEvent?.Invoke();
                StartCD();
            })
            .AddTo(this);

        // 將 DownAction 轉為 Observable
        var pointerDown = Observable.FromEvent<PointerEventData>(
            h => uiEventHandler.DownAction += h,
            h => uiEventHandler.DownAction -= h
        );

        // 將 UpAction 轉為 Observable
        var pointerUp = Observable.FromEvent<PointerEventData>(
            h => uiEventHandler.UpAction += h,
            h => uiEventHandler.UpAction -= h

        );
        // 技能按鈕長按
        pointerDown.SelectMany(_ => Observable.Timer(TimeSpan.FromSeconds(0.5f)))
            .TakeUntil(pointerUp)
            .Repeat()
            .Subscribe(_ =>
            {
                if(!_isShowDescrible)
                {
                    _isShowDescrible = true;

                    ViewManager.Instance.OpenView<DescribleView>(
                        viewType: VIEW_TYPE.DescribleView,
                        canvasType: CANVAS_TYPE.Canvas_Highest,
                        callback: (view) =>
                        {
                            view.SetDescribleData(
                                describle: _describle,
                                targetPos: uiEventHandler.MainRect.position,
                                yOffset: -120f);
                        }).Forget();
                }
            })
            .AddTo(this);

        // 放開按壓
        pointerUp.Subscribe(_ =>
        {
            if(_isShowDescrible)
            {

                DescribleView describleView = ViewManager.Instance.GetOpenView<DescribleView>(VIEW_TYPE.DescribleView);
                if (describleView != null) describleView.Close();
            }

            _isShowDescrible = false;
        }).AddTo(this);
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
        float value = (float)_currentCDTurns / _maxCDTurns;

        _cdMask.DOKill();
        _cdMask.DOFillAmount(value, 0.5f)
            .SetEase(ease: Ease.Linear)
            .SetLink(gameObject)
            .SetTarget(_cdMask);
    }
}
