using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 投擲物件
/// </summary>
public class ThrowObjectView : BaseObject
{
    [SerializeField] private GameObject _bodyObj;
    [SerializeField] private GameObject _FxObj;

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        _bodyObj.SetActive(false);
        _FxObj.SetActive(false);
    }

    /// <summary>
    /// 切換投擲物件與特效的顯示狀態
    /// </summary>
    public void SetActiveState(bool isActive)
    {
        if (_bodyObj != null) _bodyObj.SetActive(isActive);
        if (_FxObj != null) _FxObj.SetActive(isActive);
    }

    /// <summary>
    /// 更新物理位置（由 Controller 驅動）
    /// </summary>
    public void UpdatePosition(Vector3 position)
    {
        transform.position = position;
    }
}
