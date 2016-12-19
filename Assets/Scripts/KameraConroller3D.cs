using UnityEngine;
using System.Collections;

public class KameraConroller3D : MonoBehaviour {
	
	public GameObject ball;
	// Use this for initialization
	void Start () {
		Screen.orientation = ScreenOrientation.LandscapeLeft;
	}
	
	// Update is called once per frame
	void Update () {
		var ballpos =ball.transform.position;
		var y = ballpos.z < 0 ? -6:1; 
		if(ballpos.x>0){
			transform.position=Vector3.Lerp(transform.position,new Vector3(+4, 10,-y),0.02f);
		}else{
			transform.position=Vector3.Lerp(transform.position,new Vector3(-4, 10,y),0.02f);
		}
	}
}
