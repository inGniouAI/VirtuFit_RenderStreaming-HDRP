using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Siccity.GLTFUtility;
using System;
using ingenious.models;
using System.IO;

public class VirtuFit : GenericSingleton<VirtuFit>
{
#region Hidden Public Variables
    public string apparelName = null;
    public Material MyTwinSkin;
    public Avatar animationRig;
    public AccessoriesData AccessoriesData;
#endregion

#region Exposed Private Variables
    [SerializeField] private List<AvatarPresetData> avatarPresets;
    [SerializeField]public GameObject GetPresetOfType(AvatarType type_)
    {
        return avatarPresets.Find((x) => x.type == type_).preset;
    }
#endregion

#region Hidden Private Variables
    private Animator anim;
    private bool animate = false;
    private Texture2D Texture2D_;
    private byte[] bytes;
    private Transform ParentOfNecklace;
    private Transform ParentOfLBangle;
    private Transform ParentOfRBangle;
    private GameObject necklace;
    private GameObject Bangles1;
    private GameObject Bangles2;
#endregion

#region GLB Model Loading

    public void ImportGLBAsync(string filepath)
    {
        if (GameManager.Instance.MyTwin != null)
        {
            return;
        }
        AWSManager.Instance.GetS3Object(GameManager.Instance.AvatarDirectory);
        StartCoroutine(ModelLoader());
    }


    IEnumerator ModelLoader()
    {
         yield return new WaitUntil(() => AWSManager.Instance.ObjectDownloaded == true);
         GameManager.Instance.MyTwin = Importer.LoadFromBytes(AWSManager.Instance.data);
         GameManager.Instance.MyTwin.gameObject.name = "MyTwin";
         Debug.Log($"MyTwin Is Loaded with CustomerID {GameManager.Instance.AvatarCode}");
         StartCoroutine(GameManager.Instance.LoadJsonData(GameManager.Instance.AvatarCode, GameManager.Instance.TextureID));
         yield return new WaitWhile(() => AWSManager.Instance.ObjectDownloaded == true);
         LoadClothing();
         Animate();
         GameManager.Instance.GetReferences();
         GameManager.Instance.MyTwin.tag = "Focus";
         DontDestroyOnLoad(GameManager.Instance.MyTwin);
         GameManager.Instance.InvokeModelLoadedEvent();
    }
    #endregion

#region Apparels
    public void LoadClothing()
    {
        if (GameManager.Instance.MyTwin != null)
        {
            apparelName = "Choli_Stitched";
            SkinMeshTransfer(GameManager.Instance.MyTwin.transform, apparelName);
            apparelName = "Choli_Unstitched";
            SkinMeshTransfer(GameManager.Instance.MyTwin.transform, apparelName);
            apparelName = "Hair_Type1";
            SkinMeshTransfer(GameManager.Instance.MyTwin.transform, apparelName);
            ClothCopy(GameManager.Instance.MyTwin.transform);
        }
    }
    ModelData mData;
    void SkinMeshTransfer(Transform Target, string apparel)
    {
        GameObject avatarOld = GetPresetOfType(GameManager.Instance.GlobalAvatarType);
        Transform[] allGameobjects = avatarOld.GetComponentsInChildren<Transform>();
        Transform[] allGameobjectsNew = Target.GetComponentsInChildren<Transform>();

         mData = avatarOld.GetComponent<ModelData>();

        foreach (var source in allGameobjects)
        {
            bool copyGameObject = false;
            Transform parentToSearch = null;
            if (mData.objectsToCopy.Contains(source.gameObject))
            {
                copyGameObject = true;
                parentToSearch = source.parent;
            }

            foreach (var destination in allGameobjectsNew)
            {
                // for objects are around the skeleton
                if (copyGameObject && destination.gameObject.name.Equals(mData.skeletonRoot.name))
                {
                    if (source.gameObject.name.Equals(apparel))
                    {
                        SkinnedMeshRenderer smrOld = source.GetComponent<SkinnedMeshRenderer>();
                        SkinnedMeshRenderer smrNew = Instantiate(source, destination.parent).GetComponent<SkinnedMeshRenderer>();
                        smrNew.rootBone = destination.transform;

                        Transform[] newBones = new Transform[smrOld.bones.Length];
                        for (int i = 0; i < smrOld.bones.Length; i++)
                        {

                            foreach (var newBone in destination.GetComponentsInChildren<Transform>())
                            {
                                if (newBone.name == smrOld.bones[i].name)
                                {
                                    newBones[i] = newBone;
                                    continue;
                                }
                            }

                        }
                        smrNew.bones = newBones;
                    }
                }
            }
        }
    }

    void ClothCopy(Transform Target)
    {
        GameObject avatarOld = GetPresetOfType(GameManager.Instance.GlobalAvatarType);
        Transform[] allGameobjects = avatarOld.GetComponentsInChildren<Transform>();
        Transform[] allGameobjectsNew = Target.GetComponentsInChildren<Transform>();

        ModelData mData = avatarOld.GetComponent<ModelData>();

        foreach (var source in allGameobjects)
        {
            bool copyGameObject = false;

            Transform parentToSearch = null;
            if (mData.objectsToCopy.Contains(source.gameObject))
            {
                copyGameObject = true;
                parentToSearch = source.parent;
            }
            foreach (var destination in allGameobjectsNew)
            {
                if(ParentOfNecklace == null && destination.gameObject.name.Equals("Neck")){
                    ParentOfNecklace = destination.gameObject.transform;
                }else  if(ParentOfLBangle == null && destination.gameObject.name.Equals("L_Wrist")){
                    ParentOfLBangle = destination.gameObject.transform;
                }else  if(ParentOfRBangle == null && destination.gameObject.name.Equals("R_Wrist")){
                    ParentOfRBangle = destination.gameObject.transform;
                }

                if (copyGameObject && destination.gameObject.name.Equals("Pelvis"))
                {
                    switch (source.gameObject.name)
                    {
                        case "Ghaghra":
                            Instantiate(source, destination);
                            break;
                        case "Ghagra_Collider":
                            Instantiate(source, destination);
                            break;
                        case "Saree":
                            Instantiate(source, destination);
                            break;
                    }
                }
                if (source.gameObject.name.Equals(destination.gameObject.name))
                {
                    CapsuleCollider[] capsules = source.GetComponents<CapsuleCollider>();
                    foreach (var cc in capsules)
                    {
                        CapsuleCollider c = destination.gameObject.AddComponent<CapsuleCollider>();
                        c.center = cc.center;
                        c.radius = cc.radius;
                        c.height = cc.height;
                        c.direction = cc.direction;
                    }
                    break;
                }
            }

        }
        Cloth clothSim = Target.GetComponentInChildren<Cloth>();
        if (clothSim != null)
        {
            clothSim.capsuleColliders = Target.GetComponentsInChildren<CapsuleCollider>();
        }
    }
    #endregion

#region Animator
    public void Animate()
    {
        if (GameManager.Instance.MyTwin != null)
        {
            anim = GameManager.Instance.MyTwin.AddComponent<Animator>();
            anim.enabled = animate;
            Animation();
            anim.avatar = animationRig;
            anim.applyRootMotion = true;
            anim.updateMode = AnimatorUpdateMode.AnimatePhysics;
            anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }
    }
    private void Animation()
    {
        if (AvatarType.ExtraSmall == GameManager.Instance.GlobalAvatarType)
            anim.runtimeAnimatorController = Resources.Load("Animation/Virtufit_Genric") as RuntimeAnimatorController;
        if (AvatarType.Small == GameManager.Instance.GlobalAvatarType)
            anim.runtimeAnimatorController = Resources.Load("Animation/Virtufit_Genric") as RuntimeAnimatorController;
        if (AvatarType.Medium == GameManager.Instance.GlobalAvatarType)
            anim.runtimeAnimatorController = Resources.Load("Animation/Virtufit_Genric") as RuntimeAnimatorController;
        if (AvatarType.Large == GameManager.Instance.GlobalAvatarType)
            anim.runtimeAnimatorController = Resources.Load("Animation/Virtufit_Genric") as RuntimeAnimatorController;
        if (AvatarType.ExtraLarge == GameManager.Instance.GlobalAvatarType)
            anim.runtimeAnimatorController = Resources.Load("Animation/Virtufit_XL") as RuntimeAnimatorController;
    }
    #endregion

#region Accessories Change
    public void ChangeNeckLace(int id){
        if(necklace!=null){
            Destroy(necklace);
        }
        necklace = Instantiate(AccessoriesData.NecklaceList[id],ParentOfNecklace);
    } 
    public void ChangeBangles(int id){
        if(Bangles1!=null){
            Destroy(Bangles1);
        }
         if(Bangles2!=null){
            Destroy(Bangles2);
        }
        Bangles1 = Instantiate(AccessoriesData.BanglesList[id],ParentOfLBangle);
        Bangles2 = Instantiate(AccessoriesData.BanglesList[id],ParentOfRBangle);

    }
    #endregion

}