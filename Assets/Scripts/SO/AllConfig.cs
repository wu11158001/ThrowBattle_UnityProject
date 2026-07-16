using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// 所有SO資料配置檔
/// </summary>
[CreateAssetMenu(fileName = "AllConfig", menuName = "SO Config/AllConfig")]
public class AllConfig : ScriptableObject
{
    public ViewConfig ViewConfig;
    public AudioConfig AudioConfig;
    public DataConfig DataConfig;
    public ObjectPrefabConfig ObjectPrefabConfig;
}
