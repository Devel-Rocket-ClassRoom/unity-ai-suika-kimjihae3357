using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Fruit : MonoBehaviour
{
    [SerializeField] private int stage; // 각 프리팹에서 직접 설정
    public int Stage => stage;

    public void SetStage(int s) { stage = s; }
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

        if (gameObject.GetInstanceID() > other.gameObject.GetInstanceID()) return;

        isMerging = true;
        other.isMerging = true;

        Vector2 mid = ((Vector2)transform.position + (Vector2)other.transform.position) * 0.5f;
        GameManager.Instance.OnMerge(Stage, mid);

        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
