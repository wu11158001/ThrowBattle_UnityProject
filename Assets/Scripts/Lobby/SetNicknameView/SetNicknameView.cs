using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

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

    private void Start()
    {
        _text_Error.gameObject.SetActive(false);
        _text_BtnRegister.text = "註冊";

        Bind();
    }

    private void Bind()
    {
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

        RegisterRequest req = new RegisterRequest { nickname = inputName };
        _ = HttpManager.SendPostAsync<RegisterRequest, RegisterResponse>(
                subUrl: StaticDataManager.RegisterSubUrl,
                req,
                onSuccess: (res) =>
                {
                    StaticDataManager.RegisterPlayerData = new()
                    {
                        Nickname = res.nickname,
                        PlayerId = res.playerId,
                    };

                    Close();
                },
                onFailure: (code, err) =>
                {
                    _btn_Register.interactable = true;
                    _text_BtnRegister.text = "註冊";
                    _text_Error.gameObject.SetActive(true);

                    if (code == 400)
                    {
                        _text_Error.text = "註冊失敗！此暱稱已被佔用，請換一個。";
                    }
                    else if (code == 0)
                    {
                        _text_Error.text = "連不上伺服器，請檢查網路連線！";
                    }
                    else
                    {
                        _text_Error.text = "系統發生未知錯誤，請稍後再試。";
                    }              
                }
            );
    }
}
