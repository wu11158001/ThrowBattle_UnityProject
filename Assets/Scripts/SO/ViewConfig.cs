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

    LobbyView = 100,
    SetNicknameView,

}

/// <summary>
/// 介面配置檔
/// </summary>
[CreateAssetMenu(fileName = "ViewConfig", menuName = "SO Config/ViewConfig")]
public class ViewConfig : ScriptableObject
{
    public List<ViewMapping> Mappings;

    [Serializable]
    public struct ViewMapping
    {
        public VIEW_TYPE Type;
        public AssetReferenceGameObject PrefabRef;
    }

    /// <summary>
    /// 獲取介面資料
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public AssetReferenceGameObject GetPrefabRef(VIEW_TYPE type)
    {
        return Mappings.Find(m => m.Type == type).PrefabRef;
    }
}
