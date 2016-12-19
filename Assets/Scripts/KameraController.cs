using UnityEngine;
using System.Collections;

public class KameraController : MonoBehaviour {

	public GameObject ball;
	private bool translated =false;
	// Use this for initialization
	void Start () {
		Screen.orientation = ScreenOrientation.LandscapeLeft;
	}
	
	// Update is called once per frame
	void Update () {
		var ballpos =ball.transform.position;
		var y = ballpos.y < 0 ? -3:3; 
		if(ballpos.x>0){
			transform.position=Vector3.Lerp(transform.position,new Vector3(+4, y,-10),0.02f);
		}else{
			transform.position=Vector3.Lerp(transform.position,new Vector3(-4, y,-10),0.02f);
		}
	}
}
