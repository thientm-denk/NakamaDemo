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
    
    private string ticket; // save ticket when find match
    private string matchId; // save id when finded match
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
        
        // add unity action to matchmaking event  -> this method will run if socket get a matchmaking ticket from socket
        socket.ReceivedMatchmakerMatched += OnReciveMatchMakingMatched;
        socket.ReceivedMatchState += OnReciveMatchState;
        // 
        Debug.Log(session);
        Debug.Log(socket);
    }

   

    public async void FindGame()
    {
        Debug.Log("Finding match");

        var matchMakingTicket = await socket.AddMatchmakerAsync("*", 2, 2);
        ticket = matchMakingTicket.Ticket;
    }

    public async void Ping()
    {
        Debug.Log("Pinging");

        await socket.SendMatchStateAsync(matchId, 1, "", null);
    }


    private async void OnReciveMatchMakingMatched(IMatchmakerMatched matchmakerMatched)
    {
        var match = await socket.JoinMatchAsync(matchmakerMatched);
        matchId = match.Id;
        // this session
        Debug.Log("Our session ID: " + match.Self.SessionId);

        foreach (var user in match.Presences) //  user in match
        {
            Debug.Log("Connected user session ID: " + user.SessionId);
        }
    }
    
    private async void OnReciveMatchState(IMatchState matchState)
    {
        if (matchState.OpCode == 1)
        {
            Debug.Log("Receive ping:");
            Debug.Log("Sending pong");
            await socket.SendMatchStateAsync(matchId, 2, "", new[] {matchState.UserPresence});
        }
        
        if (matchState.OpCode == 2)
        {
            Debug.Log("Receive pong:");
            Debug.Log("Sending ping");
            await socket.SendMatchStateAsync(matchId, 1, "", new[] {matchState.UserPresence});
        }
    }
}
