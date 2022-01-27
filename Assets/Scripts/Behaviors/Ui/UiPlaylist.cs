using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ToggleGroupC))]
public class UiPlaylist : MonoBehaviour
{
    public GameObject content;
    public UiPlaylistItem itemPrefab;

    private ToggleGroupC _toggleGroup;

    public ToggleGroupC ToggleGroup => _toggleGroup;

    public void Start()
    {
        _toggleGroup = GetComponent<ToggleGroupC>();

        DeleteAllItems();
    }

    public void AddItem(BeatmapInfo info, UnityAction call)
    {
        if (content == null)
        {
            Debug.LogError($"{nameof(content)} has not been assigned");
            return;
        }

        if (itemPrefab == null)
        {
            Debug.LogError($"{nameof(itemPrefab)} has not been assigned");
            return;
        }

        var item = GameObject.Instantiate(itemPrefab);
        item.GetComponent<ToggleC>().onSelect.AddListener(call);
        item.GetComponent<ToggleC>().toggleGroup = _toggleGroup;
        item.UpdateBeatmapInfo(info);
        item.transform.SetParent(content.transform, false);
    }

    public void DeleteAllItems()
    {
        foreach (Transform item in content.transform)
        {
            GameObject.Destroy(item.gameObject);
        }
    }
}