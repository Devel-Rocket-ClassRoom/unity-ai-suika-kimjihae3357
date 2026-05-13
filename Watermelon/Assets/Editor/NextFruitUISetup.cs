using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class NextFruitUISetup
{
    [MenuItem("Tools/Suika/Create Next Fruit UI")]
    public static void CreateNextFruitUI()
    {
        var canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null) { Debug.LogError("UICanvas not found"); return; }

        Color cPanel = new Color(0.27f, 0.19f, 0.12f, 0.93f);
        Color cGold  = new Color(1f, 0.82f, 0.20f, 1f);

        // 이미 존재하면 재생성하지 않고 레퍼런스만 연결
        var existing = canvasGO.transform.Find("NextPanel");
        if (existing != null)
        {
            ConnectSpawnerRefs(existing.gameObject);
            Debug.Log("NextPanel이 이미 존재합니다. 레퍼런스만 재연결했습니다.");
            EditorSceneManager.SaveOpenScenes();
            return;
        }

        // ── Next 패널 신규 생성 ───────────────────────────────────
        var panelGO = new GameObject("NextPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        panelGO.AddComponent<Image>().color = cPanel;
        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot     = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-30f, -170f);
        panelRect.sizeDelta = new Vector2(260f, 180f);

        var labelGO = new GameObject("NextLabel");
        labelGO.transform.SetParent(panelGO.transform, false);
        var label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = "NEXT"; label.fontSize = 28; label.fontStyle = FontStyles.Bold;
        label.color = cGold; label.alignment = TextAlignmentOptions.Center;
        var labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.72f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

        var imgGO = new GameObject("NextFruitImage");
        imgGO.transform.SetParent(panelGO.transform, false);
        var img = imgGO.AddComponent<Image>();
        img.preserveAspect = true;
        img.color = Color.white;
        var imgRect = imgGO.GetComponent<RectTransform>();
        imgRect.anchorMin = new Vector2(0.5f, 0.5f);
        imgRect.anchorMax = new Vector2(0.5f, 0.5f);
        imgRect.pivot     = new Vector2(0.5f, 0.5f);
        imgRect.anchoredPosition = new Vector2(0f, -15f);
        imgRect.sizeDelta = new Vector2(100f, 100f);

        ConnectSpawnerRefs(panelGO);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("NextPanel 생성 완료");
    }

    static void ConnectSpawnerRefs(GameObject panel)
    {
        var spawner = Object.FindFirstObjectByType<FruitSpawner>();
        if (spawner == null) { Debug.LogWarning("FruitSpawner not found"); return; }

        var img   = panel.transform.Find("NextFruitImage")?.GetComponent<Image>();
        var label = panel.transform.Find("NextLabel")?.GetComponent<TextMeshProUGUI>();
        if (img   != null) spawner.nextFruitImage = img;
        if (label != null) spawner.nextFruitLabel = label;
        EditorUtility.SetDirty(spawner);
    }
}
