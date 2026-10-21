using UnityEngine;

public class DestroyAfterPlay : MonoBehaviour
{
    void Start()
    {
        // Get the total length of the default animation clip
        Animator animator = GetComponent<Animator>();
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        // Destroy this prefab object automatically after the animation finishes
        Destroy(gameObject, animationLength);
    }
}
