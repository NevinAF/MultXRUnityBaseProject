using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
public class HandPresence : MonoBehaviour
{
    [SerializeField]
    private XRNode m_ControllerNode = XRNode.RightHand;
    public bool showController = true;
    [SerializeField]
    private GameObject controller;
    [SerializeField]
    private GameObject handModel;
    //public bool enableHandColliders;

    private InputDeviceWrapper targetDevice;
   

    // Start is called before the first frame update
    void Start()
    {
        InputDeviceWrapper.FindInputDevice(m_ControllerNode, out targetDevice, this);
        Update_ControllerVisual();
    }

    

    private void Update_ControllerVisual()
    {
        if (showController)
        {
            
            handModel.SetActive(false);
            controller.SetActive(true);
        }
        else
        {
            handModel.SetActive(true);
            controller.SetActive(false);
        }
    }

    private bool apress = false;

    // Lets the inputs be updated, then the visuals
    void LateUpdate()
    {
        if (!apress)
        {
            targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out apress);
            if (apress)
            {
                showController = !showController;
                Update_ControllerVisual();
            }
        }
        else
            targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out apress);
    }
}
