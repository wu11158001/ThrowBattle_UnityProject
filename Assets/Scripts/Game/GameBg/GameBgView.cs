using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 遊戲背景
/// </summary>
public class GameBgView : BaseObject
{
    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        transform.position = Vector3.zero;
    }
}
