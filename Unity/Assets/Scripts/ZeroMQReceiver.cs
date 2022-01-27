using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ZeroMQReceiver : MonoBehaviour
{
    public string OriginIP;
    public string OriginPort;
    public bool StartListeningOnAwake = true;

    public static Action<List<string>> NewZeroMQMessageReceivedEvent;
    private Thread ZeroMQListenerThread;
    private bool ListenerThreadCancelled;

    private readonly ConcurrentQueue<List<string>> ZeroMQQueue = new ConcurrentQueue<List<string>>();

    // Start is called before the first frame update
    void Awake()
    {
        if (StartListeningOnAwake)
        {
            StartListening();
        }
    }

    private void OnDestroy()
    {
        StopListening();
    }

    // check queue for messages
    public void Update()
    {
        while (!ZeroMQQueue.IsEmpty)
        {
            List<string> msg_list;
            if (ZeroMQQueue.TryDequeue(out msg_list))
            {
                NewZeroMQMessageReceivedEvent?.Invoke(msg_list);
            }
            else
            {
                break;
            }
        }
    }

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        Debug.Log("Waiting for subscribers");
        using (var subSocket = new SubscriberSocket())
        {
            // set limit on how many messages in memory
            subSocket.Options.ReceiveHighWatermark = 1000;
            // socket connection
            string connectionStr = "tcp://" + OriginIP + ":" + OriginPort;
            subSocket.Connect(connectionStr);
            // subscribe to topics; "" == all topics
            subSocket.Subscribe("");
            Debug.Log($"Connected to {connectionStr}");

            while (!ListenerThreadCancelled)
            {
                List<string> msg_list = new List<string>();
                // Try to receive two packets; one for the descriptor and one for the frame. 
                if (!subSocket.TryReceiveFrameString(out string frameData)) continue;

                if (!string.IsNullOrEmpty(frameData))
                {
                   // Debug.Log($"Added Frame to {frameData}");
                    msg_list.Add(frameData);
                }

                if (msg_list.Count > 0)
                {
                    ZeroMQQueue.Enqueue(msg_list);
                }
            }
            Debug.Log("Closing subscriber socket");
            subSocket.Close();
        }
        Debug.Log("Cleaning up");
        NetMQConfig.Cleanup();
    }

    public void StartListening()
    {
        ListenerThreadCancelled = false;
        ZeroMQListenerThread = new Thread(ListenerWork);
        ZeroMQListenerThread.Start();
    }

    public void StopListening()
    {
        ListenerThreadCancelled = true;
        ZeroMQListenerThread.Join();
    }
}
