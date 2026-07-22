using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.AddressableAssets;

/// <summary>
/// AI 困難度選擇介面
/// </summary>
public class AIDifficultyView : BaseView
{
    [Header("AI 困難度選擇介面")]
    [SerializeField] private RectTransform _DifficultyBtnPanel;
    [SerializeField] private GameObject _btn_DifficultyPrefab;

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);
    }

    /// <summary>
    /// 創建AI困難度按鈕
    /// </summary>
    private void CreateDifficultyBtns()
    {

    }
}
