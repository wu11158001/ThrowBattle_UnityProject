using UnityEngine;
using NaughtyAttributes;

/// <summary>
/// 資料配置檔
/// </summary>
[CreateAssetMenu(fileName = "DataConfig", menuName = "SO Config/DataConfig")]
public class DataConfig : ScriptableObject
{
    [Label("API URL")]
    public string HttpBaseUrl = "http://localhost:3000";
}
