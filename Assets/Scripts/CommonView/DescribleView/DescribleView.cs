using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;

/// <summary>
/// 描述介面
/// </summary>
public class DescribleView : BaseView
{
    [Header("描述介面")]
    [SerializeField] private RectTransform _panel;
    [SerializeField] private TextMeshProUGUI _text_Describle;

    private DescribleViewModel _viewModel;

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        _viewModel = new(_panel, ShowDescriblePanel);

        _canvasGroup.alpha = 0;
    }

    /// <summary>
    /// 設置描述資料
    /// </summary>
    /// <param name="describle">描述文字</param>
    /// <param name="targetPos">點擊的位置</param>
    /// <param name="yOffset">偏移量Y</param>
    public void SetDescribleData(string describle, Vector3 targetPos, float yOffset)
    {
        _text_Describle.text = describle;

        _viewModel.CalculateDescribleViewPosition(targetPos, yOffset);
    }

    /// <summary>
    /// 顯示描述面板
    /// </summary>
    private void ShowDescriblePanel()
    {
        _canvasGroup.alpha = 1;
    }
}
