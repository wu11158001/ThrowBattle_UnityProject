using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;

/// <summary>
/// 遊戲介面
/// </summary>
public class GameView : BaseView
{
    [Header("遊戲介面")]
    [SerializeField] private TextMeshProUGUI _text_Battle;

    [Header("移動控制")]
    [SerializeField] private GameObject _moveControlPanel;
    [SerializeField] private UIEventHandler _leftHandler;
    [SerializeField] private UIEventHandler _rightHandler;

    [Header("投擲控制")]
    [SerializeField] private UIEventHandler _throwHandler;

    [Header("風力")]
    [SerializeField] private GameObject _wind_LeftArror;
    [SerializeField] private GameObject _wind_RightArror;
    [SerializeField] private Image _img_Wind_Left;
    [SerializeField] private Image _img_Wind_Right;

    private GameplayContext _context;

    // 目前按壓狀態
    private bool _isPressingLeft = false;
    private bool _isPressingRight = false;
    private bool _isPressingThrow = false;

    private void Initialize()
    {
        _moveControlPanel.SetActive(false);
        _wind_LeftArror.SetActive(false);
        _wind_RightArror.SetActive(false);
        _img_Wind_Left.fillAmount = 0;
        _img_Wind_Right.fillAmount = 0;
    }

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        _context = GameplayManager.CurrentContext;

        Initialize();
        Bind();
        PlayOpeningAnimation();
    }

    private void Bind()
    {
        // 投擲控制
        _throwHandler.DownAction = (eventData) => { _isPressingThrow = true; };
        _throwHandler.UpAction = (eventData) => { _isPressingThrow = false; };
        _throwHandler.ExitAction = (eventData) => { _isPressingThrow = false; };

        // 移動控制:左
        _leftHandler.DownAction = (eventData) => { _isPressingLeft = true; };
        _leftHandler.UpAction = (eventData) => { _isPressingLeft = false; };
        _leftHandler.ExitAction = (eventData) => { _isPressingLeft = false; };

        // 移動控制:右
        _rightHandler.DownAction = (eventData) => { _isPressingRight = true; };
        _rightHandler.UpAction = (eventData) => { _isPressingRight = false; };
        _rightHandler.ExitAction = (eventData) => { _isPressingRight = false; };

        // 每幀驅動
        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                // 移動控制
                float inputDir = 0f;
                if (_isPressingLeft) inputDir = -1f;
                else if (_isPressingRight) inputDir = 1f;
                _context.GameController.SetInputDirection(inputDir);

                // 投擲控制
                _context.GameController.SetThrowPressState(_isPressingThrow);
            })
            .AddTo(this);
    }

    /// <summary>
    /// 撥放開場動畫
    /// </summary>
    private void PlayOpeningAnimation()
    {
        if (_text_Battle == null) return;

        _text_Battle.fontSize = 0;
        _text_Battle.gameObject.SetActive(true);
        
        // 文字放大
        Sequence textSeq = DOTween.Sequence();
        textSeq.Append(DOTween.To(
            () => _text_Battle.fontSize,
            x => _text_Battle.fontSize = x,
            150f,
            0.5f
        ).SetEase(Ease.OutQuad));

        // 停留
        textSeq.AppendInterval(2f);

        // 結束
        textSeq.OnComplete(() =>
        {
            _text_Battle.gameObject.SetActive(false);

            // 遊戲開始
            _context.GameController.StartGameplay();
        });
    }

    /// <summary>
    /// 設置移動控制面板激活狀態
    /// </summary>
    /// <param name="isActive"></param>
    public void SetMoveControlActive(bool isActive)
    {
        if (_moveControlPanel != null)
        {
            _moveControlPanel.SetActive(isActive);
        }
    }

    /// <summary>
    /// 設置風力強度
    /// </summary>
    /// <param name="value"></param>
    public void SetWindStrength(float value)
    {
        var windMaxStrength = StaticDataManager.DataConfig.WindMaxStrength;

        _wind_LeftArror.SetActive(value < 0);
        _wind_RightArror.SetActive(value > 0);

        float leftProgress = Mathf.InverseLerp(0, -windMaxStrength, value);
        _img_Wind_Left.fillAmount = leftProgress;

        float rightProgress = Mathf.InverseLerp(0, windMaxStrength, value);
        _img_Wind_Right.fillAmount = rightProgress;
    }
}
