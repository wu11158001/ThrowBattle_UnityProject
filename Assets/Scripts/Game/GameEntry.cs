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

            // 遊戲管理中心
            var manager = gameObject.AddComponent<GameplayManager>();
            manager.SetData(_context);

            // 遊戲控制器
            GameObject obj = new("GameController");
            var gameController = obj.AddComponent<GameController>();
            _context.GameController = gameController;

            // 遊戲背景
            GameObject bgObject = await CreateObject(OBJECT_PREFAB_TYPE.GameBgPrefab);

            // 角色1(Player1)
            GameObject p1_Object = await CreateObject(OBJECT_PREFAB_TYPE.CharacterPrefab);
            p1_Object.name = "Player1";
            if (p1_Object.TryGetComponent(out CharacterView p1_characterView))
            {
                p1_characterView.SetCharacter(true);
                _context.P1_CharacterView = p1_characterView;
            }

            // 角色2(Player2)
            GameObject p2_Object = await CreateObject(OBJECT_PREFAB_TYPE.CharacterPrefab);
            p2_Object.name = "Player2";
            if (p2_Object.TryGetComponent(out CharacterView p2_characterView))
            {
                p2_characterView.SetCharacter(false);
                _context.P2_CharacterView = p2_characterView;
            }

            // 投擲物件
            GameObject throwObject = await CreateObject(OBJECT_PREFAB_TYPE.ThrowPrefab);
            throwObject.name = "ThrowObject";
            if (throwObject.TryGetComponent(out ThrowObjectView throwObjectView))
            {
                _context.ThrowObjectView = throwObjectView;
            }

            // 開啟遊戲介面
            ViewManager.Instance.OpenView<GameView>(
                viewType: VIEW_TYPE.GameView,
                canvasType: CANVAS_TYPE.Canvas_HUD,
                callback: (view) =>
                {
                    _context.GameView = view;
                }).Forget();
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
