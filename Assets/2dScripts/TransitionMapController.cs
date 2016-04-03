using UnityEngine;
using System.Collections;

public class TransitionMapController : MapController2d {

	// Use this for initialization
	protected override void Start () {
		base.Start();
		RefreshUVs ();
	}

	public void RefreshTransitions(MapController2d baseMap) {

		int[,] neighbors = { {0,1}, {1,0}, {0,-1}, {-1,0}, {1,1}, {1,-1}, {-1,-1}, {-1,1}, };

		for (int x = 0; x < tileCols; x++) {
			for (int y = 0; y < tileRows; y++) {
				int tcode = 0;
				if ( baseMap.GetTile (x,y) != 1) {
					for (int n = 0; n < neighbors.GetLength (0); n++) {
						if (baseMap.GetTile(x+neighbors[n,0], y+neighbors[n,1]) == 1) {
							tcode |= (int)Mathf.Pow(2, n);
						}
					}
				}
				SetTile (x, y, tcode+3);
			}
		}
		RefreshUVs();
	}

	// Update is called once per frame
	void Update () {
	
	}
}
