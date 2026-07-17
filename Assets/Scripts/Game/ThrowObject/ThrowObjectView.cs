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
}
