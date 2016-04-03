using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ParticleSortScript : MonoBehaviour {

	public string layerName;

	// Use this for initialization
	void Start () {
		GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingLayerName = layerName;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
