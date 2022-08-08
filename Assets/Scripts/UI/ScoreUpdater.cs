using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ScoreUpdater : MonoBehaviour
{
    public string ScoreString = "SCORE: ";

    [SerializeField]
    private Text ScoreText;
    private void Reset()
    {
        ScoreText = GetComponent<Text>();
    }

    private void OnEnable()
    {
        ScoreText.text = ScoreString + GameManager.current.CurrentLevelScore;
    }
}
