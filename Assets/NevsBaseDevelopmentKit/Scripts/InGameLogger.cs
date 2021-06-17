using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class InGameLogger : MonoBehaviour
{
    public Text logger;
    private static InGameLogger instance;
    private string[] past = new string[20];
    private int index = 0;

    private void Awake()
    {
        instance = this;
        past[index] = "Log->";
        index++;
    }

    public static void Log(string text)
    {
        instance.InstanceLog(text);
    }

    private void InstanceLog(string text)
    {
        if (index >= 20)
        {
            for(int i = 0; i < 19; i++)
            {
                past[i] = past[i + 1];
            }
            index--;
        }
        past[index] = text;
        text = past[0];
        for (int i = 1; i <= index; i++)
        {
            text += "\n" + past[i];
        }
        index++;
        logger.text = text;
    }
}
