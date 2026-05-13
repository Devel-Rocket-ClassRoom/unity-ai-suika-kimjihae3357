using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FruitSpawner : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Camera mainCamera;

    [Header("Settings")]
    public float spawnY = 5f;
    public float minX = -2.4f;
    public float maxX = 2.4f;
    public float dropCooldown = 0.8f;
    public int maxDropStage = 3;

    [Header("Next Fruit UI")]
    public Image nextFruitImage;
    public TMPro.TextMeshProUGUI nextFruitLabel;

    private GameObject previewObj;
    private float cooldownTimer;
    private bool onCooldown;
    private int nextStage;

    void Start()
    {
        nextStage = Random.Range(0, maxDropStage + 1);
        PrepareNext();
    }

    void Update()
    {
        if (gameManager.IsGameOver) return;

        if (onCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= dropCooldown)
            {
                onCooldown = false;
                cooldownTimer = 0f;
                PrepareNext();
            }
            return;
        }

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 screenPos = mouse.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        float x = Mathf.Clamp(worldPos.x, minX, maxX);
        if (previewObj != null)
            previewObj.transform.position = new Vector3(x, spawnY, 0f);

        if (mouse.leftButton.wasPressedThisFrame && previewObj != null)
            Drop(x);
    }

    void PrepareNext()
    {
        int stage = nextStage;
        nextStage = Random.Range(0, maxDropStage + 1);

        var prefab = gameManager.fruitData.stages[stage].prefab;
        if (prefab == null) return;

        previewObj = Instantiate(prefab, new Vector3(0f, spawnY, 0f), Quaternion.identity);
        RefreshNextFruitUI();
    }

    void RefreshNextFruitUI()
    {
        if (nextFruitImage == null) return;
        var stageData = gameManager.fruitData.stages[nextStage];
        if (stageData.prefab == null) return;

        var sr = stageData.prefab.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) nextFruitImage.sprite = sr.sprite;

        // 콜라이더 radius 기준으로 UI 크기 비례 조정
        var col = stageData.prefab.GetComponent<CircleCollider2D>();
        if (col != null)
        {
            // 드롭 가능한 최대 스테이지의 radius를 기준으로 최대 표시 크기 결정
            var maxStagePrefab = gameManager.fruitData.stages[maxDropStage].prefab;
            var maxCol = maxStagePrefab != null ? maxStagePrefab.GetComponent<CircleCollider2D>() : null;
            float maxRadius = maxCol != null ? maxCol.radius : col.radius;
            const float maxDisplaySize = 160f;
            float size = (col.radius / maxRadius) * maxDisplaySize;
            nextFruitImage.rectTransform.sizeDelta = new Vector2(size, size);
        }
    }

    void Drop(float x)
    {
        previewObj.transform.position = new Vector3(x, spawnY, 0f);
        previewObj.GetComponent<Fruit>().Drop();
        previewObj = null;
        onCooldown = true;
        cooldownTimer = 0f;
    }
}
