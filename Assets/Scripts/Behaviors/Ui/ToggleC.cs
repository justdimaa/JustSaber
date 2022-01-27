//Made by: HolyFot
//License: CC0 - https://creativecommons.org/share-your-work/public-domain/cc0/
//Note: ToggleC scripts must be childs of ToggleGroupC
//Note: "TargetGraphic" is a separate child image.
//Version 1.1 (Fix warnings & selection bug)
// src: https://gist.github.com/HolyFot/bcb98468200d02f083a0f03246206372
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class ToggleC : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] public ToggleTypeC toggleType;
    [SerializeField] Image TargetGraphic = null;
    [SerializeField] Sprite sprNormal = null;
    [SerializeField] Sprite sprHighlighted = null;
    [SerializeField] Sprite sprSelected = null;
    [SerializeField] Sprite sprDown = null;
    [SerializeField] Sprite sprDisabled = null;

    [SerializeField] Color colorNormal = new Color(205f, 205f, 205f);
    [SerializeField] Color colorHighlighted = Color.white;
    [SerializeField] Color colorSelected = new Color(235f, 235f, 235f);
    [SerializeField] Color colorDown = new Color(172f, 172f, 172f);
    [SerializeField] Color colorDisabled = new Color(120f, 120f, 120f);

    [SerializeField] public Animator animator = null;
    [SerializeField] string animationNormal = "";
    [SerializeField] string animationHighlighted = "";
    [SerializeField] string animationSelected = "";
    [SerializeField] string animationDown = "";
    [SerializeField] string animationDisabled = "";

    [SerializeField] public UnityEvent onSelect;
    /*[HideInInspector]*/
    public ToggleGroupC toggleGroup;

    [SerializeField] public bool useFade = false;
    [SerializeField] public float fadeTime = 0.25f;
    [SerializeField] public bool isOn = false;
    [SerializeField] public bool isDisabled = false;

    private ToggleStateC lastState;
    private Image currGraphic;

    void Start()
    {
        if (toggleGroup == null)
            toggleGroup = this.GetComponentInParent<ToggleGroupC>();
        if (toggleGroup != null)
            toggleGroup.AddToggle(this);

        currGraphic = this.GetComponent<Image>();
        if (toggleType == ToggleTypeC.Color)
            if (TargetGraphic != null)
                TargetGraphic.color = Color.clear;
        if (toggleType == ToggleTypeC.Animation)
            if (animator == null)
                animator = this.GetComponent<Animator>();
        if (toggleType == ToggleTypeC.Sprite)
            if (currGraphic == null)
                currGraphic = this.GetComponent<Image>();

        if (isDisabled)
        {
            FadeTo(ToggleStateC.Disabled, true);
        }
        else
        {
            if (isOn)
                FadeTo(ToggleStateC.Selected, true);
            else
                FadeTo(ToggleStateC.Normal, true);
        }
    }

    void OnDestroy()
    {
        if (toggleGroup == null)
        {
            return;
        }

        toggleGroup.RemoveToggle(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDisabled)
            return;
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (isOn && toggleGroup.allowToggleOff)
        {
            //OnDeselect();
        }
        else
        {
            SetIsOn(true);
            onSelect?.Invoke();
        }
    }

    public void OnDeselect()
    {
        if (isDisabled)
            return;

        SetIsOn(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDisabled)
            return;

        if (lastState != ToggleStateC.Selected && lastState != ToggleStateC.Highlighted)
            FadeTo(ToggleStateC.Highlighted, false);
    }

    private void FadeTo(ToggleStateC state, bool forceInstant)
    {
        if (toggleType == ToggleTypeC.Sprite)
        {
            if (TargetGraphic == null)
            {
                Debug.LogError("[ToggleC] TargetGraphic is not set!");
                return;
            }

            //Set Initial Sprites Before Fading
            if (state == ToggleStateC.Down)
                TargetGraphic.sprite = sprDown;
            else if (state == ToggleStateC.Normal)
                TargetGraphic.sprite = sprNormal;
            else if (state == ToggleStateC.Highlighted)
                TargetGraphic.sprite = sprHighlighted;
            else if (state == ToggleStateC.Selected)
                TargetGraphic.sprite = sprSelected;
            else if (state == ToggleStateC.Disabled)
                TargetGraphic.sprite = sprDisabled;

            if (lastState == ToggleStateC.Down)
                currGraphic.sprite = sprDown;
            else if (lastState == ToggleStateC.Normal)
                currGraphic.sprite = sprNormal;
            else if (lastState == ToggleStateC.Highlighted)
                currGraphic.sprite = sprHighlighted;
            else if (lastState == ToggleStateC.Selected)
                currGraphic.sprite = sprSelected;
            else if (lastState == ToggleStateC.Disabled)
                currGraphic.sprite = sprDisabled;

            //Set Initial Alphas
            SetImageAlpha(currGraphic, 1f);
            SetImageAlpha(TargetGraphic, 0f);

            StopAllCoroutines();

            //Fade Both
            if (useFade && !forceInstant)
            {
                StartCoroutine(FadeInImg(TargetGraphic));
                StartCoroutine(FadeOutImg(currGraphic));
            }
            else //Instant
            {
                SetImageAlpha(TargetGraphic, 1f);
                SetImageAlpha(currGraphic, 0f);
            }
        }
        else if (toggleType == ToggleTypeC.Color)
        {
            if (useFade && !forceInstant) //Fade
            {
                Color currColor = currGraphic.color;
                StopAllCoroutines();
                if (state == ToggleStateC.Normal)
                    StartCoroutine(FadeColor(currGraphic, currColor, colorNormal));
                else if (state == ToggleStateC.Selected)
                    StartCoroutine(FadeColor(currGraphic, currColor, colorSelected));
                else if (state == ToggleStateC.Disabled)
                    StartCoroutine(FadeColor(currGraphic, currColor, colorDisabled));
                else if (state == ToggleStateC.Down)
                    StartCoroutine(FadeColor(currGraphic, currColor, colorDown));
                else if (state == ToggleStateC.Highlighted)
                    StartCoroutine(FadeColor(currGraphic, currColor, colorHighlighted));
            }
            else //Instant
            {
                if (state == ToggleStateC.Normal)
                    currGraphic.color = colorNormal;
                else if (state == ToggleStateC.Selected)
                    currGraphic.color = colorSelected;
                else if (state == ToggleStateC.Disabled)
                    currGraphic.color = colorDisabled;
                else if (state == ToggleStateC.Down)
                    currGraphic.color = colorDown;
                else if (state == ToggleStateC.Highlighted)
                    currGraphic.color = colorHighlighted;
            }
        }
        else if (toggleType == ToggleTypeC.Animation)
        {
            if (animator == null)
            {
                Debug.LogError($"[ToggleC] Animator is not set for {name}!");
                return;
            }

            foreach(AnimatorControllerParameter p in animator.parameters)
                if (p.type == AnimatorControllerParameterType.Trigger)
                    animator.ResetTrigger(p.name);

            if (state == ToggleStateC.Down)
                animator.SetTrigger(animationDown);
            else if (state == ToggleStateC.Normal)
                animator.SetTrigger(animationNormal);
            else if (state == ToggleStateC.Highlighted)
                animator.SetTrigger(animationHighlighted);
            else if (state == ToggleStateC.Selected)
                animator.SetTrigger(animationSelected);
            else if (state == ToggleStateC.Disabled)
                animator.SetTrigger(animationDisabled);
        }

        lastState = state;
    }

    private IEnumerator FadeColor(Image graphic, Color fromColor, Color toColor)
    {
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            graphic.color = Color.Lerp(fromColor, toColor, timer / fadeTime);
        }

        yield return null;
    }

    private IEnumerator FadeOutImg(Image graphic)
    {
        float timer = 0f;
        Color t = graphic.color;
        float startAlpha = 1f; //t.a;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float newA = Mathf.Lerp(startAlpha, 0f, timer / fadeTime);
            graphic.color = new Color(t.r, t.g, t.b, newA);
        }

        yield return null;
    }

    private IEnumerator FadeInImg(Image graphic)
    {
        float timer = 0f;
        Color t = graphic.color;
        float startAlpha = 1f; //t.a;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float newA = Mathf.Lerp(0f, startAlpha, timer / fadeTime);
            graphic.color = new Color(t.r, t.g, t.b, newA);
        }

        yield return null;
    }

    private void SetImageAlpha(Image graphic, float alpha)
    {
        Color t = graphic.color;
        graphic.color = new Color(t.r, t.g, t.b, alpha);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDisabled)
            return;

        if (!isOn)
        {
            FadeTo(ToggleStateC.Normal, false);
        }
    }

    public void SetIsOn(bool value)
    {
        if (isDisabled)
            return;

        isOn = value;
        if (isOn)
        {
            if (toggleGroup != null)
            {
                toggleGroup.SetSelected(this);
            }
            FadeTo(ToggleStateC.Selected, false);
        }
        else
        {
            FadeTo(ToggleStateC.Normal, false);
        }
    }

    public void SetDisabled(bool value)
    {
        isDisabled = value;
        if (isDisabled)
        {
            isOn = false;
            TargetGraphic?.CrossFadeAlpha(0f, 0f, false);
            FadeTo(ToggleStateC.Disabled, true);
        }
        else
        {
            FadeTo(ToggleStateC.Normal, true);
        }
    }
}

public enum ToggleTypeC
{
    Sprite = 1,
    Color = 2,
    Animation = 3
}

public enum ToggleStateC
{
    Normal = 1,
    Selected = 2,
    Highlighted = 3,
    Down = 4,
    Disabled = 5
}