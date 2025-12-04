using UnityEngine;

public class LogoAnimator : MonoBehaviour
{
    [Header("Nefes Alma (Büyüyüp Küçülme)")]
    public bool enablePulse = true;
    public float pulseSpeed = 2.0f; // Ne kadar hızlı nefes alacak?
    public float pulseAmount = 0.05f; // Ne kadar büyüyecek? (0.05 = %5)

    [Header("Süzülme (Yukarı Aşağı)")]
    public bool enableFloat = true;
    public float floatSpeed = 1.0f; // Ne kadar hızlı süzülecek?
    public float floatAmount = 10.0f; // Kaç piksel yukarı aşağı gidecek?

    [Header("Sallanma (Sağa Sola Dönme)")]
    public bool enableRotate = false; // İstersen açabilirsin
    public float rotateSpeed = 1.5f;
    public float rotateAmount = 2.0f; // Derece cinsinden

    private Vector3 _startScale;
    private Vector3 _startPos;
    private RectTransform _rectTransform;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _startScale = transform.localScale;
        _startPos = _rectTransform.anchoredPosition;
    }

    void Update()
    {
        // 1. PULSE (Büyüme - Küçülme)
        if (enablePulse)
        {
            // Sinüs dalgası -1 ile 1 arası gider gelir. Biz bunu ölçeğe ekleriz.
            float scaleChange = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = _startScale + (Vector3.one * scaleChange);
        }

        // 2. FLOAT (Yukarı - Aşağı)
        if (enableFloat)
        {
            float yChange = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
            _rectTransform.anchoredPosition = new Vector2(_startPos.x, _startPos.y + yChange);
        }

        // 3. ROTATE (Sallanma)
        if (enableRotate)
        {
            float zChange = Mathf.Sin(Time.time * rotateSpeed) * rotateAmount;
            transform.rotation = Quaternion.Euler(0, 0, zChange);
        }
    }
}