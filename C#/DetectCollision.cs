using UnityEngine;
using System.Collections;

public class DetectCollision : MonoBehaviour {

    public bool OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == "ground")
        {
            return true;
        }
        else
            return false;
    }
}
