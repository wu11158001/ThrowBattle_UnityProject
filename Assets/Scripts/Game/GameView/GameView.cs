using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;

/// <summary>
/// 遊戲介面
/// </summary>
public class GameView : BaseView
{
    [Header("遊戲介面")]
    [SerializeField] private TextMeshProUGUI _text_Player1_Nickname;
    [SerializeField] private TextMeshProUGUI _text_Player2_Nickname;

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        bool isCreator = StaticDataManager.MatchData.isCreator;
        string localNickname = StaticDataManager.RegisterPlayerData.Nickname;
        string opponentNickname = StaticDataManager.MatchData.opponentNickname;

        // 房主當Player1
        _text_Player1_Nickname.text = isCreator ? localNickname : opponentNickname;
        _text_Player2_Nickname.text = isCreator ? opponentNickname : localNickname;
    }
}
