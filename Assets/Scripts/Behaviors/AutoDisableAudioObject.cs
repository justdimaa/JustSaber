using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AutoDisableAudioObject : MonoBehaviour
{
    private AudioSource _audioSource;

    void OnEnable()
    {
        _audioSource = GetComponent<AudioSource>();
        Invoke(nameof(OnAudioClipEnded), _audioSource.clip.length);
    }

    private void OnAudioClipEnded()
    {
        gameObject.SetActive(false);
    }
}
