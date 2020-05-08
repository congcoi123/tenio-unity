/*
The MIT License

Copyright (c) 2016-2020 kong <congcoi123@gmail.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using SimpleMsgPack;
using UnityEngine;

public class TcpConnection
{
    
    #region Delegate Variables
    public Action onClientStarted = null;
    public Action onClientClosed = null;
    #endregion

    private TcpClient __client = null;
    private BinaryReader __reader = null;
    private BinaryWriter __writer = null;
    private Thread __networkThread = null;
    private Queue<TcpMessage> __messageQueue = null;
    private ITcpListener __listener = null;
    private string __host = null;
    private int __port = 0;

    public TcpConnection(ITcpListener listener, String host, int port) 
    {
        __listener = listener;
        __messageQueue = new Queue<TcpMessage>();
        __host = host;
        __port = port;
    }

    private void __addItemToQueue(TcpMessage item) 
    {
        lock(__messageQueue) 
        {
            __messageQueue.Enqueue(item);
        }
    }

    private TcpMessage __getItemFromQueue() 
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
                    TcpMessage message = TcpMessageHelper.readFromStream(__reader);
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
        TcpMessage message = __getItemFromQueue();
        if (message != null) 
        {
            __listener.onReceivedTCP(message);
        }
    }

    public void send(TcpMessage message) 
    {
        TcpMessageHelper.writeToStream(message, __writer);
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
