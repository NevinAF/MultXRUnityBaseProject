using UnityEngine;

[CreateAssetMenu(fileName = "New Controlls", menuName = "PureFlatscreen/KeyboardControlls", order = 1)]
public class PureFlatscreenControlls : ScriptableObject
{
    [Header("Movement")]
    public KeyCode up = KeyCode.W;
    public KeyCode left = KeyCode.A;
    public KeyCode down = KeyCode.S;
    public KeyCode right = KeyCode.D;
    public KeyCode sprint = KeyCode.LeftShift;
    public KeyCode jump = KeyCode.Space;

    [Header("Menu Options")]
    public KeyCode showMenu = KeyCode.Escape;
}