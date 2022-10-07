using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Newtonsoft.Json;
using System.Text;

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
    public DataClass dataClass;
    public Avatars avatars;
    public Apparels apparels;
    [HideInInspector] public string TextureDirectory = null;
    [HideInInspector] public string TextureID = null;
    [HideInInspector] public string DefaultTextureID = "144";
    [HideInInspector] public string DefaultAvatarCode = "0001";
    [SerializeField] public GameObject[] Apparels;
    [SerializeField] public Material[] ApparelTextures;
    [SerializeField] public bool Textures1 = false;
    [SerializeField] public bool Textures2 = false;
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
                SceneManager.LoadScene($"Ballroom_{id.ToString()}");
                break;
        }
        #endregion
    }

    public IEnumerator AparelManager()
    {
        if (dataClass.apparels.type == "ghagra" && dataClass.apparels.sub_type == "stitched")
        {
            UpdateProductType(GlobalProductType = ProductType.StitchedCholiAndGhaghra);
            MyTwin.GetComponent<Animator>().enabled = true;
            Debug.Log($"{GlobalProductType}");
            StartCoroutine(LoadTextures(TextureID, "1_stitched", Texture2D_, ApparelTextures[0], Textures1));
            yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
            StartCoroutine(LoadTextures(TextureID, "2", Texture2D_, ApparelTextures[2], Textures2));
            yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
            yield return new WaitUntil(() => ApparelTextures[0].GetTexture("Texture2D_6ad6f414dfb74c20ae5d06011f2ba9ac") != null);
            yield return new WaitUntil(() => ApparelTextures[2].GetTexture("Texture2D_6ad6f414dfb74c20ae5d06011f2ba9ac") != null);
            GameManager.Instance.UpdateGameState(GameManager.Instance.GlobalGameState = GameState.Simulation);
        }
        if (dataClass.apparels.type == "ghagra" && dataClass.apparels.sub_type == "unstitched")
        {
            UpdateProductType(GlobalProductType = ProductType.UnstitchedCholiAndGhaghra);
            MyTwin.GetComponent<Animator>().enabled = true;
            Debug.Log($"{GlobalProductType}");
            StartCoroutine(LoadTextures(TextureID, "1_unstitched", Texture2D_, ApparelTextures[1], Textures1));
            yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
            StartCoroutine(LoadTextures(TextureID, "2", Texture2D_, ApparelTextures[2], Textures2));
            yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
            yield return new WaitUntil(() => ApparelTextures[1].GetTexture("Texture2D_6ad6f414dfb74c20ae5d06011f2ba9ac") != null);
            yield return new WaitUntil(() => ApparelTextures[2].GetTexture("Texture2D_6ad6f414dfb74c20ae5d06011f2ba9ac") != null);
            GameManager.Instance.UpdateGameState(GameManager.Instance.GlobalGameState = GameState.Simulation);

        }
        if (dataClass.apparels.type == "saree" && dataClass.apparels.sub_type == "unstitched")
        {
            UpdateProductType(GlobalProductType = ProductType.StitchedCholiAndSaree);
            MyTwin.GetComponent<Animator>().enabled = false;
            Debug.Log($"{GlobalProductType}");
            StartCoroutine(LoadTextures(TextureID, "S1_stitched", Texture2D_, ApparelTextures[0], Textures1));
            yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
            StartCoroutine(LoadTextures(TextureID, "3", Texture2D_, ApparelTextures[3], Textures2));
            yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
            yield return new WaitUntil(() => ApparelTextures[0].GetTexture("Texture2D_6ad6f414dfb74c20ae5d06011f2ba9ac") != null);
            yield return new WaitUntil(() => ApparelTextures[3].GetTexture("Texture2D_6ad6f414dfb74c20ae5d06011f2ba9ac") != null);
            GameManager.Instance.UpdateGameState(GameManager.Instance.GlobalGameState = GameState.Simulation);

        }
        if (dataClass.apparels.type == "saree" && dataClass.apparels.sub_type == "unstitched")
        {
            UpdateProductType(GlobalProductType = ProductType.UnstitchedCholiAndSaree);
            MyTwin.GetComponent<Animator>().enabled = false;
            Debug.Log($"{GlobalProductType}");
            StartCoroutine(LoadTextures(TextureID, "S1_unstitched", Texture2D_, ApparelTextures[1], Textures1));
            yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
            StartCoroutine(LoadTextures(TextureID, "3", Texture2D_, ApparelTextures[3], Textures2));
            yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
            yield return new WaitUntil(() => ApparelTextures[1].GetTexture("Texture2D_6ad6f414dfb74c20ae5d06011f2ba9ac") != null);
            yield return new WaitUntil(() => ApparelTextures[3].GetTexture("Texture2D_6ad6f414dfb74c20ae5d06011f2ba9ac") != null);
            GameManager.Instance.UpdateGameState(GameManager.Instance.GlobalGameState = GameState.Simulation);

        }
    }

    public void AvatarTypeManager()
    {
        if (dataClass.avatars.size == "xs")
        {
            UpdateAvatarType(GlobalAvatarType = AvatarType.ExtraSmall);
            Debug.Log($"{GlobalAvatarType}");
        }
        if (dataClass.avatars.size == "s")
        {
            UpdateAvatarType(GlobalAvatarType = AvatarType.Small);
            Debug.Log($"{GlobalAvatarType}");
        }
        if (dataClass.avatars.size == "m")
        {
            UpdateAvatarType(GlobalAvatarType = AvatarType.Medium);
            Debug.Log($"{GlobalAvatarType}");
        }
        if (dataClass.avatars.size == "l")
        {
            UpdateAvatarType(GlobalAvatarType = AvatarType.Large);
            Debug.Log($"{GlobalAvatarType}");
        }
        if (dataClass.avatars.size == "xl")
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
                Apparels[0].SetActive(true);
                Apparels[2].SetActive(true);
                Apparels[1].SetActive(false);
                Apparels[3].SetActive(false);
                break;
            case ProductType.UnstitchedCholiAndGhaghra:
                Apparels[1].SetActive(true);
                Apparels[2].SetActive(true);
                Apparels[0].SetActive(false);
                Apparels[3].SetActive(false);
                break;
            case ProductType.StitchedCholiAndSaree:
                Apparels[0].SetActive(true);
                Apparels[3].SetActive(true);
                Apparels[2].SetActive(false);
                Apparels[1].SetActive(false);
                break;
            case ProductType.UnstitchedCholiAndSaree:
                Apparels[1].SetActive(true);
                Apparels[3].SetActive(true);
                Apparels[0].SetActive(false);
                Apparels[2].SetActive(false);
                break;
        }
    }

    IEnumerator LoadTextures(string code, string subCode, Texture2D texture2D_, Material material_, bool ApparelDownloaded)
    {
        AWSManager.Instance.GetS3Object($"{TextureDirectory}/{code}_{subCode}_diffused.png");
        yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
        bytes = AWSManager.Instance.data;
        texture2D_ = new Texture2D(2, 2);
        texture2D_.hideFlags = HideFlags.HideAndDontSave;
        texture2D_.LoadImage(bytes);
        material_.SetTexture("Texture2D_6ad6f414dfb74c20ae5d06011f2ba9ac", texture2D_);
        Debug.Log(texture2D_);
        if(texture2D_ != null)
        ApparelDownloaded = true;
    }

    public void UpdateAvatarDirectory()
    {
        if (!string.IsNullOrEmpty(AvatarCode))
        {
            AvatarDirectory = $"avatars/{AvatarCode}/model.glb";
            TextureDirectory = $"apparels/{TextureID}";
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
    #region Json Reader
    public IEnumerator LoadAvatarJson()
    {
        AWSManager.Instance.GetS3Object($"avatars/{AvatarCode}/metadata.json");
        yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
        string json = Encoding.UTF8.GetString(AWSManager.Instance.data);
        Avatars avatarData = JsonConvert.DeserializeObject<Avatars>(json);
        Debug.LogWarning($"The Current Avatar Is {avatarData.size} and its weight is {avatarData.weight}");
        dataClass.avatars.size = avatarData.size;
        dataClass.avatars.weight = avatarData.weight;
        AvatarTypeManager();
    }

    public IEnumerator LoadTextureJson()
    {
        AWSManager.Instance.GetS3Object($"apparels/{TextureID}/metadata.json");
        yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
        string json = Encoding.UTF8.GetString(AWSManager.Instance.data);
        Apparels apparelData = JsonConvert.DeserializeObject<Apparels>(json);
        Debug.LogWarning($"The Current Apparel Is {apparelData.type} and its subtype is {apparelData.sub_type}");
        dataClass.apparels.type = apparelData.type;
        dataClass.apparels.sub_type = apparelData.sub_type;
    }
    #endregion
}


