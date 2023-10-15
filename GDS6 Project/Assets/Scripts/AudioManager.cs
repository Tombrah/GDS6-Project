using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour

{
    public AudioSource roundsound;
    public AudioSource BGM;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
    }

    private void GameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if(GameManager.Instance.IsCountdownActive())
        {
            roundsound.Play();
        }
        else if(GameManager.Instance.IsGamePlaying())
        {
            if(!BGM.isPlaying)
            {
                BGM.Play();
            }
            
        }    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
