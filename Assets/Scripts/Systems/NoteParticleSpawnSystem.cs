using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(NoteDestroyCollidedSystem))]
public class NoteParticleSpawnSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _ecbSystem;
    private ObjectPool _noteParticlePool;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        _noteParticlePool = UnityEngine.GameObject
            .FindGameObjectWithTag("NoteParticlePool")
            .GetComponent<ObjectPool>();
    }

    protected override void OnUpdate()
    {
        var ecb = _ecbSystem.CreateCommandBuffer();

        Entities
            .ForEach((Entity entity, in SpawnNoteParticleEvent spawnParticleEvent) =>
            {
                var particle = _noteParticlePool
                    .GetPoolObject()
                    .GetComponent<UnityEngine.ParticleSystem>();
                particle.transform.position = spawnParticleEvent.Translation.Value;
                particle.transform.rotation = spawnParticleEvent.Rotation.Value;
                
                var col = particle.colorOverLifetime;
                var color = new UnityEngine.Gradient();
                color.SetKeys(
                    new UnityEngine.GradientColorKey[]
                    {
                        new UnityEngine.GradientColorKey(
                            new UnityEngine.Color(
                                spawnParticleEvent.Color.x,
                                spawnParticleEvent.Color.y,
                                spawnParticleEvent.Color.z),
                            0.0f),
                        // new UnityEngine.GradientColorKey(spawnParticlesEvent.Color, 1.0f)
                    },
                    new UnityEngine.GradientAlphaKey[]
                    {
                        new UnityEngine.GradientAlphaKey(0.75f, 0.0f),
                        new UnityEngine.GradientAlphaKey(0.0f, 1.0f)
                    }
                );
                col.color = color;
                particle.gameObject.SetActive(true);

                ecb.DestroyEntity(entity);
            }).WithoutBurst().Run();
    }
}
