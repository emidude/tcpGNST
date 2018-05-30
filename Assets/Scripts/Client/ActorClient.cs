using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
//using UnityEngine.VR;

public class ActorClient : MonoBehaviour
{

    public GameObject chatContainer;
    public GameObject messagePrefab;

    public string clientName;

    private bool socketReady = false;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    [SerializeField]
    private VideoClip video;

    [SerializeField]
    private VideoClip loopVideo;

    [SerializeField]
    private VideoPlayer videoPlayer;

    [SerializeField]
    public AudioClip audioo;

    private AudioSource audioSource;

    [SerializeField]
    public GameObject loginWindow;

    //private bool hideLoginWindow;


    /// <summary>
    /// 
    /// </summary>

    //public GameObject vidPlayButton;

    //public void Start()
    //{

    //       //set the play video button to not active at the start

    //       //vidPlayButton = GameObject.FindWithTag("playVideoButtonTag");
    //       //vidPlayButton.SetActive(false);

    //      // GameObject.FindWithTag("playVideoButtonTag").SetActive(false);

    //}

    public void Awake()
    {
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
      //  UnityEngine.XR.XRSettings.enabled = false;
        //hideLoginWindow = false;

//        #if UNITY_IOS
//        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
//#else
//        //Set Audio Output to AudioSource
//              audioSource = gameObject.AddComponent<AudioSource>();
//              videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
//              videoPlayer.SetTargetAudioSource(0, audioSource);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioo;

//#endif

    }

    public void ConnectToServer()
    {
        //if already connected, ignore this function
        if (socketReady)
            return;

        //default host / port values
        string host = "127.0.0.1";
        int port = 6321;

        //overwrite default host/port values, if there is something in those boxes
        string h;
        int p;

        //try with find with tag later if everything working...........
        //no do serialise field!
        h = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (h != "")
        {
            host = h;
        }
        int.TryParse(GameObject.Find("PortInput").GetComponent<InputField>().text, out p);
        if (p != 0)
            port = p;

        try
        {

            socket = new TcpClient(host, port);

            stream = socket.GetStream();

            writer = new StreamWriter(stream);

            reader = new StreamReader(stream);

            socketReady = true;

            /////////////////////make play video button appear
            // GameObject.FindWithTag("playVideoButtonTag").SetActive(true);
            //hideLoginWindow = true;

        }
        catch (Exception e)
        {
            Debug.Log("socket error: " + e.Message);

        }
    }

    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                    OnIncomingData(data);
            }
        }
    }

    private void OnIncomingData(string data)
    {
        if (data == "%NAME")
        {
            Send("&NAME|" + clientName);
            return;
        }

        if (data == "PLAYLOOPVIDEO")
        {
            Debug.Log("data == PLAYLOOPVIDEO. Now playing the loop video");

            //need to change this tablet is android too!
//#if UNITY_ANDROID
//            UnityEngine.XR.XRSettings.enabled = true;
//#endif

            //hide login window
            loginWindow.SetActive(false);

            Send("Debug:ClientPlayingLoopVideo");
            //PLAY THE VIDEO
            playLoopVideo();
            return;
        }

        if (data == "PLAYVIDEO")
        {
            Debug.Log("data == PLAYVIDEO. Now playing the video");
            Send("Debug:ClientPlayingMAinVideo");
            //PLAY THE VIDEO
            playVideo();
            return;
        }

        if (data == "RESETVIDEO")
        {
            Debug.Log("data == resetvideo.");
            playLoopVideo();
            return;
        }

        if (data == "RESETORIENTATION")
        {
            OnResetOrientation();
            return;
        }
        Debug.Log("Server: " + data);
        GameObject go = Instantiate(messagePrefab, chatContainer.transform) as GameObject;
        go.GetComponentInChildren<Text>().text = data;

    }



    private void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    public void OnSendButton()
    {
        //FIND WITH TAG LATER? - LOOK UP WAYS OF OPTIMISING FIND
        string message = GameObject.Find("SendInput").GetComponent<InputField>().text;
        Send(message);
    }

    private void CloseSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }

    private void playVideo()
    {
        Debug.Log("playing main video");
        audioSource.Stop();
        videoPlayer.Stop();
        videoPlayer.clip = video;
        videoPlayer.isLooping = false;
        audioSource.Play();
        videoPlayer.Play();

        // StartCoroutine(ChangeToLoopWhenFinished());
    }

    private void playLoopVideo()
    {
        Debug.Log("playing loop video");
        videoPlayer.Stop();
        videoPlayer.clip = loopVideo;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }

    private void OnResetOrientation()
    {
      //  UnityEngine.XR.InputTracking.Recenter();
        Send("recentering orientation!");
    }
}
