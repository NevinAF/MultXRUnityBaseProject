using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PureFlatscreenManager : MonoBehaviour
{
    private static PureFlatscreenManager instance;

    [SerializeField]
    [Tooltip("Keyboard Mapping Data")]
    private PureFlatscreenControlls controlls;



    private void Start()
    {
        if (instance != null)
        {
            Debug.LogError("There are multiple pure flatscreen managers in the scene. Deleting component...");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public static PureFlatscreenControlls GetControlls()
    {
        return instance.controlls;
    }
}