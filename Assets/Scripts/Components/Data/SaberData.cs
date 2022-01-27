using Unity.Entities;

[GenerateAuthoringComponent]
public struct SaberData : IComponentData
{
    public SaberSide Side;
}

public enum SaberSide
{
    Both,
    Left,
    Right
}
