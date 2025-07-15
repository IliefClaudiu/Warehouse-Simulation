using System.IO.Ports;
using UnityEngine;

public class SerialController : MonoBehaviour
{
    public string portName = "COM4";  // ← Set your actual COM port
    public int baudRate = 9600;

    private SerialPort serial;

    void Start()
    {
        serial = new SerialPort(portName, baudRate);
        try
        {
            serial.Open();
            Debug.Log("Serial connection opened on " + portName);
        }
        catch
        {
            Debug.LogError("Failed to open serial port " + portName);
        }
    }

    void Update()
    {
        if (serial != null && serial.IsOpen && serial.BytesToRead > 0)
        {
            string message = serial.ReadLine();
            Debug.Log("Arduino says: " + message);
        }
        //if (Input.GetKeyDown(KeyCode.W)) SendCommand("F");  // Forward
        //if (Input.GetKeyDown(KeyCode.A)) SendCommand("L");  // Left
        //if (Input.GetKeyDown(KeyCode.D)) SendCommand("R");  // Right
        //if (Input.GetKeyDown(KeyCode.S)) SendCommand("S");  // Stop
    }

    public void SendCommand(string cmd)
    {
        if (serial != null && serial.IsOpen)
        {
            serial.Write(cmd);
            Debug.Log("Sent command: " + cmd);
        }
    }

    void OnApplicationQuit()
    {
        if (serial != null && serial.IsOpen)
        {
            serial.Close();
        }
    }
}