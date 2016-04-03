using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class MapController2d : MonoBehaviour {

	public float tileSize = 20.0f;
	public int[] dimensions = {1, 1};
	public float extrudeSize = 1e-10f;

	public int atlasWidth = 17;
	public int atlasHeight = 16;

	Vector3[] verts = null;
	int[] tris = null;
	Vector3[] norms = null;
	Vector2[] uvs = null;
	int[,] tiles = null;

	protected int tileRows;
	protected int tileCols;
	protected int numTiles;
	protected int vertRows;
	protected int vertCols;
	protected int numVerts;

	// Use this for initialization
	protected virtual void Start () {
		tileRows = dimensions[1];
		tileCols = dimensions[0];
		numTiles = tileRows*tileCols;
		vertRows = tileRows*2;
		vertCols = tileCols*2;
		numVerts = vertRows*vertCols;

		tiles = new int[tileCols,tileRows];
		BuildMesh();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Vector2[] GetUVs(int tileIndex) {
		int x = tileIndex%atlasWidth;
		int y = atlasHeight - 1 - tileIndex/atlasWidth;

		return new Vector2[] {
			new Vector2((float)(x)/atlasWidth, (float)(y+1)/atlasHeight),
			new Vector2((float)(x+1)/atlasWidth, (float)(y+1)/atlasHeight),
			new Vector2((float)(x)/atlasWidth, (float)(y)/atlasHeight),
			new Vector2((float)(x+1)/atlasWidth, (float)(y)/atlasHeight)};
	}

	public int[] VertexIndices(int x, int y) {
		return new int[] {
			2*x + 2*y*vertCols,	2*x + 2*y*vertCols +1,
			2*x + (2*y+1)*vertCols, 2*x + (2*y+1)*vertCols +1, };
	}

	public void SetTile(int x, int y, int tile) {
		//update the tile array
		tiles[x,y] = tile;

		//update the uv mesh data (this does not refresh the mesh on screen)
		Vector2[] tempUV = GetUVs(tile);
		int[] vindexes = VertexIndices(x, y);
		for (int i = 0; i < 4; i++) {
			uvs[vindexes[i]] = tempUV[i];
		}
	}

	public int GetTile(int x, int y) {
		if ( x >= 0 && y >= 0 && x < tileCols && y < tileRows ) {
			return tiles[x,y];
		}
		else {
			return -1;
		}
	}

	public void RefreshUVs() {
		MeshFilter mf = GetComponent<MeshFilter>();
		mf.sharedMesh.uv = uvs;
	}

	public Vector3 WorldToMapSpace(Vector3 worldPos) {
		return worldPos - transform.position;
	}

	public Vector3 MapToWorldSpace(Vector3 mapPos) {
		return mapPos + transform.position;
	}

	public Vector2 MapToTileSpace(Vector3 mapPos) {
		return new Vector2(mapPos.x/tileSize + 0.5f*tileCols, mapPos.y/tileSize + 0.5f*tileRows);
	}

	public Vector2 WorldToTileSpace(Vector3 worldPos) {
		return MapToTileSpace ( WorldToMapSpace(worldPos));
	}

	public Vector3 TileToMapSpace(Vector2 tilePos) {
		return new Vector3(tileSize*(tilePos.x - 0.5f*tileCols), 
		                   tileSize*(tilePos.y - 0.5f*tileRows), 
		                   0.0f);
	}

	public Vector3 TileToWorldSpace(Vector2 tilePos) {
		return MapToWorldSpace (TileToMapSpace (tilePos));
	}

	public Vector3 TileCenterMapSpace(int x, int y) {
		return new Vector3(tileSize*(x+0.5f - 0.5f*tileCols), 
		                   tileSize*(y+0.5f - 0.5f*tileRows), 
		                   0.0f);
	}

	public Vector3 TileCenterWorldSpace(int x, int y) {
		return MapToWorldSpace (TileCenterMapSpace (x, y));
	}

	protected void BuildMesh() {
		//generate mesh data
		verts = new Vector3[numVerts];
		tris = new int[numTiles * 6];
		norms = new Vector3[numVerts];
		uvs = new Vector2[numVerts];

		int triIndex = 0;
		for ( int x = 0; x < tileCols; ++x) {
			for (int y=0; y < tileRows; ++y) {
				int[] vindexes = VertexIndices(x, y);

				verts[vindexes[0]] = new Vector3(tileSize*(x+0.0f - 0.5f*tileCols)-extrudeSize, 
				                                 tileSize*(y+1.0f - 0.5f*tileRows)+extrudeSize, 0.0f);
				verts[vindexes[1]] = new Vector3(tileSize*(x+1.0f - 0.5f*tileCols)+extrudeSize, 
				                                 tileSize*(y+1.0f - 0.5f*tileRows)+extrudeSize, 0.0f);
				verts[vindexes[2]] = new Vector3(tileSize*(x+0.0f - 0.5f*tileCols)-extrudeSize, 
				                                 tileSize*(y+0.0f - 0.5f*tileRows)-extrudeSize, 0.0f);
				verts[vindexes[3]] = new Vector3(tileSize*(x+1.0f - 0.5f*tileCols)+extrudeSize, 
				                                 tileSize*(y+0.0f - 0.5f*tileRows)-extrudeSize, 0.0f);

				tris[triIndex+0] = vindexes[0];
				tris[triIndex+1] = vindexes[3];
	            tris[triIndex+2] = vindexes[2];
	            tris[triIndex+3] = vindexes[0];
	            tris[triIndex+4] = vindexes[1];
	            tris[triIndex+5] = vindexes[3];
				triIndex += 6;
			}
		}

		for (int i = 0; i < norms.Length; i++) {
			norms[i] = new Vector3(0,0,1);
		}

		//create new mesh populated with data
		Mesh mesh = new Mesh();
		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.normals = norms;

		//assign mesh to components
		MeshFilter mf = GetComponent<MeshFilter>();
		MeshRenderer mr = GetComponent<MeshRenderer>();

		if ( mf == null) {
			Debug.LogError("No mesh filter found");
		}
		if ( mr == null) {
			Debug.LogError("No mesh renderer found");
		}

		mf.mesh = mesh;
	}
}
