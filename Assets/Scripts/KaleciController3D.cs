using UnityEngine;
using System.Collections;
using System;

public class KaleciController3D : MonoBehaviour {

	public GameObject ball;
	public BallController3D ballCont;
	// Use this for initialization
	void Start () {
		ballCont= ball.GetComponent<BallController3D>();
	}
	
	// Update is called once per frame
	void Update () {
		var y=ball.transform.position.z;
		if(Math.Abs(y)<3){
			setPostY(ball.transform.position.z);
		}else{	//topu eski pozisyonuna getir
			setPostY(0.0f);	
		}
	}
	void setPostY(float d_y){
		float speed=Math.Abs(ballCont.getVel().x);
		var pos =transform.position;
		pos.z=d_y;
		var step =speed;
		if(step>22f) step =22f;
		if(step<8) step=8f;
		transform.position = Vector3.Lerp(this.transform.position, pos, step*Time.deltaTime);
	}
}
