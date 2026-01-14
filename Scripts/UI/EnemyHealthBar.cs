using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image BottomBar;
    [SerializeField] private Image TopBar;
    [SerializeField] private float AnimationSpeed = 1.0f;

    //Add
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration;

    private float _maxValue;
    private float _value;
    private Coroutine _coroutine;

    //Add
    private Coroutine _fadeCoroutine;

    public void Initialize(float maxValue, float value)
    {
        _maxValue = maxValue;
        _value = value;

        //Add
        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    //Add
    public void ShowHealthBar()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        if (canvasGroup != null)
        {
            _fadeCoroutine = StartCoroutine(FadeHealthBar(1f));
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    //Add
    public void HideHealthBar()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        if (canvasGroup != null)
        {
            _fadeCoroutine = StartCoroutine(FadeHealthBar(0f));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    //Add
    private IEnumerator FadeHealthBar(float targetAlpha)
    {
        if (canvasGroup == null) yield break;

        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (targetAlpha <= 0f)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    public void Change(float delta)
    {
        _value = Mathf.Clamp(_value + delta, 0, _maxValue);

        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }

        _coroutine = StartCoroutine(ChangeBars(delta));
    }

    private IEnumerator ChangeBars(float delta)
    {
        var directChangeBar = delta >= 0 ? BottomBar : TopBar;
        var animatedChangeBar = delta >= 0 ? TopBar : BottomBar;

        var targetValue = _value / _maxValue;

        directChangeBar.fillAmount = targetValue;

        while (Mathf.Abs(animatedChangeBar.fillAmount - targetValue) > 0.01f) 
        {
            animatedChangeBar.fillAmount = Mathf.MoveTowards(animatedChangeBar.fillAmount, targetValue, Time.deltaTime * AnimationSpeed);
            yield return null;
        }

        animatedChangeBar.fillAmount = targetValue;
    }
}
