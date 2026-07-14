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
        ViewManager.OpenView<LobbyView>(
            viewType: VIEW_TYPE.LobbyView,
            callback: (view) =>
            {
                
            }).Forget();

        // 還未註冊
        if(StaticDataManager.RegisterPlayerData == null)
        {
            // 開啟暱設置稱介面
            ViewManager.OpenView<SetNicknameView>(
                viewType: VIEW_TYPE.SetNicknameView,
                callback: (view) =>
                {

                }).Forget();
        }

        SceneLoader.Instance.CloseLoading();
    }
}
