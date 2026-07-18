using UnityEngine;
using TMPro;
using UniRx;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// 詢問視窗
/// </summary>
public class AskView : BaseView
{
    [SerializeField] private TextMeshProUGUI _text_Content;
    [SerializeField] private Button _btn_Confirm;
    [SerializeField] private Button _btn_Cancel;

    private Action _confirmAction;
    private Action _cancelAction;

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        Bind();
    }

    private void Bind()
    {
        // 取消按鈕
        _btn_Cancel.OnClickAsObservable()
            .Subscribe(_ =>
            {
                _cancelAction?.Invoke();
                Close();
            })
            .AddTo(this);

        // 確認按鈕
        _btn_Confirm.OnClickAsObservable()
            .Subscribe(_ =>
            {
                _confirmAction?.Invoke();
                Close();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 設置內容
    /// </summary>
    /// <param name="content"></param>
    public void SetContent(string content, Action confirmAction, Action cancelAction = null)
    {
        _text_Content.text = content;
        _confirmAction = confirmAction;
        _cancelAction = cancelAction;
    }
}
