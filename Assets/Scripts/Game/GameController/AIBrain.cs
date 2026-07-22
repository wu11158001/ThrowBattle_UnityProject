using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// AI 性格
/// </summary>
public enum AIStyle
{
    /// <summary> 中二病風格 </summary>
    ChuunibyouStyle,
    /// <summary> 傲嬌風格 </summary>
    ArrogantStyle,
    /// <summary> 陰險老練風格 </summary>
    InsidiousStyle,
}

/// <summary>
/// AI決策資料
/// </summary>
[Serializable]
public class AIDecisionData
{
    /// <summary> 移動方向 </summary>
    public float moveDirection;
    /// <summary> 移動距離 </summary>
    public float moveDistance;
    /// <summary> 投擲類型 </summary>
    public int throwType;
    /// <summary> 蓄力強度 </summary>
    public float chargeForce;
    /// <summary> 嘲諷語 </summary>
    public string tauntText;
}

/// <summary>
/// AI 大腦控制中心
/// </summary>
public class AIBrain : MonoBehaviour
{
    private string apiKey;
    private string apiUrl;

    private bool _isRequesting = false;

    private int _giantCD = 0;
    private int _strengthCD = 0;

    private GameplayContext _context;
    private DataConfig _dataConfig;
    private CharacterThrowController _throwController;

    private void Awake()
    {
        _context = GameplayManager.CurrentContext;
        _dataConfig = StaticDataManager.DataConfig;
        _throwController = _context.GameController.ThrowController;

        apiKey = _dataConfig.AiApiKey;
        apiUrl = _dataConfig.AiApiUrl;
    }

    /// <summary>
    /// 發送API請求 AI 取得決策
    /// </summary>
    public IEnumerator RequestAIDecision()
    {
        if (_isRequesting)
        {
            Debug.LogWarning("[AIBrain] AI 正在思考中，忽略重複請求。");
            yield break;
        }

        _isRequesting = true;

        // 風力
        float wind = _context.GameController.ThrowController.WindStrength;
        // 最大投擲距離
        float baseMaxDistance = _dataConfig.ThrowMaxDistance;
        // 移動範圍
        Vector2 moveRange = _dataConfig.CharacterMoveRange;
        // AI角色位置
        float aiPosX = _context.P2_CharacterView.transform.position.x;
        // AI角色Hp
        int aiHp = _context.P2_CharacterView.CurrentHp;
        // 玩家角色Hp
        int playerHp = _context.P1_CharacterView.CurrentHp;
        // 兩角色距離
        Vector3 p1Pos = _context.P1_CharacterView.transform.position;
        Vector3 p2Pos = _context.P2_CharacterView.transform.position;
        float distance = Mathf.Abs(p1Pos.x - p2Pos.x);

        // 建立技能可用狀態說明
        string giantStatus = _giantCD == 0 ? "【可用】" : $"【冷卻中，還需 {_giantCD} 回合】";
        string strengthStatus = _strengthCD == 0 ? "【可用】" : $"【冷卻中，還需 {_strengthCD} 回合】";

        string prompt = $@"你是一個彈道投擲遊戲的 AI。請根據當前物理參數與局勢做出精準決策。

            【當前戰局狀態】
            - AI(你) 血量: {aiHp} / 玩家(敵方) 血量: {playerHp}
            - AI(你) 的位置在 {aiPosX}，移動範圍為 {-moveRange.y} 到 {-moveRange.x}
            - 敵方與你的當前距離: {distance} 單位
            - 當前風力 (windStrength): {wind} (正數為順風拉長距離，負數為逆風縮短距離)
            - AI 你處於左側，玩家處於右側
            - 技能可使用就盡量使用，但不一定要使用
            - 多移動走位 (moveDirection)
            
            【技能機制與物理公式】
            所有技能皆採用【拋物線投擲】，實際投擲距離公式如下：
            - 實際距離 = Max(1, {baseMaxDistance} + {wind}) * chargeForce
            1. [throwType = 0 (普通攻擊)]: 無 CD，標準傷害。
            2. [throwType = 1 (巨大化)] {giantStatus}: 命中範圍增大{_dataConfig.SkillGiantSize}倍，預設範圍是1。
            3. [throwType = 2 (強化傷害)] {strengthStatus}: 命中後傷害大幅提升。

            【需求】
            請根據你預計的走位 moveDirection 與當前距離，算出精準的 chargeForce (範圍 0.1 ~ 1.0)，使投擲物剛好命中距離 {distance} 的玩家！
            公式參考：chargeForce = (目標距離 - 走位距離) / Max(1, {baseMaxDistance} + {wind})

            【約束條件】
            1. 嚴禁選擇冷卻中 (CD > 0) 的技能！只能選擇【可用】或普通攻擊(0)。
            2. chargeForce 絕不能為 0！範圍必須在 0.1 至 1.0 之間。
            3. moveDirection 範圍為 -1 (往左) 到 1 (往右)。若不走位請填 0.0。
            4. moveDistance 範圍為 0 到 {moveRange.y - moveRange.x}。若不走位請填 0.0。
            5.移動走位 (moveDirection) AI(你) 的位置{aiPosX} 加上 moveDirection 乘上 moveDistance 不要小於 {-moveRange.y} 或是大於 {-moveRange.x}
            6. tauntText 必須符合你的性格，且長度在 15 字以內，繁體中文。
            7. 請嚴格僅輸出 JSON 格式：
            {{""moveDirection"": 0.0, ""moveDistance"": 0.0, ""throwType"": 0, ""chargeForce"": 0.75, ""tauntText"": ""看招！""}}";

        // 安全處理字串轉義
        string cleanPrompt = EscapeString(prompt);

        // 嚴謹的 Groq / OpenAI JSON Body 結構
        string jsonRequestBody = "{" +
            "\"model\": \"llama-3.3-70b-versatile\"," +
            "\"messages\": [" +
                "{\"role\": \"system\", \"content\": \"You are a game AI. Always response in pure JSON format.\"}," +
                "{\"role\": \"user\", \"content\": \"" + cleanPrompt + "\"}" +
            "]," +
            "\"response_format\": {\"type\": \"json_object\"}" +
        "}";

        // 發送請求
        using (UnityWebRequest request = new(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequestBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string rawResponse = request.downloadHandler.text;

                JObject root = JObject.Parse(rawResponse);
                string contentJson = root["choices"]?[0]?["message"]?["content"]?.ToString();

                if (!string.IsNullOrEmpty(contentJson))
                {
                    AIDecisionData responseData = JsonConvert.DeserializeObject<AIDecisionData>(contentJson);

                    string prettyJson = JsonUtility.ToJson(responseData, true);
                    Debug.Log($"[API 獲取AI資料成功]: {prettyJson}");
                    StartCoroutine(ExecuteAITurn(responseData));
                }
                else
                {
                    Debug.LogWarning($"[Groq API] 請求內容 null");

                    AIDecisionData responseData = GetFailData();
                    StartCoroutine(ExecuteAITurn(responseData));
                }
            }
            else
            {
                Debug.LogError($"[Groq API] 請求失敗 ({request.responseCode}): {request.error}\n內文: {request.downloadHandler?.text}");

                AIDecisionData responseData = GetFailData();
                StartCoroutine(ExecuteAITurn(responseData));
            }
        }

        _isRequesting = false;
    }

    /// <summary>
    /// AI獲取失敗,保底給予數值
    /// </summary>
    private AIDecisionData GetFailData()
    {
        Debug.LogWarning("AI獲取失敗,保底給予數值");

        // 移動範圍
        Vector2 moveRange = _dataConfig.CharacterMoveRange;

        // 失敗保底
        AIDecisionData responseData = new()
        {
            moveDirection = 0,
            moveDistance = 0,
            throwType = 0,
            chargeForce = UnityEngine.Random.Range(0.1f, 1.0f),
            tauntText = "看招！"
        };

        return responseData;
    }

    /// <summary>
    /// 執行AI回合操作
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private IEnumerator ExecuteAITurn(AIDecisionData data)
    {
        CharacterView aiCharacter = _context.CurrentTurnCharacter;
        if (aiCharacter == null) yield break;

        // 處理角色走位移動
        if (Mathf.Abs(data.moveDirection) > 0.01f)
        {
            int inputDir = data.moveDirection > 0 ? 1 : -1;
            _context.GameController.SetInputDirection(inputDir);

            // 移動時間計算: 距離 / 速度
            float speed = _dataConfig.CharacterMoveSpeed > 0 ? _dataConfig.CharacterMoveSpeed : 1f;
            float moveDuration = Mathf.Abs(data.moveDistance) / speed;
            yield return new WaitForSeconds(moveDuration);

            _context.GameController.SetInputDirection(0);
            yield return new WaitForSeconds(0.3f);
        }

        // 扣除技能CD
        _giantCD = Mathf.Max(0, _giantCD - 1);
        _strengthCD = Mathf.Max(0, _strengthCD - 1);

        // 確定技能並設定進入 CD
        THROW_TYPE throwType = (THROW_TYPE)data.throwType;

        if (throwType == THROW_TYPE.Giant)
        {
            if (_giantCD == 0) _giantCD = _dataConfig.SkillGiantCD;
            else throwType = THROW_TYPE.Normal;

        }
        else if (throwType == THROW_TYPE.StrengthDamage)
        {
            if (_strengthCD == 0) _strengthCD = _dataConfig.StrengthDamageCD;
            else throwType = THROW_TYPE.Normal;
        }

        _context.GameController.SetThrowType(throwType);

        // 開始蓄力
        _context.GameController.SetChargingState(true);

        float targetForce = Mathf.Clamp(data.chargeForce, 0.1f, 1.0f);

        // 持續蓄力，直到達到指定的力道
        while (_throwController.ThrowStrength < targetForce)
        {
            yield return null;
        }

        // 釋放投擲
        _context.GameController.SetChargingState(false);
    }

    #region 功能類
    /// <summary>
    /// 轉譯字串
    /// </summary>
    private string EscapeString(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";
        return str.Replace("\\", "\\\\")
                  .Replace("\"", "\\\"")
                  .Replace("\n", "\\n")
                  .Replace("\r", "\\r")
                  .Replace("\t", "\\t");
    }

    /// <summary>
    /// 提取Json
    /// </summary>
    private string ExtractInnerJson(string fullApiResponse)
    {
        int startKeyIndex = fullApiResponse.IndexOf("\"MoveDirection\"");
        if (startKeyIndex == -1) startKeyIndex = fullApiResponse.IndexOf("\"moveDirection\"");
        if (startKeyIndex == -1) return fullApiResponse;

        int jsonStart = fullApiResponse.LastIndexOf('{', startKeyIndex);
        int jsonEnd = fullApiResponse.IndexOf('}', startKeyIndex);

        if (jsonStart != -1 && jsonEnd != -1)
        {
            return fullApiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
        }

        return fullApiResponse;
    }
    #endregion
}