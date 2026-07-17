using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 投擲物件
/// </summary>
public class ThrowObjectView : BaseObject
{
    [SerializeField] private GameObject _bodyObj;
    [SerializeField] private GameObject _FxObj;

    // 用於 Debug 繪製的變數
    private Vector3 _debugLastPos;
    private Vector3 _debugCurrentPos;
    private float _debugRadius;
    private bool _isFlying = false;

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        _bodyObj.SetActive(false);
        _FxObj.SetActive(false);
        _isFlying = false;
    }

    /// <summary>
    /// 投擲
    /// </summary>
    public void OnThrow()
    {
        if (_bodyObj != null) _bodyObj.SetActive(true);
        if (_FxObj != null) _FxObj.SetActive(false);
        _isFlying = true;
    }

    /// <summary>
    /// 擊中地面或角色
    /// </summary>
    public void OnHit()
    {
        if (_bodyObj != null) _bodyObj.SetActive(false);
        if (_FxObj != null) _FxObj.SetActive(true);
        _isFlying = false;
    }

    /// <summary>
    /// 更新物理位置（由 Controller 驅動）
    /// </summary>
    public void UpdatePosition(Vector3 position)
    {
        transform.position = position;
    }

    /// <summary>
    /// 更新 Debug 用的射線資訊
    /// </summary>
    public void UpdateDebugCastInfo(Vector3 lastPos, Vector3 currentPos, float radius)
    {
        _debugLastPos = lastPos;
        _debugCurrentPos = currentPos;
        _debugRadius = radius;
    }

    private void OnDrawGizmos()
    {
        if (!_isFlying) return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(_debugLastPos, _debugRadius);
        Gizmos.DrawWireSphere(_debugCurrentPos, _debugRadius);

        Vector3 direction = (_debugCurrentPos - _debugLastPos).normalized;
        if (direction != Vector3.zero)
        {
            Vector3 ortho = new Vector3(-direction.y, direction.x, 0) * _debugRadius;

            Gizmos.DrawLine(_debugLastPos + ortho, _debugCurrentPos + ortho);
            Gizmos.DrawLine(_debugLastPos - ortho, _debugCurrentPos - ortho);
        }
    }
}
