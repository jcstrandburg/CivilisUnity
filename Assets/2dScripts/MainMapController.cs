using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class MainMapController : MapController2d {

	public float waterThreshold = 0.25f;
	public float grassThreshold = 0.5f;
	public float transitionRate = 0.07f;

	// Use this for initialization
	protected override void Start () {
		base.Start ();
		Randomize ();
		RefreshUVs();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Randomize() {
		float pval;
		float xBias = Random.Range (0.0f,10.0f);
		float yBias = Random.Range (0.0f,10.0f);

		for (int x = 0; x < tileCols; x++) {
			for (int y = 0; y < tileRows; y++) {
				pval = Mathf.PerlinNoise (xBias + x*transitionRate, yBias + y*transitionRate);
				if ( pval < waterThreshold) {
					SetTile (x, y, 0);
				}
				else if ( pval < grassThreshold) {
					SetTile (x, y, 1);
				}
				else {
					SetTile (x, y, 2);
				}
			}
		}

		GameObject tmap = transform.Find ("Transitions").gameObject;
		if (tmap == null) {
			return;
		}
		TransitionMapController tcontroller = tmap.GetComponent<TransitionMapController>();
		tcontroller.RefreshTransitions(this);
	}
}
