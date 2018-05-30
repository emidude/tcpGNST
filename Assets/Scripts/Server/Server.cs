using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Experimental.UIElements;

public class Server : MonoBehaviour
{

    [SerializeField]
    public GameObject playVideoButton;

    [SerializeField]
    public GameObject resetButton;

    [SerializeField]
    public GameObject resetOrientationButton;

    [SerializeField]
    public GameObject loginWindow;

    private bool readyToPlayVideo;
    private bool resetVisible;

    private List<ServerClient> clients;
    private List<ServerClient> disconnectList;

    public int port = 6321;
    private TcpListener server;
    private bool serverStarted;

    private void Start()
    {
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        readyToPlayVideo = false;
        resetVisible = false;

        resetOrientationButton.SetActive(false);

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start(); //starts listening

            startListening();
            serverStarted = true;

            Debug.Log("server has been started on port: " + port.ToString());

        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }

    }

    private void Update()
    {
        if (!serverStarted)
            return;

       // foreach (ServerClient c in clients)
        for (int i = 0; i < clients.Count; i++)
        {
            ServerClient c = clients[i];//
            //If the client is not connected then close the tcp, remove the client from the clients list and broadcast a disconnected message with it's temporarily stored client name

            //is the clinet still connected?
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                string disconnectingClient = clients[i].clientName;//

                clients.RemoveAt(i);//
               // disconnectList.Add(c);
                Debug.Log("client: " + c + " disconnected.");
                i--;
                //continue;
            }

            //check for messages from the client
            else
            {
                NetworkStream s = c.tcp.GetStream();

                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(c, data);
                    }
                }
            
            }
        }

        for (int i = 0; i < disconnectList.Count - 1; i++)
        {
            Broadcast(disconnectList[i].clientName + "has disconnected", clients);
            Debug.Log(disconnectList[i].clientName + "has disconnected");
           // clients.Remove(disconnectList[i]);
           // disconnectList.RemoveAt(i);

           
        }

    
        if (readyToPlayVideo)
        {
            resetButton.SetActive(false);
            playVideoButton.SetActive(true);
            resetOrientationButton.SetActive(true);
            //loginWindow.SetActive(false);
            readyToPlayVideo = false;

            Broadcast("PLAYLOOPVIDEO", clients);
        }

        if (resetVisible)
        {
            resetButton.SetActive(true);
            playVideoButton.SetActive(false);
            resetVisible = false;

        }
    }

    private void OnIncomingData(ServerClient c, string data)
    {
        if (data.Contains("&NAME"))
        {
            c.clientName = data.Split('|')[1];
            Broadcast(c.clientName + "has connected", clients);
            return;
        }
        Debug.Log(c.clientName + " has sent the following message: " + data);
        Broadcast(c.clientName + " : " + data, clients);
    }

    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient c in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error " + e.Message + " to client " + c.clientName);
            }
        }
    }

    private bool IsConnected(TcpClient tcp)
    {
        // throw new NotImplementedException();
        try
        {
            if (tcp != null && tcp.Client != null && tcp.Client.Connected)
            {
                ////mystery code - put this in when everything works
                //if (tcp.Client.Poll(0, SelectMode.SelectRead))
                //{
                //    return !(tcp.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                //}
                return true;

            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void startListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar))); //add the connected client to the list of clients   
        startListening();

        //send a message to everyone that someone has connected
       // Broadcast(clients[clients.Count - 1].clientName + " has connected.", clients);
        Broadcast("%NAME", new List<ServerClient> { clients[clients.Count - 1] });

        //MAKE PLAY VIDEO BUTTON ACTIVE
        //playVideoButton.SetActive(true); [SET ACTIVE CAN ONLY BE CALLED IN MAIN THREAD]
        readyToPlayVideo = true;
    }

    public void OnPlayVideoButtonClicked()
    {
        //send message to the client to play video
        Broadcast("PLAYVIDEO", clients);
        resetVisible = true;

    }

    public void SendReset()
    {
        Broadcast("RESETVIDEO", clients);
        readyToPlayVideo = true;
    }

    public void OnResetOrientationClicked()
    {
        Broadcast("RESETORIENTATION", clients);
    }

}

public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }

}