using UnityEngine;
using System.Collections;
using System.IO ; //StreamReader ve StreamWriter siniflari için
using System.Net.Sockets; // Socket, TcpListener ve NetworkStrem siniflari için
using System.Threading;
using System;
using System.Text;
public class NewtorkController {
	
	public string ip{get;set;}
	public int Server_Client { get; set; }
	public static int CLIENT =0, SERVER =1; 
	TcpListener listen;
	TcpClient client;
	public Func<String,Void> onMessage { get; set; }

	public Func<String,Void> onConnected { get; set; }
	Socket IstemciSoketi;
	// Use this for initialization
	void Start () {
		
	}
	public void init(){
		if(Server_Client==SERVER)
			new Thread(new ThreadStart(_listen)).Start();
		else{
			new Thread(new ThreadStart(_listen_Server)).Start();
		}
	}
	void _listen(){
		listen =new TcpListener(9090);
		listen.Start();
		IstemciSoketi = listen.AcceptSocket(); 
		Debug.Log("Bağlandı");
		onConnected(null);
		String message="";
		while (true)
            {
                byte[] b = new byte[100];
                int k = IstemciSoketi.Receive(b);
                string szReceived = Encoding.ASCII.GetString(b, 0, k);
				message+=szReceived;
				Debug.Log("message:"+message);
				var eom_index =message.IndexOf('>');
				while(eom_index!=-1){
					String messageReceived =message.Split('>')[0];
					Debug.Log("onMessage:"+messageReceived);
					onMessage(messageReceived);
					message=message.Replace(messageReceived+">","");
					eom_index =message.IndexOf('>');
				}
            }
	}

	void _listen_Server(){
		client= new TcpClient(ip, 9090);
		String message="";
		while(true){
				byte[] b = new byte[100];
                int k = 100;//client.Receive(b);
				Int32 bytes = client.GetStream().Read(b, 0, k); //(**This receives the data using the byte method**)
				//string responseData = System.Text.Encoding.ASCII.GetString(b, 0, bytes);
                string szReceived = Encoding.ASCII.GetString(b, 0, bytes);
				message+=szReceived;
				Debug.Log("message:"+message);
				var eom_index =message.IndexOf('>');
				while(eom_index!=-1){
					String messageReceived =message.Split('>')[0];
					Debug.Log("onMessage:"+messageReceived);
					onMessage(messageReceived);
					message=message.Replace(messageReceived+">","");
					eom_index =message.IndexOf('>');
				}
		}
	}

	public void sendMessage(String message){

        byte[] outStream = System.Text.Encoding.ASCII.GetBytes(message+">");
		if(Server_Client==CLIENT){
		 	if(client==null) return;
		 	NetworkStream serverStream = client.GetStream();
         	serverStream.Write(outStream, 0, outStream.Length);
         	serverStream.Flush();
		 }else{
			NetworkStream stream =new NetworkStream(IstemciSoketi);
			StreamWriter AkimYazici = new StreamWriter(stream);
			AkimYazici.Write(message+">", 0, message.Length+1);
         	AkimYazici.Flush();
		 }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
