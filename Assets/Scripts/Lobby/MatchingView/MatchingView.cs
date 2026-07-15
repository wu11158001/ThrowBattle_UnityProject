using UnityEngine;
using UnityEngine.UI;
using UniRx;

/// <summary>
/// 配對中介面
/// </summary>
public class MatchingView : BaseView
{
    [Header("配對中介面")]
    [SerializeField] private Button _btn_Cancel;

    private MatchingViewModel _viewModel = new();

    private void Start()
    {
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
