using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Data")]
    public FruitData fruitData;
    public GameObject mergeParticlePrefab;

    [Header("References")]
    public FruitSpawner spawner;

    [Header("Deadline")]
    public float deadlineY = 4f;
    public float gameOverGracePeriod = 3f;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public GameObject warningIndicator;

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
            if (warningIndicator) warningIndicator.SetActive(overDeadlineTimer >= 0.5f);

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
            AddScore(points + 100);
            SpawnMergeParticle(position, stage);
            return;
        }

        AddScore(points);
        SpawnMergeParticle(position, stage);
        SpawnMergedFruit(nextStage, position);
    }

    void SpawnMergeParticle(Vector2 pos, int stage)
    {
        if (mergeParticlePrefab == null) return;
        var go = Instantiate(mergeParticlePrefab, pos, Quaternion.identity);

        if (stage < fruitData.stages.Length)
        {
            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Color base_ = fruitData.stages[stage].particleColor;
                Color.RGBToHSV(base_, out float h, out float s, out float v);
                Color bright = Color.HSVToRGB(h, Mathf.Max(0f, s - 0.15f), Mathf.Min(1f, v + 0.2f));
                Color dark   = Color.HSVToRGB(h, Mathf.Min(1f, s + 0.1f),  Mathf.Max(0f, v - 0.1f));
                var main = ps.main;
                main.startColor = new ParticleSystem.MinMaxGradient(bright, dark);
            }
        }
    }

    void SpawnMergedFruit(int stage, Vector2 pos)
    {
        var prefab = fruitData.stages[stage].prefab;
        if (prefab == null) return;
        var go = Instantiate(prefab, pos, Quaternion.identity);
        go.GetComponent<Fruit>().Drop();
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
