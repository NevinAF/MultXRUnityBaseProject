using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class InputTriggers : MonoBehaviour
{
    [Serializable]
    public class KeystrokeEvent
    {
        public KeyCode keyCode;
        public UnityEvent trigger;
    }

    [Serializable]
    public class XRBindedEvent
    {
        public InputFeatureUsage<bool> feature;
        public XRNode device;
        public UnityEvent trigger;

        public bool Active { get; set; } = false;
    }

    public KeystrokeEvent[] keystrokeEvents;
    public XRBindedEvent[] XRBindedEvents;

    // Update is called once per frame
    void Update()
    {
        foreach (var item in keystrokeEvents)
            if(Input.GetKeyDown(item.keyCode))
                item.trigger.Invoke();

        foreach (var item in XRBindedEvents)
        {
            (new InputDeviceWrapper(item.device)).TryGetFeatureValue(item.feature, out bool value);
            if (item.Active)
            {
                if (value == false)
                    item.Active = false;
                continue;
            }

            if (value)
            {
                item.trigger.Invoke();
                item.Active = true;
            }
        }
            
    }
}
