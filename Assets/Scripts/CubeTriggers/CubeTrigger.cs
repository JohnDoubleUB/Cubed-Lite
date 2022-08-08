using UnityEngine;
public abstract class CubeTrigger : MonoBehaviour
{
    [SerializeField]
    protected ParticleSystem particleEffect;

    [SerializeField]
    private AudioClip OnTriggerEnterSoundEffect;

    [SerializeField]
    private float Volume = 0.5f;

    [SerializeField]
    private float Pitch = 1f;

    [SerializeField]
    private bool PitchVariation = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {

            if (CubeManager.current.AssociatedCubes.ContainsKey(other.gameObject.name))
            {

                if (OnCubeEnter(CubeManager.current.AssociatedCubes[other.gameObject.name]))
                {
                    if (particleEffect != null) particleEffect.Play();
                    if (OnTriggerEnterSoundEffect != null) transform.PlayClipAtTransform(OnTriggerEnterSoundEffect, false, Volume, PitchVariation, 0, Pitch);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (CubeManager.current.AssociatedCubes.ContainsKey(other.gameObject.name))
            {
                OnCubeExit(CubeManager.current.AssociatedCubes[other.gameObject.name]);
            }
        }
    }

    protected abstract bool OnCubeEnter(CubeCharacter cube);
    protected abstract bool OnCubeExit(CubeCharacter cube);

    public virtual void OnPuzzleReset() { }
}
