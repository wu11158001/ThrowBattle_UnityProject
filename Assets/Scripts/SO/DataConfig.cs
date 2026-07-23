using UnityEngine;
using NaughtyAttributes;
using System;
using System.Collections.Generic;

/// <summary>
/// 資料配置檔
/// </summary>
[CreateAssetMenu(fileName = "DataConfig", menuName = "SO Config/DataConfig")]
public class DataConfig : ScriptableObject
{
    [Header("API")]
    [Label("Server API URL")] public string ServerApiUrl = "https://throwbattle-server.onrender.com";
    [Label("AI API Key")] public string AiApiKey = "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";
    [Label("AI API URL")] public string AiApiUrl = "https://api.groq.com/openai/v1/chat/completions";

    [Header("角色資料")]
    [Label("角色Y軸位置")] public float CharacterPosY = -2.6f;
    [Label("角色水平可移動範圍")] public Vector2 CharacterMoveRange = new(3f, 6f);
    [Label("角色移動速度")] public float CharacterMoveSpeed = 2f;
    [Label("角色皮膚顏色:P1")] public string CharacterColor_P1 = "E2FF5D";
    [Label("角色皮膚顏色:P2")] public string CharacterColor_P2 = "FFFFFF";
    [Label("角色最大Hp")] public int CharacterMaxHp = 100;

    [Header("投擲物件")]
    [Label("投擲蓄力速度(幾秒內集滿)")] public float ThrowChargeSpeed = 1.5f;
    [Label("投擲判斷地板Y軸位置")] public float ThrowGroundJudgeY = -3.5f;
    [Label("投擲最遠距離")] public float ThrowMaxDistance = 20f;
    [Label("投擲最大高度")] public float ThrowMaxHeight = 3.8f;
    [Label("投擲移動時間")] public float ThrowMoveDuration = 1f;
    [Label("射線半徑")] public float ThrowRaycastRadius = 0.35f;
    [Label("投擲傷害")] public int ThrowDamage = 10;

    [Header("風力")]
    [Label("風力最大強度")] public float WindMaxStrength = 5f;

    [Header("技能")]
    [Label("技能_巨大化Icon")] public Sprite SkillGiantIcon;
    [Label("技能_巨大化技能描述")] public string SkillGiantDescrible = "投擲物件增大。";
    [Label("技能_巨大化Size")] public float SkillGiantSize = 2f;
    [Label("技能_巨大化CD回合")] public int SkillGiantCD = 2;
    [Label("技能_強化攻擊傷害Icon")] public Sprite SkillStrengthDamageIcon;
    [Label("技能_強化攻擊技能描述")] public string SkillStrengthDamageDescrible = "傷害增加。";
    [Label("技能_強化攻擊傷害倍率")] public float SkillStrengthDamageMultiplier = 2f;
    [Label("技能_強化攻擊CD回合")] public int StrengthDamageCD = 3;
    [Label("技能_強化攻擊位置高度")] public float SkillStrengthDamagePosHeight = 2.8f;
    [Label("技能_閃避Icon")] public Sprite SkillDodgeIcon;
    [Label("技能_閃避技能描述")] public string SkillDodgeDescrible = "下回合必定閃避攻擊。";
    [Label("技能_閃避CD回合")] public int SkillDodgeCD = 5;

    [Header("AI設置")]
    [BoxGroup("AI困難度資料")] public List<AIDifficultyData> AIDifficultyDatas;
    [BoxGroup("AI風格資料資料")] public List<AIStyleData> AIStyleDatas;
}

/// <summary>
/// AI困難度資料
/// </summary>
[Serializable]
public class AIDifficultyData
{
    [AllowNesting]
    [Label("按鈕文字")] public string BtnString;
    [AllowNesting]
    [Label("命中率")] [Range(0, 100)] public int HitRate;
}

/// <summary>
/// AI風格資料
/// </summary>
[Serializable]
public class AIStyleData
{
    [AllowNesting]
    [Label("AI 風格")] public AIStyle Style;
    [AllowNesting]
    [Label("AI 風格文字")] public string StyleString;
    [AllowNesting]
    [Label("個性描述")] [TextArea] public string Describe;
}