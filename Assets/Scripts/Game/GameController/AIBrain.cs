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
    public void RequestAIDecision()
    {
        StartCoroutine(IRequestAIDecision());
    }

    private IEnumerator IRequestAIDecision()
    {
        if (_isRequesting)
        {
            Debug.LogWarning("[AIBrain] AI 正在思考中，忽略重複請求。");
            yield break;
        }

        _isRequesting = true;

        // AI風格資料
        AIStyleData aIStyleData = StaticDataManager.AIStyleData;
        // 風力
        float wind = _context.GameController.ThrowController.WindStrength;
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

            【AI 性格】
            {aIStyleData.Describe}

            【當前局勢】
            - AI(你)血量: {aiHp} / 敵方血量: {playerHp}
            - AI(你)位置: {aiPosX}，可移動範圍: {-moveRange.y} 到 {-moveRange.x}
            - 敵方距離: {distance} 單位
            - 技能狀態: 
              1.[throwType = 0 (普通攻擊)]: 無 CD
              2.[throwType = 1 (巨大化)]: {giantStatus}
              3.[throwType = 2 (強化傷害)]: {strengthStatus}

            【約束條件】
            1. 嚴禁選擇冷卻中的技能 (CD > 0)。
            2. moveDirection 範圍為 -1 (往左) 到 1 (往右)。
            3. moveDistance 範圍為 0 到 {moveRange.y - moveRange.x}。
            4. 走位後的 X 位置不可超越移動範圍 {-moveRange.y} 至 {-moveRange.x}。
            5. tauntText 必須符合性格，長度 15 字以內繁體中文，避免俗套詞彙。
            6. 請嚴格依據以下 JSON 格式輸出，絕對不能遺漏任何欄位名稱(Key)：
            {{
              ""moveDirection"": 0.0,
              ""moveDistance"": 0.0,
              ""throwType"": 0,
              ""tauntText"": ""看招！""
            }}";

        // 安全處理字串轉義
        string cleanPrompt = EscapeString(prompt);

        // 嚴謹的 Groq / OpenAI JSON Body 結構(llama-3.3-70b-versatile)
        string jsonRequestBody = "{" +
            "\"model\": \"llama-3.1-8b-instant\"," +
            "\"temperature\": 0.7," +
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
                    StartExecuteAITurn(wind, responseData);
                }
                else
                {
                    Debug.LogWarning($"[Groq API] 請求內容 null");

                    AIDecisionData responseData = GetFailData();
                    StartExecuteAITurn(wind, responseData);
                }
            }
            else
            {
                Debug.LogError($"[Groq API] 請求失敗 ({request.responseCode}): {request.error}\n內文: {request.downloadHandler?.text}");

                AIDecisionData responseData = GetFailData();
                StartExecuteAITurn(wind, responseData);
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
            moveDirection = UnityEngine.Random.Range(-1f, 1),
            moveDistance = UnityEngine.Random.Range(0f, moveRange.y - moveRange.x),
            throwType = 0,
            tauntText = "看招！"
        };

        return responseData;
    }

    /// <summary>
    /// 開始執行AI回合操作
    /// </summary>
    public void StartExecuteAITurn(float windStrength, AIDecisionData data)
    {
        float perfectForce = CalculateChargeForceForTarget(windStrength);
        StartCoroutine(IExecuteAITurn(data, perfectForce));
    }

    /// <summary>
    /// 根據目標位置反推需要的蓄力強度
    /// </summary>
    public float CalculateChargeForceForTarget(float windStrength)
    {
        // 玩家角色位置
        float playerPosX = _context.P1_CharacterView.transform.position.x;
        // AI角色位置
        float aiPosX = _context.P2_CharacterView.transform.position.x;

        float maxDistance = Mathf.Max(1f, _dataConfig.ThrowMaxDistance + windStrength);

        // AI 在左邊往右投擲，距離為目標 X - 攻擊者 X
        float distance = Mathf.Abs(playerPosX - aiPosX);

        // 反推完美力道，並限制在 0.1 ~ 1.0 之間
        float idealForce = distance / maxDistance;
        return Mathf.Clamp(idealForce, 0.1f, 1.0f);
    }

    /// <summary>
    /// 執行AI回合操作
    /// </summary>
    private IEnumerator IExecuteAITurn(AIDecisionData data, float perfectForce)
    {
        CharacterView aiCharacter = _context.CurrentTurnCharacter;
        if (aiCharacter == null) yield break;

        // 顯示嘲諷內容
        if(!string.IsNullOrEmpty(data.tauntText))
        {
            aiCharacter.ShowTextBubble(data.tauntText);
        }

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

        // 根據 AI 困難度套用誤差
        AIDifficultyData aIDifficultyData = StaticDataManager.AIDifficultyData;
        float hitRateNormalized = aIDifficultyData.HitRate / 100f;
        float maxError = (1f - hitRateNormalized) * 0.35f; // 最大誤差範圍

        float error = UnityEngine.Random.Range(-maxError, maxError);
        float finalTargetForce = Mathf.Clamp(perfectForce + error, 0.1f, 1.0f);


        // 持續蓄力，直到達到指定的力道
        while (_throwController.ThrowStrength < finalTargetForce)
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