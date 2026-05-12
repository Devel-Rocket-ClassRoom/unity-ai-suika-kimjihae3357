using UnityEngine;
using UnityEngine.InputSystem;

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
    // 드롭 가능한 최대 스테이지 인덱스 (GDD 기준 1~4단계 → 인덱스 0~3)
    public int maxDropStage = 3;

    private GameObject previewObj;
    private float cooldownTimer;
    private bool onCooldown;

    void Start()
    {
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

        // 마우스 X 좌표를 따라 미리보기 과일 이동
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
        int stage = Random.Range(0, maxDropStage + 1);
        previewObj = Instantiate(gameManager.fruitPrefab, new Vector3(0f, spawnY, 0f), Quaternion.identity);
        previewObj.GetComponent<Fruit>().Initialize(stage, gameManager.fruitData);
        // 플레이어가 드롭하기 전까지 Kinematic 유지
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
