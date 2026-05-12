using UnityEngine;

[CreateAssetMenu(fileName = "FruitData", menuName = "SuikaGame/FruitData")]
public class FruitData : ScriptableObject
{
    public FruitStage[] stages;
}

[System.Serializable]
public class FruitStage
{
    public string fruitName;
    [Range(0.1f, 3f)] public float radius;
    public int score;
    public Color color;
    // 스프라이트가 지정되면 원형 기본 스프라이트 대신 사용
    public Sprite sprite;
}
