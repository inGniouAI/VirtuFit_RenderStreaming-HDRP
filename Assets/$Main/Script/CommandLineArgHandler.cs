using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandLineArgHandler : MonoBehaviour
{
        private string SignalingUrlParamName = "-SignalingUrl";
        private string AvatarCodeParamName = "-AvatarCode";
        private string SignalingUrl = null;
        private string AvatarCode = null;
          
	string args = "no args";

	void Awake ()
	{
        args = string.Join("\n", System.Environment.GetCommandLineArgs());
        SetParam();
	}
	void OnGUI()
	{
		GUILayout.Label(args);
	}
  private void SetParam()
{           

   var CmdArgs = System.Environment.GetCommandLineArgs();
    for (int i = 0; i < CmdArgs.Length; i++)
    {
        if (CmdArgs[i] == SignalingUrlParamName && CmdArgs.Length > i + 1)
        {
            RenderStreamingHandler.Instance.SignalingAddress = CmdArgs[i+1];
            SignalingUrl = CmdArgs[i+1];
            Debug.Log("SignalingUrl "+SignalingUrl);

        }
        else if( CmdArgs[i] == AvatarCodeParamName && CmdArgs.Length > i + 1){
            GameManager.Instance.UpdateAvatarCode(CmdArgs[i+1]);
            AvatarCode = CmdArgs[i+1];
            Debug.Log("AvatarCode "+AvatarCode);

        }
    }
        Debug.Log("SignalingUrl" + SignalingUrl);

    if(AvatarCode == null){
         GameManager.Instance.UpdateAvatarCode();
    }
}


}
