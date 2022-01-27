using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public class SyncTransformFromGameObject : IComponentData
{
    public Transform From;
}
