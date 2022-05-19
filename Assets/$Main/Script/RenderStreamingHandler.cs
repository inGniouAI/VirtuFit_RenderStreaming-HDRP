using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RenderStreaming.Signaling;
using Unity.RenderStreaming;
using System.Threading;
using System;
public class RenderStreamingHandler : GenericSingleton<RenderStreamingHandler>
{
    [SerializeField] RenderStreaming renderStreaming;
    public enum SignalingTypeEnum
    {
        WebSocket,
        Http,
        Furioos
    }
    [SerializeField] private  bool enableHWCodec = false;
     [SerializeField]private  SignalingTypeEnum signalingType = SignalingTypeEnum.WebSocket;
     [SerializeField]private  string signalingAddress = "localhost";
     [SerializeField]private  bool signalingSecured = false;
     [SerializeField]private static float signalingInterval = 5;

    public bool EnableHWCodec
        {
            get { return enableHWCodec; }
            set { enableHWCodec = value; }
        }

        public SignalingTypeEnum SignalingType
        {
            get { return signalingType; }
            set { signalingType = value; }
        }

        public string SignalingAddress
        {
            get { return signalingAddress; }
            set { signalingAddress = value; }
        }

        public bool SignalingSecured
        {
            get { return signalingSecured; }
            set { signalingSecured = value; }
        }
        public static float SignalingInterval
        {
            get { return signalingInterval; }
            set { signalingInterval = value; }
        }

        public ISignaling Signaling
        {
            get
            {
                switch (signalingType)
                {
                    case SignalingTypeEnum.Furioos:
                    {
                        var schema = signalingSecured ? "https" : "http";
                        return new FurioosSignaling(
                            $"{schema}://{signalingAddress}", signalingInterval, SynchronizationContext.Current);
                    }
                    case SignalingTypeEnum.WebSocket:
                    {
                        var schema = signalingSecured ? "wss" : "ws";
                        return new WebSocketSignaling(
                            $"{schema}://{signalingAddress}", signalingInterval, SynchronizationContext.Current);
                    }
                    case SignalingTypeEnum.Http:
                    {
                        var schema = signalingSecured ? "https" : "http";
                        return new HttpSignaling(
                            $"{schema}://{signalingAddress}", signalingInterval, SynchronizationContext.Current);
                    }
                }
                throw new InvalidOperationException();
            }
        }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start() from RenderStreamingHandler");
         if (!renderStreaming.runOnAwake)
            {
                Debug.Log("Call from RenderStreamingHandler");
                renderStreaming.Run(
                    hardwareEncoder: EnableHWCodec,
                    signaling: Signaling);
            }
    }

   
}
