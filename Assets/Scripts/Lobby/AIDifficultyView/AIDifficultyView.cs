using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// AI 困難度選擇介面
/// </summary>
public class AIDifficultyView : BaseView
{
    [Header("AI 困難度選擇介面")]
    [SerializeField] private RectTransform _DifficultyBtnPanel;
    [SerializeField] private AIDifficultyBtn _btn_DifficultyPrefab;
    [SerializeField] private TMP_Dropdown dd_AiStyle;

    private DataConfig _dataConfig;

    public override void OnDestroy()
    {
        if (dd_AiStyle != null)
        {
            dd_AiStyle.onValueChanged.RemoveListener(OnAIStyleChanged);
        }

        base.OnDestroy();
    }

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        _dataConfig = StaticDataManager.DataConfig;

        CreateDifficultyBtns();
        SetAiStyleDropdown();
    }

    /// <summary>
    /// 創建AI困難度按鈕
    /// </summary>
    private void CreateDifficultyBtns()
    {
        _btn_DifficultyPrefab.gameObject.SetActive(false);
        for (int i = 0; i < _dataConfig.AIDifficultyDatas.Count; i++)
        {
            int index = i;
            AIDifficultyData data = _dataConfig.AIDifficultyDatas[i];

            GameObject obj = Instantiate(_btn_DifficultyPrefab.gameObject, _DifficultyBtnPanel);
            obj.SetActive(true);

            if(obj.TryGetComponent(out AIDifficultyBtn aiDifficultyBtn))
            {
                aiDifficultyBtn.SetData(
                    data: data,
                    clickAction: () =>
                    {
                        StaticDataManager.AIDifficultyData = data;
                        StaticDataManager.PlayType = PLAY_TYPE.WithAi;

                        SceneLoader.Instance.LoadSceneAsync(SCENE_TYPE.GameScene).Forget();
                    });
            }
        }
    }

    /// <summary>
    /// 設置AI風格下拉式選單
    /// </summary>
    private void SetAiStyleDropdown()
    {
        dd_AiStyle.ClearOptions();
        dd_AiStyle.onValueChanged.RemoveAllListeners();

        // 2. 建立新的選項清單
        List<TMP_Dropdown.OptionData> options = new();

        foreach (var styleData in _dataConfig.AIStyleDatas)
        {
            string displayText = string.IsNullOrEmpty(styleData.StyleString)
                ? styleData.Style.ToString()
                : styleData.StyleString;

            options.Add(new TMP_Dropdown.OptionData(displayText));
        }

        dd_AiStyle.AddOptions(options);
        dd_AiStyle.onValueChanged.AddListener(OnAIStyleChanged);

        // Optional: 預設主動觸發一次
        if (_dataConfig.AIStyleDatas.Count > 0)
        {
            OnAIStyleChanged(0);
        }
    }

    /// <summary>
    /// AI風格選項改變
    /// </summary>
    /// <param name="index"></param>
    private void OnAIStyleChanged(int index)
    {
        if (index < 0 || index >= _dataConfig.AIStyleDatas.Count) return;

        AIStyleData selectedData = _dataConfig.AIStyleDatas[index];
        StaticDataManager.AIStyleData = selectedData;
    }
}
