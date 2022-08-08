using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersticialTestAdScript : MonoBehaviour
{
    int no = 1;

    public void DoAd() 
    {
        InGameNotifcationManager.current.Notify("its a test notification! x" + no);
        no++;
    }

}
