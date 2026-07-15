using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameEntry : MonoBehaviour
{
    private void Start()
    {
        // 開啟遊戲介面
        ViewManager.Instance.OpenView<GameView>(
            viewType: VIEW_TYPE.GameView,
            canvasType: CANVAS_TYPE.Canvas_HUD,
            callback: (view) =>
            {

            }).Forget();
    }
}
