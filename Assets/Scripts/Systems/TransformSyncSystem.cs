using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public class TransformSyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities
            .ForEach((SyncTransformFromGameObject sync,
            ref PhysicsVelocity physicsVelocity,
            in PhysicsMass physicsMass,
            in Translation translation,
            in Rotation rotation) =>
            {
                physicsVelocity = PhysicsVelocity.CalculateVelocityToTarget(
                    physicsMass,
                    translation,
                    rotation,
                    new RigidTransform(sync.From.rotation, sync.From.position),
                    1.0f / deltaTime
                );
            }).WithoutBurst().Run();
    }
}
