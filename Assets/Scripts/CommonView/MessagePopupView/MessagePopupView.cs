using UnityEngine;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;

/// <summary>
/// 訊息彈窗介面
/// </summary>
public class MessagePopupView : BaseView
{
    [Header("訊息彈窗介面")]
    [SerializeField] CanvasGroup _mainCanvasGroup;
    [SerializeField] private TextMeshProUGUI _text_Message;

    [BoxGroup("參數設置")][Label("淡入時間")] [SerializeField] private float _fadeInDuration = 0.5f;
    [BoxGroup("參數設置")][Label("停留時間")] [SerializeField] private float _delayDuration = 2.0f;
    [BoxGroup("參數設置")][Label("淡出時間")] [SerializeField] private float _fadeOutDuration = 0.5f;

    private Sequence _popupSequence;

    public override void OnDestroy()
    {
        _popupSequence?.Kill();
        base.OnDestroy();
    }

    private void OnDisable()
    {
        _popupSequence?.Kill();
    }

    public void DoShow()
    {
        PlayPopupAnimation();
    }

    /// <summary>
    /// 撥放彈窗動畫
    /// </summary>
    private void PlayPopupAnimation()
    {
        _popupSequence?.Kill();

        _mainCanvasGroup.alpha = 0f;

        _popupSequence = DOTween.Sequence();
        _popupSequence
            .Append(_mainCanvasGroup.DOFade(1f, _fadeInDuration))  // 淡入
            .AppendInterval(_delayDuration)                        // 停留
            .Append(_mainCanvasGroup.DOFade(0f, _fadeOutDuration)) // 淡出
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            })
            .SetTarget(gameObject)
            .SetLink(gameObject);
    }

    /// <summary>
    /// 設置訊息內容
    /// </summary>
    /// <param name="message"></param>
    public void SetMessage(string message)
    {
        _text_Message.text = message;
    }
}
