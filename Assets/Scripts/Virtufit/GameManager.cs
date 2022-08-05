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
    StitchedCholiAndSaree,
    UnstitchedCholiAndSaree
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
        base.Awake();
        if (isRunFromCmd) {
            return;
        }
    }
    #region Hidden Public Variables
    [HideInInspector] public string AvatarDirectory = null;
    [HideInInspector] public AvatarType GlobalAvatarType;

    [HideInInspector] public string TextureDirectory = null;
    [HideInInspector] public string TextureID = null;
    [HideInInspector] public string DefaultTextureID = "144";
    [HideInInspector] public string DefaultAvatarCode = "0001";
    [SerializeField] public GameObject[] Apparels;
    [SerializeField] public Material[] ApparelTextures;
    [SerializeField] public bool AnimationState;

    [HideInInspector] public GameObject MyTwin = null;
    [HideInInspector] public string AvatarCode = null;


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
                AparelManager();
                SceneManager.LoadScene($"Ballroom_{id.ToString()}");
                break;
        }
        #endregion
    }

    private void AparelManager()
    {
        if (File.Exists($"{TextureDirectory}/{TextureID}/{TextureID}_1_stitched_diffused.png"))
        {
            UpdateProductType(GlobalProductType = ProductType.StitchedCholiAndGhaghra);
            MyTwin.GetComponent<Animator>().enabled = true;
            Debug.Log($"{GlobalProductType}");
        }
        if (File.Exists($"{TextureDirectory}/{TextureID}/{TextureID}_1_unstitched_diffused.png"))
        {
            UpdateProductType(GlobalProductType = ProductType.UnstitchedCholiAndGhaghra);
            MyTwin.GetComponent<Animator>().enabled = true;
            Debug.Log($"{GlobalProductType}");
        }
        if (File.Exists($"{TextureDirectory}/{TextureID}/{TextureID}_S1_stitched_diffused.png"))
        {
            UpdateProductType(GlobalProductType = ProductType.StitchedCholiAndSaree);
            MyTwin.GetComponent<Animator>().enabled = false;
            Debug.Log($"{GlobalProductType}");
        }
        if (File.Exists($"{TextureDirectory}/{TextureID}/{TextureID}_S1_unstitched_diffused.png"))
        {
            UpdateProductType(GlobalProductType = ProductType.UnstitchedCholiAndSaree);
            MyTwin.GetComponent<Animator>().enabled = false;
            Debug.Log($"{GlobalProductType}");
        }
    }

    public void AvatarTypeManager()
    {
        if (File.Exists($"/home/arch/Documents/VirtuFit_Root/VirtuFit_Models/{AvatarCode}/avatar/XS.txt"))
        {
            UpdateAvatarType(GlobalAvatarType = AvatarType.ExtraSmall);
            Debug.Log($"{GlobalAvatarType}");
        }
        if (File.Exists($"/home/arch/Documents/VirtuFit_Root/VirtuFit_Models/{AvatarCode}/avatar/S.txt"))
        {
            UpdateAvatarType(GlobalAvatarType = AvatarType.Small);
            Debug.Log($"{GlobalAvatarType}");
        }
        if (File.Exists($"/home/arch/Documents/VirtuFit_Root/VirtuFit_Models/{AvatarCode}/avatar/M.txt"))
        {
            UpdateAvatarType(GlobalAvatarType = AvatarType.Medium);
            Debug.Log($"{GlobalAvatarType}");
        }
        if (File.Exists($"/home/arch/Documents/VirtuFit_Root/VirtuFit_Models/{AvatarCode}/avatar/L.txt"))
        {
            UpdateAvatarType(GlobalAvatarType = AvatarType.Large);
            Debug.Log($"{GlobalAvatarType}");
        }
        if (File.Exists($"/home/arch/Documents/VirtuFit_Root/VirtuFit_Models/{AvatarCode}/avatar/XL.txt"))
        {
            UpdateAvatarType(GlobalAvatarType = AvatarType.ExtraLarge);
            Debug.Log($"{GlobalAvatarType}");
        }
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
                LoadTextures(TextureID, "1_stitched", Texture2D_, ApparelTextures[0]);
                LoadTextures(TextureID, "2", Texture2D_, ApparelTextures[2]);
                Apparels[0].SetActive(true);
                Apparels[2].SetActive(true);
                Apparels[1].SetActive(false);
                Apparels[3].SetActive(false);
                break;
            case ProductType.UnstitchedCholiAndGhaghra:
                LoadTextures(TextureID, "1_unstitched", Texture2D_, ApparelTextures[1]);
                LoadTextures(TextureID, "2", Texture2D_, ApparelTextures[2]);
                Apparels[1].SetActive(true);
                Apparels[2].SetActive(true);
                Apparels[0].SetActive(false);
                Apparels[3].SetActive(false);
                break;
            case ProductType.StitchedCholiAndSaree:
                LoadTextures(TextureID, "S1_stitched", Texture2D_, ApparelTextures[0]);
                LoadTextures(TextureID, "3", Texture2D_, ApparelTextures[3]);
                Apparels[0].SetActive(true);
                Apparels[3].SetActive(true);
                Apparels[2].SetActive(false);
                Apparels[1].SetActive(false);
                break;
            case ProductType.UnstitchedCholiAndSaree:
                LoadTextures(TextureID, "S1_unstitched", Texture2D_, ApparelTextures[1]);
                LoadTextures(TextureID, "3", Texture2D_, ApparelTextures[3]);
                Apparels[1].SetActive(true);
                Apparels[3].SetActive(true);
                Apparels[0].SetActive(false);
                Apparels[2].SetActive(false);
                break;
        }
    }

    public void LoadTextures(string code, string subCode, Texture2D texture2D_, Material material_)
    {
        bytes = File.ReadAllBytes($"{TextureDirectory}/{code}/{code}_{subCode}_diffused.png");
        texture2D_ = new Texture2D(2, 2);
        texture2D_.hideFlags = HideFlags.HideAndDontSave;
        texture2D_.LoadImage(bytes);
        material_.SetTexture("Texture2D_6ad6f414dfb74c20ae5d06011f2ba9ac", texture2D_);
        Debug.Log(texture2D_);

        bytes = File.ReadAllBytes($"{TextureDirectory}/{code}/{code}_{subCode}_normal.png");
        texture2D_ = new Texture2D(2, 2);
        texture2D_.hideFlags = HideFlags.HideAndDontSave;
        texture2D_.LoadImage(bytes);
        material_.SetTexture("Texture2D_223d41bb0338467abb2b4c71b5026b14", texture2D_);
    }
    public void UpdateAvatarDirectory()
    {
        if (!string.IsNullOrEmpty(AvatarCode))
        {
            AvatarDirectory = $"avatars/{AvatarCode}/model.glb";
            TextureDirectory = $"C:/Users/inGnious AI Pvt Ltd/Documents/VirtuFit_Root/VirtuFit_Textures";
        }
    }

        public void UpdateAvatarCode(string aNewAvatarCode = null, string aSku = null)
        {
            if (aNewAvatarCode == null)
            {
                AvatarCode = DefaultAvatarCode;
                Debug.LogWarning("Default Avatar Loaded");
            }
            else
            {
                AvatarCode = aNewAvatarCode;
            }
            if (aSku == null)
            {
                TextureID = DefaultTextureID;

                Debug.LogWarning("Default Textures Loaded");
            }
            else
            {
                TextureID = aSku;
            }
            Debug.LogWarning($"Received Avatar Code : {AvatarCode} & Texture ID : {TextureID}"); // Verifies the variable String

            UpdateAvatarDirectory();
        }

        public void GetReferences()
        {
            Apparels[0] = GameObject.FindGameObjectWithTag("StitchedCholi");
            Apparels[1] = GameObject.FindGameObjectWithTag("UnstitchedCholi");
            Apparels[2] = GameObject.FindGameObjectWithTag("Ghaghra");
            Apparels[3] = GameObject.FindGameObjectWithTag("Saree");
        }

        public void InvokeModelLoadedEvent() {

            if (ModelLoadedEvent != null)
            {
                ModelLoadedEvent.Invoke();
            }
        }
    }
