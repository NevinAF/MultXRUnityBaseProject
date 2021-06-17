using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugInfoRedirect : MonoBehaviour
{
    // Component Editor Fields.

    [Header("Display Information")]
    [SerializeField]
    [Tooltip("The types of logs the panel will display.")]
    private LogType[] logTypes;
    [SerializeField]
    [Tooltip("The types of logs the panel will display.")]
    private bool includeTrace;
    [SerializeField]
    [Tooltip("The length of history that text will display.")]
    [Range(1,50)]
    private int numberOfSavedLogs = 20;

    [Header("Text Component")]
    [SerializeField]
    [Tooltip("The \".text\" field is used to display log.")]
    private Text textOutput;

    // private data

    /// <summary>
    /// Holds all strings from past. This allows the logs to fall off once the text box is filled.
    /// </summary>
    private string[] logHistory;

    /// <summary>
    /// This keeps track of how many logs are inside the history.
    /// </summary>
    private int index;

    private void Awake()
    {
        index = 0;

        logHistory = new string[numberOfSavedLogs];
        logHistory[index] = "-------|Log is initalized|-------";
        index++;
    }

    private void OnEnable()
    {
        if (logTypes.Length <= 0)
        {
            Debug.LogWarning("DebugInfoRedirect has no log types! Disabling component...");
            this.enabled = false;
            return;
        }
        else if (textOutput == null)
        {
            Debug.LogWarning("DebugInfoRedirect has no text component for output! Disabling component...");
            this.enabled = false;
            return;
        }

        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (Array.Exists(logTypes, e => e == type))
            AddLog(logString + ((includeTrace) ? stackTrace : ""));
    }

    private void AddLog(string text)
    {
        if (index >= 20)
        {
            for (int i = 0; i < 19; i++)
            {
                logHistory[i] = logHistory[i + 1];
            }
            index--;
        }
        logHistory[index] = text;
        text = logHistory[0];
        for (int i = 1; i <= index; i++)
        {
            text += "\n" + logHistory[i];
        }
        index++;
        textOutput.text = text;
    }
}
