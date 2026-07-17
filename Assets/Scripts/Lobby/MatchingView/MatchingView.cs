using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.AddressableAssets;

/// <summary>
/// 配對中介面
/// </summary>
public class MatchingView : BaseView
{
    [Header("配對中介面")]
    [SerializeField] private Button _btn_Cancel;

    private MatchingViewModel _viewModel = new();

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        Bind();
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
}
