using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 遊戲內物件
/// </summary>
public class BaseObject : MonoBehaviour
{
    protected AssetReferenceGameObject _myRef;

    // 防止重複釋放
    private bool _isRemove = false;

    public virtual void OnDestroy()
    {
        Remove();
    }

    public virtual void SetData(AssetReferenceGameObject myRef)
    {
        _myRef = myRef;
    }

    /// <summary>
    /// 移除並釋放資源
    /// </summary>
    public virtual void Remove()
    {
        if (_isRemove) return;
        _isRemove = true;

        if (_myRef != null)
        {
            Addressables.ReleaseInstance(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
