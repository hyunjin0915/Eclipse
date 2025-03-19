using UnityEngine;

public class MenuAudioController : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip audioClipNEWS;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.PlayOneShot(audioClipNEWS);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
