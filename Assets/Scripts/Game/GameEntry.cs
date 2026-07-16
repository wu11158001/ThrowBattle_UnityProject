using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameEntry : MonoBehaviour
{
    private async void Start()
    {
        // 開啟遊戲介面
        ViewManager.Instance.OpenView<GameView>(
            viewType: VIEW_TYPE.GameView,
            canvasType: CANVAS_TYPE.Canvas_HUD,
            callback: (view) =>
            {

            }).Forget();

        // 產生遊戲內容物件
        var prefabRef = StaticDataManager.ObjectPrefabConfig.GetPrefabRef(OBJECT_PREFAB_TYPE.GameContentView);
        if (prefabRef == null)
        {
            Debug.LogError("找不到「遊戲內容物件」");
            return;
        }
        var handle = prefabRef.InstantiateAsync();
        GameObject obj = await handle.Task;

        SceneLoader.Instance.CloseLoading();
    }
}
