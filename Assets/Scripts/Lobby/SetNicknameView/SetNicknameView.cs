using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine.InputSystem;

/// <summary>
/// 註冊成功訊息
/// </summary>
public class RegisterSuccessMessage
{
    public PlayerData PlayerData { get; set; }
}

/// <summary>
/// 設置暱稱介面
/// </summary>
public class SetNicknameView : BaseView
{
    [Header("設置暱稱介面")]
    [SerializeField] private TMP_InputField if_Nickname;
    [SerializeField] private Button _btn_Register;
    [SerializeField] private TextMeshProUGUI _text_BtnRegister;
    [SerializeField] private TextMeshProUGUI _text_Error;

    private SetNicknameViewModel _viewModel = new();

    private void Start()
    {
        _text_Error.gameObject.SetActive(false);
        _text_BtnRegister.text = "註冊";
        if_Nickname.ActivateInputField();

        Bind();
    }

    private void Bind()
    {
        // 每幀驅動
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                var keyboard = Keyboard.current;
                if (keyboard == null) return;

                if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
                {
                    OnRegisterClick();
                }
            })
            .AddTo(this);

        // 輸入框
        if_Nickname.onValueChanged.AddListener((value) => _btn_Register.interactable = value.Length >= 2);

        // 註冊按紐
        _btn_Register.OnClickAsObservable()
            .Subscribe(_ =>
            {
                OnRegisterClick();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 註冊點擊事件
    /// </summary>
    private void OnRegisterClick()
    {
        string inputName = if_Nickname.text;

        _text_Error.gameObject.SetActive(false);
        _text_BtnRegister.text = "註冊中...";
        _btn_Register.interactable = false;

        _viewModel.SendRegisterRequest(
            nickname: inputName,
            successCallback: () =>
            {
                Close();
            },
            failCallback: (errorCode) =>
            {
                _btn_Register.interactable = true;
                _text_BtnRegister.text = "註冊";
                _text_Error.gameObject.SetActive(true);

                if (errorCode == 400)
                {
                    _text_Error.text = "註冊失敗！此暱稱已被佔用，請換一個。";
                }
                else if (errorCode == 0)
                {
                    _text_Error.text = "連不上伺服器，請檢查網路連線！";
                }
                else
                {
                    _text_Error.text = "系統發生未知錯誤，請稍後再試。";
                }
            });
    }
}
