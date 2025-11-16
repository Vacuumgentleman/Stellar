using UnityEngine;
using System.Collections.Generic;

public class MusicPlayer : MonoBehaviour
{
    [Header("Lista de pistas de audio")]
    [SerializeField] private List<AudioClip> musicClips = new List<AudioClip>();

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    private List<AudioClip> shuffledList;
    private int currentIndex = 0;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (musicClips.Count == 0)
        {
            Debug.LogWarning("No se han asignado pistas de música.");
            return;
        }

        ShuffleMusicList();
        PlayNextTrack();
    }

    private void Update()
    {
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            PlayNextTrack();
        }
    }

    private void PlayNextTrack()
    {
        if (shuffledList.Count == 0) return;

        audioSource.clip = shuffledList[currentIndex];
        audioSource.Play();

        currentIndex++;
        if (currentIndex >= shuffledList.Count)
        {
            ShuffleMusicList(); // volver a mezclar para próxima ronda
            currentIndex = 0;
        }
    }

    private void ShuffleMusicList()
    {
        shuffledList = new List<AudioClip>(musicClips);
        int n = shuffledList.Count;

        // Fisher-Yates Shuffle
        for (int i = 0; i < n; i++)
        {
            int j = Random.Range(i, n);
            AudioClip temp = shuffledList[i];
            shuffledList[i] = shuffledList[j];
            shuffledList[j] = temp;
        }
    }
}
