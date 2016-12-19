using UnityEngine;
using System.Collections;
using System;

public class KaleciController : MonoBehaviour {

	public GameObject ball;
	public BallContoller ballCont;
	// Use this for initialization
	void Start () {
		ballCont= ball.GetComponent<BallContoller>();
	}
	
	// Update is called once per frame
	void Update () {
		var y=ball.transform.position.y;
		if(Math.Abs(y)<3){
			setPostY(ball.transform.position.y);
		}else{	//topu eski pozisyonuna getir
			setPostY(0.0f);	
		}
	}
	void setPostY(float d_y){
		float speed=Math.Abs(ballCont.getVel().x);
		var pos =transform.position;
		pos.y=d_y;
		transform.position = Vector3.Lerp(this.transform.position, pos, speed/120);
	}
}
