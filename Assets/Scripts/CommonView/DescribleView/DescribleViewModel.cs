using UnityEngine;
using UnityEngine.UI;
using System;

public class DescribleViewModel
{
    private RectTransform _mainRect;

    private Action _callback;

    public DescribleViewModel(RectTransform mainRect, Action callback)
    {
        _mainRect = mainRect;
        _callback = callback;
    }

    /// <summary>
    /// 計算描述介面位置
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="yOffset"></param>
    public void CalculateDescribleViewPosition(Vector3 targetPos, float yOffset)
    {
        if (_mainRect == null) return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_mainRect);

        // 取得該 UI 所屬的 Canvas 與其對應的相機
        Canvas canvas = _mainRect.GetComponentInParent<Canvas>();
        // 如果 Canvas 是 Screen Space - Overlay，相機 null
        Camera uiCamera = (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : Camera.main;

        // 先移到原本預計的位置
        _mainRect.transform.position = targetPos;

        _mainRect.transform.localPosition += new Vector3(0, yOffset, 0);

        // 取得 UI 實際四個角的世界座標
        Vector3[] objectCorners = new Vector3[4];
        _mainRect.GetWorldCorners(objectCorners);

        // 轉成螢幕像素座標
        Vector2 minScreenCorner = RectTransformUtility.WorldToScreenPoint(uiCamera, objectCorners[0]);
        Vector2 maxScreenCorner = RectTransformUtility.WorldToScreenPoint(uiCamera, objectCorners[2]);

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float shiftX = 0;
        float shiftY = 0;

        // 檢查右邊界與左邊界
        if (maxScreenCorner.x > screenWidth) shiftX = screenWidth - maxScreenCorner.x;
        else if (minScreenCorner.x < 0) shiftX = -minScreenCorner.x;

        // 檢查上邊界與下邊界
        if (maxScreenCorner.y > screenHeight) shiftY = screenHeight - maxScreenCorner.y;
        else if (minScreenCorner.y < 0) shiftY = -minScreenCorner.y;

        // 如果有超出，把 UI 往回推
        if (shiftX != 0 || shiftY != 0)
        {
            Vector2 currentScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, _mainRect.transform.position);
            currentScreenPos.x += shiftX;
            currentScreenPos.y += shiftY;

            // 轉回世界座標給 UI
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _mainRect.parent as RectTransform,
                currentScreenPos,
                uiCamera,
                out Vector3 clampedWorldPos))
            {
                _mainRect.transform.position = clampedWorldPos;
            }
        }

        _callback?.Invoke();
    }
}
