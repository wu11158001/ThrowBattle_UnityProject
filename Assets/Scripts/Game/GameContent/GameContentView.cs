using UnityEngine;
using TMPro;

/// <summary>
/// 遊戲內容物件
/// </summary>
public class GameContentView : MonoBehaviour
{
    [Header("玩家1")]
    [SerializeField] private Transform _p1_Character;
    [SerializeField] private Animator _p1_Anim;
    [SerializeField] private TextMeshPro _text_P1_Nickname;

    [Header("玩家2")]
    [SerializeField] private Transform _p2_Character;
    [SerializeField] private Animator _p2_Anim;
    [SerializeField] private TextMeshPro _text_P2_Nickname;

    [Header("投擲物件")]
    [SerializeField] private GameObject ThrowObject;
    [SerializeField] private GameObject ThrowObject_Body;
    [SerializeField] private GameObject ThrowObject_FX;

    private void Start()
    {
        ThrowObject.SetActive(false);

        // 設置角色暱稱
        switch (StaticDataManager.PlayType)
        {
            // 連線配對
            case PLAY_TYPE.Match:
                MatchSuccessData matchData = StaticDataManager.MatchData;
                string localPlayerNickname = StaticDataManager.RegisterPlayerData.Nickname;
                string opponentPlayerNickname = matchData.opponentNickname;
                _text_P1_Nickname.text = matchData.isCreator ? localPlayerNickname : opponentPlayerNickname;
                _text_P2_Nickname.text = matchData.isCreator ? opponentPlayerNickname : localPlayerNickname;
                break;

            // AI對戰
            case PLAY_TYPE.WithAi:
                _text_P1_Nickname.text = $"{StaticDataManager.RegisterPlayerData.Nickname}";
                _text_P2_Nickname.text = "AI";
                break;

            // 兩名玩家
            case PLAY_TYPE.TwoPlayer:
                _text_P1_Nickname.text = "Player1";
                _text_P2_Nickname.text = "Player2";
                break;
        }
    }
}
