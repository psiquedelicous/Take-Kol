using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestructAfter : MonoBehaviour
{
    [SerializeField]
    private float TimeToDestruction = 6.0f; 

    // Start is called before the first frame update
    IEnumerator Start()
    {
        //Self destruct instance of game object with animation after a few seconds
        yield return new WaitForSeconds(TimeToDestruction);
        GameObject.Destroy(this.gameObject);
    }
}
