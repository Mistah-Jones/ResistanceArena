/*
 * 3D Gesture Recognition - Unity Plug-In
 * 
 * Copyright (c) 2021 MARUI-PlugIn (inc.)
 * This software is free to use for non-commercial purposes.
 * You may use this software in part or in full for any project
 * that does not pursue financial gain, including free software 
 * and projectes completed for evaluation or educational purposes only.
 * Any use for commercial purposes is prohibited.
 * You may not sell or rent any software that includes
 * this software in part or in full, either in it's original form
 * or in altered form.
 * If you wish to use this software in a commercial application,
 * please contact us at support@marui-plugin.com to obtain
 * a commercial license.
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, 
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY 
 * OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.Networking;


public class WorkoutRecognition : MonoBehaviour
{
    // Crate members for training mode
    public GameObject crate;
    private static GameObject internalCrate;
    private static List<GameObject> crates;
    private int cratesLeft;

    // Projectile Objects
    private GameObject bcStrike;
    private GameObject ohpAttack;
    //public ParticleSystem system;

    // Convenience ID's for the "left" and "right" sub-gestures.
    public const int Side_Left = 0;
    public const int Side_Right = 1;

    // Whether the user is currently pressing the contoller trigger.
    private bool trigger_pressed_left = false;
    private bool trigger_pressed_right = false;

    // Wether a gesture was already started and if it is being performed by both hands
    private bool gesture_started = false;
    //private bool twoHands = false;

    // The gesture recognition object 
    private GestureCombinations gc = new GestureCombinations(2);

    // The text field to display instructions.
    private Text HUDText;
    //private Text CoolDown;
    //private Text BCLCoolDown;
    //private Text OHPCoolDown;

    private float timer = 0.0f;

    // Temporary storage for objects to display the gesture stroke.
    List<string> stroke = new List<string>();

    // Temporary counter variable when creating objects for the stroke display:
    int tutorialIndex = 0;

    // Indicates whether workout recognition should be active
    private static bool isEnabled;
    // Indicates whether tutorial mode is active
    private static bool isTutorial;
    // The number of unique implemented attacks
    private static int numAttacks;

    // Initialization:
    void Start()
    {
        // Initialize GameObjects

        // Set the welcome message.
        HUDText = GameObject.Find("HUDText").GetComponent<Text>();
        bcStrike = GameObject.Find("BC Strike");
        ohpAttack = GameObject.Find("OHP Attack");
        numAttacks = 2;

        // Set the Cooldown HUD
        //CoolDown = GameObject.Find("CoolDownText").GetComponent<Text>();
        //CoolDown.text = "" + timer;

        // Load the default set of gestures.
        // When running the scene inside the Unity editor,
        // we can just load the file from the Assets/ folder:
        string gestureFilePath = "Assets/GestureRecognition/Sample_TwoHanded_MyGestures.dat";

        int ret = gc.loadFromFile(gestureFilePath);
        if (ret != 0)
        {
            byte[] fileContents = File.ReadAllBytes(gestureFilePath);
            if (fileContents == null || fileContents.Length == 0)
            {
                HUDText.text = $"Could not find gesture database file ({gestureFilePath}).";
                return;
            }
            ret = gc.loadFromBuffer(fileContents);

            if (ret != 0)
            {
                HUDText.text = $"Failed to load sample gesture database file ({ret}).";
                return;
            }
        }
    }


    // Update:
    void Update()
    {
        // First update the cooldown timer
        //if (timer > 0.0)
        //{
        //    timer -= Time.deltaTime;
        //    CoolDown.text = (timer % 60).ToString("0");
        //}
        //else
        //{
        //    CoolDown.text = "0";
        //}

        float escape = Input.GetAxis("escape");
        if (escape > 0.0f)
        {
            Application.Quit();
        }
        if (isEnabled)
        {
            // TODO Make into seperate methods (classes?)
            if (!isTutorial)
            {
                float trigger_left = Input.GetAxis("LeftControllerTrigger");
                float trigger_right = Input.GetAxis("RightControllerTrigger");

                GameObject hmd = GameObject.Find("VR Camera");
                Vector3 hmd_p = hmd.transform.position;
                Quaternion hmd_q = hmd.transform.rotation;

                // If the user presses either controller's trigger, we start a new gesture.
                if (trigger_pressed_left == false && trigger_left > 0.9)
                {
                    // Controller trigger pressed.
                    trigger_pressed_left = true;
                    gc.startStroke(Side_Left, hmd_p, hmd_q);
                    gesture_started = true;
                }
                if (trigger_pressed_right == false && trigger_right > 0.9)
                {
                    // Controller trigger pressed.
                    trigger_pressed_right = true;
                    gc.startStroke(Side_Right, hmd_p, hmd_q);
                    gesture_started = true;
                }
                if (gesture_started == false)
                {
                    // nothing to do.
                    return;
                }

                // If we arrive here, the user is currently dragging with one of the controllers.
                if (trigger_pressed_left == true)
                {
                    if (trigger_left < 0.85)
                    {
                        // User let go of a trigger and held controller still
                        gc.endStroke(Side_Left);
                        trigger_pressed_left = false;
                    }
                    else
                    {
                        // User still dragging or still moving after trigger pressed
                        GameObject left_hand = GameObject.Find("Left Hand");
                        gc.contdStrokeQ(Side_Left, left_hand.transform.position, left_hand.transform.rotation);
                        // Show the stroke by instatiating new objects
                        GameObject left_hand_pointer = GameObject.FindGameObjectWithTag("Left Pointer");
                    }
                }

                if (trigger_pressed_right == true)
                {
                    if (trigger_right < 0.85)
                    {
                        // User let go of a trigger and held controller still
                        gc.endStroke(Side_Right);
                        trigger_pressed_right = false;
                    }
                    else
                    {
                        // User still dragging or still moving after trigger pressed
                        GameObject right_hand = GameObject.Find("Right Hand");
                        gc.contdStrokeQ(Side_Right, right_hand.transform.position, right_hand.transform.rotation);

                        // Show the stroke by instatiating new objects
                        GameObject right_hand_pointer = GameObject.FindGameObjectWithTag("Right Pointer");
                    }
                }

                if (trigger_pressed_left || trigger_pressed_right)
                {
                    // User still dragging with either hand
                    return;
                }
                // else: if we arrive here, the user let go of both triggers, ending the gesture.
                gesture_started = false;

                double similarity = -1.0;

                int gestureId = gc.identifyGestureCombination(ref similarity);

                if (gestureId < 0)
                {
                    HUDText.text = "Failed to identify gesture";
                    return; // something went wrong
                }

                string gestureName = gc.getGestureCombinationName(gestureId);

                if (similarity < /*0.46*/0.3)
                {
                    HUDText.text = "Gesture not fully recognized\nWere you trying to perform a(n): " + gestureName + "?\nAccuracy Score: " + similarity;
                }
                else
                {
                    if (gestureName.Contains("bc"))
                    {
                        if (gestureName.Contains("left"))
                        {
                            HUDText.text = "You performed a left-handed bicep curl!";
                        }
                        else
                        {
                            HUDText.text = "You performed a right-handed bicep curl!";
                        }

                        bcStrike.transform.position = new Vector3
                            (
                                hmd_p.x + (hmd.transform.forward.x * 1.5f),
                                hmd_p.y + (hmd.transform.forward.y * 1.5f),
                                hmd_p.z + (hmd.transform.forward.z * 1.5f)
                            );
                        bcStrike.transform.rotation = hmd_q;
                        bcStrike.GetComponent<Rigidbody>().velocity = hmd.transform.forward * 6.5f;
                        bcStrike.GetComponent<Rigidbody>().angularVelocity = new Vector3();
                    }

                    if (gestureName.Contains("ohp"))
                    {
                        HUDText.text = "You performed an overhead press!";

                        ohpAttack.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                        ohpAttack.transform.position = new Vector3
                            (
                                hmd_p.x + (hmd.transform.forward.x * 1.5f),
                                hmd_p.y + (hmd.transform.forward.y * 1.5f),
                                hmd_p.z + (hmd.transform.forward.z * 1.5f)
                            );
                        ohpAttack.transform.rotation = hmd_q;
                        ohpAttack.GetComponent<Rigidbody>().velocity = new Vector3
                            (
                                hmd.transform.forward.x * 4.5f,
                                (hmd.transform.forward.y + 5f) * 2f,
                                hmd.transform.forward.z * 4.5f
                            );
                        ohpAttack.GetComponent<Rigidbody>().angularVelocity = new Vector3(1.5f, 1f, 0f);
                    }
                }
            }
            else
            {
                TutorialWalkthrough();
            }
        }
    }

    public void SetWorkOutEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    public void StartTutorial()
    {
        HUDText.text = "Welcome to Resistance Arean (RA)!\n"
                      + "RA is a VR Exercise game where doing exercises is how you attack!\n"
                      + "If you have wrist weights, attach them now.\n"
                      + "Press the trigger of either controller to scroll the menu.";
                      //+ "Press triggers of either or both controllers\n"
                      //+ "and perform an exercise.";

        isTutorial = true;
    }

    private void TutorialWalkthrough()
    {
        float trigger_left = Input.GetAxis("LeftControllerTrigger");
        float trigger_right = Input.GetAxis("RightControllerTrigger");

        GameObject hmd = GameObject.Find("VR Camera");
        Vector3 hmd_p = hmd.transform.position;
        Quaternion hmd_q = hmd.transform.rotation;

        // If the user presses either controller's trigger, we start a new gesture.
        if (trigger_pressed_left == false && trigger_left > 0.9)
        {
            // Controller trigger pressed.
            trigger_pressed_left = true;
            gc.startStroke(Side_Left, hmd_p, hmd_q);
            gesture_started = true;
        }
        if (trigger_pressed_right == false && trigger_right > 0.9)
        {
            // Controller trigger pressed.
            trigger_pressed_right = true;
            gc.startStroke(Side_Right, hmd_p, hmd_q);
            gesture_started = true;
        }
        if (gesture_started == false)
        {
            // nothing to do.
            return;
        }

        // If we arrive here, the user is currently dragging with one of the controllers.
        if (trigger_pressed_left == true)
        {
            if (trigger_left < 0.85)
            {
                // User let go of a trigger and held controller still
                gc.endStroke(Side_Left);
                trigger_pressed_left = false;
            }
            else
            {
                // User still dragging or still moving after trigger pressed
                GameObject left_hand = GameObject.Find("Left Hand");
                gc.contdStrokeQ(Side_Left, left_hand.transform.position, left_hand.transform.rotation);
                // Show the stroke by instatiating new objects
                GameObject left_hand_pointer = GameObject.FindGameObjectWithTag("Left Pointer");
            }
        }

        if (trigger_pressed_right == true)
        {
            if (trigger_right < 0.85)
            {
                // User let go of a trigger and held controller still
                gc.endStroke(Side_Right);
                trigger_pressed_right = false;
            }
            else
            {
                // User still dragging or still moving after trigger pressed
                GameObject right_hand = GameObject.Find("Right Hand");
                gc.contdStrokeQ(Side_Right, right_hand.transform.position, right_hand.transform.rotation);

                // Show the stroke by instatiating new objects
                GameObject right_hand_pointer = GameObject.FindGameObjectWithTag("Right Pointer");
            }
        }

        if (trigger_pressed_left || trigger_pressed_right)
        {
            // User still dragging with either hand
            return;
        }
        // else: if we arrive here, the user let go of both triggers, ending the gesture.
        gesture_started = false;

        double similarity = -1.0;

        int gestureId = gc.identifyGestureCombination(ref similarity);

        if (gestureId < 0)
        {
            HUDText.text = "Failed to identify gesture, please try again";
            return; // something went wrong
        }

        string gestureName = gc.getGestureCombinationName(gestureId);

        // First step of tutorial (Moving to next menu)
        switch (tutorialIndex)
        {
            case 0:
                HUDText.text = $"There are {numAttacks} attacks available to you in this game:\n" +
                    $"Fire Strike: Done by completing a bicep curl with the right or left hand\n" +
                    $"Stone Hurl: Done by completing an overhead press with both hands";
                tutorialIndex++;
                break;
            case 1:
                HUDText.text = $"Let's begin by performing a bicep curl with your right hand.\n" +
                    $"Start with your hand at your hip, press the trigger, and perform one full bicep curl\n" +
                    $"from start to finish.";
                SpawnTargets();
                tutorialIndex++;
                break;
            case 2:
                if (!gestureName.Contains("right"))
                {
                    HUDText.text = "Please perform the bicep curl with just your right hand";
                }
                else if (similarity < 0.35)
                {
                    HUDText.text = "Gesture not fully recognized \n Please try the bicep curl again in a slow and controlled manner.";
                }
                else
                {
                    HUDText.text = "Excellent work!\n" +
                        "You just performed the Fire Strike! This fireball will travel until it hits something.\n" +
                        "This fast moving projectile will be helpful for hitting far away enemies, or when you need a quick burst of damage.";

                    bcStrike.transform.position = new Vector3
                        (
                            hmd_p.x + (hmd.transform.forward.x * 1.5f),
                            hmd_p.y + (hmd.transform.forward.y * 1.5f),
                            hmd_p.z + (hmd.transform.forward.z * 1.5f)
                        );
                    bcStrike.transform.rotation = hmd_q;
                    bcStrike.GetComponent<Rigidbody>().velocity = hmd.transform.forward * 6.5f;
                    bcStrike.GetComponent<Rigidbody>().angularVelocity = new Vector3();
                    tutorialIndex++;
                }
                break;
            case 3:
                HUDText.text = "Now please try performing the bicep curl with your left hand.";
                tutorialIndex++;
                break;
            case 4:
                if (!gestureName.Contains("left"))
                {
                    HUDText.text = "Please perform the bicep curl with just your left hand";
                }
                else if (similarity < 0.35)
                {
                    HUDText.text = "Gesture not fully recognized \n Please try the bicep curl again in a slow and controlled manner.";
                }
                else
                {
                    HUDText.text = "Excellent work!\n" +
                        "It is important to make sure that you are getting a good mix of right and left arm exercises.";

                    bcStrike.transform.position = new Vector3
                        (
                            hmd_p.x + (hmd.transform.forward.x * 1.5f),
                            hmd_p.y + (hmd.transform.forward.y * 1.5f),
                            hmd_p.z + (hmd.transform.forward.z * 1.5f)
                        );
                    bcStrike.transform.rotation = hmd_q;
                    bcStrike.GetComponent<Rigidbody>().velocity = hmd.transform.forward * 6.5f;
                    bcStrike.GetComponent<Rigidbody>().angularVelocity = new Vector3();
                    tutorialIndex++;
                }
                break;
            case 5:
                HUDText.text = "Lastly, let's perform an overhead press.\n" +
                    "Start with your hands in front of your shoulders, and press the trigger button on both controllers. " +
                    "Press your hands over your head until your elbows have locked. " +
                    "Lower your arms back to in front of your shoulders.";
                tutorialIndex++;
                break;
            case 6:
                if (!gestureName.Contains("ohp"))
                {
                    HUDText.text = "Please perform the overhead press with both of your hands";
                }
                else if (similarity < 0.35)
                {
                    HUDText.text = "Gesture not fully recognized \n Please try the overhead press again in a slow and controlled manner.";
                }
                else
                {
                    HUDText.text = "Excellent work!\n" +
                        "You just performed the Stone Hurl! This stone will travel until it hits the ground. " +
                        "This slow moving projectile will deal high damage to any enemies it makes direct contact with, and " +
                        "sends out a shockwave to deal damage to all enemies in the area.";

                    ohpAttack.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    ohpAttack.transform.position = new Vector3
                        (
                            hmd_p.x + (hmd.transform.forward.x * 1.5f),
                            hmd_p.y + (hmd.transform.forward.y * 1.5f),
                            hmd_p.z + (hmd.transform.forward.z * 1.5f)
                        );
                    ohpAttack.transform.rotation = hmd_q;
                    ohpAttack.GetComponent<Rigidbody>().velocity = new Vector3
                        (
                            hmd.transform.forward.x * 4.5f,
                            (hmd.transform.forward.y + 5f) * 2f,
                            hmd.transform.forward.z * 4.5f
                        );
                    ohpAttack.GetComponent<Rigidbody>().angularVelocity = new Vector3(1.5f, 1f, 0f);

                    tutorialIndex++;
                }
                break;
            case 7:
                HUDText.text = "You are now ready to practice your skills and start the game!\n" +
                    "Destroy all of the crates to complete the tutorial.";
                tutorialIndex++;
                break;
            case 8:
                HUDText.text = "";
                // TODO Spawn Targets
                tutorialIndex++;
                break;
                // TODO add case that remains until all targets are broken.
            default:
                isTutorial = false;
                return;

        }
    }

    private void SpawnTargets()
    {
        /*
         * TODO The variable crate of WorkoutRecognition has not been assigned. ERROR
        var pos = new Vector3(-0.05f, 1.3f, 16.58f);
        var rot = new Quaternion();
        Instantiate(crate, pos, rot);
        pos = new Vector3(6.34f, 4.42f, 16.58f);
        Instantiate(crate, pos, rot);
        pos = new Vector3(-8.39f, 7.88f, 16.58f);
        Instantiate(crate, pos, rot);
        */
    }
}