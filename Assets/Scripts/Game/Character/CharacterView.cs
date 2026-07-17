using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

/// <summary>
/// 角色
/// </summary>
public class CharacterView : BaseObject
{
    [SerializeField] private Animator _p1_Anim;
    [SerializeField] private TextMeshPro _text_Nickname;
    [SerializeField] private GameObject _controlTip;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("投擲力道")]
    [SerializeField] private Canvas _throwStrengthCanvas;
    [SerializeField] private Image _img_ThrowStrength;

    private void Start()
    {
        _throwStrengthCanvas.worldCamera = Camera.main;
        CloseThrowStrength();
    }

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        _controlTip.SetActive(false);
    }

    /// <summary>
    /// 角色設定
    /// </summary>
    /// <param name="isPlayer1"></param>
    public void SetCharacter(bool isPlayer1)
    {
        // 設置初始位置(預設在最遠處, player1在右側)
        transform.position =
            isPlayer1 ?
            new(StaticDataManager.DataConfig.CharacterMoveRange.y, StaticDataManager.DataConfig.CharacterPosY, 0) :
            new(-StaticDataManager.DataConfig.CharacterMoveRange.y, StaticDataManager.DataConfig.CharacterPosY, 0);

        // 設置角色暱稱
        switch (StaticDataManager.PlayType)
        {
            // 連線配對
            case PLAY_TYPE.Match:
                MatchSuccessData matchData = StaticDataManager.MatchData;
                string localPlayerNickname = StaticDataManager.RegisterPlayerData.Nickname;
                string opponentPlayerNickname = matchData.opponentNickname;

                _text_Nickname.text = 
                    isPlayer1 && matchData.isCreator ? 
                    localPlayerNickname : 
                    opponentPlayerNickname;
                break;

            // AI對戰
            case PLAY_TYPE.WithAi:
                _text_Nickname.text =
                    isPlayer1 ?
                    $"{StaticDataManager.RegisterPlayerData.Nickname}" :
                    "AI";
                break;

            // 兩名玩家
            case PLAY_TYPE.TwoPlayer:
                _text_Nickname.text =
                    isPlayer1 ?
                    "Player1" :
                    "Player2";
                break;
        }

        // 設置角色顏色
        string colorHtml =
            isPlayer1 ?
            $"#{StaticDataManager.DataConfig.CharacterColor_P1}" :
            $"#{StaticDataManager.DataConfig.CharacterColor_P2}";

        if (ColorUtility.TryParseHtmlString(colorHtml, out Color customColor))
        {
            _spriteRenderer.color = customColor;
        }

        // 設置角色面向方向
        _spriteRenderer.flipX =
            isPlayer1 ?
            true :
            false;
    }

    /// <summary>
    /// 顯示投擲力道
    /// </summary>
    /// <param name="value">投擲力道(0~1)</param>
    public void ShowThrowStrength(float value)
    {
        if (!_throwStrengthCanvas.gameObject.activeSelf) _throwStrengthCanvas.gameObject.SetActive(true);

        _img_ThrowStrength.fillAmount = value;
    }

    /// <summary>
    /// 關閉投擲力道
    /// </summary>
    public void CloseThrowStrength()
    {
        _throwStrengthCanvas.gameObject.SetActive(false);
    }
}
