using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public UiPlaylist playlist;
    public UiSongInfo songInfo;
    public ToggleGroupC difficulties;
    public Button playButton;
    public AudioSource songPlayer;

    private BeatmapLoader _beatmapLoader;
    private BeatmapInfo _selectedBeatmap;
    private string _selectedDifficulty;

    void Start()
    {
        // Application.targetFrameRate = 120;
//
        _beatmapLoader = GameObject
            .FindWithTag("BeatmapLoader")
            .GetComponent<BeatmapLoader>();

        if (difficulties != null)
        {
            foreach (var toggle in difficulties.togglez)
            {
                toggle.SetDisabled(true);
                toggle.onSelect.AddListener(OnDifficultySelect);
            }
        }

        if (playButton != null)
        {
            playButton.interactable = false;
            playButton.onClick.AddListener(OnPlayButtonClick);
        }

        foreach (var info in _beatmapLoader.LoadedBeatmaps)
        {
            playlist.GetComponent<UiPlaylist>().AddItem(info.Value, () => OnPlaylistItemClick().Forget());
        }
    }

    private async UniTaskVoid OnPlaylistItemClick()
    {
        if (_selectedBeatmap == null)
        {
            foreach (var toggle in difficulties.togglez)
            {
                toggle.SetDisabled(false);
            }
        }

        var item = playlist.ToggleGroup.currToggle.GetComponent<UiPlaylistItem>();
        _selectedBeatmap = item.BeatmapInfo;

        Debug.Log($"selected beatmap {_selectedBeatmap.SongName} - {_selectedBeatmap.SongAuthorName}");

        if (_selectedBeatmap.Song == null)
        {
            var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(2000);

            try
            {
                await _beatmapLoader.LoadSongAsync(_selectedBeatmap, cancellationSource.Token);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        songInfo.UpdateBeatmapInfo(_selectedBeatmap);
        UpdateDifficulties();

        if (songPlayer != null && _selectedBeatmap.Song != null)
        {
            songPlayer.clip = _selectedBeatmap.Song;
            songPlayer.Play();
        }
        
        // if (_selectedDifficulty != null)
        // {
        //     playButton.interactable = true;
        // }
    }

    private void UpdateDifficulties()
    {
        var standardSet = _selectedBeatmap.DifficultySets.FirstOrDefault(s => s.CharacteristicName == "Standard");

        if (standardSet == null)
        {
            Debug.LogError("beatmap doesnt have the standard difficulty set");
            return;
        }

        foreach (var toggle in difficulties.togglez)
        {
            string difficultyName = DifficultyButtonNameToDifficulty(toggle.name);

            if (!standardSet.Difficulties.Any(d => d.Difficulty == difficultyName))
            {
                if (difficultyName == _selectedDifficulty)
                {
                    playButton.interactable = false;
                    _selectedDifficulty = null;
                }

                toggle.gameObject.SetActive(false);
                toggle.SetIsOn(false);
                continue;
            }

            toggle.gameObject.SetActive(true);
        }
    }

    public void OnNoteSpawnerFinished()
    {
        gameObject.SetActive(true);
    }

    private void OnDifficultySelect()
    {
        playButton.interactable = true;
        _selectedDifficulty = DifficultyButtonNameToDifficulty(difficulties.currToggle.name);
        Debug.Log($"selected difficulty {_selectedDifficulty}");
    }

    private void OnPlayButtonClick()
    {
        gameObject.SetActive(false);
        songPlayer.Stop();

        UniTask.Run(async () =>
        {
            await UniTask.Delay(4000);
        
            var noteSpawner = GameObject
                .FindWithTag("NoteSpawner")
                .GetComponent<NoteSpawner>();

            noteSpawner.SetBeatmap(_selectedBeatmap, "Standard", _selectedDifficulty);
            songPlayer.Play();
        });
    }

    private string DifficultyButtonNameToDifficulty(string name) => name switch
    {
        "EasyButton" => "Easy",
        "NormalButton" => "Normal",
        "HardButton" => "Hard",
        "ExpertButton" => "Expert",
        "ExpertPlusButton" => "ExpertPlus",
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };
}
