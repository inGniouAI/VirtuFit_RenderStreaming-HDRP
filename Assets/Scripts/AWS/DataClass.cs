using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class DataClass : MonoBehaviour
{
    [SerializeField]public Avatars avatars;
    [SerializeField]public Apparels apparels;
}

[Serializable]
public class Avatars
{
    public string size;
    public float weight;
}
[Serializable]
public class Apparels
{
    public string type;
    public string sub_type;
}
