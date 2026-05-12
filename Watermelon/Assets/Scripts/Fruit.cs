using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Fruit : MonoBehaviour
{
    public int Stage { get; private set; }
    public bool IsDropped { get; private set; }

    private bool isMerging;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void OnEnable()  => GameManager.Instance?.RegisterFruit(this);
    void OnDisable() => GameManager.Instance?.UnregisterFruit(this);

    public void Initialize(int stage, FruitData data)
    {
        Stage = stage;
        FruitStage s = data.stages[stage];

        // CircleCollider2D의 radius는 0.5 고정, localScale로 실제 크기를 조절
        float diameter = s.radius * 2f;
        transform.localScale = new Vector3(diameter, diameter, 1f);

        GetComponent<CircleCollider2D>().radius = 0.5f;

        var sr = GetComponent<SpriteRenderer>();
        if (s.sprite != null)
        {
            // 투명 여백을 제거한 트림 스프라이트로 콜라이더에 꽉 차게 표시
            sr.sprite = TrimSprite(s.sprite);
            sr.color  = Color.white;
        }
        else
        {
            // 스프라이트가 없으면 생성된 원형 스프라이트에 색상 적용
            sr.sprite = GetCircleSprite();
            sr.color  = s.color;
        }
    }

    // 투명 픽셀을 스캔해 실제 이미지 영역만 잘라낸 스프라이트를 반환
    static Sprite TrimSprite(Sprite src)
    {
        var tex = src.texture;
        if (!tex.isReadable) return src;

        int w = tex.width, h = tex.height;
        var pixels = tex.GetPixels();

        int minX = w, maxX = 0, minY = h, maxY = 0;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (pixels[y * w + x].a > 0.01f)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (minX > maxX) return src; // 완전 투명인 경우 원본 반환

        // 정사각형 유지를 위해 더 긴 쪽에 맞춰 확장
        int cw = maxX - minX + 1;
        int ch = maxY - minY + 1;
        int size = Mathf.Max(cw, ch);
        int cx = minX - (size - cw) / 2;
        int cy = minY - (size - ch) / 2;
        cx = Mathf.Clamp(cx, 0, w - size);
        cy = Mathf.Clamp(cy, 0, h - size);
        size = Mathf.Min(size, Mathf.Min(w - cx, h - cy));

        var rect = new Rect(cx, cy, size, size);
        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), src.pixelsPerUnit);
    }

    // 콜라이더 경계에 딱 맞는 픽셀 퍼펙트 원형 스프라이트를 생성하고 캐싱
    static Sprite _circleSprite;
    static Sprite GetCircleSprite()
    {
        if (_circleSprite != null) return _circleSprite;

        const int res = 128;
        var tex = new Texture2D(res, res, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = res / 2f;
        float r      = center;
        var pixels = new Color[res * res];
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dx   = x - center + 0.5f;
                float dy   = y - center + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                // 가장자리 1px 안티앨리어싱
                float alpha = Mathf.Clamp01(r - dist + 0.5f);
                pixels[y * res + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        _circleSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
        return _circleSprite;
    }

    public void Drop()
    {
        IsDropped = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsDropped || isMerging) return;

        Fruit other = col.gameObject.GetComponent<Fruit>();
        if (other == null || !other.IsDropped || other.isMerging || other.Stage != Stage) return;

        // 인스턴스 ID가 낮은 쪽만 머지를 시작해 이중 트리거 방지
        if (gameObject.GetInstanceID() > other.gameObject.GetInstanceID()) return;

        isMerging = true;
        other.isMerging = true;

        Vector2 mid = ((Vector2)transform.position + (Vector2)other.transform.position) * 0.5f;
        GameManager.Instance.OnMerge(Stage, mid);

        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
