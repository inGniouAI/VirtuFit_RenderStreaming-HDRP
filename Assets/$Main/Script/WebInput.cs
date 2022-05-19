using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.RenderStreaming;
using Unity.WebRTC;
using System;
public class WebInput : WebBrowserInputChannelReceiver
{
  RTCDataChannel channel;

   public override void SetChannel(string connectionId, RTCDataChannel channel){

          this.channel = channel;
        base.SetChannel(connectionId,channel);
     if (channel == null)
            {
               
            }
            else
            {
                
                channel.OnMessage += VirtuFitProcessInput;
               
            }
           


    }
    enum EventType
    {
        stringinput = 6,
    }
    int inputtextenum;

        public void VirtuFitProcessInput(byte[] bytes){
             
            if (bytes == null)
                throw new ArgumentNullException();
            if(bytes.Length == 0)
                throw new ArgumentException("byte length is zero");

            string str = Convert.ToBase64String(bytes);
            Int32.TryParse(str[0].ToString(),out inputtextenum);
                  
             switch ((EventType)inputtextenum)
            {
                case EventType.stringinput:
                   Debug.Log("match 6 = "+str[0]);
        
                    break;
            
            }
        }

    public override void OnButtonClick(int elementId)
        {
           Debug.Log("WebInput Sku number " +elementId);
        }
}
