using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UiSongInfo : MonoBehaviour
{
    public TMP_Text songTitle;
    public TMP_Text songAuthor;
    public Image coverImage;

    public TMP_Text songDuration;
    public TMP_Text songBpm;
    public TMP_Text difficultyNotes;
    public TMP_Text difficultyMines;
    public TMP_Text difficultyWalls;

    private BeatmapInfo _beatmapInfo;

    public BeatmapInfo BeatmapInfo => _beatmapInfo;

    void Start()
    {
        ResetSongText();
        ResetDifficultyText();
    }

    public void UpdateBeatmapInfo(BeatmapInfo value)
    {
        if (value == _beatmapInfo)
        {
            return;
        }

        _beatmapInfo = value;

        if (coverImage != null && _beatmapInfo.CoverImage != null)
        {
            coverImage.sprite = Sprite.Create(
                _beatmapInfo.CoverImage,
                new Rect(
                    0.0f,
                    0.0f,
                    _beatmapInfo.CoverImage.width,
                    _beatmapInfo.CoverImage.height
                ),
                new Vector2(0.5f, 0.5f)
            );
        }

        songTitle?.SetText(_beatmapInfo.SongName);
        songAuthor?.SetText(_beatmapInfo.SongAuthorName);

        songDuration?.SetText(TimeSpan.FromSeconds(_beatmapInfo.Song.length).ToString(@"mm\:ss"));
        songBpm?.SetText(math.round(_beatmapInfo.BeatsPerMinute).ToString());
    }

    public void ResetSongText()
    {
        songTitle?.SetText("No Song Selected");
        songAuthor?.SetText("-");

        songDuration?.SetText("-");
        songBpm?.SetText("-");
    }

    public void ResetDifficultyText()
    {
        difficultyNotes?.SetText("-");
        difficultyMines?.SetText("-");
        difficultyWalls?.SetText("-");
    }
}
