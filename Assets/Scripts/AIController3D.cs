using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AIController3D : MonoBehaviour {

	public List<GameObject> o1List,o2List;
	public List<OyuncuGucu> o1Gucler=new List<OyuncuGucu>(),o2Gucler=new List<OyuncuGucu>();
	public List<Vector3> orginList1, orginList2, attackList1, attackList2, defList1, defList2, adefList1, adefList2, aattackList1, aattackList2;
	public GameObject ball, o5,o8,o2,o3,o4,o7,o9,o11, o1Closest,o2Closest, prevClosest, kaleci1,kaleci2;
	public BallController3D ballCont;
	public Vector3 movementTarget=new Vector3(-1,-1,-1);
	public bool targetUp =false, isRunning=true, kaleciSahip=false;
	public GameObject target,ps, forvet1, forvet2;
	public float prevDist=0;

	/*
	RED-> Kırmızılar (o1List)
	BLUE-> Maviler (o2List)
	NONE-> top havada, muhtemelen pas neyin verilmiştir henüz kimsenin topu değil
	*/
	public enum Takım{
		RED,BLUE,NONE
	}
	/*
	Takımların oyun davranışlarının listesidir
	adı üstünde zaten defans yap vs..
	*/
	public enum Durum{
		ACIL_DEFANS,
		ACIL_ATAK,
		SAKIN,
		DEFANS,
		ATAK
	}
	/*
	Hangi takımın topa sahip olduğunu anlatır.. whoHasTheBall o anda top kimdeyse ona döner, RED vs gibi
	prevWho ise ondan önce kimdeymiş onu tutar. 
	örneğin kırmızı dan kırmızıya pas gitti. o halde prevWho =RED ve whoHasTheBall =RED
	NOT: Aynı zamanda NONE da olabilirler.
	*/
	public Takım whoHasTheBall=Takım.RED, prevWho =Takım.RED, whichKaleci=Takım.NONE; 
	
	// Use this for initialization
	void Start () {
		//Top için yazılan script. topun velocity'sini bulmak için ihtiyaç duyuyoruz
		ballCont= ball.GetComponent<BallController3D>();
		//kırmızı oyuncuların listesi
		o1List=new List<GameObject>(new GameObject[]{o2,o3,o4,o5});
		//mavi oyuncuların listesi
		o2List=new List<GameObject>(new GameObject[]{o7,o9,o11,o8});
		//kırmızıalrın orijinal posizyonları
		orginList1=new List<Vector3>();
		//aynı şekilde mavilerin. (DURUM=SAKIN olduğunda herkesi ilk yerine çekmek için kullanılıyorlar)
		orginList2=new List<Vector3>();
		//listelerin initialize edilmesi
		for(int i=0;i<o1List.Count;i++){
			orginList1.Add(new Vector3(o1List[i].transform.position.x,0,o1List[i].transform.position.z));
		}
		for(int i=0;i<o2List.Count;i++){
			orginList2.Add(new Vector3(o2List[i].transform.position.x,0,o2List[i].transform.position.z));
		}
		for(int i=0;i<o1List.Count;i++){
			attackList1.Add(new Vector3(o1List[i].transform.position.x-4,0,o1List[i].transform.position.z));
		}
		for(int i=0;i<o2List.Count;i++){
			attackList2.Add(new Vector3(o2List[i].transform.position.x+4,0,o2List[i].transform.position.z));
		}
		for(int i=0;i<o1List.Count;i++){
			aattackList1.Add(new Vector3(o1List[i].transform.position.x-7,0,o1List[i].transform.position.z));
		}
		for(int i=0;i<o2List.Count;i++){
			aattackList2.Add(new Vector3(o2List[i].transform.position.x+7,0,o2List[i].transform.position.z));
		}
		for(int i=0;i<o1List.Count;i++){
			defList1.Add(new Vector3(o1List[i].transform.position.x+2,0,o1List[i].transform.position.z));
		}
		for(int i=0;i<o2List.Count;i++){
			defList2.Add(new Vector3(o2List[i].transform.position.x-2,0,o2List[i].transform.position.z));
		}
		/*
		Oyuncu Güölerini init et*/
		for(int i=0;i<o1List.Count;i++){
			o1Gucler.Add(new OyuncuGucu());
		}
		for(int i=0;i<o2List.Count;i++){
			o2Gucler.Add(new OyuncuGucu());
		}
		o1Gucler[0].topaSahipOlma=68;
		o1Gucler[1].topaSahipOlma=71;
		o1Gucler[2].topaSahipOlma=59;
		o1Gucler[3].topaSahipOlma=77;

		o2Gucler[0].topaSahipOlma=69;
		o2Gucler[1].topaSahipOlma=51;
		o2Gucler[2].topaSahipOlma=74;
		o2Gucler[3].topaSahipOlma=71;

	}
	public void santra(){
		ball.transform.position=new Vector3(0,2,0);
		ball.GetComponent<Rigidbody>().velocity=new Vector3(0,0,0);
		isRunning=true;
	}
	
	// Update is called once per frame
	void Update () {
		//basitçe pause tuşu için kullanılmak üzere br değişken
		if(isRunning==false) return;
		var ballPos =ball.transform.position.x;
		if(kaleciSahip){
			bool devam=true;
			if(whichKaleci==Takım.RED){
				if((kaleci1.transform.position-ball.transform.position).sqrMagnitude > 4) devam=false;
			}
			if(whichKaleci==Takım.BLUE){
				if((kaleci2.transform.position-ball.transform.position).sqrMagnitude > 4) devam=false;
			}
			if(devam){
				moveTakım(Takım.RED,Durum.SAKIN);
				moveTakım(Takım.BLUE,Durum.SAKIN);
			}else{
				kaleciSahip=false;
			}
		}
		else{ 
			if(whoHasTheBall==Takım.RED){ //top kırmızıda mavi defans
				if(ballPos <-5){//saha uzunluğu sol tarafta -lerde. tam santra 0 noktası. kale direği ise 14.4 gibi bişey. eğer top -3 kadar yakınsa sol kaleye maviler acilen defansa geçsin
					moveTakım(Takım.BLUE,Durum.ACIL_DEFANS);
				}else if(ballPos >-5 && ballPos < -1){ //eğer santra ile -3 arasındaysa defansa dönme yeteli olacaktır
					moveTakım(Takım.BLUE,Durum.DEFANS);
				}else if(ballPos > -1){ // eğer hemen hemen satnra noktası civarında ise sakin durun herkes ilk pozisyonuna
					moveTakım(Takım.BLUE,Durum.SAKIN);
				}	
				//KIRMIZI ATAK
				if(ballPos < -2){ //aynı şekilde kırmızılar acil atakta ise bunu -2 noktasından iler gitmeleriyle anlıyoruz
					moveTakım(Takım.RED,Durum.ACIL_ATAK);
				}else{
					moveTakım(Takım.RED,Durum.ATAK); //top kırmızıda ise ve henüz -2 kadar kaleye yaklaşamadıysak o zaman normal atak düzenine geçelim
				}		
			}else if(whoHasTheBall==Takım.BLUE){
				///MAVI ATAK
				if(ballPos >2){
					moveTakım(Takım.BLUE,Durum.ACIL_ATAK);
				}else{
					moveTakım(Takım.BLUE,Durum.ATAK);
				}
				//KIRMIZI DEFANS
				if(ballPos > 5){
					moveTakım(Takım.RED,Durum.ACIL_DEFANS);
				}else if(ballPos < 5 && ballPos > 1){
					moveTakım(Takım.RED,Durum.DEFANS);
				}else if(ballPos < 1){
					moveTakım(Takım.RED,Durum.SAKIN);
				}	
			}
		
			//neredeyse durmuş
			moveBallToClosest(); // topu en yakın adama veriyor
		
			pressYap(); // hem o1Closest hem de o2 closest bu method ile topa doğru koşuyor
					
			if(prevWho!=whoHasTheBall){ //top el değiştirmiş ayarlar çöp
				targetUp=false; //target dediğimiz zımbırtı tek dokunuşla dokunuşla oyuncuları koşturduğun kısım, target up true ise koş
				target.active=false; //target değişkeni GameObject ve o byeaz kutu sprite'i bu
			}
			if(targetUp){
				if(whoHasTheBall==Takım.RED){
					moveOneTo(o1Closest,movementTarget,2.0f);
				}
				if(whoHasTheBall==Takım.BLUE){
					moveOneTo(o2Closest,movementTarget,2.0f);
				}
			}
		}
		
	}
	//oyuncular buraya doğru koşsun
	//ball controller tarafından (yada başka bir yerden) kullanılmak üzree. targetin (koşulacak yerin) pozisyonunu belirliyor
	public void setMovementTarget(Vector3 v){
		target.active=true;
		movementTarget=v;
		targetUp=true;
		v.y=1.6f;
		target.transform.position=v;
	}
	//aldığı takım nesnesini verilen duruma göre sahaya yerleştirir
	//mesela kırmızıları atağa kaldır
	void moveTakım(Takım t,Durum d){		
		var list_to_use = t==Takım.RED ? o1List : o2List;
		var originList= t==Takım.RED ? orginList1 : orginList2;
		var defList= t==Takım.RED ? defList1 : defList2;
		var adefList= t==Takım.RED ? adefList1 : adefList2;
		var attackList= t==Takım.RED ? attackList1 : attackList2;
		var aattackList= t==Takım.RED ? aattackList1 : aattackList2;
		var defGeri =t==Takım.RED ? +2: -2;
		var forvet =t==Takım.RED ? forvet1: forvet2;
		switch (d)
		{			
			case Durum.ACIL_ATAK: 
				for(int i=0;i<list_to_use.Count;i++){					
					moveOneTo(list_to_use[i],aattackList[i],1.5f);
				}
			break;
			case Durum.ACIL_DEFANS: 
				//Debug.Log("ACIL DEFANS");
				for(int i=0;i<list_to_use.Count;i++){					
					moveOneTo(list_to_use[i],ball.transform.position+new Vector3(defGeri,0,0),2);
					if(list_to_use[i]==forvet){
						moveOneTo(list_to_use[i],originList[i],0);
					}
				}
			break;
			case Durum.SAKIN: 
				//Debug.Log("SAKİN");
				for(int i=0;i<list_to_use.Count;i++){					
					moveOneTo(list_to_use[i],originList[i],0);
				}
			break;
			case Durum.DEFANS: 
				//Debug.Log("DEFANS");
				for(int i=0;i<list_to_use.Count;i++){					
					moveOneTo(list_to_use[i],defList[i],0);
				}
			break;
			case Durum.ATAK: 
				for(int i=0;i<list_to_use.Count;i++){					
					moveOneTo(list_to_use[i],attackList[i],0);
				}
			break;
		}
	}
	//bir oyuncuyu bir noktaya verilen hızda koşturur
	//e_force 1 ise unit vektör*2 hızda koşar
	void moveOneTo(GameObject g,Vector3 v, float e_force){
		Vector3 force =(v-g.transform.position);
		if(force.sqrMagnitude > 1){ 
			force.Normalize();
			force*=2f;
			if(e_force>0){
				force*=e_force;
			}
			g.GetComponent<Rigidbody>().velocity=force;
		}else{
			g.GetComponent<Rigidbody>().velocity=force*2;
		}		
	}
	//top çıkmamışsa (oyun alanındaysa) press yap
	void pressYap(){
		if(ball.transform.position.x < 12 && ball.transform.position.x > -12){
			var x_far_1 =-1;
			var x_far_2 = 1;
			moveOneTo(o1Closest,ball.transform.position+new Vector3(x_far_1,0,0),1.5f);
			moveOneTo(o2Closest,ball.transform.position+new Vector3(x_far_2,0,0),1.5f);
		}else{
			moveTakım(Takım.BLUE,Durum.SAKIN);
			moveTakım(Takım.RED,Durum.SAKIN);
		}		
	}
	//ps o anda kontrol ettiğimiz oyuncunun kafasındaki üçgen sprite
	void movePs(GameObject o){
		var pos =o.transform.position;
		pos.y=3;
		ps.transform.position=pos;
	}
	// gol olduğunda herekes tekrar ilk duruma dönsün. santra yapılacak
	public void setEveryThingBack(){
		for(int i=0;i<o1List.Count;i++){
			o1List[i].transform.position=Vector3.Lerp(o1List[i].transform.position,orginList1[i],0.05f);
			o1List[i].GetComponent<Rigidbody>().velocity=new Vector3(0,0,0);
		}
		for(int i=0;i<o2List.Count;i++){
			o2List[i].transform.position=Vector3.Lerp(o2List[i].transform.position,orginList2[i],0.05f);
			o2List[i].GetComponent<Rigidbody>().velocity=new Vector3(0,0,0);
		}
		
		isRunning=false;
	}
	//topu eğer hızı 5'ten küçükse, ve en yakın oyunculardan birine (2 tane en yakın var) mesafesi 2 birimden küçükse ver
	void moveBallToClosest(){		 
		 Vector3 position = ball.transform.position;
		 var balVel =ballCont.getVel().magnitude;
		 var kaleci1Mesafe=kaleci1.transform.position-position;
		 var kaleci2Mesafe=kaleci2.transform.position-position;
		 if(balVel < 5 && kaleci1Mesafe.sqrMagnitude<1.2f){
			 ball.transform.position=new Vector3(kaleci1.transform.position.x+.7f,position.y, kaleci1.transform.position.z);
			 ball.GetComponent<Rigidbody>().velocity=new Vector3(0,0,0);
			 kaleciSahip=true;
			 whichKaleci=Takım.RED;
			 return;
		 }else if(balVel < 5 && kaleci2Mesafe.sqrMagnitude<1.2f){
			 ball.transform.position=new Vector3(kaleci2.transform.position.x-.7f,position.y, kaleci2.transform.position.z);
			 kaleciSahip=true;
			 ball.GetComponent<Rigidbody>().velocity=new Vector3(0,0,0);
			 whichKaleci=Takım.BLUE;
			 return;
		 }
		 o1Closest=getClosest(o1List);
		 o2Closest=getClosest(o2List);
		 var distance1=(o1Closest.transform.position-position).magnitude;
		 var distance2=(o2Closest.transform.position-position).magnitude;
		 //if(distance1 > 3f ){ whoHasTheBall=Takım.NONE; return;}
		 var o1Gucu =o1Gucler[o1List.IndexOf(o1Closest)];
		 var o2Gucu =o2Gucler[o2List.IndexOf(o2Closest)];

		 bool redWins=(distance1 *(o1Gucu.topaSahipOlma) <distance2*(o2Gucu.topaSahipOlma));
		 /*var o1Vel =o1Closest.GetComponent<Rigidbody2D>().velocity;
		 var o2Vel =o1Closest.GetComponent<Rigidbody2D>().velocity;
		 o1Vel.Normalize();
		 o2Vel.Normalize();*/
		 
		 if(redWins){
			 if(balVel < 7 && distance1 <2 ){
				// o1Closest.transform.position=new Vector3(ball.transform.position.x+2,ball.transform.position.y,-0.1f);
				 ball.transform.position=new Vector3(o1Closest.transform.position.x-.7f,2f,o1Closest.transform.position.z);
				// ball.transform.position+=1.2f;
				 prevWho=whoHasTheBall;
			 	 whoHasTheBall=Takım.RED; 
				 movePs(o1Closest);
				 ps.active=true;		
			 } else if(distance1 > 3f){
				 whoHasTheBall=Takım.NONE;
				 ps.active=false;
			 }
		 }else{
			 if(balVel< 7  && distance2 <2){		
				 ball.transform.position=new Vector3(o2Closest.transform.position.x+.7f,2f,o2Closest.transform.position.z);
				 prevWho=whoHasTheBall;
				 whoHasTheBall=Takım.BLUE; 
				 movePs(o2Closest);
				 ps.active=true;
		 	}else if(distance2 > 3f){
				 prevWho=whoHasTheBall;
				 whoHasTheBall=Takım.NONE;
				 ps.active=false;
			 }
		 } 
	}
	GameObject getClosest(List<GameObject> list){
		Vector3 position = ball.transform.position;
		var distance=Mathf.Infinity;
		var selectedOne=list[0];
		foreach(GameObject t in list){
			 var _dist =(t.transform.position-position).sqrMagnitude;
			 if(_dist<distance){
				 distance=_dist;
				 selectedOne=t;
			 }
		 }
		 return selectedOne;
	}
	float GetNormal(Vector3 a, Vector3 b, Vector3 c) {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        return Vector3.Cross(side1, side2).sqrMagnitude;
    }
	public class OyuncuGucu{
		public int hiz { get; set; }
		public int teknik { get; set; }
		public int sut { get; set; }
		public int topaSahipOlma { get; set; }
	}
}
