using UnityEngine;
using UnityEditor;
using System.IO;

public class JuiceDropSetup
{
    [MenuItem("Tools/Suika/Create JuiceDrop Material")]
    public static void CreateJuiceDropMaterial()
    {
        // ── 물방울 텍스쳐 생성 ────────────────────────────────────
        const int res = 64;
        var tex = new Texture2D(res, res, TextureFormat.ARGB32, false);
        float c = res / 2f;
        var pixels = new Color[res * res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dx   = x - c + 0.5f;
                float dy   = y - c + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = Mathf.Clamp01((c * 0.85f - dist) / (c * 0.25f));
                float ring  = Mathf.Clamp01(1f - Mathf.Abs(dist - c * 0.6f) / (c * 0.15f)) * 0.4f;
                alpha = Mathf.Clamp01(alpha + ring * (1f - alpha));

                pixels[y * res + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        string texPath = "Assets/Sprites/JuiceDrop.png";
        File.WriteAllBytes(
            Path.Combine(Application.dataPath, "Sprites/JuiceDrop.png"),
            tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(texPath);

        var importer = (TextureImporter)AssetImporter.GetAtPath(texPath);
        importer.textureType         = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = res;
        importer.filterMode          = FilterMode.Bilinear;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        // ── 머티리얼 생성 ─────────────────────────────────────────
        if (!Directory.Exists("Assets/Materials"))
            Directory.CreateDirectory("Assets/Materials");

        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

        string matPath = "Assets/Materials/JuiceDrop.mat";
        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.SaveAssets();

        // ── MergeParticle 프리팹에 적용 ───────────────────────────
        string prefabPath = "Assets/Prefabs/MergeParticle.prefab";
        var prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabInstance != null)
        {
            var ren = prefabInstance.GetComponent<ParticleSystemRenderer>();
            ren.material   = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            ren.renderMode = ParticleSystemRenderMode.Billboard;
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }

        // ── GameManager 연결 ──────────────────────────────────────
        var gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.mergeParticlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            EditorUtility.SetDirty(gm);
            EditorUtility.DisplayDialog("완료", "JuiceDrop 머티리얼 생성 및 연결 완료!", "OK");
        }

        Debug.Log("JuiceDrop 텍스쳐 + 머티리얼 생성 완료: " + matPath);
    }
}
