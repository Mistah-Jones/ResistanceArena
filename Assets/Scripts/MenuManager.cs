using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Button (3D GameObject) Prefabs
    public GameObject tutorialButtonPrefab;
    public GameObject hoardModeButtonPrefab;
    public GameObject creditsButtonPrefab;

    // Internal buttons
    private static GameObject tutorialButton;
    private static GameObject hoardModeButton;
    private static GameObject creditsButton;

    // The workout recognizer
    private static WorkoutRecognition workoutRecognizer;

    // Start is called before the first frame update
    void Start()
    {
        // Disable attacking
        GameObject workoutRecognizerComponent = GameObject.Find("VR Rig");
        workoutRecognizer = workoutRecognizerComponent.GetComponent<WorkoutRecognition>();
        workoutRecognizer.SetWorkOutEnabled(false);

        // Initialize all of the menu buttons
        tutorialButton = Instantiate(tutorialButtonPrefab);
        hoardModeButton = Instantiate(hoardModeButtonPrefab);
        creditsButton = Instantiate(creditsButtonPrefab);
    }

    public void OnTutorialButtonClick()
    {
        ClearMenu();
        GameObject.Find("Tutorial").SetActive(false);

        workoutRecognizer.SetWorkOutEnabled(true);
        workoutRecognizer.StartTutorial();
    }

    public void OnBattleButtonClick()
    {
        ClearMenu();
        GameObject.Find("HoardMode").SetActive(false);

        workoutRecognizer.SetWorkOutEnabled(true);
    }

    public void OnCreditsButtonClick()
    {
        ClearMenu();
        GameObject.Find("Credits").SetActive(false);

    }

    /// <summary>
    /// Helper method to clear all menu items
    /// </summary>
    private void ClearMenu()
    {
        tutorialButton.SetActive(false);

        hoardModeButton.SetActive(false);
        creditsButton.SetActive(false);
    }
}
