using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateAfter(typeof(NoteDestroyCollidedSystem))]
public class ControllerRumbleSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _ecbSystem;
    private UnityEngine.XR.InputDevice _leftController;
    private UnityEngine.XR.InputDevice _rightController;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        _leftController = inputDevices.First(d =>
            d.characteristics.HasFlag(UnityEngine.XR.InputDeviceCharacteristics.Left));
        _rightController = inputDevices.First(d =>
            d.characteristics.HasFlag(UnityEngine.XR.InputDeviceCharacteristics.Right));
    }

    protected override void OnUpdate()
    {
        var ecb = _ecbSystem.CreateCommandBuffer();

        Entities.ForEach((Entity entity, in RumbleControllerEvent rumbleControllerEvent) =>
        {
            if (rumbleControllerEvent.Side == SaberSide.Left || rumbleControllerEvent.Side == SaberSide.Both)
            {
                _leftController.SendHapticImpulse(0, 1.0f);
            }

            if (rumbleControllerEvent.Side == SaberSide.Right || rumbleControllerEvent.Side == SaberSide.Both)
            {
                _rightController.SendHapticImpulse(0, 1.0f);
            }

            ecb.DestroyEntity(entity);
        }).WithoutBurst().Run();
    }
}
