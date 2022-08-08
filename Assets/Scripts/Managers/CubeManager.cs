using System.Collections.Generic;
using UnityEngine;

public class CubeManager : MonoBehaviour
{
    public static CubeManager current;

    public CubeCharacter cubePrefab;
    public GameObject cubeDestructionPrefab;

    public List<CubeCharacter> ActiveCubes;
    private List<TransformData> InitialCubes = new List<TransformData>();
    public Dictionary<string, CubeCharacter> AssociatedCubes = new Dictionary<string, CubeCharacter>();

    private Transform levelParent;

    public bool AllCubesHaveFallen 
    {
        get 
        {
            foreach (CubeCharacter activeCube in ActiveCubes) 
            {
                if (!activeCube.hasFallen) return false;
            }

            return true;
        }
    }

    public bool RotationInProgress
    {
        get
        {
            foreach (CubeCharacter activeCube in ActiveCubes)
            {
                if (activeCube.hasFallen) continue;
                if (activeCube.RotateInProgress) return true;
            }

            return false;
        }
    }

    public bool ForwardMovementInProgress
    {
        get
        {
            foreach (CubeCharacter activeCube in ActiveCubes)
            {
                if (activeCube.hasFallen) continue;
                if (activeCube.ForwardMovementInProgress) return true;
            }

            return false;
        }
    }

    public bool CubeHasReachedGoal 
    {
        get 
        {
            foreach (CubeCharacter activeCube in ActiveCubes)
            {
                if (activeCube.hasFallen) continue;
                if (activeCube.hasReachedGoal) return true;
            }

            return false;
        }
    }

    private void Awake()
    {
        if (current != null) Debug.LogWarning("Oops! it looks like there might already be a " + GetType().Name + " in this scene!");
        current = this;
    }

    public void DestroyAllExisting() 
    {
        foreach (CubeCharacter activeCube in ActiveCubes)
        {
            if (activeCube != null) Destroy(activeCube.gameObject);
        }

        ActiveCubes.Clear();
    }

    public void InitializePuzzleCubes(PuzzleLevelObject levelObject, bool destroyAllExisting = false) 
    {
        levelParent = levelObject.gameObject.transform;

        if (destroyAllExisting)
        {
            DestroyAllExisting();
        }

        ActiveCubes = levelObject.ActiveCubes;
        
        AssociatedCubes.Clear();
        InitialCubes.Clear();

        foreach (CubeCharacter activeCube in ActiveCubes)
        {
            InitialCubes.Add(new TransformData() { Position = activeCube.transform.position, Rotation = activeCube.transform.rotation });
            AssociatedCubes.Add(activeCube.gameObject.name, activeCube);
        }
    }

    public void RotateCubeY(float rotate) 
    {
        foreach (CubeCharacter activeCube in ActiveCubes) 
        {
            activeCube.RotateCubeY(rotate);
        }
    }

    public void MoveCubeForward() 
    {
        List<CubeCharacter> cubesForDestruction = null;

        foreach (CubeCharacter activeCube in ActiveCubes)
        {
            activeCube.MoveCubeForward();
            
            if (activeCube.IsQueuedForDestruction) 
            {
                if (cubesForDestruction == null)
                {
                    cubesForDestruction = new List<CubeCharacter>() { activeCube };
                }
                else 
                {
                    cubesForDestruction.Add(activeCube); 
                }
            }
        }

        if (cubesForDestruction != null) 
        {
            foreach (CubeCharacter cubeForDestruction in cubesForDestruction) DestroyCube(cubeForDestruction);
        }
    }

    public CubeCharacter CreateCubeAt(Vector3 position, Quaternion rotation) 
    {
        CubeCharacter newCube = Instantiate(cubePrefab, position, rotation);
        newCube.name += "_" + ActiveCubes.Count;
        ActiveCubes.Add(newCube);
        AssociatedCubes.Add(newCube.gameObject.name, newCube);
        return newCube;
    }

    public bool DestroyCube(CubeCharacter cube) 
    {
        if (AssociatedCubes.ContainsKey(cube.name))
        {
            AssociatedCubes.Remove(cube.name);
            ActiveCubes.Remove(cube);
            Vector3 cubePosition = cube.transform.position;
            Destroy(cube.gameObject);
            Instantiate(cubeDestructionPrefab, cubePosition, Quaternion.identity);
            return true;
        }
        return false;
    }

    public void ResetCubes()
    {
        //Destroy all existing cubes
        foreach (CubeCharacter activeCube in ActiveCubes) 
        {
            Destroy(activeCube.gameObject);
        }

        ActiveCubes.Clear();
        AssociatedCubes.Clear();

        for (int i = 0; i < InitialCubes.Count; i++) 
        {
            TransformData intialCube = InitialCubes[i];
            CubeCharacter newCube = Instantiate(cubePrefab, intialCube.Position, intialCube.Rotation);
            newCube.name = newCube.name + "_" + i;
            ActiveCubes.Add(newCube);
            AssociatedCubes.Add(newCube.gameObject.name, newCube);
        }
    }
}

public class TransformData 
{
    public Vector3 Position;
    public Quaternion Rotation;
}