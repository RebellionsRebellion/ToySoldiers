using AYellowpaper.SerializedCollections;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using PrimeTween;
using UnityEngine.SceneManagement;
using EditorAttributes;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    [SerializeField] private SerializedDictionary<SceneTypes, SceneReference> scenes;

    [Header("Animation")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInTime = 1;
    [SerializeField] private Ease fadeInEase;
    [SerializeField] private float fadeOutTime = 1;
    [SerializeField] private Ease fadeOutEase;
    [SerializeField] private float waitTime;

    private Tween fadeInTween;
    private Tween fadeOutTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        Init();
    }

    private void Init()
    {
        canvasGroup.alpha = 0.0f;
    }


    public static void TransitionScene(SceneTypes sceneType)
    {
        SceneReference scene = GetSceneReference(sceneType);

        Instance.StartTransition(scene);

    }
    public static SceneReference GetSceneReference(SceneTypes type)
    {
        return Instance.scenes.GetValueOrDefault(type);
    }

    public void StartTransition(SceneReference scene)
    {
        StartCoroutine(TransitioningCoroutine(scene));
    }

    private IEnumerator TransitioningCoroutine(SceneReference scene)
    {
        fadeInTween = Tween.Alpha(canvasGroup, 1.0f, fadeInTime);

        yield return new WaitForSeconds(fadeInTime);

        var sceneLoading = SceneManager.LoadSceneAsync(scene.ScenePath);
        while (!sceneLoading.isDone)
            yield return null;

        yield return new WaitForSeconds(waitTime);

        fadeOutTween = Tween.Alpha(canvasGroup, 0.0f, fadeOutTime);

    }


    [Button]
    public void DebugLoadGameScene()
    {
        TransitionScene(SceneTypes.Ingame);
    }



    public enum SceneTypes
    {
        MainMenu,
        Ingame
    }

}
