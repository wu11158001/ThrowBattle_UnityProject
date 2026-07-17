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

        if (_currentInputDir != 0f)
        {
            _context.CurrentTurnCharacter.Move(_currentInputDir);
            _hasStopped = false;
        }
        else
        {
            // 速度為 0 時只呼叫一次
            if (!_hasStopped)
            {
                _context.CurrentTurnCharacter.Move(0f);
                _hasStopped = true;
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
    }
}