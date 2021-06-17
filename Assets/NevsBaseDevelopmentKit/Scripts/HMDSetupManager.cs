using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class HMDSetupManager : MonoBehaviour
{
    public enum RigType
    {
        XR, PureFlatscreen, DroneCamera
    }

    private static HMDSetupManager instance;

    // Unity Component Feilds
    [Header("Startup Rig Type")]
    [SerializeField]
    [Tooltip("This is the rig type that will be set up as soon as the game loads. XR can run three different modes: Standalone -> SteamVR / XR Flatscreen , Andriod -> Quest")]
    private RigType DefaultRig = RigType.XR;

    [Header("Rigs")]
    [SerializeField]
    [Tooltip("This is the VR rig. It's used for gameplay in vr and pretending to be in vr on flatscreen")]
    private GameObject XRRig;
    [SerializeField]
    [Tooltip("This is the Flatcreen rig used for gameplay without vr")]
    private GameObject PureFlatscreenRig;
    [SerializeField]
    [Tooltip("This is the Drone Camera user for flying around the scene without any of the built in gameplay.")]
    private GameObject DroneCameraRig;

    [Header("SteamVR Input Managers")]
    [SerializeField]
    [Tooltip("An object for enabling and disabling Behavoir pose scripts (these are what manage the steamvr inputs).")]
    private GameObject SteamVRBehaviorObjects;

    [Header("XR Flatscreen Tools")]
    [SerializeField]
    [Tooltip("An object for enabling extra flatscreen features such as a camera script and hud.")]
    private GameObject XRFlatscreenBehaviorObjects;
    [SerializeField]
    [Tooltip("Data object that defines how to read in keyboard as XR device (Create a new binding with the create menu \"XR/KeyboardBinding\").")]
    private KeyboardBinding KeyboardControlls;


    private RigType currentRig;

    /// <summary>
    /// We need to call this setup before everything else.
    /// </summary>
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There are multiple instances of HMDSetupManager in the scene (only one is allowed");
            GameObject.Destroy(this);
        }
        else
            instance = this;
        
        if (DefaultRig == RigType.XR)
            if(!XRSettings.isDeviceActive)
            {
                Debug.Log("HMD Manager Init: Using XR with No Headset is connected. Loading inputs... ");
            } else
            {
                Debug.Log("HMD Manager Init: Using XR with a headset -> \"" + XRSettings.loadedDeviceName + "\". Loading inputs... ");
            }
        else
        {
            Debug.Log("HMD Manager Init: Not using XR. Loading other rigs... ");
        }

        currentRig = DefaultRig;
        FixCurrentRig();
        SetKeyboardBindingInput();
    }

    public void SwitchRig(RigType rigType)
    {
        if (rigType == currentRig)
            return;

        RigEnabled(rigType, true);
        RigEnabled(currentRig, false);
        currentRig = rigType;

        FixDeviceInputTo(currentRig);
    }

    private void RigEnabled(RigType rigType, bool enabled)
    {
        GameObject rig = null;
        switch (rigType)
        {
            case RigType.XR:
                rig = XRRig;
                break;
            case RigType.PureFlatscreen:
                rig = PureFlatscreenRig;
                break;
            case RigType.DroneCamera:
                rig = DroneCameraRig;
                break;
            default:
                Debug.LogError("HMD Manager could not find a rig associated with rig type " + rigType);
                return;
        }

        rig.SetActive(enabled);
    }

    private void FixDeviceInputTo(RigType rigType)
    {
        switch (rigType)
        {
            case RigType.XR:
                if (!XRSettings.isDeviceActive)
                {
                    InputDeviceWrapper.ForceIsFlatscreen(true);
                    InputDeviceWrapper.ForceIsSteamVR(false);
                    SteamVRBehaviorObjects.SetActive(false);
                    XRFlatscreenBehaviorObjects.SetActive(true);
                    Debug.Log(" -- XR Flatscreen Inputs are loaded as main input.");
                    return;
                }
                else if (XRSettings.loadedDeviceName == "OpenVR Display")
                {
                    InputDeviceWrapper.ForceIsFlatscreen(false);
                    InputDeviceWrapper.ForceIsSteamVR(true);
                    SteamVRBehaviorObjects.SetActive(true);
                    XRFlatscreenBehaviorObjects.SetActive(false);
                    Debug.Log(" -- SteamVR Inputs are loaded as main input");
                }
                else
                {
                    InputDeviceWrapper.ForceIsFlatscreen(false);
                    InputDeviceWrapper.ForceIsSteamVR(true);
                    SteamVRBehaviorObjects.SetActive(false);
                    XRFlatscreenBehaviorObjects.SetActive(false);
                    Debug.Log(" -- XR Inputs are loaded as main input.");
                }
                break;

            case RigType.PureFlatscreen:
                InputDeviceWrapper.ForceIsFlatscreen(false);
                InputDeviceWrapper.ForceIsSteamVR(false);
                SteamVRBehaviorObjects.SetActive(false);
                XRFlatscreenBehaviorObjects.SetActive(false);
                Debug.Log(" -- Pure Flatscreen is loaded. XR rig is disabled and XR Inputs are loaded as main input.");
                break;

            case RigType.DroneCamera:
                InputDeviceWrapper.ForceIsFlatscreen(false);
                InputDeviceWrapper.ForceIsSteamVR(false);
                SteamVRBehaviorObjects.SetActive(true);
                XRFlatscreenBehaviorObjects.SetActive(false);
                Debug.Log(" -- The Drone Camera Rig is loaded. XR rig is disabled and XR Inputs are loaded as main input.");
                break;
        }
    }

    public void FixCurrentRig()
    {
        foreach(RigType rigType in (RigType[])Enum.GetValues(typeof(RigType)))
            RigEnabled(rigType, rigType == currentRig);
        
        FixDeviceInputTo(currentRig);
    }

    public static void SetKeyboardBindingInput(KeyboardBinding keyboardBinding)
    {
        instance.KeyboardControlls = keyboardBinding;
        SetKeyboardBindingInput();
    }

    private static void SetKeyboardBindingInput()
    {
        InputDeviceWrapper.SetKeyboardInputs(instance.KeyboardControlls);
    }

    public static KeyboardBinding GetKeyboardBindingInput()
    {
        return instance.KeyboardControlls;
    }
}
