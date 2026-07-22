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

    public void SetData(AIDifficultyData data, Action clickAction)
    {
        _clickAction = clickAction;

        _text_BtnText.text = data.BtnString;

        Bind();
    }

    private void Bind()
    {
        _btn_Main.OnClickAsObservable()
            .Subscribe(_ => _clickAction?.Invoke())
            .AddTo(this);
    }
}
