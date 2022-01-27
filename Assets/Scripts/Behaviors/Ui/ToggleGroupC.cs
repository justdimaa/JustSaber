//Made by: HolyFot
//License: CC0 - https://creativecommons.org/share-your-work/public-domain/cc0/
//Note: ToggleC scripts must be childs of ToggleGroupC
// src: https://gist.github.com/HolyFot/bcb98468200d02f083a0f03246206372
using UnityEngine;
using System.Collections.Generic;

public class ToggleGroupC : MonoBehaviour
{
    [SerializeField] public ToggleC currToggle;
    [SerializeField] public List<ToggleC> togglez;
    [SerializeField] public bool allowToggleOff = false;

    public void SetSelected(ToggleC temp)
    {
        for (int i = 0; i < togglez.Count; i++)
        {
            if (togglez[i] == temp)
            {
                currToggle = temp;
            }
            else
                togglez[i].SetIsOn(false);
        }
    }

    public void DeselectAll()
    {
        currToggle = null;

        for (int i = 0; i < togglez.Count; i++)
        {
            togglez[i].SetIsOn(false);
        }
    }

    public void AddToggle(ToggleC temp)
    {
        bool wasFound = false;
        for (int i = 0; i < togglez.Count; i++)
        {
            if (togglez[i] == temp)
                wasFound = true;
        }

        if (!wasFound)
            togglez.Add(temp);
    }

    public void RemoveToggle(ToggleC temp)
    {
        togglez.Remove(temp);
    }
}