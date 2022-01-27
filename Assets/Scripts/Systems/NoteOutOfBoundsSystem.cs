using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(NoteDestroyCollidedSystem))]
public class NoteOutOfBoundsSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _ecbSystem.CreateCommandBuffer();

        Entities
            .WithNone<NoteCollidedData>()
            .ForEach((Entity entity, in Translation translation, in NoteData note) =>
            {
                if (translation.Value.z > -1.0f)
                {
                    return;
                }

                ecb.DestroyEntity(entity);
            }).Schedule();

        _ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
