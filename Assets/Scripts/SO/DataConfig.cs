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
    [Label("角色皮膚顏色:P1")] public string CharacterColor_P1 = "E2FF5D";
    [Label("角色皮膚顏色:P2")] public string CharacterColor_P2 = "FFFFFF";

    [Header("投擲物件")]
    [Label("判斷地板Y軸位置")] public float GroundJudgeY = -3.5f;
}
