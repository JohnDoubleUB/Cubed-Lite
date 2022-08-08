using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeCharacter : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] cubeMoveSoundEffects;
    [SerializeField]
    private AudioClip[] cubeBlockedSoundEffects;

    private float totalRotateAmount = 90;
    public float currentTotalRotateAmount;
    public int rotatePoint = 0;
    public float rotateSpeed = 100;

    public bool hasReachedGoal;
    public bool hasFallen;

    [SerializeField]
    private Rigidbody rb;

    private LayerMask groundMask;
    private LayerMask blockingObjectMask;

    public bool ForwardMovementInProgress { get { return forwardMovementInProgress; } }
    private bool forwardMovementInProgress;

    private float rotateTargetY;
    private float currentTotalYRotation;
    public bool RotateInProgress { get { return rotateInProgress; } }
    private bool rotateInProgress;
    private bool rotIsNegative;

    private Vector3 _rotatePointLocation;
    private Vector3 _nextRotatePoint;

    private Dictionary<PuzzleKeyType, List<PickupableItem>> keyObjectDictionary = new Dictionary<PuzzleKeyType, List<PickupableItem>>();

    public bool IsQueuedForDestruction { get { return queuedForDestruction; } }

    private bool queuedForDestruction;

    private bool couldntMoveLastAttempt;

    private int keyObjectCount 
    { 
        get 
        {
            int count = 0;
            foreach (KeyValuePair<PuzzleKeyType, List<PickupableItem>> kVP in keyObjectDictionary) count += kVP.Value.Count;
            return count;
        } 
    }

    // Start is called before the first frame update
    void Start()
    {
        groundMask = LayerMask.GetMask("Ground");
        blockingObjectMask = LayerMask.GetMask("PuzzleBlock", "Player");
        
        foreach (PuzzleKeyType keyType in (PuzzleKeyType[])Enum.GetValues(typeof(PuzzleKeyType))) 
        {
            keyObjectDictionary.Add(keyType, new List<PickupableItem>());
        }
    }

    private void Update()
    {
        _rotatePointLocation = GetRelativeRotationCorner(rotatePoint);
        _nextRotatePoint = GetRelativeRotationCorner((rotatePoint + 1) % 4);

        bool currentFallStatus = CheckIfFallen(_nextRotatePoint);


        if (currentFallStatus != hasFallen)
        {
            hasFallen = currentFallStatus;

            if (hasFallen)
            {
                rb.isKinematic = false;
                Vector3 forcePosition = GetRelativeRotationCorner((rotatePoint + 2) % 4);
                Vector3 forceDirection = (transform.position - _nextRotatePoint).normalized;
                forceDirection.y = 0;
                rb.AddForceAtPosition(-forceDirection * 1000, forcePosition);
            }
        }


        if (!hasFallen)
        {
            RotateForwardUpdate(_rotatePointLocation);
            RotateYUpdate();
        }
        else
        {
            forwardMovementInProgress = false;
        }
    }

    private bool CanMoveForward(Vector3 rotatePointLocation)
    {
        Vector3 trueForward = (transform.position - new Vector3(rotatePointLocation.x, transform.position.y, rotatePointLocation.z)).normalized;

        if (Physics.Linecast(transform.position, transform.position + (trueForward * -0.6f), out RaycastHit hit, blockingObjectMask))
        {
            if (hit.collider.gameObject.tag == "Player") 
            {
                if (CubeManager.current.AssociatedCubes.ContainsKey(hit.collider.gameObject.name)) 
                {
                    CubeCharacter otherCube = CubeManager.current.AssociatedCubes[hit.collider.gameObject.name];
                    return otherCube.CanMoveForward(this);
                }
            }
            else if (PuzzleObjectManager.current.AssociatedBlockObjects.ContainsKey(hit.collider.gameObject.name))
            {
                PuzzleBlock currentLockBlock = PuzzleObjectManager.current.AssociatedBlockObjects[hit.collider.gameObject.name];
      
                int keyObjectCount = keyObjectDictionary[currentLockBlock.RequiredKeyType].Count;

                if (keyObjectCount > 0)
                {
                    PickupableItem lastKey = keyObjectDictionary[currentLockBlock.RequiredKeyType][keyObjectCount - 1];
                    keyObjectDictionary[currentLockBlock.RequiredKeyType].RemoveAt(keyObjectCount - 1);
                    currentLockBlock.GiveKey(lastKey, this);
                    KeyObjectOffsets();
                    return true;
                }
                else 
                {
                    currentLockBlock.OnFailedInteraction();
                }
            }

            return false;
        }
        else 
        {
            return true;
        }
    }

    public bool CanMoveForward(CubeCharacter cubeChecking)
    {
        Vector3 rotatePointLocation = _nextRotatePoint;
        Vector3 trueForward = (transform.position - new Vector3(rotatePointLocation.x, transform.position.y, rotatePointLocation.z)).normalized;

        if (Physics.Linecast(transform.position, transform.position + (trueForward * -0.6f), out RaycastHit hit, blockingObjectMask))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                if (CubeManager.current.AssociatedCubes.ContainsKey(hit.collider.gameObject.name))
                {
                    CubeCharacter otherCube = CubeManager.current.AssociatedCubes[hit.collider.gameObject.name];
                    
                    return cubeChecking.name == otherCube.name ? false : otherCube.CanMoveForward(this);
                }
            }
            if (PuzzleObjectManager.current.AssociatedBlockObjects.ContainsKey(hit.collider.gameObject.name))
            {
                PuzzleBlock currentLockBlock = PuzzleObjectManager.current.AssociatedBlockObjects[hit.collider.gameObject.name];

                int keyObjectCount = keyObjectDictionary[currentLockBlock.RequiredKeyType].Count;

                if (keyObjectCount > 0)
                {
                    return true;
                }
            }

            return false;
        }
        else
        {
            return true;
        }
    }

    public bool MoveCubeForward()
    {
        if (CanMoveForward(_nextRotatePoint))
        {
            forwardMovementInProgress = true;
            rotatePoint = (rotatePoint + 1) % 4;
            currentTotalRotateAmount = 0;
            couldntMoveLastAttempt = false;
            return true;
        }

        if (!couldntMoveLastAttempt)
        {
            transform.PlayClipAtTransform(cubeBlockedSoundEffects[UnityEngine.Random.Range(0, cubeBlockedSoundEffects.Length)], false, 0.5f);
            couldntMoveLastAttempt = true;
        }

        return false;
    }

    public bool RotateCubeY(float rotate) 
    {
        rotateInProgress = true;
        rotIsNegative = rotate < 0;
        rotateTargetY = Mathf.Abs(rotate);
        currentTotalYRotation = 0;
        return true;
    }

    private bool CheckIfFallen(Vector3 rotatePointLocation)
    {
        rotatePointLocation += Vector3.up * 0.1f;
        if (!hasFallen)
        {
            if (!Physics.Raycast(rotatePointLocation, Vector3.down, 3f, groundMask))
            {
                return true;
            }
            else
            {
                Debug.DrawRay(rotatePointLocation, Vector3.down * 4f, Color.yellow);
            }
        }


        return hasFallen;
    }

    private bool RotateForwardUpdate(Vector3 rotatePointLocation)
    {
        if (currentTotalRotateAmount == totalRotateAmount)
        {
            if (forwardMovementInProgress) forwardMovementInProgress = false;
            return true;
        }

        float currentRotateAmount = rotateSpeed * Time.deltaTime;
        if (currentRotateAmount + currentTotalRotateAmount > totalRotateAmount)
        {
            currentRotateAmount = (totalRotateAmount - currentTotalRotateAmount);
            currentTotalRotateAmount = totalRotateAmount;
            transform.PlayClipAtTransform(cubeMoveSoundEffects[UnityEngine.Random.Range(0, cubeMoveSoundEffects.Length)], false, 0.2f);
        }
        else
        {
            currentTotalRotateAmount += currentRotateAmount;
        }


        transform.RotateAround(rotatePointLocation, transform.right, currentRotateAmount);

        return false;
    }

    private bool RotateYUpdate() 
    {
        if (currentTotalYRotation == rotateTargetY)
        {
            if (rotateInProgress) rotateInProgress = false;
            return true;
        }

        float currentRotateAmount = rotateSpeed * Time.deltaTime;
        if (currentRotateAmount + currentTotalYRotation > rotateTargetY)
        {
            currentRotateAmount = (rotateTargetY - currentTotalYRotation);
            currentTotalYRotation = rotateTargetY;
        }
        else
        {
            currentTotalYRotation += currentRotateAmount;
        }


        transform.RotateAround(transform.position, Vector3.up, rotIsNegative ? -currentRotateAmount : currentRotateAmount);

        return false;
    }


    private Vector3 GetRelativeRotationCorner(int index, bool withCurrentTransform = true)
    {
        Vector3 result;

        switch (index % 4)
        {
            case 0:
                result = (transform.up * -0.5f) + (transform.forward * 0.5f);
                break;
            case 1:
                result = (transform.up * 0.5f) + (transform.forward * 0.5f);
                break;
            case 2:
                result = (transform.up * 0.5f) + (transform.forward * -0.5f);
                break;
            default:
            case 3:
                result = (transform.up * -0.5f) + (transform.forward * -0.5f);
                break;
        }

        return withCurrentTransform ? transform.position + result : result;
    }

    public int AddKeyObject(PickupableItem keyObject) 
    {
        keyObjectDictionary[keyObject.ObjectType].Add(keyObject);
        return keyObjectCount;
    }

    private int KeyObjectOffsets() 
    {
        int count = 1;
        bool isAny = false;
        foreach (KeyValuePair<PuzzleKeyType, List<PickupableItem>> kVP in keyObjectDictionary)
        {
            foreach (PickupableItem keyObj in kVP.Value) 
            {
                isAny = true;
                keyObj.SetTrueOffset(count);
                count++;
            }
        }

        return isAny ? count : 0;
    }

    public void QueueForDestruction() 
    {
        queuedForDestruction = true;
    }
}
