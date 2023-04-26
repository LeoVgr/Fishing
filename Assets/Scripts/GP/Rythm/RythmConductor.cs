using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.GP.Input;
using UnityEngine.InputSystem;

namespace Core.GP.Rythm
{
    [RequireComponent(typeof(AudioSource))]
    public class RythmConductor : MonoBehaviour
    {
        public float Bpm; // Bpm of the song
        public float Crotchet; // Time duration of a beat, calculated from the bpm
        public float SongPosition; // songposition = (float)(AudioSettings.dspTime – dsptimesong) * song.pitch – offset;
        public float Offset; // positive means the song must be minussed!


        public UnityEngine.GameObject cube1;
        public UnityEngine.GameObject cube2;
        public AudioSource clapSource;
        private float lastBeat;
        private double dspTimeSong;
        private AudioSource audioSource;

        public void Start()
        {
            this.Crotchet = 60 / Bpm;

            StartRythm();
            this.cube1.SetActive(true);
            this.cube2.SetActive(false);

            for (var idx = 0; idx < 100; idx++)
                clapSource.PlayScheduled(AudioSettings.dspTime + 3 * idx);
        }

        public void Update()
        {
            if (!this.audioSource.isPlaying)
                return;

            this.SongPosition = (float)(AudioSettings.dspTime - this.dspTimeSong) * this.audioSource.pitch - this.Offset;

            if (this.SongPosition >= this.lastBeat + this.Crotchet)
            {
                this.cube1.SetActive(!this.cube1.activeInHierarchy);
                this.cube2.SetActive(!this.cube2.activeInHierarchy);
                this.lastBeat += this.Crotchet;
            }
        }

        public void StartRythm()
        {
            if (this.audioSource == null)
                this.audioSource = this.GetComponent<AudioSource>();

            this.audioSource.Play();
            this.dspTimeSong = AudioSettings.dspTime;
        }
    }
}
