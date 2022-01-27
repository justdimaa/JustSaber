using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(NoteDestroyCollidedSystem))]
public class NoteHitSoundPlaySystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _ecbSystem;
    private ObjectPool _goodCutSoundPool;
    private ObjectPool _badCutSoundPool;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        _goodCutSoundPool = UnityEngine.GameObject
            .FindGameObjectWithTag("GoodCutSoundPool")
            .GetComponent<ObjectPool>();
        _badCutSoundPool = UnityEngine.GameObject
            .FindGameObjectWithTag("BadCutSoundPool")
            .GetComponent<ObjectPool>();
    }

    protected override void OnUpdate()
    {
        var ecb = _ecbSystem.CreateCommandBuffer();

        Entities
            .ForEach((Entity entity, in PlayNoteHitSoundEvent playNoteHitSoundEvent) =>
            {
                var hitPlayer = (playNoteHitSoundEvent.IsGoodCut ? _goodCutSoundPool : _badCutSoundPool)
                    .GetPoolObject()
                    .GetComponent<UnityEngine.AudioSource>();
                hitPlayer.gameObject.SetActive(true);

                ecb.DestroyEntity(entity);
            }).WithoutBurst().Run();
    }
}
