using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Networking;

public class BeatmapLoader : MonoBehaviour
{
    private ConcurrentDictionary<string, BeatmapInfo> _loadedBeatmaps;

    public ConcurrentDictionary<string, BeatmapInfo> LoadedBeatmaps => _loadedBeatmaps;

    void Start()
    {
        _loadedBeatmaps = new ConcurrentDictionary<string, BeatmapInfo>();
        ReadAllBeatmaps();
    }

    private void ReadAllBeatmaps()
    {
        _loadedBeatmaps.Clear();

#if UNITY_EDITOR
        string songsPath = Path.Combine(Application.dataPath, "songs~");
#else
        string songsPath = Path.Combine(Application.dataPath, "songs");
#endif
        Debug.Log($"reading all beatmaps in folder {songsPath}");

        var songsDirectory = Directory.CreateDirectory(songsPath);

#if UNITY_EDITOR
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
#endif

        using (var sha = new SHA1Managed())
        {

            Parallel.ForEach(songsDirectory.GetDirectories(), songDirectory =>
            {
                try
                {
                    var info = ReadBeatmap(songDirectory.FullName, sha);
                    string hash = string.Concat(sha.Hash.Select(h => h.ToString("x2")));
                    _loadedBeatmaps.TryAdd(hash, info);

                    Debug.Log($"loaded beatmap \"{info.SongName}\" with hash {hash}");
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            });

        }

#if UNITY_EDITOR
        double readTime = stopwatch.Elapsed.TotalMilliseconds;
        stopwatch.Restart();
#endif

        var jobHandles = new NativeList<JobHandle>(_loadedBeatmaps.Count, Allocator.Temp);

        try
        {
            foreach (var info in _loadedBeatmaps)
            {
                LoadCoverImage(info.Value);
                jobHandles.AddNoResize(BlurCoverImage(info.Value));
            }

            JobHandle.CompleteAll(jobHandles);

            foreach (var info in _loadedBeatmaps)
            {
                info.Value.BlurredCoverImage.Apply();
            }
        }
        finally
        {
            jobHandles.Dispose();
        }

#if UNITY_EDITOR
        double imageTime = stopwatch.Elapsed.TotalMilliseconds;
        stopwatch.Stop();
        Debug.Log($"loaded {_loadedBeatmaps.Count} beatmap infos in {readTime}ms and images in {imageTime}ms");
#endif
    }

    private BeatmapInfo ReadBeatmap(string path, SHA1Managed hasher)
    {
        var infoBuffer = File.ReadAllText(Path.Combine(path, "info.dat"));
        var info = JsonConvert.DeserializeObject<BeatmapInfo>(infoBuffer);
        info.Path = path;

        // todo: calculate hash with IncrementalHash?
        var buffer = infoBuffer.ToArray();

        foreach (var set in info.DifficultySets)
        {
            foreach (var difficulty in set.Difficulties)
            {
                var beatmapBuffer = File.ReadAllText(Path.Combine(path, difficulty.Filename));
                difficulty.Beatmap = JsonConvert.DeserializeObject<Beatmap>(beatmapBuffer);
                buffer = buffer.Concat(beatmapBuffer).ToArray();
            }
        }

        lock (hasher)
        {
            hasher.ComputeHash(Encoding.UTF8.GetBytes(buffer));
        }

        return info;
    }

    private void LoadCoverImage(BeatmapInfo info)
    {
        var buffer = File.ReadAllBytes(
            Path.Combine(info.Path, info.CoverImageFilename)
        );
        var texture = new Texture2D(256, 256);

        if (texture.LoadImage(buffer))
        {
            info.CoverImage = texture;
        }
    }

    private JobHandle BlurCoverImage(BeatmapInfo info)
    {
        if (info.CoverImage.format == TextureFormat.RGB24)
        {
            info.BlurredCoverImage = new Texture2D(
                info.CoverImage.width,
                info.CoverImage.height,
                TextureFormat.RGB24,
                true);
            Graphics.CopyTexture(info.CoverImage, info.BlurredCoverImage);

            return new BoxBlurRGB24Job(
                info.BlurredCoverImage.GetRawTextureData<RGB24>(),
                info.CoverImage.width,
                info.CoverImage.height,
                10
            ).Schedule();
        }
        else if (info.CoverImage.format == TextureFormat.ARGB32)
        {
            info.BlurredCoverImage = new Texture2D(
                info.CoverImage.width,
                info.CoverImage.height,
                TextureFormat.RGBA32,
                true);
            Graphics.CopyTexture(info.CoverImage, info.BlurredCoverImage);

            return new BoxBlurRGBA32Job(
                info.BlurredCoverImage.GetRawTextureData<RGBA32>(),
                info.CoverImage.width,
                info.CoverImage.height,
                10
            ).Schedule();
        }

        throw new System.Exception($"could not blur cover image, format not supported: {info.CoverImage.format}");
    }

    public async UniTask LoadSongAsync(BeatmapInfo info, CancellationToken cancellationToken)
    {
        using (var uwr = UnityWebRequestMultimedia.GetAudioClip(
            Path.Combine(info.Path, info.SongFilename), AudioType.OGGVORBIS))
        {
            ((DownloadHandlerAudioClip)uwr.downloadHandler).streamAudio = true;
            var result = await uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(uwr.error);
                return;
            }

            info.Song = DownloadHandlerAudioClip.GetContent(uwr);
        }
    }
}

public class BeatmapInfo
{
    [JsonProperty("_songName")]
    public string SongName { get; set; }

    [JsonProperty("_songSubName")]
    public string SongSubName { get; set; }

    [JsonProperty("_songAuthorName")]
    public string SongAuthorName { get; set; }

    [JsonProperty("_levelAuthorName")]
    public string LevelAuthorName { get; set; }

    [JsonProperty("_beatsPerMinute")]
    public float BeatsPerMinute { get; set; }

    [JsonProperty("_shuffle")]
    public float Shuffle { get; set; }

    [JsonProperty("_shufflePeriod")]
    public float ShufflePeriod { get; set; }

    [JsonProperty("_previewStartTime")]
    public float PreviewStartTime { get; set; }

    [JsonProperty("_previewDuration")]
    public float PreviewDuration { get; set; }

    [JsonProperty("_songFilename")]
    public string SongFilename { get; set; }

    [JsonProperty("_coverImageFilename")]
    public string CoverImageFilename { get; set; }

    [JsonProperty("_songTimeOffset")]
    public float SongTimeOffset { get; set; }

    [JsonProperty("_difficultyBeatmapSets")]
    public List<BeatmapDifficultySet> DifficultySets { get; set; }

    [JsonIgnore]
    public string Path { get; set; }

    [JsonIgnore]
    public AudioClip Song { get; set; }

    [JsonIgnore]
    public Texture2D CoverImage { get; set; }

    [JsonIgnore]
    public Texture2D BlurredCoverImage { get; set; }
}

public class BeatmapDifficultySet
{
    [JsonProperty("_beatmapCharacteristicName")]
    public string CharacteristicName { get; set; }

    [JsonProperty("_difficultyBeatmaps")]
    public List<BeatmapDifficulty> Difficulties { get; set; }
}

public class BeatmapDifficulty
{
    [JsonProperty("_difficulty")]
    public string Difficulty { get; set; }

    [JsonProperty("_beatmapFilename")]
    public string Filename { get; set; }

    [JsonProperty("_noteJumpMovementSpeed")]
    public float NoteJumpSpeed { get; set; }

    [JsonProperty("_noteJumpStartBeatOffset")]
    public float NoteJumpOffset { get; set; }

    [JsonIgnore]
    public Beatmap Beatmap { get; set; }
}

public class Beatmap
{
    [JsonProperty("_notes")]
    public List<BeatmapNote> Notes { get; set; }
}

public class BeatmapNote
{
    [JsonProperty("_time")]
    public float Time { get; set; }

    [JsonProperty("_lineIndex")]
    public int LineIndex { get; set; }

    [JsonProperty("_lineLayer")]
    public int LineLayer { get; set; }

    [JsonProperty("_type")]
    public NoteType Type { get; set; }

    [JsonProperty("_cutDirection")]
    public NoteCutDirection CutDirection { get; set; }
}
