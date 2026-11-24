using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhotoFlash : MonoBehaviour
{
    [SerializeField] private Button cameraButton;
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashInTime = 0.04f;
    [SerializeField] private float flashOutTime = 0.18f;

    private Coroutine flashRoutine;

    private void Awake()
    {
        SetFlashAlpha(0f);
        cameraButton.onClick.AddListener(TakePhoto);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            TakePhoto();
        }
    }

    public void TakePhoto()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        yield return Fade(0f, 1f, flashInTime);
        yield return Fade(1f, 0f, flashOutTime);
        flashRoutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            SetFlashAlpha(Mathf.Lerp(from, to, timer / duration));
            yield return null;
        }

        SetFlashAlpha(to);
    }

    private void SetFlashAlpha(float alpha)
    {
        Color color = flashImage.color;
        color.a = alpha;
        flashImage.color = color;
    }
}
