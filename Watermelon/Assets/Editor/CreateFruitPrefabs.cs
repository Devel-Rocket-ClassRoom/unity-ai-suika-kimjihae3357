using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Tools > Suika > Create Fruit Prefabs
/// 기존 FruitData의 스프라이트와 radius를 읽어 11개 프리팹을 생성하고
/// FruitData.stages[i].prefab에 자동 할당합니다.
/// 생성 후 각 프리팹을 Inspector에서 직접 콜라이더/스프라이트를 조정하세요.
/// </summary>
public class CreateFruitPrefabs
{
    // 기존 FruitData (radius, sprite 정보가 있는 구버전 에셋)에서 읽어올 데이터
    static readonly string[] fruitNames = {
        "Blueberry", "Strawberry", "Grape", "Dekopon", "Persimmon",
        "Apple", "Pear", "Peach", "Pineapple", "Melon", "Watermelon"
    };
    static readonly float[] radii = {
        0.30f, 0.38f, 0.46f, 0.54f, 0.62f,
        0.72f, 0.84f, 0.96f, 1.10f, 1.28f, 1.50f
    };
    static readonly string[] spriteNames = {
        "Blueberry", "Strawberry 1", "Grape", "Dekopon", "Persimmon",
        "Apple", "Pear", "Peach", "Pineapple", "Melon", "Watermelon"
    };

    [MenuItem("Tools/Suika/Create Fruit Prefabs")]
    public static void CreatePrefabs()
    {
        string prefabFolder = "Assets/Prefabs/Fruits";
        if (!Directory.Exists(prefabFolder))
            Directory.CreateDirectory(prefabFolder);

        // FruitData 로드
        var fruitData = AssetDatabase.LoadAssetAtPath<FruitData>("Assets/Resources/FruitData.asset");
        if (fruitData == null) { Debug.LogError("FruitData.asset not found"); return; }

        // PhysicsMaterial2D 로드 (있으면 사용)
        var physicsMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Physics/FruitPhysics.physicsMaterial2D");

        for (int i = 0; i < fruitNames.Length; i++)
        {
            string name = fruitNames[i];
            float radius = i < radii.Length ? radii[i] : 0.5f;

            // 스프라이트 로드
            Sprite sprite = null;
            if (i < spriteNames.Length)
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{spriteNames[i]}.png");

            // 루트 오브젝트
            var go = new GameObject(name);

            // Fruit 컴포넌트
            var fruit = go.AddComponent<Fruit>();
            fruit.SetStage(i);

            // Rigidbody2D — RequireComponent로 이미 추가되어 있으므로 GetComponent 사용
            var rb = go.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 1.5f;

            // CircleCollider2D — 마찬가지로 GetComponent 사용
            var col = go.GetComponent<CircleCollider2D>();
            col.radius = radius;
            if (physicsMat != null) col.sharedMaterial = physicsMat;

            // 자식 Visual 오브젝트에 SpriteRenderer 배치
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            if (sprite != null) sr.sprite = sprite;
            sr.sortingOrder = 1;

            // 이미 존재하는 프리팹은 덮어쓰지 않음
            string prefabPath = $"{prefabFolder}/{name}.prefab";
            if (File.Exists(prefabPath))
            {
                Object.DestroyImmediate(go);
                var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (i < fruitData.stages.Length) fruitData.stages[i].prefab = existing;
                Debug.Log($"건너뜀 (이미 존재): {prefabPath}");
                continue;
            }
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            // FruitData에 할당
            if (i < fruitData.stages.Length)
            {
                fruitData.stages[i].prefab = prefab;
                fruitData.stages[i].fruitName = name;
            }

            Debug.Log($"생성: {prefabPath}");
        }

        EditorUtility.SetDirty(fruitData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료",
            $"11개 프리팹이 {prefabFolder}에 생성되고 FruitData에 할당됐습니다.\n\n" +
            "이제 각 프리팹을 열어 콜라이더 크기와 스프라이트를 직접 조정하세요.",
            "OK");
    }
}
