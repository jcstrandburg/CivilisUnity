using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours {
    [SavableMonobehaviour(16)]
    public class AnimalController : ActorController {

        public bool wild = true;
        public bool startupflag = true;
        public Vector3 targetLocation;
        public Vector3 stupid;
        public float wanderRange = 1.0f;
        public int age = 0;
        public GameObject babyVersion;
        public GameObject adultVersion;

        public bool IsAdult => age > 500;

        public override void Start() {
            base.Start();
            SnapToGround();
            targetLocation = GameController.SnapToGround(transform.position);

            if (IsAdult) {
                babyVersion.SetActive(false);
                adultVersion.SetActive(true);
            } else {
                babyVersion.SetActive(true);
                adultVersion.SetActive(false);
            }
        }

        public override void FixedUpdate() {
            //follow migration path
            if (wild) {
                //Debug.Log(targetLocation + " " + transform.position);
                if (MoveTowards(targetLocation)) {
                    Herd herd = GetComponentInParent<Herd>();

                    targetLocation = herd.GetRabbitLocation();
                    targetLocation = GameController.SnapToGround(targetLocation + wanderRange * (Quaternion.Euler(0, Random.Range(-180, 180), 0) * transform.forward));
                }

                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetLocation - transform.position), 0.2f);
                stupid = transform.position;
            }

            //maturation
            if (!IsAdult) {
                age += 1;
                if (IsAdult) {
                    babyVersion.SetActive(false);
                    adultVersion.SetActive(true);
                }
            }
            else {
                age += 1;
            }
        }
    }
}
