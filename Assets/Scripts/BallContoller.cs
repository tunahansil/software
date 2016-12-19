using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BallContoller : MonoBehaviour {

 	public Rigidbody2D rb ;
	public Vector3 startPos;
	public Vector3 direction, endPos;
	private DateTime dt1,dt2;
	private double touchPow=0;
	private List<Vector3> shotPath=new List<Vector3>(200);
	private int pathIndex=0;
	public GameObject progress;
	public GameObject ok;
	public AIController ai;
	public GameObject zemin;
	public AIController.Takım takım; // Hangi tarafta oynuyorsak o.. şu an kırmızı (şu anki mantığa göre biz hep kırmızı karşı taraf hep mavi)
	public bool touching=false;
	// Use this for initialization
	void Start () {
		rb=  GetComponent<Rigidbody2D>();
		ai=zemin.GetComponent<AIController>();
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
			/*if(touching){
				var v3 = Input.mousePosition;
 				v3.z = 10;
 				v3 = Camera.main.ScreenToWorldPoint(v3);
				shotPath.Add(v3);
				//Debug.Log(shotPath.Count);
			}*/
		//}
		//resizeOk();
		falsoEkle();
		limitBallSpeed();
	}
	void limitBallSpeed(){
		var vel =getVel();
		if(vel.magnitude > 60){
			rb.velocity= Vector3.ClampMagnitude(vel,60);
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
			if(touchPow < 100){
				var v3 = Input.mousePosition;
 				v3.z = 10;
 				v3 = Camera.main.ScreenToWorldPoint(v3);
				ai.setMovementTarget(v3);
			}
			Push(direction.x*400/(float)(touchPow*1),direction.y*400/(float)(touchPow*1));//hızlı atmak için kısa sürede çok çek
	}
	
	public void Push(float xF,float yF){
		Vector3 v3Force =new Vector3(xF,yF,0);
		if(ai.Server_Client == ai.SERVER){
			ai.addBallPush_InformServer(v3Force * Time.deltaTime);
		}else
 			rb.AddForce(v3Force * Time.deltaTime);
		Debug.Log("Ball force:"+v3Force);
	}
	void falsoEkle(){
		/*if(pathIndex<shotPath.Count){
			Push((shotPath[pathIndex].x-transform.position.x)*8,(shotPath[pathIndex].y-transform.position.y)*8);			
			pathIndex++;
		}*/		
		//Debug.Log(shotPath.Count);
	}

}
