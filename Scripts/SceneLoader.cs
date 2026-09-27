using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 2f;

    bool isLoading = false;

    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(Fade(Color.black, Color.clear, fadeDuration));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(Fade(Color.black, Color.clear, fadeDuration));
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (isLoading)
            return;

        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    IEnumerator FadeAndLoadScene(string sceneName)
    {
        isLoading = true;

        yield return Fade(Color.clear, Color.black, fadeDuration);

        isLoading = false;

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator Fade(Color fromColor, Color toColor, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration); // Calculate how far along the duration we are.
            fadeImage.color = Color.Lerp(fromColor, toColor, normalizedTime); // Set the color based on the normalised time.
            yield return null;
        }
        fadeImage.color = toColor; // Ensure the fade image is exactly the right color at the end
    }
}
