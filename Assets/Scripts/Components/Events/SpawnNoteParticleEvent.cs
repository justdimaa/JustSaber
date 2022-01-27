using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct SpawnNoteParticleEvent : IComponentData
{
    public Translation Translation;
    public Rotation Rotation;
    public float3 Color;
}
