using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AIController : MonoBehaviour {
	public int SERVER =1;
	public int CLIENT =0;
	public int Server_Client=0;// 1->Server 0-> Client	
	public List<GameObject> o1List,o2List;
	public List<OyuncuGucu> o1Gucler=new List<OyuncuGucu>(),o2Gucler=new List<OyuncuGucu>();
	public List<Vector3> orginList1, orginList2, attackList1, attackList2, defList1, defList2, adefList1, adefList2, aattackList1, aattackList2;
	public GameObject ball, o5,o8,o2,o3,o4,o7,o9,o11, o1Closest,o2Closest, kaleci1,kaleci2;
	public BallContoller ballCont;
	public NewtorkController network;
	public Vector3 movementTarget=new Vector3(-1,-1,-1);
	private bool targetUp =false, isRunning=false,topGeldi=false, isConnected =false;
	public GameObject target,ps;
	private Vector3 ballPushVal_InformServer;
	private bool ballPush=false;
	String _in;
	/*
	RED-> Kırmızılar (o1List)
	BLUE-> Maviler (o2List)
	NONE-> top havada, muhtemelen pas neyin verilmiştir henüz kimsenin topu değil
	*/
	void Awake() {
        Application.targetFrameRate = 20;
    }
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
	public Takım whoHasTheBall=Takım.RED, prevWho =Takım.RED; 
	
	// Use this for initialization
	void Start () {
		Server_Client=SERVER;
		//Top için yazılan script. topun velocity'sini bulmak için ihtiyaç duyuyoruz
		ballCont= ball.GetComponent<BallContoller>();
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
			orginList1.Add(new Vector3(o1List[i].transform.position.x,o1List[i].transform.position.y));
		}
		for(int i=0;i<o2List.Count;i++){
			orginList2.Add(new Vector3(o2List[i].transform.position.x,o2List[i].transform.position.y));
		}
		for(int i=0;i<o1List.Count;i++){
			attackList1.Add(new Vector3(o1List[i].transform.position.x-4,o1List[i].transform.position.y));
		}
		for(int i=0;i<o2List.Count;i++){
			attackList2.Add(new Vector3(o2List[i].transform.position.x+4,o2List[i].transform.position.y));
		}
		for(int i=0;i<o1List.Count;i++){
			aattackList1.Add(new Vector3(o1List[i].transform.position.x-7,o1List[i].transform.position.y));
		}
		for(int i=0;i<o2List.Count;i++){
			aattackList2.Add(new Vector3(o2List[i].transform.position.x+7,o2List[i].transform.position.y));
		}
		for(int i=0;i<o1List.Count;i++){
			defList1.Add(new Vector3(o1List[i].transform.position.x+2,o1List[i].transform.position.y));
		}
		for(int i=0;i<o2List.Count;i++){
			defList2.Add(new Vector3(o2List[i].transform.position.x-2,o2List[i].transform.position.y));
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
		network= new NewtorkController();
		network.Server_Client =Server_Client;
		network.onConnected =delegate(string s){
			onConnected(null);
		};
		network.onMessage=delegate(string s)
            {
				onMessage(s);
			};
		network.ip="192.168.43.2";
		network.init();
		if(Server_Client==CLIENT){
			isRunning=true;
		}
	}
	/*
	top:0,
	kale1:1,
	2-5,
	6:kale2,
	7-10,
	11:topa vurma kuvveti
	*/
	/*
	0:x1,x2|1:x2,x2
	*/
	void onConnected(string s){
		isRunning=true;
		isConnected=true;
	}
	void onMessage(string _in){
		//if(Server_Client==CLIENT) return;
		Debug.Log("AI ON MESSAGE:"+_in);
		UnityMainThreadDispatcher.Instance().Enqueue(() => {			 
		 	try{		
			 	 var splittedList =_in.Split('|');
				 foreach(var info in splittedList){
					 if(!info.Contains(":")) continue;
					 var numara =int.Parse(info.Split(':')[0]);
					 var inf =info.Replace(numara+":","");
					 var commaSeperated =inf.Split(',');
					 var x =float.Parse(commaSeperated[0]);
				     var y =float.Parse(commaSeperated[1]);
					 //clientPositions[numara]=new Vector3(x,y,-1f);
					 switch (numara)
					 {						 
						 case 0://top
				 			 ball.transform.position=new Vector3(x,y,ball.transform.position.z);
						 break;
						 case 1://kaleci
				 			 kaleci1.transform.position=new Vector3(x,y,kaleci1.transform.position.z);
						 break;
						 case 2://o1[0]
							 o1List[0].transform.position=new Vector3(x,y,o1List[0].transform.position.z);
							 break;
						 case 3://o1[0]
							 o1List[1].transform.position=new Vector3(x,y,o1List[1].transform.position.z);
						 break;
						 case 4://o1[0]
							 o1List[2].transform.position=new Vector3(x,y,o1List[2].transform.position.z);
							 break;
						 case 5://o1[0]
							 o1List[3].transform.position=new Vector3(x,y,o1List[3].transform.position.z);
							 break;
						 case 6://kaleci
				 			 kaleci2.transform.position=new Vector3(x,y,kaleci2.transform.position.z);
							 break;
						 case 7://o1[0]
							 o2List[0].transform.position=new Vector3(x,y,o2List[0].transform.position.z);
							 break;
						 case 8://o1[0]
							 o2List[1].transform.position=new Vector3(x,y,o2List[1].transform.position.z);
							 break;
						 case 9://o1[0]
							 o2List[2].transform.position=new Vector3(x,y,o2List[2].transform.position.z);
							 break;
						 case 10://o1[0]
							 o2List[3].transform.position=new Vector3(x,y,o2List[3].transform.position.z);
							 break;
						case 11://top push pozisyonu 
							 Debug.Log("Server trows ball to :"+x+","+y);
							 ballCont.Push(x,y);
							 break;						
						 default:break;
					 }
				 }
			 }catch(Exception ex){
				 Debug.Log(ex);
			 }
		});		
	}
	public void addBallPush_InformServer(Vector3 pushV){
		ballPushVal_InformServer=pushV;
		string message="11:"+ballPushVal_InformServer.x*100+","+ballPushVal_InformServer.y*100+"|";
		network.sendMessage(message);
	}
	void informServer(){
		//string posInfo ="|"+ball.transform.position.x+","+ball.transform.position.y;
		String message ="";
		network.sendMessage("0:"+ball.transform.position.x+","+ball.transform.position.y+"|");
		for(int i=0;i<o1List.Count;i++){
			message+=(i+2)+":"+o1List[i].transform.position.x+","+o1List[i].transform.position.y+"|";
			//posInfo+=o1List[i].transform.position.x+","+o1List[i].transform.position.y+"|";
		}
		for(int i=0;i<o2List.Count;i++){
			message+=(i+7)+":"+o2List[i].transform.position.x+","+o2List[i].transform.position.y+"|";
			//posInfo+=o2List[i].transform.position.x+","+o2List[i].transform.position.y+"|";
		}
		message+="1:"+kaleci1.transform.position.x+","+kaleci1.transform.position.y+"|";
		message+="6:"+kaleci2.transform.position.x+","+kaleci2.transform.position.y+"|";
		network.sendMessage(message);
	}
	public void santra(){
		ball.transform.position=new Vector3(0,0,-1);
		ball.GetComponent<Rigidbody2D>().velocity=new Vector3(0,0,0);
		isRunning=true;
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log(Server_Client);
		//Debug.Log("isRunning:"+isRunning);
		//basitçe pause tuşu için kullanılmak üzere br değişken
		if(isRunning==false) return;
		if(Server_Client==CLIENT){
		var ballPos =ball.transform.position.x;
		if(whoHasTheBall==Takım.RED){ //top kırmızıda mavi defans
			if(ballPos <-7){//saha uzunluğu sol tarafta -lerde. tam santra 0 noktası. kale direği ise 14.4 gibi bişey. eğer top -3 kadar yakınsa sol kaleye maviler acilen defansa geçsin
				moveTakım(Takım.BLUE,Durum.ACIL_DEFANS);
			}else if(ballPos >-7 && ballPos < -1){ //eğer santra ile -3 arasındaysa defansa dönme yeteli olacaktır
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
			if(ballPos > 7){
				moveTakım(Takım.RED,Durum.ACIL_DEFANS);
			}else if(ballPos < 7 && ballPos > 1){
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
				moveOneTo(o1Closest,movementTarget,2.5f);
			}
			if(whoHasTheBall==Takım.BLUE){
				moveOneTo(o2Closest,movementTarget,2.5f);
			}
		}		
		informServer();
		}else{
		}
		
	}
	//oyuncular buraya doğru koşsun
	//ball controller tarafından (yada başka bir yerden) kullanılmak üzree. targetin (koşulacak yerin) pozisyonunu belirliyor
	public void setMovementTarget(Vector3 v){
		target.active=true;
		movementTarget=v;
		targetUp=true;
		v.z=-2.1f;
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
		var defGeri =t==Takım.RED ? +3: -3;
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
			g.GetComponent<Rigidbody2D>().velocity=force;
		}else{
			g.GetComponent<Rigidbody2D>().velocity=force*2;
		}		
	}
	//top çıkmamışsa (oyun alanındaysa) press yap
	void pressYap(){
		if(ball.transform.position.x < 17 && ball.transform.position.x > -17){
			var x_far_1 =-2;
			var x_far_2 = 2;
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
		pos.z=-2;
		ps.transform.position=pos;
	}
	// gol olduğunda herekes tekrar ilk duruma dönsün. santra yapılacak
	public void setEveryThingBack(){
		for(int i=0;i<o1List.Count;i++){
			o1List[i].transform.position=Vector3.Lerp(o1List[i].transform.position,orginList1[i],0.05f);
			o1List[i].GetComponent<Rigidbody2D>().velocity=new Vector3(0,0,0);
		}
		for(int i=0;i<o2List.Count;i++){
			o2List[i].transform.position=Vector3.Lerp(o2List[i].transform.position,orginList2[i],0.05f);
			o2List[i].GetComponent<Rigidbody2D>().velocity=new Vector3(0,0,0);
		}
		
		isRunning=false;
	}
	//topu eğer hızı 5'ten küçükse, ve en yakın oyunculardan birine (2 tane en yakın var) mesafesi 2 birimden küçükse ver
	void moveBallToClosest(){		 
		 Vector3 position = ball.transform.position;
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
		 var balVel =ballCont.getVel().magnitude;
		 if(redWins){	
			 if(balVel < 15 && distance1 <2.5){
				// o1Closest.transform.position=new Vector3(ball.transform.position.x+2,ball.transform.position.y,-0.1f);
				 ball.transform.position=new Vector3(o1Closest.transform.position.x-1.1f,o1Closest.transform.position.y,-0.1f) ;
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
			 if(balVel< 15  && distance2 <2.5){		
				 ball.transform.position=new Vector3(o2Closest.transform.position.x+1.2f,o2Closest.transform.position.y,-0.1f) ;
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
