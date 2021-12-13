using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BCStrike : MonoBehaviour
{
    public GameObject explosion;
    void OnCollisionEnter(Collision collision)
    {
        // Place explosion Prefab on impact
        var expl = Instantiate(explosion, gameObject.transform.position, new Quaternion());
        Destroy(expl, 1.0f);

        // Reset the strike's position
        gameObject.transform.rotation = new Quaternion();
        gameObject.transform.position = new Vector3(0, -100f, 0);
        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
    }
}
