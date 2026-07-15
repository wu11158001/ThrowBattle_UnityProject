using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using Cysharp.Threading.Tasks;

/// <summary>
/// 大廳介面
/// </summary>
public class LobbyView : BaseView
{
    [Header("大廳介面")]
    [SerializeField] private TextMeshProUGUI _text_Nickname;
    [SerializeField] private Button _btn_Match;
    [SerializeField] private TextMeshProUGUI _text_BtnMatch;

    private void Start()
    {
        _text_Nickname.text = "";
        _text_BtnMatch.text = "玩家配對";

        Bind();
    }

    private void Bind()
    {
        // 訂閱:註冊成功廣播
        MessageBroker.Default.Receive<RegisterSuccessMessage>()
            .Subscribe(msg =>
            {
                _text_Nickname.text = $"{msg.PlayerData.Nickname}";
            })
            .AddTo(this);

        // 玩家配對按鈕
        _btn_Match.OnClickAsObservable()
            .Subscribe(_ =>
            {
                OnMatchClick();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 配對點擊事件
    /// </summary>
    private void OnMatchClick()
    {
        _btn_Match.interactable = false;
        _text_BtnMatch.text = "配對中...";

        MatchRequest req = new()
        {
            playerId = StaticDataManager.RegisterPlayerData.PlayerId,
            characterIndex = StaticDataManager.RegisterPlayerData.CharacterIndex,
        };

        _ = HttpManager.Instance.SendPostAsync<MatchRequest, MatchResponse>(
                subUrl: StaticDataManager.MatchSubUrl,
                requestData: req,
                onSuccess: (res) =>
                {
                    _btn_Match.interactable = true;

                    // 開啟配對中介面
                    ViewManager.Instance.OpenView<MatchingView>(
                        viewType: VIEW_TYPE.MatchingView,
                        canvasType: CANVAS_TYPE.Canvas_Highest).Forget();
                },
                onFailure: (code, err) =>
                {
                    _btn_Match.interactable = true;

                    string message = "";
                    if (code == 0)
                    {
                        message = "連不上伺服器，請檢查網路連線！";
                    }
                    else
                    {
                        message = "系統發生未知錯誤，請稍後再試。";
                    }

                    ViewManager.Instance.ShowMessagePopupView(message);
                }
            );
    }
}
