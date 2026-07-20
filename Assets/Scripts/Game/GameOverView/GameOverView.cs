using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UniRx;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

/// <summary>
/// 遊戲結束介面
/// </summary>
public class GameOverView : BaseView
{
    [SerializeField] private TextMeshProUGUI _text_Result;
    [SerializeField] private Button _btn_Confirm;

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        Bind();
    }

    private void Bind()
    {
        _btn_Confirm.OnClickAsObservable()
            .First()
            .Subscribe(_ =>
            {
                HttpManager.Instance.SendLeaveBattle();
                SceneLoader.Instance.LoadSceneAsync(SCENE_TYPE.LobbyScene).Forget();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 設置遊戲結果
    /// </summary>
    /// <param name="winMessage"></param>
    public void SetResult(string winMessage)
    {
        _text_Result.text = winMessage;
    }
}
