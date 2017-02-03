using UnityEngine;

public class Toast : MonoBehaviour {
    float lifetime = 2.5f;
    string message = "Toast";
	
    public void Init(Vector3 pos, string m, float life=2.5f) {
        transform.position = pos;
        lifetime = life;
        message = m;
    }

    void OnGUI() {
        var pos = Camera.main.WorldToScreenPoint(transform.position);
        var position = new Vector2(pos.x, Screen.height-pos.y);
        var size     = new Vector2(100, 100);
        var rect     = new Rect(position, size);
        GUI.Label(rect, message);
    }

    void FixedUpdate() {
        lifetime -= Time.fixedDeltaTime;
        transform.position += new Vector3(0, .2f, 0);

        if (lifetime <= 0) {
            Destroy(this);
        }
    }
}
