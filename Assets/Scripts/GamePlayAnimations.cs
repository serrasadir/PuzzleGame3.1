using UnityEngine;
using DG.Tweening;


    public class GamePlayAnimations : MonoBehaviour
    {

        public GameObject redParticles;
        public GameObject blueParticles;
        public GameObject yellowParticles;
        public GameObject greenParticles;


        public int numberOfparticles = 3;
        public float explosionForce = 1.0f;
        public float explosionRadius = 1.0f;
        public float upForce = 1.0f;
        public float particleLifetime = 2.0f;

        public void PopAnimation(CubeColor cubeColor)
        {
            
            for (int i = 0; i < numberOfparticles; i++)
            {
                GameObject particleColor = GetParticle(cubeColor);
                Vector3 position = new Vector3(transform.position.x + Random.Range(-0.2f, 0.2f), transform.position.y + Random.Range(-0.2f, 0.2f), 0); //used Random to spread particles randomly
                GameObject particle = Instantiate(particleColor, position, Random.rotation);

                Rigidbody particleRb = particle.AddComponent<Rigidbody>();
                Vector3 explosionDir = (particle.transform.position - transform.position).normalized + Vector3.up * upForce;
                particleRb.AddForce(explosionDir * explosionForce, ForceMode.VelocityChange);
                particle.transform.DOScale(0, particleLifetime).SetEase(Ease.InQuad).OnComplete(() =>
                {
                    Destroy(particle);//destroy particles after the animation
                });

            }
            Destroy(gameObject, particleLifetime);
        }
        private GameObject GetParticle(CubeColor color)
        {
            switch (color)
            {
                case CubeColor.r:
                    return redParticles;
                case CubeColor.b:
                    return blueParticles;
                case CubeColor.y:
                    return yellowParticles;
                case CubeColor.g:
                    return greenParticles;
                default:
                    return null;
            }
        }
    }
