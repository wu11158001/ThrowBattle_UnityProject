using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System;

/// <summary>
/// AI困難度按鈕
/// </summary>
public class AIDifficultyBtn : MonoBehaviour
{
    [SerializeField] private Button _btn_Main;
    [SerializeField] private TextMeshProUGUI _text_BtnText;

    private Action _clickAction;

    public void SetData()
    {

    }
}
