using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 遊戲場景入口
/// </summary>
public class GameEntry : MonoBehaviour
{
    private GameplayContext _context;

    private async void Start()
    {
        try
        {
            _context = new GameplayContext();

            // 開啟遊戲介面
            ViewManager.Instance.OpenView<GameView>(
                viewType: VIEW_TYPE.GameView,
                canvasType: CANVAS_TYPE.Canvas_HUD,
                callback: (view) =>
                {

                }).Forget();

            // 遊戲管理中心
            var manager = gameObject.AddComponent<GameplayManager>();
            manager.Setup(_context);

            // 遊戲控制器
            GameObject obj = new("GameController");
            _context.GameController = obj.AddComponent<GameController>();

            // 遊戲背景
            GameObject bgObject = await CreateObject(OBJECT_PREFAB_TYPE.GameBgPrefab);

            // 角色1(Player1)
            GameObject p1_Object = await CreateObject(OBJECT_PREFAB_TYPE.CharacterPrefab);
            if(p1_Object.TryGetComponent(out CharacterView p1_characterView))
            {
                p1_characterView.SetCharacter(true);
                _context.P1_CharacterView = p1_characterView;
            }

            // 角色2(Player2)
            GameObject p2_Object = await CreateObject(OBJECT_PREFAB_TYPE.CharacterPrefab);
            if (p2_Object.TryGetComponent(out CharacterView p2_characterView))
            {
                p2_characterView.SetCharacter(false);
                _context.P2_CharacterView = p2_characterView;
            }

            // 投擲物件
            GameObject throwObject = await CreateObject(OBJECT_PREFAB_TYPE.ThrowPrefab);
            if (p2_Object.TryGetComponent(out ThrowObjectView throwObjectView))
            {
                _context.ThrowObjectView = throwObjectView;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"遊戲初始化錯誤: {e}");
        }

        SceneLoader.Instance.CloseLoading();
    }

    /// <summary>
    /// 產生遊戲物件
    /// </summary>
    /// <param name="type"></param>
    private async UniTask<GameObject> CreateObject(OBJECT_PREFAB_TYPE type)
    {
        var prefabRef = StaticDataManager.ObjectPrefabConfig.GetPrefabRef(type);
        if (prefabRef == null)
        {
            Debug.LogError("找不到「遊戲內容物件」");
            return null;
        }
        var handle = prefabRef.InstantiateAsync();
        GameObject obj = await handle.Task;

        if(obj.TryGetComponent(out BaseObject baseObject))
        {
            baseObject.SetData(prefabRef);
        }

        return obj;
    }
}
