using System.Collections.Generic;
using UnityEngine;

namespace ingenious.models
{
    public class ModelData : MonoBehaviour
    {
        #region Inspector Assignment
        public List<GameObject> objectsToCopy;
        public GameObject skeletonRoot;
        public Transform Neck;
        public Transform LeftWrist;
        public Transform RightWrist;

        #endregion
    }
}