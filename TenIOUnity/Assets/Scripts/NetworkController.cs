using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using SimpleMsgPack;

public class NetworkController : MonoBehaviour {

    void Awake() {
        DontDestroyOnLoad(this);
    }

    // Use this for initialization
    void Start() {
        startServer();
        
        MsgPack msgpack = new MsgPack();
        msgpack.ForcePathObject("u").AsString = "张三";

        // pack file
        // msgpack.ForcePathObject("p.filedata").LoadFileAsBytes("C:\\a.png");

        // pack msgPack binary
        byte[] packData = msgpack.Encode2Bytes();

        var msg = new Message(packData);
        send(msg);
    }

    // Update is called once per frame
    void Update() {
        processMessage();
    }

    static TcpClient client = null;
    static BinaryReader reader = null;
    static BinaryWriter writer = null;
    static Thread networkThread = null;
    private static Queue<Message> messageQueue = new Queue<Message>();

    static void addItemToQueue(Message item) {
        lock(messageQueue) {
            messageQueue.Enqueue(item);
        }
    }

    static Message getItemFromQueue() {
        lock(messageQueue) {
            if (messageQueue.Count > 0) {
                return messageQueue.Dequeue();
            } else {
                return null;
            }
        }
    }

    static void processMessage() {
        Message msg = getItemFromQueue();
        if (msg != null) {
      // do some processing here, like update the player state
            MsgPack unpack_msgpack = new MsgPack();
        
        // unpack msgpack
            unpack_msgpack.DecodeFromBytes(msg.content); 

            UnityEngine.Debug.LogFormat("c:{0}",
            unpack_msgpack.ForcePathObject("c").AsString);

            UnityEngine.Debug.LogFormat("==================================");
            UnityEngine.Debug.LogFormat("use foreach statement:");
            foreach (MsgPack item in unpack_msgpack.ForcePathObject("d"))
            {
                if (item.ValueType != MsgPackType.Array) {
                    UnityEngine.Debug.LogFormat(item.AsString);
                }                
            }
        }
    }

    static void startServer() {
        if (networkThread == null) {
            connect();
            networkThread = new Thread(() => {
                while (reader != null) {
                    Message msg = Message.ReadFromStream(reader);
                    if (msg != null)
                        addItemToQueue(msg);
                }
                lock(networkThread) {
                    networkThread = null;
                }
            });
            networkThread.Start();
        }
    }

    static void connect() {
        if (client == null) {
            string server = "localhost";
            int port = 8032;
            client = new TcpClient(server, port);
            Stream stream = client.GetStream();
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
        }
    }

    public static void send(Message msg) {
        msg.WriteToStream(writer);
        writer.Flush();
    }
}
