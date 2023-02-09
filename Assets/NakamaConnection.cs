using System.Collections;
using System.Collections.Generic;
using Nakama;
using UnityEngine;

public class NakamaConnection : MonoBehaviour
{
    // http or https -> this demo using local sever -> http
    private string scheme = "http";
    
    // host name of local sever, can be a local ip or just a "localhost"
    private string hostName = "localhost";
    
    // port to connect, contain in docker compose file, origin = 7350
    // heath check: test
    private int port = 7350;
    
    private string key = "defaultkey";
    
    // more information: https://heroiclabs.com/docs/nakama/concepts/authentication/#authentication

    private IClient client; 
    private ISession session; 
    private ISocket socket; // allow to send and recive message thought nakama sever 
    async void Start()
    {
        // create client for connection
        client = new Client(scheme,hostName,port,key, UnityWebRequestAdapter.Instance);
        
        // connect to nakama, this method to create a user account base on their device identifyer
        // await to make sure this method finish before run next line of code
        session = await client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier);

        // create socket and connect to session (now thought socket we can send and recive message )
        socket = client.NewSocket();
        await socket.ConnectAsync(session, true);
        
        // 
        Debug.Log(session);
        Debug.Log(socket);
    }

   
}
