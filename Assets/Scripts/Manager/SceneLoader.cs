using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 場景類型
/// </summary>
public enum SCENE_TYPE
{
    LobbyScene,
    GameScene,
}

/// <summary>
/// 場景載入中心
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class SceneLoader : SingletonMonoBehaviour<SceneLoader>
{
    private CanvasGroup _canvasGroup;

    protected override void Awake()
    {
        base.Awake();

        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
    }

    /// <summary>
    /// 載入場景
    /// </summary>
    /// <param name="sceneType"></param>
    /// <returns></returns>
    public async UniTask LoadSceneAsync(SCENE_TYPE sceneType)
    {
        ViewManager.ClearAll();

        _canvasGroup.alpha = 1;

        // 當前場景與轉換場景一樣
        if (SceneManager.GetActiveScene().name == sceneType.ToString())
            return;

        await SceneManager.LoadSceneAsync(sceneType.ToString()).ToUniTask();

        AudioManager.Instance.ClearAll();
    }

    /// <summary>
    /// 關閉載入畫面
    /// </summary>
    public void CloseLoading()
    {
        _canvasGroup.alpha = 0;
    }
}
