using UnityEngine;

/// <summary>
/// 遊戲管理中心
/// </summary>
public class GameplayManager : MonoBehaviour
{
    /// <summary> 全域唯一當前關卡存取點 </summary>
    public static GameplayContext CurrentContext { get; private set; }

    private void OnDestroy()
    {
        CurrentContext = null;
    }

    public void SetData(GameplayContext context)
    {
        CurrentContext = context;
    }
}

/// <summary>
/// 當前關卡資料內容，遊戲中資料由這取得
/// </summary>
public class GameplayContext
{
    /// <summary> 遊戲控制器 </summary>
    public GameController GameController { get; set; }
    /// <summary> 遊戲介面 </summary>
    public GameView GameView { get; set; }
    /// <summary> 角色1(Player1) </summary>
    public CharacterView P1_CharacterView { get; set; }
    /// <summary> 角色2(Player2) </summary>
    public CharacterView P2_CharacterView { get; set; }
    /// <summary> 投擲物件 </summary>
    public ThrowObjectView ThrowObjectView { get; set; }

    /// <summary> 當前可操作的角色實體（本地控制用） </summary>
    public CharacterView CurrentTurnCharacter { get; set; }
}