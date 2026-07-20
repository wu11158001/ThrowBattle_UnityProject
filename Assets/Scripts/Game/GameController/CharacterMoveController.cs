using UnityEngine;

/// <summary>
/// 處理角色移動控制
/// </summary>
public class CharacterMoveController
{
    private GameplayContext _context;

    public CharacterMoveController()
    {
        _context = GameplayManager.CurrentContext;
    }

    /// <summary>
    /// 設置移動輸入方向
    /// </summary>
    /// <param name="direction"></param>
    public void SetInputDirection(float direction)
    {
        float posX = _context.CurrentTurnCharacter.transform.position.x;

        if (StaticDataManager.PlayType == PLAY_TYPE.Match)
        {
            MoveData data = new()
            {
                roomId = StaticDataManager.MatchData.roomId,
                inputDir = direction,
                posX = posX
            };

            SocketManager.Instance.SendSyncMove(data);
        }
        else
        {
            _context.CurrentTurnCharacter.SetMove(direction, posX);
        }
    }
}