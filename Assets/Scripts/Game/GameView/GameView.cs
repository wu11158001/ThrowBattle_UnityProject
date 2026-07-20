using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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

    [Header("生命條")]
    [SerializeField] private Image Img_P1_HpBar;
    [SerializeField] private Image Img_P2_HpBar;

    [Header("技能按鈕")]
    [SerializeField] private RectTransform _skillBtnParent_p1;
    [SerializeField] private RectTransform _skillBtnParent_p2;
    [SerializeField] private SkillBtn _skillBtnPrefab;

    [Header("退出")]
    [SerializeField] private Button _btn_Exit;

    // 技能按鈕
    private List<SkillBtn> _p1SkillButtons = new();
    private List<SkillBtn> _p2SkillButtons = new();

    // 開場動畫
    private Sequence _openingAnimationSeq;

    private GameplayContext _context;
    private DataConfig _dataConfig;

    public override void OnDestroy()
    {
        if (_openingAnimationSeq != null && _openingAnimationSeq.IsActive())
        {
            _openingAnimationSeq.Kill();
        }

        base.OnDestroy();
    }

    private void Awake()
    {
        _moveControlPanel.SetActive(false);
        _wind_LeftArror.SetActive(false);
        _wind_RightArror.SetActive(false);
        _img_Wind_Left.fillAmount = 0;
        _img_Wind_Right.fillAmount = 0;

        Img_P1_HpBar.fillAmount = 1;
        Img_P2_HpBar.fillAmount = 1;
    }

    public override void SetData(AssetReferenceGameObject myRef)
    {
        base.SetData(myRef);

        _context = GameplayManager.CurrentContext;
        _dataConfig = StaticDataManager.DataConfig;

        CreateSkillBtn();
        Bind();
        PlayOpeningAnimation();
        RefreshAllSkillButtons(isP1Turn: false, isP2Turn: false);
    }

    private void Bind()
    {
        // 控制:蓄力 & 投擲
        _throwHandler.DownAction = (eventData) => { _context.GameController.SetChargingState(true); };
        _throwHandler.UpAction = (eventData) => { _context.GameController.SetChargingState(false); };

        // 移動控制:左
        _leftHandler.DownAction = (eventData) => { _context.GameController.SetInputDirection(-1); };
        _leftHandler.UpAction = (eventData) => { _context.GameController.SetInputDirection(0); };

        // 移動控制:右
        _rightHandler.DownAction = (eventData) => { _context.GameController.SetInputDirection(1); };
        _rightHandler.UpAction = (eventData) => { _context.GameController.SetInputDirection(0); };

        // 退出按鈕
        _btn_Exit.OnClickAsObservable()
            .Subscribe(_ =>
            {
                ViewManager.Instance.OpenView<AskView>(
                    viewType: VIEW_TYPE.AskView,
                    canvasType: CANVAS_TYPE.Canvas_Highest,
                    callback: (view) =>
                    {
                        view.SetContent(
                            content: "是否退出遊戲?",
                            confirmAction: () =>
                            {
                                HttpManager.Instance.SendLeaveBattle();
                                SceneLoader.Instance.LoadSceneAsync(SCENE_TYPE.LobbyScene).Forget();
                            });
                    }).Forget();
            })
            .AddTo(this);
    }

    /// <summary>
    /// 產生技能按鈕
    /// </summary>
    private void CreateSkillBtn()
    {
        _skillBtnPrefab.gameObject.SetActive(false);

        switch (StaticDataManager.PlayType)
        {
            case PLAY_TYPE.TwoPlayer:
                // 單機雙人：兩邊都要生成
                GenerateButtonsForPlayer(true);  // P1
                GenerateButtonsForPlayer(false); // P2
                break;

            case PLAY_TYPE.Match:
                // 連線配對：看自己是 Creator (P1) 還是 Joiner (P2)，只生成自己的
                bool isP1 = StaticDataManager.MatchData.isCreator;
                GenerateButtonsForPlayer(isP1);
                break;

            case PLAY_TYPE.WithAi:
                // AI對戰：本地玩家永远是 P1，只生成 P1 的（AI 不需要 UI 按鈕）
                GenerateButtonsForPlayer(true);
                break;
        }
    }

    /// <summary>
    /// 生成指定角色的全套技能
    /// </summary>
    /// <param name="isPlayer1"></param>
    private void GenerateButtonsForPlayer(bool isPlayer1)
    {
        RectTransform parent = isPlayer1 ? _skillBtnParent_p1 : _skillBtnParent_p2;
        List<SkillBtn> targetList = isPlayer1 ? _p1SkillButtons : _p2SkillButtons;

        if (parent == null) return;

        // 生成巨大化技能
        SkillBtn giantBtn = Instantiate(_skillBtnPrefab, parent);
        giantBtn.gameObject.SetActive(true);
        giantBtn.SetData(THROW_TYPE.Giant, _dataConfig.SkillGiantCD, () => OnSkillClick(THROW_TYPE.Giant), _dataConfig.SkillGiantIcon);
        targetList.Add(giantBtn);

        // 生成強化傷害技能
        SkillBtn strengthBtn = Instantiate(_skillBtnPrefab, parent);
        strengthBtn.gameObject.SetActive(true);
        strengthBtn.SetData(THROW_TYPE.StrengthDamage, _dataConfig.StrengthDamageCD, () => OnSkillClick(THROW_TYPE.StrengthDamage), _dataConfig.SkillStrengthDamageIcon);
        targetList.Add(strengthBtn);
    }

    /// <summary>
    /// 刷新所有技能按鈕的互動狀態
    /// </summary>
    /// <param name="isP1Turn">目前是不是 P1 的回合</param>
    /// <param name="isP2Turn">目前是不是 P2 的回合</param>
    public void RefreshAllSkillButtons(bool isP1Turn, bool isP2Turn)
    {
        // 刷新 P1 的所有按鈕：只有當「是 P1 回合」且「按鈕本身沒 CD」時才可以點
        foreach (var btn in _p1SkillButtons)
        {
            btn.SetInteractable(isP1Turn);
        }

        // 刷新 P2 的所有按鈕：只有當「是 P2 回合」且「按鈕本身沒 CD」時才可以點
        foreach (var btn in _p2SkillButtons)
        {
            btn.SetInteractable(isP2Turn);
        }
    }

    /// <summary>
    /// 技能點擊
    /// </summary>
    private void OnSkillClick(THROW_TYPE type)
    {
        _context.GameController.SetThrowType(type);

        // 一旦點擊了任何一個技能，這一回合「該玩家的所有技能」都關閉互動
        bool isP1 = (_context.CurrentTurnCharacter == _context.P1_CharacterView);

        if (isP1)
            RefreshAllSkillButtons(isP1Turn: false, isP2Turn: false); 
        else
            RefreshAllSkillButtons(isP1Turn: false, isP2Turn: false);
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
        _openingAnimationSeq?.Kill();
        _openingAnimationSeq = DOTween.Sequence();
        _openingAnimationSeq.Append(DOTween.To(
            () => _text_Battle.fontSize,
            x => _text_Battle.fontSize = x,
            150f,
            0.5f
        ).SetEase(Ease.OutQuad));

        // 停留
        _openingAnimationSeq.AppendInterval(2f);

        // 結束
        _openingAnimationSeq.OnComplete(() =>
        {
            _text_Battle.gameObject.SetActive(false);

            // 遊戲開始
            _context.GameController.StartGameplay();
        });
    }

    /// <summary>
    /// 設置可控制面板激活狀態
    /// </summary>
    /// <param name="isActive"></param>
    public void SetControlPanelActive(bool isActive)
    {
        if (_moveControlPanel != null)
        {
            _moveControlPanel.SetActive(isActive);
        }

        if(!isActive)
        {
            RefreshAllSkillButtons(isP1Turn: false, isP2Turn: false);
        }        
    }

    /// <summary>
    /// 設置是否是本地回合
    /// </summary>
    /// <param name="isLocalTurn"></param>
    public void SetIsLocalTurn(bool isLocalTurn)
    {
        // 預設使用「一般投擲」類型
        _context.GameController.SetThrowType(THROW_TYPE.Normal);

        SetControlPanelActive(isLocalTurn);

        if (_context.CurrentTurnCharacter == null) return;

        // 判斷當前回合是P1或P2
        bool isCurrentP1 = (_context.CurrentTurnCharacter == _context.P1_CharacterView);

        if (isLocalTurn)
        {
            // 輪到本地玩家時，技能扣除 CD 回合
            List<SkillBtn> activeButtons = isCurrentP1 ? _p1SkillButtons : _p2SkillButtons;
            foreach (var btn in activeButtons)
            {
                btn.ReduceCD();
            }

            // 刷新按鈕互動狀態
            RefreshAllSkillButtons(isP1Turn: isCurrentP1, isP2Turn: !isCurrentP1);
        }
        else
        {
            // 按鈕互動狀態全數關閉
            RefreshAllSkillButtons(isP1Turn: false, isP2Turn: false);
        }
    }

    /// <summary>
    /// 設置風力強度
    /// </summary>
    /// <param name="value"></param>
    public void SetWindStrength(float value)
    {
        var windMaxStrength = _dataConfig.WindMaxStrength;

        _wind_LeftArror.SetActive(value < 0);
        _wind_RightArror.SetActive(value > 0);

        float leftProgress = Mathf.InverseLerp(0, -windMaxStrength, value);
        _img_Wind_Left.fillAmount = leftProgress;

        float rightProgress = Mathf.InverseLerp(0, windMaxStrength, value);
        _img_Wind_Right.fillAmount = rightProgress;
    }

    /// <summary>
    /// 更新特定角色的血條 UI
    /// </summary>
    /// <param name="isPlayer1"></param>
    /// <param name="currentHp"></param>
    public void UpdateHpBar(bool isPlayer1, int currentHp)
    {
        float fillValue = (float)currentHp / _dataConfig.CharacterMaxHp;

        if (isPlayer1)
        {
            if (Img_P1_HpBar != null) Img_P1_HpBar.fillAmount = fillValue;
        }
        else
        {
            if (Img_P2_HpBar != null) Img_P2_HpBar.fillAmount = fillValue;
        }
    }
}
