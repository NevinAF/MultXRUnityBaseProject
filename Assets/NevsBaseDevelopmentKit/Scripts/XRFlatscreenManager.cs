using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(MouseLook))]
public class XRFlatscreenManager : MonoBehaviour
{
    [SerializeField]
    private GameObject HMDPanel;
    [SerializeField]
    private GameObject LeftPanel;
    [SerializeField]
    private GameObject RightPanel;



    private bool isRightHand;
    private bool isHMD;

    private MouseLook _mouseLook;


    // Start is called before the first frame update
    void Start()
    {
        isRightHand = false;
        isHMD = true;

        _mouseLook = GetComponent<MouseLook>();

        UpdateIsHMD();
        UpdateIsRight();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(HMDSetupManager.GetKeyboardBindingInput().toggle_HMD))
            ToggleHMD();

        if (Input.GetKeyDown(HMDSetupManager.GetKeyboardBindingInput().toggle_active_controller))
            SwitchHands();
    }

    private void UpdateIsHMD()
    {
        if (!isHMD)
        {
            _mouseLook.enabled = false;
            HMDPanel.SetActive(false);
        }
        else
        {
            _mouseLook.enabled = true;
            HMDPanel.SetActive(true);
        }
    }

    private void UpdateIsRight()
    {
        if (isRightHand)
        {
            InputDeviceWrapper.SetCurrentXRFlatscreenDevice(UnityEngine.XR.XRNode.RightHand);
            RightPanel.SetActive(false);
            LeftPanel.SetActive(true);
        }
        else
        {
            InputDeviceWrapper.SetCurrentXRFlatscreenDevice(UnityEngine.XR.XRNode.LeftHand);
            RightPanel.SetActive(true);
            LeftPanel.SetActive(false);
        }
    }

    public void SwitchHands()
    {
        isRightHand = !isRightHand;

        UpdateIsRight();
    }

    public void ToggleHMD()
    {
        isHMD = !isHMD;

        UpdateIsHMD();
    }

    public void SetHMDEnabled(bool enabled)
    {
        if (enabled != isHMD)
            ToggleHMD();
    }

    public void SetIsRightEnabled(bool enabled)
    {
        if (enabled != isRightHand)
            SwitchHands();
    }

}
