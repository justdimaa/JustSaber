using Unity.Entities;

[GenerateAuthoringComponent]
public struct ScoreData : IComponentData
{
    public int Value;
}
