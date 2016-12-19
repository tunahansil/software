using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BallController3D : MonoBehaviour {

 	public Rigidbody rb ;
	public Vector3 startPos;
	public Vector3 direction, endPos;
	private DateTime dt1,dt2;
	private double touchPow=0;
	private List<Vector3> shotPath=new List<Vector3>(200);
	private int pathIndex=0;
	public GameObject progress;
	public GameObject ok;
	public AIController3D ai;
	public GameObject zemin;
	public AIController.Takım takım; // Hangi tarafta oynuyorsak o.. şu an kırmızı (şu anki mantığa göre biz hep kırmızı karşı taraf hep mavi)
	public bool touching=false;
	// Use this for initialization
	void Start () {
		rb=  GetComponent<Rigidbody>();
		ai=zemin.GetComponent<AIController3D>();
	}
	


	public Vector3 getVel(){ 
		return rb.velocity;
	}

	// Update is called once per frame
	void Update () {
		//if(ai.whoHasTheBall ==takım){
			if(Input.GetMouseButtonDown(0)){
				startPos= Input.mousePosition;
				dt1=DateTime.Now;
				touching=true;
			}
			if(Input.GetMouseButtonUp(0)){
			//mouse'u bıraktı, direction'ı ikisi arasındaki farktan hesapla		
				dt2=DateTime.Now;	
				direction= Input.mousePosition-startPos;
				onTouchEnd();
				touching=false;
				shotPath.Clear();
				pathIndex=0;
			}
			if(touching){
				var v3 = Input.mousePosition;
 				v3.z = 10;
 				v3 = Camera.main.ScreenToWorldPoint(v3);
				shotPath.Add(v3);
				//Debug.Log(shotPath.Count);
			}
		//}
		//resizeOk();
		falsoEkle();
		limitBallSpeed();
	}
	void limitBallSpeed(){
		var vel =getVel();
		if(vel.magnitude > 40){
			rb.velocity= Vector3.ClampMagnitude(vel,40);
		}
	}

	void resizeProgress(){
		if(touching){
			//touchPow=Convert.ToSingle((dt2-dt1).TotalMilliseconds)*1f;
			progress.transform.localScale+=new Vector3(0.1F, 0, 0);
			
		}else{
			progress.transform.localScale=new Vector3(0f,1f,0f);
		}
		progress.transform.position=new Vector3(this.transform.position.x,this.transform.position.y-1);
	}
	void resizeOk(){
		if(touching){
			ok.active=true;	
			direction= Input.mousePosition-startPos;		
         	float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
         	ok.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}else{
			ok.active=false;
		}
		ok.transform.position=transform.position;
	}
	//oyuncu mouse'u bırakırsa
	private void onTouchEnd(){
			touchPow=Convert.ToSingle((dt2-dt1).TotalMilliseconds);		
		//	Debug.Log(direction);
			if(touchPow < 100){
				var v3 = Input.mousePosition;
 				v3.z = 15;
 				v3 = Camera.main.ScreenToWorldPoint(v3);
				ai.setMovementTarget(v3);
			}
			Push(direction.x*800/(float)(touchPow*1),direction.y*800/(float)(touchPow*1));//hızlı atmak için kısa sürede çok çek
	}
	
	public void Push(float xF,float zF){
		Vector3 v3Force =new Vector3(xF,0,zF);
 		rb.AddForce(v3Force * Time.deltaTime);
	}
	void falsoEkle(){
		if(pathIndex<shotPath.Count){
			Push((shotPath[pathIndex].x-transform.position.x)*15,(shotPath[pathIndex].z-transform.position.z)*15);			
			pathIndex++;
		}		
		//Debug.Log(shotPath.Count);
	}

}
