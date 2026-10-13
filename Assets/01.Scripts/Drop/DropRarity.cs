using UnityEngine;

public class DropRarity
{
    private readonly float normal;
    private readonly float rare;
    private readonly float epic;
    private readonly float rarePerStage;
    private readonly float epicPerStage;

    public DropRarity(float normal, float rare, float epic, float rarePerStage, float epicPerStage)
    {
        this.normal = normal;
        this.rare = rare;
        this.epic = epic;
        this.rarePerStage = rarePerStage;
        this.epicPerStage = epicPerStage;
    }
    public ItemGrade Roll(int stage)
    {
        float r = rare + rarePerStage * stage;
        float e = epic + epicPerStage * stage;
        float total = normal + r + e;
        float roll = Random.Range(0f, total);

        if(roll < e)
            return ItemGrade.Epic;

        if(roll < e + r)
            return ItemGrade.Rare;

        return ItemGrade.Normal;
    }

}
