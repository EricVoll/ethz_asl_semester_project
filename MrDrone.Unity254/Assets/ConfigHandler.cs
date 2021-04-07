using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConfigHandler : MonoBehaviour
{
    public GameObject[] RestOfScene;
    public RosConnector Connector;
    public TMP_InputField Input;

    // Start is called before the first frame update
    void Start()
    {
        OpenSystemKeyboard();
    }

    public void OpenSystemKeyboard()
    {
        string ip = PlayerPrefs.GetString("laptopIp");
        TouchScreenKeyboard.Open(ip, TouchScreenKeyboardType.URL, false, false, false, false);
        Input.text = ip;
        Input.ActivateInputField();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickedOk()
    {
        string input = Input.text;

        if (!input.StartsWith("ws://"))
        {
            input = $"ws://{input}";
        }
        if (!input.EndsWith(":9090"))
        {
            input = $"{input}:9090";
        }

        PlayerPrefs.SetString("laptopIp", input);
        PlayerPrefs.Save();

        Connector.RosBridgeServerUrl = input;

        foreach (var go in RestOfScene)
        {
            go.SetActive(true);
        }

        gameObject.SetActive(false);
    }
}
