using UnityEngine;
using UnityEngine.UI;

public class Spinner : MonoBehaviour
{
    // 使用 System.Flags 讓 Inspector 出現多選下拉選單
    [System.Flags]
    public enum Axis
    {
        X = 1 << 0, // 1
        Y = 1 << 1, // 2
        Z = 1 << 2  // 4
    }

    [Header("Rotation")]
    public bool Rotation = true;
    [Tooltip("選擇要旋轉的軸向，可複選")]
    public Axis RotationAxes = Axis.Z; // 預設為 Z 軸
    [Range(-10, 10), Tooltip("Value in Hz (revolutions per second).")]
    public float RotationSpeed = 1;
    public AnimationCurve RotationAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Rainbow")]
    public bool Rainbow = true;
    [Range(-10, 10), Tooltip("Value in Hz (revolutions per second).")]
    public float RainbowSpeed = 0.5f;
    [Range(0, 1)]
    public float RainbowSaturation = 1f;
    public AnimationCurve RainbowAnimationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Options")]
    public bool RandomPeriod = true;

    private Image _image;
    private SpriteRenderer _spriteRenderer;
    private float _period;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_image == null && _spriteRenderer == null)
        {
            Debug.LogWarning($"[Spinner] {gameObject.name} 上找不到 Image 或 SpriteRenderer 組件！", this);
        }
    }

    public void Start()
    {
        _period = RandomPeriod ? Random.Range(0f, 1f) : 0;
    }

    public void Update()
    {
        if (Rotation)
        {
            float progress = RotationAnimationCurve.Evaluate((RotationSpeed * Time.time + _period) % 1);
            float angle = -360f * progress;

            // 根據選取的軸向計算旋轉角度
            float x = RotationAxes.HasFlag(Axis.X) ? angle : 0f;
            float y = RotationAxes.HasFlag(Axis.Y) ? angle : 0f;
            float z = RotationAxes.HasFlag(Axis.Z) ? angle : 0f;

            transform.localEulerAngles = new Vector3(x, y, z);
        }

        if (Rainbow)
        {
            Color rainbowColor = Color.HSVToRGB(RainbowAnimationCurve.Evaluate((RainbowSpeed * Time.time + _period) % 1), RainbowSaturation, 1);

            if (_image != null)
            {
                _image.color = rainbowColor;
            }
            else if (_spriteRenderer != null)
            {
                _spriteRenderer.color = rainbowColor;
            }
        }
    }
}