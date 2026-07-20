using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.AddressableAssets;
using System;

/// <summary>
/// 配對中介面
/// </summary>
public class MatchingView : BaseView
{
    [Header("配對中介面")]
    [SerializeField] private Button _btn_Cancel;

    private Action _cancelAcrion;

    private MatchingViewModel _viewModel = new();

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        Bind();
    }

    /// <summary>
    /// 設置取消事件
    /// </summary>
    /// <param name="cancalAction"></param>
    public void SetCancelAction(Action cancalAction)
    {
        _cancelAcrion = cancalAction;
    }

    private void Bind()
    {
        // 取消配對安紐
        _btn_Cancel.OnClickAsObservable()
            .First()
            .Subscribe(_ =>
            {
                _viewModel.SendCancelMatchRequest(
                    successCallback: () => Close(),
                    failCallback: (errorCode) => Close());
            })
            .AddTo(this);
    }

    public override void Close()
    {
        _cancelAcrion?.Invoke();
        base.Close();
    }
}
