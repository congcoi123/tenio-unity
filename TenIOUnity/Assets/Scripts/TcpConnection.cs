using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using SimpleMsgPack;
using UnityEngine;

public class TcpConnection {

    #region Delegate Variables
    public Action onClientStarted = null;
    public Action onClientClosed = null;
    #endregion

    private TcpClient __client = null;
    private BinaryReader __reader = null;
    private BinaryWriter __writer = null;
    private Thread __networkThread = null;
    private Queue<Message> __messageQueue = null;
    private ITcpListener __listener = null;
    private string __host = null;
    private int __port = 0;

    public TcpConnection(ITcpListener listener, String host, int port) 
    {
        __listener = listener;
        __messageQueue = new Queue<Message>();
        __host = host;
        __port = port;
    }

    private void __addItemToQueue(Message item) 
    {
        lock(__messageQueue) 
        {
            __messageQueue.Enqueue(item);
        }
    }

    private Message __getItemFromQueue() 
    {
        lock(__messageQueue) 
        {
            if (__messageQueue.Count > 0) 
            {
                return __messageQueue.Dequeue();
            }
            else
            {
                return null;
            }
        }
    }

    private void __connect() {
        if (__client == null) 
        {
            __client = new TcpClient(__host, __port);
            Stream stream = __client.GetStream();
            __reader = new BinaryReader(stream);
            __writer = new BinaryWriter(stream);
            
            onClientStarted?.Invoke();
        }
    }

    public void start() 
    {
        if (__networkThread == null) 
        {
            __connect();
            __networkThread = new Thread(() => {
                while (__reader != null) 
                {
                    Message message = Message.ReadFromStream(__reader);
                    if (message != null) 
                    {
                        __addItemToQueue(message);
                    }
                }
                lock(__networkThread) 
                {
                    __networkThread = null;
                }
            });
            __networkThread.Start();
        }
    }

    public void processMessage() 
    {
        Message message = __getItemFromQueue();
        if (message != null) 
        {
            __listener.onReceivedTCP(message);
        }
    }

    public void send(Message message) 
    {
        message.WriteToStream(__writer);
        __writer.Flush();
    }

    public void close() 
    {
        if (__client.Connected)
        {
            __client.Close();
        }

        onClientClosed?.Invoke();

        __messageQueue.Clear();
    }

}
