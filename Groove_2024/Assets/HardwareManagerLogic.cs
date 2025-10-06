using UnityEngine;

public class HardwareManagerLogic : MonoBehaviour
{
    enum InputType
    {
        KeyboardMouse,
        Controller,
        SteamDeck,
        VRController,
        MobileTouch
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Device Model: " + SystemInfo.deviceModel);
        Debug.Log("Device Type: " + SystemInfo.deviceType);
        Debug.Log("Graphics Device: " + SystemInfo.graphicsDeviceName);
        Debug.Log("Graphics Memory: " + SystemInfo.graphicsMemorySize + "MB");
        Debug.Log("Processor Type: " + SystemInfo.processorType);
        Debug.Log("System Memory: " + SystemInfo.systemMemorySize + "MB");
    }

}
