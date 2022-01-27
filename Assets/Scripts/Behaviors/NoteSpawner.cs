using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class NoteSpawner : MonoBehaviour
{
    public Note notePrefab;

    public float gridSpacing;
    public byte gridIndices;
    public byte gridLayers;

    public GameObject songPlayer;

    public UnityEvent songFinishedEvent;

    private Entity _noteEntityPrefab;
    private Entity _noteLeftDirectionalEntityPrefab;
    private Entity _noteLeftNonDirectionalEntityPrefab;
    private Entity _noteRightDirectionalEntityPrefab;
    private Entity _noteRightNonDirectionalEntityPrefab;

    private World _defaultWorld;
    private EntityManager _entityManager;
    private BlobAssetStore _blobAssetStore;

    private float _elapsedTime;
    private float _lastPlaybackPos;

    private bool _isBeatmapSelected;
    private float _beatsPerMinute;
    private float _noteJumpSpeed;
    private float _noteJumpOffset;
    private float _halfJumpDuration;
    private float _noteJumpDistance;
    private Queue<BeatmapNote> _noteQueue;

    void Start()
    {
        _defaultWorld = World.DefaultGameObjectInjectionWorld;
        _entityManager = _defaultWorld.EntityManager;
        _blobAssetStore = new BlobAssetStore();

        var settings = GameObjectConversionSettings.FromWorld(_defaultWorld, _blobAssetStore);

        _noteEntityPrefab = GameObjectConversionUtility
                .ConvertGameObjectHierarchy(notePrefab.gameObject, settings);
        _noteLeftDirectionalEntityPrefab =
            GameObjectConversionUtility
                .ConvertGameObjectHierarchy(notePrefab.leftDirectionalPrefab, settings);
        _noteLeftNonDirectionalEntityPrefab =
            GameObjectConversionUtility
                .ConvertGameObjectHierarchy(notePrefab.leftNonDirectionalPrefab, settings);
        _noteRightDirectionalEntityPrefab =
            GameObjectConversionUtility
                .ConvertGameObjectHierarchy(notePrefab.rightDirectionalPrefab, settings);
        _noteRightNonDirectionalEntityPrefab =
            GameObjectConversionUtility
                .ConvertGameObjectHierarchy(notePrefab.rightNonDirectionalPrefab, settings);
    }

    void OnDestroy()
    {
        _blobAssetStore.Dispose();
    }

    void Update()
    {
        if (!_isBeatmapSelected)
        {
            return;
        }

        var audioSource = songPlayer.GetComponent<AudioSource>();

        if (!audioSource.isPlaying)
        {
            if (_elapsedTime > 0.0f)
            {
                ResetBeatmap();
            }

            return;
        }

        UpdateElapsedTime(audioSource);
        CheckNoteQueue(audioSource);
    }

    public void OnSpawnNotePressed(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }

        Debug.Log($"pressed button {context.control.path}");

        if (context.control.path == "/Keyboard/w")
        {
            InstantiateEntity(
                new float3(0.0f, 1.0f, 1.0f),
                0.0f,
                1.0f,
                NoteType.Left,
                NoteCutDirection.Up
            );

            Debug.Log("spawned note up");
        }

        if (context.control.path == "/Keyboard/a")
        {
            InstantiateEntity(
                new float3(0.0f, 1.0f, 1.0f),
                0.0f,
                0.0f,
                NoteType.Left,
                NoteCutDirection.Left
            );

            Debug.Log("spawned note left");
        }

        if (context.control.path == "/Keyboard/s")
        {
            InstantiateEntity(
                new float3(0.0f, 1.0f, 1.0f),
                0.0f,
                0.0f,
                NoteType.Left,
                NoteCutDirection.Down
            );

            Debug.Log("spawned note down");
        }

        if (context.control.path == "/Keyboard/d")
        {
            InstantiateEntity(
                new float3(0.0f, 1.0f, 1.0f),
                0.0f,
                0.0f,
                NoteType.Left,
                NoteCutDirection.Right
            );

            Debug.Log("spawned note right");
        }
    }

    public void SetBeatmap(BeatmapInfo info, string set, string difficulty)
    {
        var diff = info
            .DifficultySets
            .First(s => s.CharacteristicName == set)
            .Difficulties
            .Find(d => d.Difficulty == difficulty);

        _beatsPerMinute = info.BeatsPerMinute;
        _noteJumpSpeed = diff.NoteJumpSpeed;
        _noteJumpOffset = diff.NoteJumpOffset;

        _halfJumpDuration = GetHalfJumpDuration();
        _noteJumpDistance = GetNoteJumpDistance();

        _noteQueue = new Queue<BeatmapNote>(diff.Beatmap.Notes);

        _isBeatmapSelected = true;
    }

    public void ResetBeatmap()
    {
        _beatsPerMinute = 0.0f;
        _noteJumpSpeed = 0.0f;
        _noteJumpOffset = 0.0f;
        
        _halfJumpDuration = 0.0f;
        _noteJumpDistance = 0.0f;

        _noteQueue = null;

        _isBeatmapSelected = false;
    }

    private void UpdateElapsedTime(AudioSource audioSource)
    {
        _elapsedTime += Time.deltaTime;

        if (audioSource.time != _lastPlaybackPos)
        {
            _elapsedTime = (_elapsedTime + audioSource.time) / 2.0f;
            _lastPlaybackPos = audioSource.time;
        }
    }

    private void CheckNoteQueue(AudioSource audioSource)
    {
        while (_noteQueue.Count != 0)
        {
            var note = _noteQueue.Peek();

            float deltaSpawnTime =
                BeatToSecond(note.Time)
                - BeatToSecond(_halfJumpDuration)
                - _elapsedTime;

            if (deltaSpawnTime > 0.0f)
            {
                break;
            }

            try
            {
                InstantiateEntity(
                    GetSpawnOffset((byte)note.LineIndex, (byte)note.LineLayer)
                        - new float3(0.0f, 0.0f, deltaSpawnTime * _noteJumpSpeed),
                    note.Time,
                    _noteJumpSpeed,
                    note.Type,
                    note.CutDirection
                );
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }

            _noteQueue.Dequeue();
        }
    }

    private Entity InstantiateEntity(float3 position, float time, float speed, NoteType type, NoteCutDirection cutDirection)
    {
        float rotation = CutDirectionToRotation(cutDirection);
        var meshEntityPrefab = GetMeshEntityPrefab(type, cutDirection);

        var noteEntity = _entityManager.Instantiate(_noteEntityPrefab);

#if UNITY_EDITOR
        _entityManager.SetName(noteEntity, "Note");
#endif

        _entityManager.SetComponentData(noteEntity, new Translation
        {
            Value = position
        });
        _entityManager.SetComponentData(noteEntity, new Rotation
        {
            Value = quaternion.RotateZ(rotation)
        });
        _entityManager.AddComponentData(noteEntity, new PhysicsVelocity
        {
            Linear = new float3(0.0f, 0.0f, -speed)
        });
        _entityManager.SetComponentData(noteEntity, new NoteData
        {
            Time = time,
            Speed = speed,
            Type = type,
            CutDirection = cutDirection
        });

        var meshEntity = _entityManager.Instantiate(meshEntityPrefab);

        var buffer = _entityManager.GetBuffer<LinkedEntityGroup>(noteEntity);
        buffer.Add(meshEntity);

        using var bufferArray = buffer.ToNativeArray(Allocator.Temp);
        using var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var child in bufferArray.Skip(1))
        {
            var childEntity = child.Value;

            ecb.AddComponent(childEntity, new Parent
            {
                Value = noteEntity
            });
            ecb.AddComponent(childEntity, new LocalToParent());
        }

        ecb.Playback(_entityManager);
        return noteEntity;
    }

    private float CutDirectionToRotation(NoteCutDirection cutDirection) => cutDirection switch
    {
        NoteCutDirection.Up => math.radians(180.0f),
        NoteCutDirection.Down => 0.0f,
        NoteCutDirection.Left => math.radians(-90.0f),
        NoteCutDirection.Right => math.radians(90.0f),
        NoteCutDirection.UpLeft => math.radians(-135.0f),
        NoteCutDirection.UpRight => math.radians(135.0f),
        NoteCutDirection.DownLeft => math.radians(-45.0f),
        NoteCutDirection.DownRight => math.radians(45.0f),
        NoteCutDirection.Any => math.radians(0.0f),
        _ => throw new System.ArgumentException()
    };

    private Entity GetMeshEntityPrefab(NoteType type, NoteCutDirection cutDirection) => (type, cutDirection) switch
    {
        (NoteType.Left, NoteCutDirection.Any) => _noteLeftNonDirectionalEntityPrefab,
        (NoteType.Right, NoteCutDirection.Any) => _noteRightNonDirectionalEntityPrefab,
        (NoteType.Left, _) => _noteLeftDirectionalEntityPrefab,
        (NoteType.Right, _) => _noteRightDirectionalEntityPrefab,
        (NoteType.Bomb, _) => throw new System.NotImplementedException(),
        _ => throw new System.ArgumentException()
    };

    // src: https://git.io/JaFVr
    private float GetHalfJumpDuration()
    {
        float result = 4.0f;
        float bpmFrequency = 60.0f / _beatsPerMinute;

        while (_noteJumpSpeed * bpmFrequency * result > 18.0f)
        {
            result /= 2.0f;
        }

        if (result < 1.0f)
        {
            result = 1.0f;
        }

        return math.max(result + _noteJumpOffset, 1.0f);
    }

    private float GetNoteJumpDistance()
    {
        return _noteJumpSpeed
            * (60.0f / _beatsPerMinute)
            * _halfJumpDuration;
    }

    private float3 GetGridOffset(byte index, byte layer)
    {
        return new float3(
            (float)index * gridSpacing - gridSpacing * ((float)gridIndices - 1.0f) / 2.0f,
            (float)layer * gridSpacing,
            0.0f
        );
    }

    private float3 GetSpawnOffset(byte index, byte layer)
    {
        return GetGridOffset(index, layer)
            + new float3(
                0.0f,
                transform.position.y,
                _noteJumpDistance
            );
    }

    private float BeatToSecond(float value)
    {
        return (value / _beatsPerMinute) * 60.0f;
    }
}
