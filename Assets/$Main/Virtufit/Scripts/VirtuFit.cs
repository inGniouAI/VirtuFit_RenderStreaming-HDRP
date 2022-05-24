using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Siccity.GLTFUtility;
using System;
using ingenious.models;
using System.IO;

public class VirtuFit : GenericSingleton<VirtuFit>
{
    [SerializeField] private List<AvatarPresetData> avatarPresets;
    public string apparelName = null;
    private GameObject myTwin;
    private Animator anim;
    private bool animate = true;

    public Avatar animationRig;

    public GameObject GetPresetOfType(AvatarType type_)
    {
        return avatarPresets.Find((x) => x.type == type_).preset;
    }

    private void Start()
    {
      //  ImportGLBAsync(GameManager.Instance.AvatarDirectory);
        GameManager.Instance.UpdateGameState(GameManager.Instance.GlobalGameState = GameState.Simulation);

    }

    #region GLTF Model Loading

    public void ImportGLBAsync(string filepath)
    {
        Importer.ImportGLBAsync(filepath, new ImportSettings(), OnFinishAsync);
    }
    private void OnFinishAsync(GameObject result, AnimationClip[] clips)
    {
        GameManager.Instance.MyTwin = result;
        GameManager.Instance.InvokeModelLoadedEvent();
        DontDestroyOnLoad(GameManager.Instance.MyTwin);
        LoadClothing();
        GameManager.Instance.GetReferences();
        Animate();
       
    }
    #endregion

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

    void SkinMeshTransfer(Transform Target, string apparel)
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
        Debug.Log("Current avatar Type = " + GameManager.Instance.GlobalAvatarType);
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
                if (copyGameObject && destination.gameObject.name.Equals("Pelvis"))
                {
                    switch (source.gameObject.name)
                    {
                        case "Ghaghra":
                            Instantiate(source, destination);
                            break;
                        case "Ghaghra_Collider":
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

    public void Animate()
    {
        if (GameManager.Instance.MyTwin != null)
        {
            myTwin = GameManager.Instance.MyTwin;
            anim = myTwin.AddComponent<Animator>();
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
}
