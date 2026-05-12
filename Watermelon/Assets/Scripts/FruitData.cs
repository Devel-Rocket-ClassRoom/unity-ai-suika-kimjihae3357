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
    public int score;
    public GameObject prefab;
    public Color particleColor = Color.white;
}
