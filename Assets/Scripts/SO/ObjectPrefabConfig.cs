using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 遊戲物件類型
/// </summary>
public enum OBJECT_PREFAB_TYPE
{
    /// <summary> 遊戲內容 </summary>
    GameContentView,
}

/// <summary>
/// 遊戲物件配置檔
/// </summary>
[CreateAssetMenu(fileName = "ObjectPrefabConfig", menuName = "SO Config/ObjectPrefabConfig")]
public class ObjectPrefabConfig : ScriptableObject
{
    public List<ObjectMapping> Mappings;

    // 快取查找
    private Dictionary<OBJECT_PREFAB_TYPE, AssetReferenceGameObject> _cacheLookup = new();

    [Serializable]
    public struct ObjectMapping
    {
        public OBJECT_PREFAB_TYPE Type;
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
    /// 獲取遊戲物件
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public AssetReferenceGameObject GetPrefabRef(OBJECT_PREFAB_TYPE type)
    {
        return _cacheLookup.TryGetValue(type, out var prefabRef) ? prefabRef : null;
    }
}
