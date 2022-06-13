using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public enum GameState
{ 
VirtuFit,
Simulation
}
public enum AvatarType
{
    ExtraSmall,
    Small,
    Medium,
    Large,
    ExtraLarge
}

public enum ProductType
{
    StitchedCholiAndGhaghra,
    UnstitchedCholiAndGhaghra,
}

[Serializable]
public class AvatarPresetData
{
    [SerializeField] public GameObject preset;
    [SerializeField] public AvatarType type;
}
public class GameManager : GenericSingleton<GameManager>
{
    [SerializeField] public Camera RenderStreamCam = null;
    [SerializeField] private bool isRunFromCmd = false;
     public UnityEvent ModelLoadedEvent;
   public override void Awake()
    {
        Application.targetFrameRate = 30;
        base.Awake();
        if(isRunFromCmd){
            return;
        }
        if (!string.IsNullOrEmpty(AvatarCode))
        {
            AvatarDirectory = $"/home/ubuntu/Documents/VirtuFit/VirtuFit_Models/{AvatarCode}/avatar/model.glb";
        }
        else
        {
#if UNITY_EDITOR
           AvatarDirectory = $"{Application.dataPath}/VirtuFit_Models/{DefaultAvatarCode}/avatar/model.glb";
           TextureDirectory = $"{Application.dataPath}/VirtuFit_Directory";
#endif
#if UNITY_STANDALONE_LINUX
           AvatarDirectory = $"/home/arch/Documents/VirtuFit_Root/VirtuFit_Models/{DefaultAvatarCode}/avatar/model.glb";
           TextureDirectory = $"/home/arch/Documents/VirtuFit_Root/VirtuFit_Textures";
#endif
#if UNITY_STANDALONE_OSX
            AvatarDirectory = $"/Users/hetalchirag/InGnious/RenderStreaming/Assets/VirtuFit_Models/{DefaultAvatarCode}/avatar/model.glb";
            TextureDirectory = $"/Users/hetalchirag/InGnious/RenderStreaming/Assets/VirtuFit_Directory";
#endif

        }
        
    }
#region Hidden Public Variables
    [HideInInspector] public string AvatarDirectory = null;
    [HideInInspector] public AvatarType GlobalAvatarType;

    [HideInInspector]public string TextureDirectory = null;
    [HideInInspector] private string TextureID = 144.ToString();
    [HideInInspector] public GameObject UnstitchedObj;
    [HideInInspector] public Material Unstitched_CholiMat;
    [HideInInspector] public GameObject StitchedObj;
    [HideInInspector] public Material Stitched_CholiMat;
    [HideInInspector] public GameObject GhaghraObj;
    [HideInInspector] public Material GhaghraMat;

    [HideInInspector] public GameObject MyTwin = null;
    [HideInInspector] public string AvatarCode = null;
    [HideInInspector] private string DefaultAvatarCode = "0001";

    
    [HideInInspector] public GameState GlobalGameState;
    [HideInInspector] public ProductType GlobalProductType;

    private Texture2D Texture2D_;
    private byte[] bytes;
#endregion

#region Public Functions
    public void UpdateGameState(GameState newGameState, int id = 0)
    {
        newGameState = GlobalGameState;

        switch (newGameState)
        {
            case GameState.VirtuFit:
                SceneManager.LoadScene("VirtuFit");
                break;
            case GameState.Simulation:
                SceneManager.LoadScene("Ballroom"+id.ToString());
                UpdateProductType(GlobalProductType = ProductType.UnstitchedCholiAndGhaghra);
                break;
        }
#endregion
    }

    public void UpdateAvatarType(AvatarType newAvatarType)
    {
        newAvatarType = GlobalAvatarType;

        switch (newAvatarType)
        {
            case AvatarType.ExtraSmall:
                break;
            case AvatarType.Small:
                break;
            case AvatarType.Medium:
                break;
            case AvatarType.Large:
                break;
            case AvatarType.ExtraLarge:
                break;
        }
    }

    public void UpdateProductType(ProductType newProductType)
    {
        newProductType = GlobalProductType;

        switch (newProductType)
        {
            case ProductType.StitchedCholiAndGhaghra:
                LoadTextures(TextureID, "_1_stitched", Texture2D_, Stitched_CholiMat); 
                LoadTextures(TextureID, "2", Texture2D_, GhaghraMat);
                StitchedObj.SetActive(true);
                UnstitchedObj.SetActive(false);
                break;
            case ProductType.UnstitchedCholiAndGhaghra:
                LoadTextures(TextureID, "1_unstitched", Texture2D_, Unstitched_CholiMat);
                LoadTextures(TextureID, "2", Texture2D_, GhaghraMat);
                UnstitchedObj.SetActive(true);
                StitchedObj.SetActive(false);
                break;
        }
    }

    public void LoadTextures(string code, string subCode, Texture2D texture2D_, Material material_)
    {
        bytes = File.ReadAllBytes($"{TextureDirectory}/{code}/{code}_{subCode}_diffused.png"); 
        texture2D_ = new Texture2D(2, 2);
        texture2D_.hideFlags = HideFlags.HideAndDontSave;

        texture2D_.LoadImage(bytes);
        material_.SetTexture("_MainTex", texture2D_);
        Debug.Log(texture2D_);

        bytes = File.ReadAllBytes($"{TextureDirectory}/{code}/{code}_{subCode}_normal.png");
        texture2D_ = new Texture2D(2, 2);
        texture2D_.hideFlags = HideFlags.HideAndDontSave;
        texture2D_.LoadImage(bytes);
        material_.SetTexture("_BumpMap", texture2D_);

    }
    public void UpdateAvatarDirectory()
    {
             {
#if UNITY_EDITOR
           AvatarDirectory = $"{Application.dataPath}/VirtuFit_Models/{DefaultAvatarCode}/avatar/model.glb";
           TextureDirectory = $"{Application.dataPath}/VirtuFit_Directory";
#endif
#if UNITY_STANDALONE_LINUX
           AvatarDirectory = $"/home/arch/Documents/VirtuFit_Root/VirtuFit_Models/{DefaultAvatarCode}/avatar/model.glb";
           TextureDirectory = $"/home/arch/Documents/VirtuFit_Root/VirtuFit_Textures";
#endif
#if UNITY_STANDALONE_OSX
            AvatarDirectory = $"/Users/hetalchirag/InGnious/RenderStreaming/Assets/VirtuFit_Models/{AvatarCode}/avatar/model.glb";
            TextureDirectory = $"/Users/hetalchirag/InGnious/RenderStreaming/Assets/VirtuFit_Directory";
#endif

        }
    }
    public void UpdateAvatarCode(string aNewAvatarCode = null, string aSku = null)
    {
        if(aNewAvatarCode == null){
           AvatarCode = DefaultAvatarCode; 
        }else{
            AvatarCode = aNewAvatarCode;
        }
      if(aSku == null){
           TextureID = "144"; 
        }else{
            TextureID = aSku;
        }
        UpdateAvatarDirectory();
    }

    public void GetReferences()
    {
        StitchedObj = GameObject.FindGameObjectWithTag("StitchedCholi");
        UnstitchedObj = GameObject.FindGameObjectWithTag("UnstitchedCholi");
        GhaghraObj = GameObject.FindGameObjectWithTag("Ghaghra");
    }

    public void InvokeModelLoadedEvent(){

         if (ModelLoadedEvent != null)
        {
            ModelLoadedEvent.Invoke();
        }
    }
}