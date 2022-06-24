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
                channel.OnClose += CloseChannel;
               
            }
           


    }
    void CloseChannel(){
        Debug.Log("Datachannel Closed");
        GameManager.Instance.ReloadApplication();
    }
    enum EventType
    {
        stringinput = 6,
        EnvironmentInput = 7,
        NecklaceInput = 8,
        BanglesInput = 9,
        CloseWebapp = 10

    }
    int inputtextenum;
    int inputId = 0;
        public void VirtuFitProcessInput(byte[] bytes){
             
            if (bytes == null)
                throw new ArgumentNullException();
            if(bytes.Length == 0)
                throw new ArgumentException("byte length is zero");

            string str = Convert.ToBase64String(bytes);
            Int32.TryParse(str[0].ToString(),out inputtextenum);
            string trimString;   
             switch ((EventType)inputtextenum)
            {
                case EventType.stringinput:
                    Debug.Log("match 6 = "+ $"{str[0]}");
                    str = str.Remove(0,1);
                    Debug.Log("UpdateAvatarCode = "+ $"{str}");
                    trimString = str;
                    Debug.Log($"{str}");
                    string[] subs = str.Split('/');
                foreach (var sub in subs)
                {
                     Debug.Log("Substring:"+$"{sub}");
                }
                    GameManager.Instance.UpdateAvatarCode(subs[0],subs[1]);    
                    VirtuFit.Instance.ImportGLBAsync(GameManager.Instance.AvatarDirectory);
               break;

                case EventType.EnvironmentInput:
                    Debug.Log("match 7 = "+str[0]);
                    str = str.Remove(0,1);
                    Debug.Log("EnvironmentInput id = "+str);
                    trimString = str;
                    trimString = str.Replace("/","");
                    Debug.Log("trimString = "+trimString);
                    int.TryParse(trimString,out inputId);
                    GameManager.Instance.UpdateGameState(GameManager.Instance.GlobalGameState = GameState.Simulation,inputId);
                break;
            
                case EventType.NecklaceInput:
                    Debug.Log("match 8 = "+str[0]);
                    str = str.Remove(0,1);
                    Debug.Log("NecklaceInput id = "+str);
                    trimString = str;
                    trimString = str.Replace("/","");
                    Debug.Log("trimString = "+trimString);
                    int.TryParse(trimString,out inputId);
                    VirtuFit.Instance.ChangeNeckLace(inputId);
               break;
                case EventType.BanglesInput:
                    Debug.Log("match 9 = "+str[0]);
                    str = str.Remove(0,1);
                    Debug.Log("BanglesInput id = "+str);
                    trimString = str;
                    trimString = str.Replace("/","");
                    Debug.Log("trimString = "+trimString);
                    int.TryParse(trimString,out inputId);
                    VirtuFit.Instance.ChangeBangles(inputId);
               break;
                case EventType.CloseWebapp:
                 Debug.Log("match 10 = "+str[0]);
                 
                 //restart / reload scene

                break;

            }
           }

    public override void OnButtonClick(int elementId)
        {
           Debug.Log("WebInput Sku number " +elementId);
        }
}
