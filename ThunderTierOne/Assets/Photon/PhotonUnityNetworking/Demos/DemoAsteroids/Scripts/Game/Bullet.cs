using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun.Demo.Asteroids
{
    public class Bullet : MonoBehaviour
    {
        
        public float BulletVelocity = 30.0f;
        [SerializeField] GameObject[] BulletImpact;
       
        Rigidbody rb;

        
        public Player Owner { get; private set; }

        public void Start()
        {
            rb = GetComponent<Rigidbody>();
         
        }

        private void FixedUpdate()
        {
            rb.AddForce(transform.forward * BulletVelocity, ForceMode.Impulse);
        }

        private void OnCollisionEnter(Collision collision)
        {
            switch (collision.transform.tag)
            {
                case "Metal":
                    Instantiate(BulletImpact[0], transform.position, transform.rotation);
                    //this.gameObject.SetActive(false);
                    Destroy(gameObject);
                    break;
                case "Dirt":
                    Instantiate(BulletImpact[1], transform.position, transform.rotation);
                    //this.gameObject.SetActive(false);
                    Destroy(gameObject);
                    break;
                case "Sand":
                    Instantiate(BulletImpact[2], transform.position, transform.rotation);
                    //this.gameObject.SetActive(false);
                    Destroy(gameObject);
                    break;
                case "Wood":
                    Instantiate(BulletImpact[3], transform.position, transform.rotation);
                    //this.gameObject.SetActive(false);
                    Destroy(gameObject);
                    break;
                case "Ground":
                    Instantiate(BulletImpact[1], transform.position, transform.rotation);
                    //this.gameObject.SetActive(false);
                    Debug.Log("Bullet Hit! [Ground]");
                    Destroy(gameObject);
                    break;
                case "Player":
                case "MyChar":
                    Debug.Log("Bullet Hit! [Player]");
                    Destroy(gameObject);
                    //this.gameObject.SetActive(false);
                    break;
                default:
                    Instantiate(BulletImpact[2], transform.position, transform.rotation);
                    //this.gameObject.SetActive(false);
                    Destroy(gameObject);
                    break;
            }
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