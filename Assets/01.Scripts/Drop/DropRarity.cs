using UnityEngine;

public class DropRarity
{
    private static readonly ItemGrade[] Grades = { ItemGrade.Legendary, ItemGrade.Unique, ItemGrade.Epic, ItemGrade.Rare, ItemGrade.Normal };

    private readonly float[] baseWeights;
    private readonly float[] perStage;

    public DropRarity(float[] baseWeights, float[] perStage)
    {
        this.baseWeights = baseWeights;
        this.perStage = perStage;
    }

    public ItemGrade Roll(int stage)
    {
        float total = 0f;

        for(int i = 0; i < Grades.Length; i++)
        {
            total += GetWeight(i, stage);
        }

        float roll = Random.Range(0f, total);

        for(int i = 0; i < Grades.Length; i++)
        {
            roll -= GetWeight(i, stage);

            if(roll < 0f)
                return Grades[i];
        }

        return ItemGrade.Normal;
    }

    private float GetWeight(int index, int stage)
    {
        float growth = index < perStage.Length ? perStage[index] * stage : 0f;
        return Mathf.Max(0f, baseWeights[index] + growth);
    }
}