using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScriptThing : MonoBehaviour
{
    [SerializeField]
    private Text debug;

    private void Start()
    {
        string teststring = "";
        teststring += "UIManager \n";
        string newObjectString;



        foreach (KeyValuePair<EUIContext, List<UIContextObject>> thing in UIManager.current._UIContexts) 
        {
            newObjectString = thing.Key.ToString() + ": - ";
            foreach (UIContextObject uICO in thing.Value) newObjectString += uICO.gameObject.name + ", ";
            newObjectString += "\n";
            teststring += newObjectString;
        }


        teststring += "PuzzleBlocks \n";
        
        newObjectString = "";

        foreach (KeyValuePair<PuzzleKeyType, PuzzleBlock> puzzleBlockObject in PuzzleObjectManager.current.puzzleBlockObjectsData.Dictionary) 
        {
            newObjectString += puzzleBlockObject.Key.ToString() + ": - " + puzzleBlockObject.Value.name + "\n";
        }

        teststring += newObjectString;


        teststring += "KeyItems \n";
        newObjectString = "";

        foreach (KeyValuePair<PuzzleKeyType, PickupableItem> keyItem in PuzzleObjectManager.current.keyObjectsData.Dictionary)
        {
            newObjectString += keyItem.Key.ToString() + ": - " + keyItem.Value.name + "\n";
        }

        teststring += newObjectString;

        debug.text = teststring;
    }
}
