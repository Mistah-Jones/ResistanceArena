using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OHPAttack : MonoBehaviour
{
    public GameObject shockWave;
    public GameObject shockWaveHitBox;

    void OnCollisionEnter(Collision collision)
    {
        // Only handle collisions at ground level AND don't handle collisions with the hitbox
        if (gameObject.transform.position.y < 0.8 && !collision.gameObject.name.Equals("ShockwaveCollision(Clone)"))
        {
            // Place impact effects
            var swPos = new Vector3
                (
                    gameObject.transform.position.x,
                    0.0f,
                    gameObject.transform.position.z
                );

            var sw = Instantiate(shockWave, swPos, new Quaternion());
            Destroy(sw, 1.0f);
            var swHit = Instantiate(shockWaveHitBox, swPos, new Quaternion());
            Destroy(swHit, 1.0f);

            // Freeze rock in place to simulate "landing"
            gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
