using UnityEngine;
using System.Collections;
using System;
using System.Timers;
public class RulesController3D : MonoBehaviour {

	public GameObject ball, goalSprite;
	float goalVisibleSec;
	bool goalVisible=false;
	public AIController3D ai;


	public enum KaleTip{
		SAG_KALE,SOL_KALE
	}
	// Use this for initialization
	void Start () {
		ai=GetComponent<AIController3D>();
	}
	
	// Update is called once per frame
	void Update () {
		var ballPos =ball.transform.position;
	
		if(ballPos.x >14.2f && (ballPos.z > -2.5f)  && (ballPos.z < 2.5f)){
			onGol(KaleTip.SAG_KALE);
		}
		if(ballPos.x <-14.2f && (ballPos.z > -2.5f)  && (ballPos.z < 2.5f)){
			onGol(KaleTip.SOL_KALE);
		}
		if(goalVisible){
			goalVisibleSec+=Time.deltaTime;
			if(goalVisibleSec > 2){
				goalSprite.active=false;
				goalVisibleSec=0;
				goalVisible=false;
				ai.santra();
			}
		}
	}

	//--------------------------------------------------------
	void onOut(){

	}
	void onKorner(){

	}
	void onGol(KaleTip t){
		goalSprite.active=true;
		goalVisible=true;
		ai.setEveryThingBack();
	}
	void onDevre(){

	}
	void onEnd(){

	}
}
