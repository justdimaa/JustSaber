using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public class NoteDestroyCollidedSystem : SystemBase
{
    // hardcoded particle colors
    private readonly static float3 LeftParticleColor =
        new float3(255.0f, 165.0f, 0.0f) / 255.0f;
    private readonly static float3 RightParticleColor =
        new float3(0.0f, 255.0f, 255.0f) / 255.0f;

    private EndSimulationEntityCommandBufferSystem _ecbSystem;
    private EntityArchetype _spawnNoteParticleEventArchetype;
    private EntityArchetype _playNoteHitSoundEventArchetype;
    private EntityArchetype _rumbleControllerEventArchetype;
    private EntityArchetype _updateScoreEventArchetype;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        
        _spawnNoteParticleEventArchetype = EntityManager.CreateArchetype(
            typeof(SpawnNoteParticleEvent)
        );
        _playNoteHitSoundEventArchetype = EntityManager.CreateArchetype(
            typeof(PlayNoteHitSoundEvent)
        );
        _rumbleControllerEventArchetype = EntityManager.CreateArchetype(
            typeof(RumbleControllerEvent)
        );
        _updateScoreEventArchetype = EntityManager.CreateArchetype(
            typeof(UpdateScoreEvent)
        );
    }
    protected override void OnUpdate()
    {
        var ecb = _ecbSystem.CreateCommandBuffer();
        var spawnNoteParticleEventArchetype = _spawnNoteParticleEventArchetype;
        var playNoteHitSoundEventArchetype = _playNoteHitSoundEventArchetype;
        var rumbleControllerEventArchetype = _rumbleControllerEventArchetype;
        var updateScoreEventArchetype = _updateScoreEventArchetype;

        Entities
            .ForEach((Entity entity,
            in Translation translation,
            in Rotation rotation,
            in NoteData note,
            in NoteCollidedData noteCollision) =>
            {
                // spawn particles
                var spawnNoteParticlesEventEntity = ecb.CreateEntity(spawnNoteParticleEventArchetype);
                ecb.SetComponent(spawnNoteParticlesEventEntity, new SpawnNoteParticleEvent
                {
                    Translation = translation,
                    Rotation = rotation,
                    Color = note.Type switch
                    {
                        NoteType.Left => LeftParticleColor,
                        NoteType.Right => RightParticleColor,
                        _ => new float3()
                    }
                });

                // play hit sound
                var playNoteHitSoundEventEntity = ecb.CreateEntity(playNoteHitSoundEventArchetype);
                ecb.SetComponent(playNoteHitSoundEventEntity, new PlayNoteHitSoundEvent
                {
                    IsGoodCut = noteCollision.IsGoodCut
                });

                // haptic feedback
                var rumbleControllerEventEntity = ecb.CreateEntity(rumbleControllerEventArchetype);
                ecb.SetComponent(rumbleControllerEventEntity, new RumbleControllerEvent
                {
                    Side = noteCollision.SaberSide
                });
                
                // update scoreboard
                var updateScoreEventEntity = ecb.CreateEntity(updateScoreEventArchetype);
                ecb.SetComponent(updateScoreEventEntity, new UpdateScoreEvent
                {
                    Value = 1
                });

                ecb.DestroyEntity(entity);
            }).Run();

        // _ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
