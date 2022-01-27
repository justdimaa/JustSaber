using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(ToggleC))]
public class UiPlaylistItem : MonoBehaviour
{
    public Image coverImage;
    public TMP_Text songTitle;
    public TMP_Text songAuthor;
    public float imageGrayscale;

    private BeatmapInfo _beatmapInfo;

    public BeatmapInfo BeatmapInfo => _beatmapInfo;

    void Start()
    {
        if (coverImage != null && coverImage.material != null)
        {
            // give each image its own material so it doesnt affect other images
            coverImage.material = Instantiate(coverImage.material);
        }
    }

    void Update()
    {
        coverImage.materialForRendering.SetFloat("_EffectAmount", imageGrayscale);
    }

    public void UpdateBeatmapInfo(BeatmapInfo value)
    {
        _beatmapInfo = value;

        if (coverImage != null && _beatmapInfo.BlurredCoverImage != null)
        {
            coverImage.sprite = Sprite.Create(
                _beatmapInfo.BlurredCoverImage,
                new Rect(
                    0.0f,
                    0.0f,
                    _beatmapInfo.BlurredCoverImage.width,
                    _beatmapInfo.BlurredCoverImage.height
                ),
                new Vector2(0.5f, 0.5f)
            );
        }

        songTitle?.SetText(_beatmapInfo.SongName);
        songAuthor?.SetText(_beatmapInfo.SongAuthorName);
    }
}
