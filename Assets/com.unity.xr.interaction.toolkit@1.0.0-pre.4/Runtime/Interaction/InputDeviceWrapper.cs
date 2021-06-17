////MODIFIED BY TONY!

using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_STANDALONE
using Valve.VR;
#endif

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Wraps an input device for the XR Interaction toolkit to add support for SteamVR Input System
    /// </summary>
    public struct InputDeviceWrapper
    {
        /// <summary>
        /// True if there is steamvr activated, false otherwise
        /// </summary>
        public static bool IsSteamVR { get; private set; } = false;

        /// <summary>
        /// True if XR inputs should be interperated through keyboard, false otherwise
        /// </summary>
        public static bool IsFlatscreen { get; private set; } = false;
        
        /// <summary>
        /// The button mapping for XR flatscreen.
        /// </summary>
        public static KeyboardBinding KeyboardInputs { get; private set; }

        public static XRNode CurrentXRFlatscreenDevice { get; private set; }
        
        /// <summary>
        /// Forces the input to try to read from steamvr. If steamvr isn't even loaded, it'll throw a warning but still overide default value.
        /// </summary>
        /// <param name="isSteamVR">True if input should read from steamvr helper classes.</param>
        public static void ForceIsSteamVR(bool isSteamVR)
        {
            // The Following line is replaced. I moved the steamvr from being dependent on devices and instead a static variable that can be changed
            // during runtime In addition, for some reason, the subsysem on input devices is not being set.
            //          this.m_isSteamVR = m_inputDevice.subsystem != null && m_inputDevice.subsystem.SubsystemDescriptor.id == "OpenVR Input";

            // I instead use the HMD (loaded device name) to see if steamvr is playing. 
            if (isSteamVR && XRSettings.loadedDeviceName != "OpenVR Display")
                Debug.LogWarning("InputDeviceWrapper is being set to use SteamVR when SteamVR is not loaded. Proceed with caution.");
            InputDeviceWrapper.IsSteamVR = isSteamVR;
        }

        /// <summary>
        /// Forces the input to try to read from keyboard.
        /// </summary>
        /// <param name="isFlatscreen">True if input should be redirected to keybord.</param>
        public static void ForceIsFlatscreen(bool isFlatscreen)
        {
            InputDeviceWrapper.IsFlatscreen = isFlatscreen;
        }

        /// <summary>
        /// Sets the keyboard mapping for when playing xr without HMD.
        /// </summary>
        /// <param name="keyBindings">The mapping that the InputDeviceWrapper will use.</param>
        public static void SetKeyboardInputs(KeyboardBinding keyBindings)
        {
            InputDeviceWrapper.KeyboardInputs = keyBindings;
        }

        /// <summary>
        /// This updates the InputDeviceWrapper class to focus on a specific device while in flatscreen
        /// </summary>
        /// <param name="node"></param>
        public static void SetCurrentXRFlatscreenDevice(XRNode node)
        {
            InputDeviceWrapper.CurrentXRFlatscreenDevice = node;
        }

        /// <summary>
        /// The wrapped Input Device. We'll take positions and rotations from it in any case.
        /// It will also provide inputs with non-SteamVR headsets
        /// </summary>
        private InputDevice m_inputDevice;

        /// <summary>
        /// Node we must provide input from
        /// </summary>
        private XRNode m_deviceNode;

        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="deviceNode">Device from which take the input</param>
        public InputDeviceWrapper(XRNode deviceNode)
        {
            this.m_deviceNode = deviceNode;
            m_inputDevice = InputDevices.GetDeviceAtXRNode(deviceNode);
        }

        /// <summary>
        /// Constructor that only sets the device as soon as the device is detected. The InputDeviceWrapper is not valid until
        /// a device is found, all try-get functions are all false until found.
        /// </summary>
        /// <param name="deviceNode">The desired device</param>
        /// <param name="inputDeviceWrapper">The object that is being constructed</param>
        /// <param name="obj">A MonoBehaviour that will run the coroutine after function call</param>
        /// <returns></returns>
        public static bool FindInputDevice(XRNode deviceNode, out InputDeviceWrapper inputDeviceWrapper, MonoBehaviour obj = null)
        {
            inputDeviceWrapper = new InputDeviceWrapper(deviceNode);
            bool isValid = inputDeviceWrapper.isValid;
            if (obj != null && !isValid)
                obj.StartCoroutine(inputDeviceWrapper.GetInputDevice());
            return isValid;
        }

        private IEnumerator GetInputDevice()
        {
            while (!m_inputDevice.isValid)
            {
                if (IsFlatscreen)
                    yield return new WaitUntil(() => !IsFlatscreen);
                else yield return new WaitForSeconds(0.2f);
                m_inputDevice = InputDevices.GetDeviceAtXRNode(this.m_deviceNode);
            }
        }

        /// <summary>
        ///   <para>Read Only. True if the device is currently a valid input device; otherwise false.</para>
        /// </summary>
        public bool isValid
        {
            get
            {
                return m_inputDevice.isValid;
            }
        }

        /// <summary>
        ///   <para>Read Only. The name of the device in the XR system. This is a platform provided unique identifier for the device.</para>
        /// </summary>
        public string name
        {
            get
            {
                return m_inputDevice.name;
            }
        }

        /// <summary>
        ///   <para>Read Only. The InputDeviceRole of the device in the XR system. This is a platform provided description of how the device is used.</para>
        /// </summary>
        [Obsolete("This API has been marked as deprecated and will be removed in future versions. Please use InputDevice.characteristics instead.")]
        public InputDeviceRole role
        {
            get
            {
                return m_inputDevice.role;
            }
        }

        /// <summary>
        ///   <para>The manufacturer of the connected Input Device.</para>
        /// </summary>
        public string manufacturer
        {
            get
            {
                return m_inputDevice.manufacturer;
            }
        }

        /// <summary>
        ///   <para>The serial number of the connected Input Device.  Blank if no serial number is available.</para>
        /// </summary>
        public string serialNumber
        {
            get
            {
                return m_inputDevice.serialNumber;
            }
        }

        /// <summary>
        ///   <para>Read Only. A bitmask of enumerated flags describing the characteristics of this InputDevice.</para>
        /// </summary>
        public InputDeviceCharacteristics characteristics
        {
            get
            {
                return m_inputDevice.characteristics;
            }
        }

        /// <summary>
        ///   <para>Sends a haptic impulse to a device.</para>
        /// </summary>
        /// <param name="channel">The channel to receive the impulse.</param>
        /// <param name="amplitude">The normalized (0.0 to 1.0) amplitude value of the haptic impulse to play on the device.</param>
        /// <param name="duration">The duration in seconds that the haptic impulse will play. Only supported on Oculus.</param>
        /// <returns>
        ///   <para>Returns true if successful. Returns false otherwise.</para>
        /// </returns>
        public bool SendHapticImpulse(uint channel, float amplitude, float duration = 1f)
        {
            return m_inputDevice.SendHapticImpulse(channel, amplitude, duration);
        }

        /// <summary>
        ///   <para>Sends a raw buffer of haptic data to the device.</para>
        /// </summary>
        /// <param name="channel">The channel to receive the data.</param>
        /// <param name="buffer">A raw byte buffer that contains the haptic data to send to the device.</param>
        /// <returns>
        ///   <para>Returns true if successful. Returns false otherwise.</para>
        /// </returns>
        public bool SendHapticBuffer(uint channel, byte[] buffer)
        {
            return m_inputDevice.SendHapticBuffer(channel, buffer);
        }

        public bool TryGetHapticCapabilities(out HapticCapabilities capabilities)
        {
            return m_inputDevice.TryGetHapticCapabilities(out capabilities);
        }

        /// <summary>
        ///   <para>Stop all haptic playback for a device.</para>
        /// </summary>
        public void StopHaptics()
        {
            m_inputDevice.StopHaptics();
        }

        public bool TryGetFeatureUsages(List<InputFeatureUsage> featureUsages)
        {
            return m_inputDevice.TryGetFeatureUsages(featureUsages);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<bool> usage, out bool value)
        {
#if UNITY_STANDALONE
            if (IsFlatscreen && CurrentXRFlatscreenDevice == this.m_deviceNode)
            {
                if (usage == CommonUsages.triggerButton)
                {
                    value = Input.GetKey(KeyboardInputs.trigger);
                    return true;
                }
                else if (usage == CommonUsages.gripButton)
                {
                    value = Input.GetKey(KeyboardInputs.grip);
                    return true;
                }
                else if (usage == CommonUsages.primaryButton)
                {
                    value = Input.GetKey(KeyboardInputs.primary);
                    return true;
                }
                else if (usage == CommonUsages.secondaryButton)
                {
                    value = Input.GetKey(KeyboardInputs.secondary);
                    return true;
                }
                else if (usage == CommonUsages.primaryTouch)
                {
                    value = Input.GetKey(KeyboardInputs.primary_touch);
                    return true;
                }
                else if (usage == CommonUsages.secondaryTouch)
                {
                    value = Input.GetKey(KeyboardInputs.secondary_touch);
                    return true;
                }
                // NO GRIP TOUCH in the XR Common usages.
                //else if (usage == CommonUsages.grip)
                //{
                //    value = SteamVR_Actions._default.GripTouch[m_deviceNode.ToSteamVrSource()].state;
                //    return true;
                //}
                else if (usage == CommonUsages.primary2DAxisClick)
                {
                    value = Input.GetKey(KeyboardInputs.joystick_click);
                    return true;
                }
                else if (usage == CommonUsages.primary2DAxisTouch)
                {
                    value = Input.GetKey(KeyboardInputs.joystick_touch);
                    return true;
                }
            }
            else if (IsSteamVR && m_deviceNode.IsHands())
            {
                if (usage == CommonUsages.triggerButton)
                {
                    value = SteamVR_Actions._default.TriggerClick[m_deviceNode.ToSteamVrSource()].state;
                    return true;
                }
                else if (usage == CommonUsages.gripButton)
                {
                    value = SteamVR_Actions._default.GripClick[m_deviceNode.ToSteamVrSource()].state;
                    return true;
                }
                else if (usage == CommonUsages.primaryButton)
                {
                    value = SteamVR_Actions._default.PrimaryButton[m_deviceNode.ToSteamVrSource()].state;
                    return true;
                }
                else if (usage == CommonUsages.secondaryButton)
                {
                    value = SteamVR_Actions._default.SecondaryButton[m_deviceNode.ToSteamVrSource()].state;
                    return true;
                }
                else if (usage == CommonUsages.primaryTouch)
                {
                    value = SteamVR_Actions._default.PrimaryTouch[m_deviceNode.ToSteamVrSource()].state;
                    return true;
                }
                else if (usage == CommonUsages.secondaryTouch)
                {
                    value = SteamVR_Actions._default.SecondaryTouch[m_deviceNode.ToSteamVrSource()].state;
                    return true;
                }
                // NO GRIP TOUCH in the XR Common usages.
                //else if (usage == CommonUsages.grip)
                //{
                //    value = SteamVR_Actions._default.GripTouch[m_deviceNode.ToSteamVrSource()].state;
                //    return true;
                //}
                else if (usage == CommonUsages.primary2DAxisClick)
                {
                    value = SteamVR_Actions._default.JoystickClick[m_deviceNode.ToSteamVrSource()].state;
                    return true;
                }
                else if (usage == CommonUsages.primary2DAxisTouch)
                {
                    value = SteamVR_Actions._default.JoystickTouch[m_deviceNode.ToSteamVrSource()].state;
                    return true;
                }
            }
#endif
            return m_inputDevice.TryGetFeatureValue(usage, out value);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<uint> usage, out uint value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, out value);
        }

        
        public bool TryGetFeatureValue(InputFeatureUsage<float> usage, out float value)
        {
#if UNITY_STANDALONE
            if (IsFlatscreen && CurrentXRFlatscreenDevice == this.m_deviceNode)
            {
                if (usage == CommonUsages.grip)
                {
                    value = Input.GetKey(KeyboardInputs.grip) ? 1 : 0;
                    return true;
                }
                if (usage == CommonUsages.trigger)
                {
                    value = Input.GetKey(KeyboardInputs.trigger) ? 1 : 0;
                    return true;
                }
#pragma warning disable CS0618 // Type or member is obsolete
                if (usage == CommonUsages.indexTouch)
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    value = Input.GetKey(KeyboardInputs.trigger_touch) ? 1 : 0;
                    return true;
                }
            }
            else if (IsSteamVR && m_deviceNode.IsHands())
            {
                if (usage == CommonUsages.grip)
                {
                    value = SteamVR_Actions._default.GripValue[m_deviceNode.ToSteamVrSource()].axis;
                    return true;
                }
                else if (usage == CommonUsages.trigger)
                {
                    value = SteamVR_Actions._default.TriggerValue[m_deviceNode.ToSteamVrSource()].axis;
                    return true;
                }
#pragma warning disable CS0618 // Type or member is obsolete
                else if (usage == CommonUsages.indexTouch)
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    value = (SteamVR_Actions._default.TriggerTouch[m_deviceNode.ToSteamVrSource()].state) ? 1 : 0;
                    return true;
                }
            }
#endif
            return m_inputDevice.TryGetFeatureValue(usage, out value);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<Vector2> usage, out Vector2 value)
        {
#if UNITY_STANDALONE
            if (IsFlatscreen && CurrentXRFlatscreenDevice == this.m_deviceNode)
            {
                if (usage == CommonUsages.primary2DAxis)
                {
                    value = new Vector2(
                        ( (Input.GetKey(KeyboardInputs.joystick_left) ? -1 : 0) + (Input.GetKey(KeyboardInputs.joystick_right) ? 1 : 0)),
                        ( (Input.GetKey(KeyboardInputs.joystick_down) ? -1 : 0) + (Input.GetKey(KeyboardInputs.joystick_up) ? 1 : 0))
                        );
                    return true;
                }
            }
            else if (IsSteamVR && m_deviceNode.IsHands())
            {
                if (usage == CommonUsages.primary2DAxis)
                {
                    value = SteamVR_Actions._default.JoystickValue[m_deviceNode.ToSteamVrSource()].axis;
                    return true;
                }              
            }
#endif
            return m_inputDevice.TryGetFeatureValue(usage, out value);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<Vector3> usage, out Vector3 value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, out value);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<Quaternion> usage, out Quaternion value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, out value);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<Hand> usage, out Hand value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, out value);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<Bone> usage, out Bone value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, out value);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<Eyes> usage, out Eyes value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, out value);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<byte[]> usage, byte[] value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, value);
        }

        public bool TryGetFeatureValue(
          InputFeatureUsage<InputTrackingState> usage,
          out InputTrackingState value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, out value);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<bool> usage, DateTime time, out bool value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, time, out value);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<uint> usage, DateTime time, out uint value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, time, out value);
        }

        public bool TryGetFeatureValue(InputFeatureUsage<float> usage, DateTime time, out float value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, time, out value);
        }

        public bool TryGetFeatureValue(
          InputFeatureUsage<Vector2> usage,
          DateTime time,
          out Vector2 value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, time, out value);
        }

        public bool TryGetFeatureValue(
          InputFeatureUsage<Vector3> usage,
          DateTime time,
          out Vector3 value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, time, out value);
        }

        public bool TryGetFeatureValue(
          InputFeatureUsage<Quaternion> usage,
          DateTime time,
          out Quaternion value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, time, out value);
        }

        public bool TryGetFeatureValue(
          InputFeatureUsage<InputTrackingState> usage,
          DateTime time,
          out InputTrackingState value)
        {
            return m_inputDevice.TryGetFeatureValue(usage, time, out value);
        }

    }

#if UNITY_STANDALONE
 
    /// <summary>
    /// Helpers for use of XRNode input types with steam
    /// </summary>
    public static class InputXrNodeUtilities
    {
        /// <summary>
        /// True if the node represents a hand
        /// </summary>
        /// <param name="node"></param>
        public static bool IsHands(this XRNode node)
        {            
            return node == XRNode.LeftHand || node == XRNode.RightHand;
        }
 
        /// <summary>
        /// Converts between XRNode and SteamVR Input Sources
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static SteamVR_Input_Sources ToSteamVrSource(this XRNode node)
        {
            //this switch should be expanded according to necessities
            switch(node)
            {
                case XRNode.LeftHand:
                    return SteamVR_Input_Sources.LeftHand;
                case XRNode.RightHand:
                    return SteamVR_Input_Sources.RightHand;
                case XRNode.Head:
                    return SteamVR_Input_Sources.Head;
 
                default:
                    return SteamVR_Input_Sources.Any;
            }
        }
    }
 
#endif
}