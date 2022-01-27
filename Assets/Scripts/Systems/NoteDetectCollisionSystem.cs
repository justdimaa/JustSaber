using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class NoteDetectCollisionSystem : SystemBase
{
    private BuildPhysicsWorld _buildPhysicsWorld;
    private StepPhysicsWorld _stepPhysicsWorld;
    private EndSimulationEntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _buildPhysicsWorld = World.GetExistingSystem<BuildPhysicsWorld>();
        _stepPhysicsWorld = World.GetExistingSystem<StepPhysicsWorld>();
        _ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _ecbSystem.CreateCommandBuffer();

        Entities
            .WithAny<GoodCutColliderTag, BadCutColliderTag>()
            .ForEach((Entity colliderEntity, ref DynamicBuffer<StatefulCollisionEvent> collisionEvents, in Parent noteEntity, in LocalToWorld localToWorld) =>
            {
                // notes spawn at position 0 first for some reason
                if (localToWorld.Position.Equals(float3.zero))
                {
                    return;
                }

                bool isGoodCutCollider = HasComponent<GoodCutColliderTag>(colliderEntity);
                bool isBadCutCollider = HasComponent<BadCutColliderTag>(colliderEntity);

                foreach (var collisionEvent in collisionEvents)
                {
                    if (collisionEvent.CollidingState != EventCollidingState.Enter)
                    {
                        continue;
                    }

                    var saberEntity = collisionEvent.GetOtherEntity(colliderEntity);
                    var saber = GetComponent<SaberData>(saberEntity);

                    var note = GetComponent<NoteData>(noteEntity.Value);

                    bool isGoodCut =
                        note.Type == NoteType.Left && saber.Side == SaberSide.Left ||
                        note.Type == NoteType.Right && saber.Side == SaberSide.Right;

                    // detect cut direction
                    // needs to be tweaks even further, too many false positives
                    // if (isGoodCut && note.CutDirection != NoteCutDirection.Any)
                    // {
                    //     float collisionAngle =
                    //         math.acos(
                    //             math.clamp(
                    //                 math.dot(
                    //                     collisionEvent.Normal,
                    //                     localToWorld.Up
                    //                 ),
                    //                 -1.0f,
                    //                 1.0f
                    //             )
                    //         ) * 57.29578f;

                    //     UnityEngine.Debug.Log($"normal: {collisionEvent.Normal} angle: {collisionAngle}");

                    //     if (collisionAngle > 90.0f)
                    //     {
                    //         isGoodCut = false;
                    //         UnityEngine.Debug.Log($"dir: {note.CutDirection} badCollider: {isBadCutCollider} | bad");

                    //         if (!isBadCutCollider)
                    //         {
                    //             return;
                    //         }
                    //     }
                    // }

                    if (!isGoodCut)
                    {
                        UnityEngine.Debug.Log($"dir: {note.CutDirection} badCollider: {isBadCutCollider} | bad");
                        return;
                    }

                    // if (isGoodCut)
                    {
                        UnityEngine.Debug.Log($"dir: {note.CutDirection} badCollider: {isBadCutCollider} | good");
                    }

                    ecb.AddComponent(noteEntity.Value, new NoteCollidedData
                    {
                        Saber = saberEntity,
                        SaberSide = saber.Side,
                        IsGoodCut = isGoodCut
                    });
                }
            }).Run();
    }
}
