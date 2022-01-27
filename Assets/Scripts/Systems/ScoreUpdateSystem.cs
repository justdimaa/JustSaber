using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(NoteDestroyCollidedSystem))]
public class ScoreUpdateSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _ecbSystem;

    private Entity _scoreUiEntity;
    private ScoreData _score;
    private ScoreUiData _scoreUi;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        RequireSingletonForUpdate<ScoreData>();
        _scoreUiEntity = GetSingletonEntity<ScoreData>();
        _score = EntityManager.GetComponentData<ScoreData>(_scoreUiEntity);
        _scoreUi = EntityManager.GetComponentData<ScoreUiData>(_scoreUiEntity);
    }

    protected override void OnUpdate()
    {
        var ecb = _ecbSystem.CreateCommandBuffer();

        int deltaScore = 0;

        Entities.ForEach((Entity entity, in UpdateScoreEvent updateScoreEvent) =>
        {
            deltaScore += updateScoreEvent.Value;
            ecb.DestroyEntity(entity);
        }).WithoutBurst().Run();

        if (deltaScore != 0)
        {
            _score.Value += deltaScore;
            _scoreUi.Text.text = _score.Value.ToString();
        }
    }
}
