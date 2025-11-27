using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class FanTrigger : MonoBehaviour
{
    public int relayID; // 1–4 per collider

    private static SerialPort serial;
    private static Thread readThread;
    private static bool running = false;
    bool isTriggered = false;
    float lastSendTime = 0f;
    float cooldown = 0.5f;
    private static readonly object lockObj = new object();

    void Start()
    {
        // Open the serial port only once globally
        if (serial == null)
        {
            serial = new SerialPort("COM9", 9600);
            try
            {
                serial.Open();
                serial.ReadTimeout = 100;
                running = true;
                readThread = new Thread(ReadSerial);
                readThread.Start();
                Debug.Log("Serial port opened successfully.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to open serial port: " + e.Message);
            }
        }
    }

    private static void ReadSerial()
    {
        while (running)
        {
            try
            {
                if (serial != null && serial.IsOpen && serial.BytesToRead > 0)
                    serial.ReadLine();
            }
            catch { }

            Thread.Sleep(300);
        }
    }

    public static void SendSerial(string message)
    {
        lock (lockObj)
        {
            if (serial != null && serial.IsOpen)
            {
                try
                {
                    serial.Write(message);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Serial write failed: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("Serial port not open when trying to write.");
            }
        }
    }

    void OnTriggerEnter(Collider player)
    {
        if (!isTriggered && player.CompareTag("Player"))
        {
            isTriggered = true;
            SendSerial(relayID.ToString()); // e.g. “1”, “2”, “3”, “4”
        }
    }

    void OnTriggerExit(Collider player)
    {
        if (isTriggered && player.CompareTag("Player"))
        {
            isTriggered = false;
            string message;

            if (relayID == 5)
                message = "A"; // special case for relay 5 OFF
            else
                message = (relayID + 5).ToString(); 

            SendSerial(message);
        }
    }

    void OnApplicationQuit()
    {
        running = false;
        if (serial != null && serial.IsOpen)
        {
            serial.Close();
            serial.Dispose();
            Debug.Log("Serial port closed cleanly.");
        }
    }
}
