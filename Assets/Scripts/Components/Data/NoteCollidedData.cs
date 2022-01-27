using Unity.Entities;

public struct NoteCollidedData : IComponentData
{
    public Entity Saber;
    public SaberSide SaberSide;
    public bool IsGoodCut;
}
