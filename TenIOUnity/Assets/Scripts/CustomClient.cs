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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SimpleMsgPack;

public class CustomClient : MonoBehaviour, ITcpListener
{
    #region Public Variables
    [Header("Network")]
    public string host = "localhost";
    public int port = 8032;
    #endregion

    [Header("UI References")]
    [SerializeField] private Button __startButton = null;
    [SerializeField] private Button __closeButton = null;
    [SerializeField] private Text __clientLogger = null;

    private TcpConnection __tcpConnection = null;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        __tcpConnection = new TcpConnection(this, host, port);

        __startButton.onClick.AddListener(connectToServer);

        __closeButton.interactable = false;
        __closeButton.onClick.AddListener(disconnectFromServer);

        __tcpConnection.onClientStarted = () =>
        {
            __closeButton.interactable = true;
        };

        __tcpConnection.onClientClosed = () =>
        {
            __startButton.interactable = true;
        };
    }

    void Update() {
        __tcpConnection.processMessage();
    }

    public void onReceivedTCP(TcpMessage message) 
    {
        MsgPack msgpack = new MsgPack();
        
        msgpack.DecodeFromBytes(message.getContent());

        StringBuilder builder = new StringBuilder();
        builder.Append("[");
        builder.Append("c: ");
        builder.Append(msgpack.ForcePathObject("c").AsString);
        builder.Append(", ");
        builder.Append("d: [");
        foreach (MsgPack item in msgpack.ForcePathObject("d"))
        {
            if (item.ValueType != MsgPackType.Array) 
            {
                builder.Append(item.AsString);
                builder.Append(", ");
            }
            else
            {
                builder.Append("[");
                foreach (MsgPack i in item)
                {
                    builder.Append(i.AsString);
                    builder.Append(", ");
                }
                builder.Append("]");
            }

        }
        builder.Append("]");

        clientLog(builder.ToString());
    }

    public void connectToServer()
    {
        __tcpConnection.start();
        
        MsgPack msgpack = new MsgPack();
        msgpack.ForcePathObject("u").AsString = "kong";
        byte[] packData = msgpack.Encode2Bytes();
        var msg = new TcpMessage(packData);
        __tcpConnection.send(msg);
    }

    public void disconnectFromServer()
    {
        __tcpConnection.close();
    }

    //Custom Client Log
    #region ClientLog
    public void clientLog(string msg, Color color)
    {
        __clientLogger.text += '\n' + "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">- " + msg + "</color>";
    }
    public void clientLog(string msg)
    {
        __clientLogger.text += '\n' + "- " + msg;
    }
    #endregion

}
