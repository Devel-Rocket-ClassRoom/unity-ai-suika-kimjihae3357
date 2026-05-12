using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Data")]
    public FruitData fruitData;
    public GameObject fruitPrefab;

    [Header("References")]
    public FruitSpawner spawner;

    [Header("Deadline")]
    public float deadlineY = 4f;           // 데드라인의 월드 Y 좌표
    public float gameOverGracePeriod = 3f; // 데드라인 위에 머물 수 있는 유예 시간(초)

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public GameObject warningIndicator; // 경고 표시용 오브젝트(빨간 선 등)

    public bool IsGameOver { get; private set; }

    private int score;
    private float overDeadlineTimer;
    private readonly List<Fruit> activeFruits = new List<Fruit>();

    public void RegisterFruit(Fruit f)   => activeFruits.Add(f);
    public void UnregisterFruit(Fruit f) => activeFruits.Remove(f);

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (warningIndicator) warningIndicator.SetActive(false);
        RefreshBestScoreUI();
        UpdateScoreUI();

        // 에디터에서 AddListener로 추가한 이벤트는 직렬화되지 않으므로 런타임에 직접 연결
        if (gameOverPanel != null)
        {
            var btn = gameOverPanel.GetComponentInChildren<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(RestartGame);
            }
        }
    }

    void Update()
    {
        if (IsGameOver) return;
        CheckDeadline();
    }

    // ── 데드라인 / 게임 오버 ──────────────────────────────────────────────────

    void CheckDeadline()
    {
        bool anyAbove = false;
        foreach (Fruit f in activeFruits)
        {
            if (f.IsDropped && f.transform.position.y > deadlineY)
            {
                anyAbove = true;
                break;
            }
        }

        if (anyAbove)
        {
            overDeadlineTimer += Time.deltaTime;
            if (warningIndicator) warningIndicator.SetActive(true);

            if (overDeadlineTimer >= gameOverGracePeriod)
                TriggerGameOver();
        }
        else
        {
            overDeadlineTimer = 0f;
            if (warningIndicator) warningIndicator.SetActive(false);
        }
    }

    void TriggerGameOver()
    {
        IsGameOver = true;
        spawner.enabled = false;
        if (warningIndicator) warningIndicator.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (finalScoreText) finalScoreText.text = "Score: " + score;
    }

    // ── 머지 ─────────────────────────────────────────────────────────────────

    public void OnMerge(int stage, Vector2 position)
    {
        int nextStage = stage + 1;
        int points = (stage + 1) * 10;

        if (nextStage >= fruitData.stages.Length)
        {
            // 수박끼리 합체 시 두 과일 모두 사라지고 보너스 점수 획득
            AddScore(points + 100);
            return;
        }

        AddScore(points);
        SpawnMergedFruit(nextStage, position);
    }

    void SpawnMergedFruit(int stage, Vector2 pos)
    {
        GameObject go = Instantiate(fruitPrefab, pos, Quaternion.identity);
        Fruit f = go.GetComponent<Fruit>();
        f.Initialize(stage, fruitData);
        f.Drop(); // 머지로 생성된 과일은 즉시 낙하
    }

    // ── 점수 ─────────────────────────────────────────────────────────────────

    void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();

        int best = PlayerPrefs.GetInt("BestScore", 0);
        if (score > best)
        {
            PlayerPrefs.SetInt("BestScore", score);
            RefreshBestScoreUI();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = "Score: " + score;
    }

    void RefreshBestScoreUI()
    {
        if (bestScoreText) bestScoreText.text = "Best: " + PlayerPrefs.GetInt("BestScore", 0);
    }

    // ── 재시작 ───────────────────────────────────────────────────────────────

    public void RestartGame()
    {
        var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(active.name);
    }
}
