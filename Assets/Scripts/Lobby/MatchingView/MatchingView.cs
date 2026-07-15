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
                OnCancelClick();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 取消配對點擊事件
    /// </summary>
    private void OnCancelClick()
    {
        CancelMatchRequest req = new()
        {
            playerId = StaticDataManager.RegisterPlayerData.PlayerId
        };

        _ = HttpManager.Instance.SendPostAsync<CancelMatchRequest, CancelMatchResponse>(
                subUrl: "/api/lobby/cancel-match",
                requestData: req,
                onSuccess: (res) =>
                {
                    Close();
                },
                onFailure: (code, err) =>
                {
                    Close();
                }
            );
    }
}
