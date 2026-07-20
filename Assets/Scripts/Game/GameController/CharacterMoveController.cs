using UnityEngine;

/// <summary>
/// 處理角色移動控制
/// </summary>
public class CharacterMoveController
{
    private GameplayContext _context;

    // 當前移動輸入
    private float _currentInputDir = 0f;
    // 是否已停止移動
    private bool _hasStopped = false;

    // Server 同步的計時時間
    private float _syncTimer = 0f;
    // 多少秒同步一次
    private const float SYNC_INTERVAL = 0.05f;

    public CharacterMoveController()
    {
        _context = GameplayManager.CurrentContext;
    }

    /// <summary>
    /// 移動輸入方向接收
    /// </summary>
    /// <param name="direction"></param>
    public void SetInputDirection(float direction)
    {
        _currentInputDir = direction;
    }

    /// <summary>
    /// 重設狀態
    /// </summary>
    public void ResetState()
    {
        _currentInputDir = 0f;
        _hasStopped = false;
    }

    /// <summary>
    /// 每幀驅動
    /// </summary>
    public void Tick()
    {
        if (_context.CurrentTurnCharacter == null) return;

        // 連線模式:對手的回合跳過
        if (StaticDataManager.PlayType == PLAY_TYPE.Match)
        {
            bool isMyTurn = _context.CurrentTurnCharacter.IsLocalPlayer;
            if (!isMyTurn) return;
        }

        // 本地角色移動
        if (_currentInputDir != 0f)
        {
            _context.CurrentTurnCharacter.Move(_currentInputDir);
            _hasStopped = false;

            // 連線模式:發送位置
            if (StaticDataManager.PlayType == PLAY_TYPE.Match)
            {
                _syncTimer += Time.deltaTime;
                if (_syncTimer >= SYNC_INTERVAL)
                {
                    _syncTimer = 0f;

                    MoveData data = new MoveData()
                    {
                        roomId = StaticDataManager.MatchData.roomId,
                        posX = _context.CurrentTurnCharacter.transform.position.x,
                        inputDir = _currentInputDir
                    };

                    SocketManager.Instance.SendSyncMove(data);
                }
            }
        }
        else
        {
            // 速度0時只呼叫1次
            if (!_hasStopped)
            {
                _context.CurrentTurnCharacter.Move(0f);
                _hasStopped = true;

                // 連線模式:發送位置
                if (StaticDataManager.PlayType == PLAY_TYPE.Match)
                {
                    MoveData data = new MoveData()
                    {
                        roomId = StaticDataManager.MatchData.roomId,
                        posX = _context.CurrentTurnCharacter.transform.position.x,
                        inputDir = 0
                    };

                    SocketManager.Instance.SendSyncMove(data);
                }
            }
        }
    }

    /// <summary>
    /// 強制角色立刻停止移動
    /// </summary>
    public void ForceStop()
    {
        if (_context.CurrentTurnCharacter != null)
        {
            _context.CurrentTurnCharacter.Move(0f);
        }
        _hasStopped = true;

        // 連線模式:發送位置
        bool isMyTurn = _context.CurrentTurnCharacter.IsLocalPlayer;
        if (StaticDataManager.PlayType == PLAY_TYPE.Match && _context.CurrentTurnCharacter != null && isMyTurn)
        {
            MoveData data = new MoveData()
            {
                roomId = StaticDataManager.MatchData.roomId,
                posX = _context.CurrentTurnCharacter.transform.position.x,
                inputDir = 0
            };

            SocketManager.Instance.SendSyncMove(data);
        }
    }
}