using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 大廳入口
/// </summary>
public class LobbyEntry : MonoBehaviour
{
    private void Start()
    {
        // 開啟大廳介面
        ViewManager.Instance.OpenView<LobbyView>(
            viewType: VIEW_TYPE.LobbyView,
            canvasType: CANVAS_TYPE.Canvas_HUD,
            callback: (view) =>
            {
                
            }).Forget();

        // 還未註冊
        if(StaticDataManager.RegisterPlayerData == null)
        {
            // 開啟暱設置稱介面
            ViewManager.Instance.OpenView<SetNicknameView>(
                viewType: VIEW_TYPE.SetNicknameView,
                canvasType: CANVAS_TYPE.Canvas_Highest,
                callback: (view) =>
                {

                }).Forget();
        }

        SceneLoader.Instance.CloseLoading();
    }
}
