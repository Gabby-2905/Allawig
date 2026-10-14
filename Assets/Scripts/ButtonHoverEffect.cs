using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Hover effect: zoom + white border + transparent fill tint.
/// - fillColor   : the tint shown on hover (set per-button in Inspector)
/// - borderColor : color of the 4-edge border (default white)
/// Text children are never affected because we only touch the button's own Image alpha.
/// </summary>
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Zoom")]
    public float hoverScale  = 1.08f;
    public float scaleSpeed  = 10f;

    [Header("Fill Tint on Hover")]
    [Tooltip("Color shown as a transparent overlay when hovered. Alpha controls max opacity.")]
    public Color fillColor = new Color(0.2f, 0.5f, 1f, 0.25f);   // blueish, 25% opacity

    [Header("Border")]
    public Color borderColor     = Color.white;
    public float borderThickness = 3f;

    // ── internals ──────────────────────────────────────────────────────────
    private Vector3 _originalScale;
    private Vector3 _targetScale;
    private bool    _isHovered;

    private Image _fillImage;          // the button's own Image component
    private Color _fillColorHidden;    // same hue, alpha = 0
    private Color _fillColorVisible;   // same hue, target alpha

    // Four border strips
    private Image _borderTop, _borderBottom, _borderLeft, _borderRight;

    // ── lifecycle ──────────────────────────────────────────────────────────
    private void Awake()
    {
        _originalScale = transform.localScale;
        _targetScale   = _originalScale;

        _fillImage        = GetComponent<Image>();
        _fillColorHidden  = new Color(fillColor.r, fillColor.g, fillColor.b, 0f);
        _fillColorVisible = fillColor;

        // Start fully transparent
        if (_fillImage != null)
            _fillImage.color = _fillColorHidden;

        CreateBorder();
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime * scaleSpeed;

        // Smooth scale
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, dt);

        // Smooth fill tint
        if (_fillImage != null)
        {
            Color target = _isHovered ? _fillColorVisible : _fillColorHidden;
            _fillImage.color = Color.Lerp(_fillImage.color, target, dt);
        }

        // Smooth border alpha
        float curAlpha    = _borderTop != null ? _borderTop.color.a : 0f;
        float targetAlpha = _isHovered ? 1f : 0f;
        SetBorderAlpha(Mathf.Lerp(curAlpha, targetAlpha, dt));
    }

    // ── pointer events ─────────────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData e)
    {
        _isHovered   = true;
        _targetScale = _originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData e)
    {
        _isHovered   = false;
        _targetScale = _originalScale;
    }

    public void OnPointerDown(PointerEventData e)
    {
        _targetScale = _originalScale * (hoverScale * 0.95f);
    }

    public void OnPointerUp(PointerEventData e)
    {
        _targetScale = _isHovered ? _originalScale * hoverScale : _originalScale;
    }

    private void OnDisable()
    {
        transform.localScale = _originalScale;
        _targetScale         = _originalScale;
        _isHovered           = false;
        if (_fillImage != null) _fillImage.color = _fillColorHidden;
        SetBorderAlpha(0f);
    }

    // ── border creation ────────────────────────────────────────────────────
    private void CreateBorder()
    {
        Transform existing = transform.Find("__HoverBorder__");
        if (existing != null) Destroy(existing.gameObject);

        GameObject container = new GameObject("__HoverBorder__");
        container.transform.SetParent(transform, false);

        RectTransform ct = container.AddComponent<RectTransform>();
        ct.anchorMin = Vector2.zero;
        ct.anchorMax = Vector2.one;
        ct.offsetMin = Vector2.zero;
        ct.offsetMax = Vector2.zero;
        container.transform.SetAsLastSibling();

        _borderTop    = MakeEdge(container.transform, "Top");
        _borderBottom = MakeEdge(container.transform, "Bottom");
        _borderLeft   = MakeEdge(container.transform, "Left");
        _borderRight  = MakeEdge(container.transform, "Right");

        LayoutEdges();
        SetBorderAlpha(0f);
    }

    private Image MakeEdge(Transform parent, string edgeName)
    {
        GameObject go = new GameObject(edgeName);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        Image img = go.AddComponent<Image>();
        img.color         = new Color(borderColor.r, borderColor.g, borderColor.b, 0f);
        img.raycastTarget = false;
        return img;
    }

    private void LayoutEdges()
    {
        float t = borderThickness;
        SetStretch(_borderTop.rectTransform,
            new Vector2(0,1), new Vector2(1,1), new Vector2(0.5f,1f),
            new Vector2(0,-t), Vector2.zero);
        SetStretch(_borderBottom.rectTransform,
            new Vector2(0,0), new Vector2(1,0), new Vector2(0.5f,0f),
            Vector2.zero, new Vector2(0,t));
        SetStretch(_borderLeft.rectTransform,
            new Vector2(0,0), new Vector2(0,1), new Vector2(0f,0.5f),
            Vector2.zero, new Vector2(t,0));
        SetStretch(_borderRight.rectTransform,
            new Vector2(1,0), new Vector2(1,1), new Vector2(1f,0.5f),
            new Vector2(-t,0), Vector2.zero);
    }

    private static void SetStretch(RectTransform rt,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.pivot     = pivot;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
    }

    private void SetBorderAlpha(float alpha)
    {
        SetEdgeAlpha(_borderTop,    alpha);
        SetEdgeAlpha(_borderBottom, alpha);
        SetEdgeAlpha(_borderLeft,   alpha);
        SetEdgeAlpha(_borderRight,  alpha);
    }

    private void SetEdgeAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color; c.a = alpha; img.color = c;
    }
}
