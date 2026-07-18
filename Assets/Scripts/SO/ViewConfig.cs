using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 介面類型
/// </summary>
public enum VIEW_TYPE
{
    BackgroundView = 0,
    MessagePopupView,
    AskView,

    LobbyView = 100,
    SetNicknameView,
    MatchingView,

    GameView = 200,
    GameOverView,
}

/// <summary>
/// 介面配置檔
/// </summary>
[CreateAssetMenu(fileName = "ViewConfig", menuName = "SO Config/ViewConfig")]
public class ViewConfig : ScriptableObject, ISerializationCallbackReceiver
{
    public List<ViewMapping> Mappings;

    // 快取查找
    private Dictionary<VIEW_TYPE, AssetReferenceGameObject> _cacheLookup = new();

    [Serializable]
    public struct ViewMapping
    {
        public VIEW_TYPE Type;
        public AssetReferenceGameObject PrefabRef;
    }

    // 序列化前
    public void OnBeforeSerialize() { }

    // 反序列化後
    public void OnAfterDeserialize()
    {
        _cacheLookup.Clear();
        foreach (var mapping in Mappings)
        {
            _cacheLookup[mapping.Type] = mapping.PrefabRef;
        }
    }

    /// <summary>
    /// 獲取介面物件
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public AssetReferenceGameObject GetPrefabRef(VIEW_TYPE type)
    {
        return _cacheLookup.TryGetValue(type, out var prefabRef) ? prefabRef : null;
    }
}
