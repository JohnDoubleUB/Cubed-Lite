using UnityEngine;

public abstract class ToggleObject : MonoBehaviour
{
    public bool StartingState;
    public abstract bool ToggleCurrent { get; }
    public abstract void Toggle(bool toggle, bool immediate = false);
}
