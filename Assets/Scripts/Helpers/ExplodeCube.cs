using UnityEngine;
public class ExplodeCube : MonoBehaviour
{
    [SerializeField]
    private GameObject parentObject;

    [SerializeField]
    private Animator animator;


    private void Start()
    {
        animator.Play("Explode_WithDestroy");
        PuzzleObjectManager.current.CreateExplosionAtLocation(transform.position);
    }

    public void DestroyCube() 
    {
        Destroy(parentObject);
    }
}
