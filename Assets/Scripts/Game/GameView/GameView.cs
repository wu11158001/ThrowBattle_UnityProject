using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using DG.Tweening;
using UniRx;

/// <summary>
/// 遊戲介面
/// </summary>
public class GameView : BaseView
{
    [Header("遊戲介面")]
    [SerializeField] private TextMeshProUGUI _text_Battle;

    [Header("移動控制按鈕")]
    [SerializeField] private Button _btn_LeftMove;
    [SerializeField] private Button _btn_RightMove;

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        Bind();
        PlayOpeningAnimation();
    }

    private void Bind()
    {
        // 移動控制按鈕_左
        _btn_LeftMove.OnClickAsObservable()
            .Subscribe(_ =>
            {
            })
            .AddTo(this);

        // 移動控制按鈕_右
        _btn_RightMove.OnClickAsObservable()
            .Subscribe(_ =>
            {
            })
            .AddTo(this);
    }

    /// <summary>
    /// 撥放開場動畫
    /// </summary>
    private void PlayOpeningAnimation()
    {
        if (_text_Battle == null) return;

        _text_Battle.fontSize = 0;
        _text_Battle.gameObject.SetActive(true);
        
        // 文字放大
        Sequence textSeq = DOTween.Sequence();
        textSeq.Append(DOTween.To(
            () => _text_Battle.fontSize,
            x => _text_Battle.fontSize = x,
            150f,
            0.35f
        ).SetEase(Ease.OutQuad));

        // 停留
        textSeq.AppendInterval(2f);

        // 結束
        textSeq.OnComplete(() =>
        {
            _text_Battle.gameObject.SetActive(false);
        });
    }
}
