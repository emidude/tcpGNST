using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour {

    [SerializeField]
    private Button playVideoButton;

    [SerializeField]
    private Button resetButton;

    [SerializeField]
    private Button resetOrientationButton;

    [SerializeField]
    private Text delayText;

    [SerializeField]
    private Text ipText;

	// Use this for initialization
	void Awake () {

        playVideoButton.gameObject.SetActive(false);
        resetButton.gameObject.SetActive(false);
      //  resetOrientationButton.gameObject.SetActive(false);

        ipText.text = string.Format("Your ip is: {0}", Network.player.ipAddress);
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    //when the client connects to the server, make the play video button appear (serverSide
    //but actually it will appear server side because only actor app has UserInterface script)

    private void OnClientConnected()
    {
       // waitingForClientText.gameObject.SetActive(false);
     //   playVideoButton.gameObject.SetActive(true);
     //   resetOrientationButton.gameObject.SetActive(true);

    }

}
