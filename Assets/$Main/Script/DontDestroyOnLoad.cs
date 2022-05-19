
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    // Start is called before the first frame update
     private void Awake() {
      DontDestroyOnLoad(this.gameObject);

    }
}
