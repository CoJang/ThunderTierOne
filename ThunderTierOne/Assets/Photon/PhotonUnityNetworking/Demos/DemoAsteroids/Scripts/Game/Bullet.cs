using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.Demo.Asteroids
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] GameObject[] BulletImpact;

        public Player Owner { get; private set; }

        public void Start()
        {
            Destroy(gameObject, 3.0f);
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.other.tag == "Metal")
                Instantiate(BulletImpact[0], transform.position, transform.rotation);
            if (collision.other.tag == "Ground")
                Instantiate(BulletImpact[1], transform.position, transform.rotation);
            if (collision.other.tag == "Wood")
                Instantiate(BulletImpact[2], transform.position, transform.rotation);
            if (collision.other.tag == "Concrete")
                Instantiate(BulletImpact[3], transform.position, transform.rotation);
            Destroy(gameObject);
        }

        public void InitializeBullet(Player owner, Vector3 originalDirection, float lag)
        {
            Owner = owner;

            transform.forward = originalDirection;

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.velocity = originalDirection * 200.0f;
            rigidbody.position += rigidbody.velocity * lag;
        }
    }
}