using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

/// <summary>
/// Canvas類型
/// </summary>
public enum CANVAS_TYPE
{
    Canvas_Static,
    Canvas_Dynamic,
    Canvas_HUD,
    Canvas_Highest,
}

/// <summary>
/// 介面管理中心
/// </summary>
public class ViewManager : SingletonMonoBehaviour<ViewManager>
{
    [SerializeField] private RectTransform _canvas_Static;
    [SerializeField] private RectTransform _canvas_Dynamic;
    [SerializeField] private RectTransform _canvas_HUD;
    [SerializeField] private RectTransform _canvas_Highest;

    private Stack<BaseView> _viewStack = new();

    private MessagePopupView _messagePopupView;

    /// <summary>
    /// 開啟介面
    /// </summary>
    /// <param name="viewType"></param>
    /// <param name="canvasType"></param>
    /// <param name="isClosePreView">是否關閉前個介面</param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public async UniTask OpenView<T>(
        VIEW_TYPE viewType, CANVAS_TYPE canvasType, bool isClosePreView = false, Action<T> callback = null) where T : BaseView
    {
        var prefabRef = StaticDataManager.ViewConfig.GetPrefabRef(viewType);
        if (prefabRef == null)
        {
            Debug.LogError($"找不到 ViewType: {viewType} 的配置");
            return;
        }

        RectTransform canvasRoot = null;
        switch (canvasType)
        {
            case CANVAS_TYPE.Canvas_Static:
                canvasRoot = _canvas_Static;
                break;

            case CANVAS_TYPE.Canvas_Dynamic:
                canvasRoot = _canvas_Dynamic;
                break;

            case CANVAS_TYPE.Canvas_HUD:
                canvasRoot = _canvas_HUD;
                break;

            case CANVAS_TYPE.Canvas_Highest:
                canvasRoot = _canvas_Highest;
                break;
        }

        if (canvasRoot == null)
        {
            Debug.LogError("無法找到Canvas!");
            return;
        }

        var handle = prefabRef.InstantiateAsync(canvasRoot);
        GameObject obj = await handle.Task;

        T view = obj.GetComponent<T>();
        view.SetData(prefabRef);

        obj.transform.SetAsLastSibling();

        if (isClosePreView && _viewStack.Count > 0)
        {
            BaseView preView = _viewStack.Peek();
            if (preView != null)
            {
                preView.gameObject.SetActive(false);
            }
        }

        _viewStack.Push(view);
        callback?.Invoke(view);
    }

    /// <summary>
    /// 關閉介面
    /// </summary>
    /// <param name="isOpenPreView">是否開啟前個介面</param>
    public void CloseView(bool isOpenPreView = false)
    {
        if (_viewStack == null || _viewStack.Count == 0) return;

        // 取出釋放當前介面
        BaseView currentView = _viewStack.Pop();
        if (currentView != null)
        {
            Addressables.ReleaseInstance(currentView.gameObject);
        }

        // 檢查並開啟前一個介面
        if (_viewStack.Count > 0 && isOpenPreView)
        {
            BaseView preView = _viewStack.Peek();
            if (preView != null)
            {
                preView.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 獲取介面
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="viewType"></param>
    /// <returns></returns>
    public T GetView<T>(VIEW_TYPE viewType) where T : BaseView
    {
        foreach (var view in _viewStack)
        {
            if (view is T targetView)
            {
                return targetView;
            }
        }

        Debug.LogWarning($"找不到類型為 {typeof(T).Name} 的已開啟 View。");
        return null;
    }

    /// <summary>
    /// 清除所有介面
    /// </summary>
    public void ClearAll()
    {
        try
        {
            while (_viewStack != null && _viewStack.Count > 0)
            {
                BaseView baseView = _viewStack.Pop();
                if (baseView != null && baseView.gameObject != null)
                {
                    Addressables.ReleaseInstance(baseView.gameObject);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"清除介面時發生錯誤: {e}");
        }
    }

    #region 唯一介面

    /// <summary>
    /// 顯示訊息彈窗
    /// </summary>
    /// <param name="message">訊息內容</param>
    public void ShowMessagePopupView(string message)
    {
        if(_messagePopupView == null)
        {
            OpenView<MessagePopupView>(
                viewType: VIEW_TYPE.MessagePopupView,
                canvasType: CANVAS_TYPE.Canvas_Highest,
                callback: (view) =>
                {
                    view.DoShow();
                    view.SetMessage(message);

                    _messagePopupView = view;
                }).Forget();
        }
        else
        {
            _messagePopupView.gameObject.SetActive(true);
            _messagePopupView.DoShow();
            _messagePopupView.SetMessage(message);
        }
    }

    #endregion
}
