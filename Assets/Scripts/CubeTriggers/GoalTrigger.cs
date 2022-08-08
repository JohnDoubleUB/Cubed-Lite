using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTrigger : CubeTrigger
{
    public bool GoalCovered;
    protected override bool OnCubeEnter(CubeCharacter cube)
    {
        if (!cube.hasFallen)
        {
            cube.hasReachedGoal = true;
            GoalCovered = true;
            return true;
        }
        return false;
    }

    protected override bool OnCubeExit(CubeCharacter cube)
    {
        cube.hasReachedGoal = false;
        GoalCovered = false;
        return false;
    }
}
