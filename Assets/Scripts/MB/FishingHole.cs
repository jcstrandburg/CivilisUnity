using UnityEngine;
using System.Collections.Generic;

public class FishingHole : MonoBehaviour {

    [SerializeField, DontSaveField]
    private GameObject[] fishies;
    [SerializeField, DontSaveField]
    private Vector3[] targets;

    public float fishRange = 2.0f;
    public float fishMoveSpeed = 0.05f;
    public float fishTurnSpeed = 0.1f;

    void Start() {
        var objects = new List<GameObject>();
        foreach (Transform t in transform) {
            if (t.name == "fishy") {
                objects.Add(t.gameObject);
            }
        }
        fishies = objects.ToArray();

        targets = new Vector3[fishies.Length];
        for (int i = 0; i < fishies.Length; i++) {
            targets[i] = fishies[i].transform.position;
        }
    }

	void FixedUpdate() {
        for (int i = 0; i < fishies.Length; i++) {
            Vector3 diff = targets[i] - fishies[i].transform.position;
            if (diff.magnitude <= fishMoveSpeed*10) {
                var offset = new Vector3(
                    Random.Range(-fishRange, fishRange),
                    -1.0f,//Random.Range(-2.0f, -1.0f),
                    Random.Range(-fishRange, fishRange));
                targets[i] = transform.position + offset;
            } else {
                fishies[i].transform.forward = Vector3.Lerp(
                    fishies[i].transform.forward, 
                    diff, 
                    fishTurnSpeed);
                //fishies[i].transform.position += fishMoveSpeed * diff.normalized;
                fishies[i].transform.position += fishies[i].transform.forward.normalized * fishMoveSpeed;
            }
        }
    }
}
