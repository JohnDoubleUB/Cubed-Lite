using UnityEngine;

public class ToggleButtonTrigger : CubeTrigger
{
    [SerializeField]
    private MeshRenderer meshRenderer;
    private Material buttonMaterial;

    private ParticleSystem.MainModule mainModule;

    public ToggleObject[] toggleObjects;

    public Color ToggleOff = Color.red;
    public Color ToggleOn = Color.cyan;
    public Color ParticleToggleOff = Color.red;
    public Color ParticleoggleOn = Color.cyan;


    private bool toggle;

    private void Start()
    {
        mainModule = particleEffect.main;
        buttonMaterial = meshRenderer.material;
        //buttonMaterial.color = toggle ? ToggleOff : ToggleOn;
        //mainModule.startColor = toggle ? ParticleToggleOff : ParticleoggleOn;
        ResetToggles();
    }

    protected override bool OnCubeEnter(CubeCharacter cube)
    {
        if (!cube.hasFallen) 
        {
            ToggleAll();
            return true;
        }

        return false;
    }

    protected override bool OnCubeExit(CubeCharacter cube)
    {
        return false;
    }

    public override void OnPuzzleReset()
    {
        ResetToggles();
    }

    private void ResetToggles()
    {
        foreach (ToggleObject toggleObject in toggleObjects)
        {
            toggleObject.Toggle(toggleObject.StartingState, true);
        }

        toggle = false;
        buttonMaterial.color = ToggleOff;
        mainModule.startColor = ParticleToggleOff;
    }

    private void ToggleAll(bool immediate = false)
    {
        foreach (ToggleObject toggleObject in toggleObjects)
        {
            toggleObject.Toggle(!toggleObject.ToggleCurrent, immediate);
        }

        toggle = !toggle;
        buttonMaterial.color = toggle ? ToggleOn : ToggleOff;
        mainModule.startColor = toggle ? ParticleToggleOff : ParticleoggleOn;
    }

    private void Reset()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
}
