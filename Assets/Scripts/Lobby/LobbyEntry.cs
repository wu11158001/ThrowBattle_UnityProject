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

        SceneLoader.Instance.CloseLoading();
    }
}
