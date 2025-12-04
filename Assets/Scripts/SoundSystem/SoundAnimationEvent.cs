using SoundSystem;
using UnityEngine;

public class SoundAnimationEvent : BaseAnimationEventBehaviour
{
    public SoundDataSO soundData;
    private WwisePlayer soundPlayer;
    private GameObject selectedObject;

    protected override void OnEnter(Animator animator)
    {
        soundPlayer = animator.gameObject.GetComponent<WwisePlayer>();
        if (soundPlayer == null)
            soundPlayer = animator.gameObject.AddComponent<WwisePlayer>();
    }

    protected override void Trigger(Animator animator)
    {
        if (soundPlayer != null && soundData != null)
        {
            soundPlayer.PlaySound(soundData);
        }

        if (receiver != null)
        {
            receiver.OnAnimationEventTriggered(soundData != null ? soundData.name : "UnnamedSound");
        }
    }

#if UNITY_EDITOR
    public void DeleteComponents()
    {
        if (soundPlayer != null)
        {
            Object.DestroyImmediate(soundPlayer);
            soundPlayer = null;
        }
        receiver = null;
    }

    public void ApplySound()
    {
        selectedObject = UnityEditor.Selection.activeGameObject;
        if (selectedObject == null) return;

        soundPlayer = selectedObject.GetComponent<WwisePlayer>();
        if (soundPlayer == null)
            soundPlayer = selectedObject.AddComponent<WwisePlayer>();
    }
#endif
}