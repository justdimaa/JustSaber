using Unity.Entities;

[GenerateAuthoringComponent]
public struct NoteData : IComponentData
{
    public float Time;
    public float Speed;
    public NoteType Type;
    public NoteCutDirection CutDirection;
}

public enum NoteType
{
    Left = 0,
    Right = 1,
    Bomb = 3,
}

public enum NoteCutDirection
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
    UpLeft = 4,
    UpRight = 5,
    DownLeft = 6,
    DownRight = 7,
    Any = 8
}
