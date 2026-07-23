using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// 對戰模式
/// </summary>
public enum PLAY_TYPE
{
    /// <summary> 連線配對 </summary>
    Match,
    /// <summary> AI對戰 </summary>
    WithAi,
    /// <summary> 兩名玩家 </summary>
    TwoPlayer,
}

/// <summary>
/// 大廳介面
/// </summary>
public class LobbyView : BaseView
{
    [Header("大廳介面")]
    [SerializeField] private TextMeshProUGUI _text_Nickname;
    [SerializeField] private Button _btn_Match;
    [SerializeField] private TextMeshProUGUI _text_BtnMatch;
    [SerializeField] private Button _btn_TwoPlayer;
    [SerializeField] private Button _btn_WithAi;

    private LobbyViewModel _viewModel = new();

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        _text_BtnMatch.text = "玩家配對";

        if (StaticDataManager.RegisterPlayerData != null)
        {
            _text_Nickname.text = $"玩家:{StaticDataManager.RegisterPlayerData.Nickname}"; 
        }

        Bind();
    }

    private void Bind()
    {
        // 訂閱:註冊成功廣播
        MessageBroker.Default.Receive<RegisterSuccessMessage>()
            .Subscribe(msg =>
            {
                _text_Nickname.text = $"玩家:{msg.PlayerData.Nickname}";
            })
            .AddTo(this);

        // AI對戰按鈕
        _btn_WithAi.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ViewManager.Instance.OpenView<AIDifficultyView>(
                    viewType: VIEW_TYPE.AIDifficultyView,
                    canvasType: CANVAS_TYPE.Canvas_Highest)
                .Forget();
            })
            .AddTo(this);

        // 兩名玩家按鈕
        _btn_TwoPlayer.OnClickAsObservable()
            .Subscribe(_ =>
            {
                StaticDataManager.PlayType = PLAY_TYPE.TwoPlayer;
                SceneLoader.Instance.LoadSceneAsync(SCENE_TYPE.GameScene).Forget();
            })
            .AddTo(this);

        // 玩家配對按鈕
        _btn_Match.OnClickAsObservable()
            .Subscribe(_ =>
            {
                StaticDataManager.PlayType = PLAY_TYPE.Match;

                _btn_Match.interactable = false;
                _text_BtnMatch.text = "配對中...";

                _viewModel.SendMatchRequest(
                    successCallback: () =>
                    {
                        _btn_Match.interactable = true;

                        // 開啟配對中介面
                        ViewManager.Instance.OpenView<MatchingView>(
                            viewType: VIEW_TYPE.MatchingView,
                            canvasType: CANVAS_TYPE.Canvas_Highest,
                            callback: (view) =>
                            {
                                view.SetCancelAction(() => _text_BtnMatch.text = "玩家配對");
                            }).Forget();
                    },
                    failCallback: (errorCode) =>
                    {
                        _btn_Match.interactable = true;

                        string message = "";
                        if (errorCode == 0)
                        {
                            message = "連不上伺服器，請檢查網路連線！";
                        }
                        else
                        {
                            message = "系統發生未知錯誤，請稍後再試。";
                        }

                        ViewManager.Instance.ShowMessagePopupView(message);
                    });
            })
            .AddTo(this);
    }
}
