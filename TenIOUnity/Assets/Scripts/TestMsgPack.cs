using System;
using System.Diagnostics;
using UnityEngine;
using SimpleMsgPack;

public class TestMsgPack : MonoBehaviour
{
    void Awake ()
    {
        // TObject to = TObject.TestBuild();
        
        // byte[] bytes = MessagePackSerializer.Serialize(to);
        // String jmsgPack = MessagePackSerializer.ConvertToJson(bytes);
        // Console.WriteLine(jmsgPack);

        // Stopwatch sw4 = new Stopwatch();
        // sw4.Start();
        // TObject toMsgPack = MessagePack.MessagePackSerializer.Deserialize<TObject>(bytes);
        // sw4.Stop();
        // UnityEngine.Debug.LogFormat("*[JsonString To Object] - MsgPack :  {0}ms.", sw4.ElapsedMilliseconds);

        MsgPack msgpack = new MsgPack();
        msgpack.ForcePathObject("p.name").AsString = "张三";
        msgpack.ForcePathObject("p.age").AsInteger = 25;
        msgpack.ForcePathObject("p.datas").AsArray.Add(90);
        msgpack.ForcePathObject("p.datas").AsArray.Add(80);
        msgpack.ForcePathObject("p.datas").AsArray.Add("李四");
        msgpack.ForcePathObject("p.datas").AsArray.Add(3.1415926);

        // pack file
        // msgpack.ForcePathObject("p.filedata").LoadFileAsBytes("C:\\a.png");

        // pack msgPack binary
        byte[] packData = msgpack.Encode2Bytes();

        MsgPack unpack_msgpack = new MsgPack();
        
        // unpack msgpack
        unpack_msgpack.DecodeFromBytes(packData);

        UnityEngine.Debug.LogFormat("name:{0}, age:{1}",
            unpack_msgpack.ForcePathObject("p.name").AsString,
            unpack_msgpack.ForcePathObject("p.age").AsInteger);

        UnityEngine.Debug.LogFormat("==================================");
        UnityEngine.Debug.LogFormat("use index property, Length{0}:{1}",
            unpack_msgpack.ForcePathObject("p.datas").AsArray.Length,
            unpack_msgpack.ForcePathObject("p.datas").AsArray[0].AsString
            );

        UnityEngine.Debug.LogFormat("==================================");
        UnityEngine.Debug.LogFormat("use foreach statement:");
        foreach (MsgPack item in unpack_msgpack.ForcePathObject("p.datas"))
        {
            UnityEngine.Debug.LogFormat(item.AsString);
        }

        // unpack filedata 
        // unpack_msgpack.ForcePathObject("p.filedata").SaveBytesToFile("C:\\b.png");
        // Console.Read();
    }
 
    void OnEnable ()
    {
        
    }
   
    void Main ()
    {
        
    }
 
    // Use this for initialization
    void Start ()
    {
        
    }
}