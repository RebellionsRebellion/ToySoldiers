using EditorAttributes;
using PrimeTween;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool Ingame { get; private set; }
    
    #region Singleton
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
        
        Init();
    }
    #endregion

    private void Init()
    {
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
        PrimeTweenConfig.warnTweenOnDisabledTarget = false;
        PrimeTweenConfig.warnZeroDuration = false;
    }
    [Button]
    public void LoadGame()
    {
        TransitionManager.TransitionScene(TransitionManager.SceneTypes.Ingame, StartGame);
    }

    public void StartGame()
    {
        Ingame = true;
        
        print("Game Started");
    }
    [Button]
    public void EndGame()
    {
        Ingame = false;
        
        print("Game Ended");
        
        TransitionManager.TransitionScene(TransitionManager.SceneTypes.MainMenu);

    }
    
}
