using UnityEngine;

public class AUDIO : MonoBehaviour
{
    public AudioSource musica;

    public void MusicaON()
    {
        musica.mute = false;
    }

    public void MusicaOFF()
    {
        musica.mute = true;
    }
}
