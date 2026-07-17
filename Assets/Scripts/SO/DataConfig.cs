using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// 資料配置檔
/// </summary>
[CreateAssetMenu(fileName = "DataConfig", menuName = "SO Config/DataConfig")]
public class DataConfig : ScriptableObject
{
    [Header("API")]
    [Label("API URL")] public string BaseUrl = "http://localhost:3000";

    [Header("角色資料")]
    [Label("角色Y軸位置")] public float CharacterPosY = -2.6f;
    [Label("角色水平可移動範圍")] public Vector2 CharacterMoveRange = new(3f, 6f);
    [Label("移動速度")] public float CharacterMoveSpeed = 2f;
    [Label("角色皮膚顏色:P1")] public string CharacterColor_P1 = "E2FF5D";
    [Label("角色皮膚顏色:P2")] public string CharacterColor_P2 = "FFFFFF";

    [Header("投擲物件")]
    [Label("投擲蓄力速度(幾秒內集滿)")] public float ThrowChargeSpeed = 1.5f;
    [Label("投擲判斷地板Y軸位置")] public float ThrowGroundJudgeY = -3.5f;
    [Label("投擲最遠距離")] public float ThrowMaxDistance = 20f;
    [Label("投擲最大高度")] public float ThrowMaxHeight = 3.8f;
    [Label("投擲移動時間")] public float ThrowMoveDuration = 1f;

    [Header("風力")]
    [Label("風力最大強度")] public float WindMaxStrength = 5f;
}
