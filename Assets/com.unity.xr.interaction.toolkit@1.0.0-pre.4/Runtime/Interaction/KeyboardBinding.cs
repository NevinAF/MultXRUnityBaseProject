using UnityEngine;

[CreateAssetMenu(fileName = "Key Binding", menuName = "XR/KeyboardBinding", order = 1)]
public class KeyboardBinding : ScriptableObject
{
    [Header("Directly Mapped Buttons")]
    public KeyCode trigger = KeyCode.Mouse0;
    public KeyCode grip = KeyCode.Mouse1;
    public KeyCode primary = KeyCode.Q;
    public KeyCode secondary = KeyCode.E;
    public KeyCode joystick_click = KeyCode.F;

    [Header("Joysticks")]
    public KeyCode joystick_up = KeyCode.W;
    public KeyCode joystick_left = KeyCode.A;
    public KeyCode joystick_down = KeyCode.S;
    public KeyCode joystick_right = KeyCode.D;

    [Header("Keyboard Only")]
    public KeyCode openMenu = KeyCode.Escape;
    public KeyCode toggle_HMD = KeyCode.Space;
    public KeyCode toggle_active_controller = KeyCode.Tab;

    [Header("Touch Buttons")]
    public KeyCode trigger_touch = KeyCode.Alpha1;
    public KeyCode grip_touch = KeyCode.Alpha2;
    public KeyCode primary_touch = KeyCode.Alpha3;
    public KeyCode secondary_touch = KeyCode.Alpha4;
    public KeyCode joystick_touch = KeyCode.Alpha5;
    //[Header("Custom")]

}