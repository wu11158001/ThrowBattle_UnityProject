using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets;
using NaughtyAttributes;

/// <summary>
/// 音訊類型
/// </summary>
public enum AUDIO_TYPE
{
    None = 0,

    /// <summary> 按鈕點擊音效 </summary>
    ButtonClick = 100,
}

/// <summary>
/// 音訊配置檔
/// </summary>
[CreateAssetMenu(fileName = "AudioConfig", menuName = "SO Config/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [Label("背景淡入淡出時間")] public float BgmFadeDuration;

    [HorizontalLine(color: EColor.Gray)]
    [AllowNesting]
    [BoxGroup("音樂資料")]
    public List<AudioData> AudioDatas;
}

/// <summary>
/// 音訊資料
/// </summary>
[Serializable]
public class AudioData
{
    public AUDIO_TYPE AudioType;
    public AssetReferenceT<AudioClip> AudioClip;
    [Range(0f, 1f)] public float Volume = 1f;
}
