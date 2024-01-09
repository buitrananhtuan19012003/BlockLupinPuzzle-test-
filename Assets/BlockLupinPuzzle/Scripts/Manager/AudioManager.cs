using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.Storage;
using System;

[RequireComponent(typeof(AudioSource))]

public class AudioManager : Singleton<AudioManager>
{
    private float bgmFadeSpeedRate = CONST.BGM_FADE_SPEED_RATE_HIGH;

    //Next BGM name, SE name
    private string nextBGMName;
    private string nextSEName;

    //Is the background music fading out?
    private bool isFadeOut = false;

    //Separate audio sources for BGM and SE
    public AudioSource BGMSource;
    public AudioSource SESource;

    //Keep All Audio
    private Dictionary<string, AudioClip> bgmDic, seDic;

    //protected override void Awake()
    //{
    //    base.Awake();
    //    //Load all SE & BGM files from resource folder
    //    bgmDic = new Dictionary<string, AudioClip>();
    //    seDic = new Dictionary<string, AudioClip>();

    //    object[] bgmList = Resources.LoadAll("Audio/BGM");
    //    object[] seList = Resources.LoadAll("Audio/SE");

    //    foreach (AudioClip bgm in bgmList)
    //    {
    //        bgmDic[bgm.name] = bgm;
    //    }
    //    foreach (AudioClip se in seList)
    //    {
    //        seDic[se.name] = se;
    //    }
    //}

    private void Start()
    {
        BGMSource.volume = ObscuredPrefs.GetFloat(CONST.BGM_VOLUME_KEY, CONST.BGM_VOLUME_DEFAULT);
        SESource.volume = ObscuredPrefs.GetFloat(CONST.SE_VOLUME_KEY, CONST.SE_VOLUME_DEFAULT);
        BGMSource.mute = ObscuredPrefs.GetBool(CONST.BGM_MUTE_KEY, CONST.BGM_MUTE_DEFAULT);
        SESource.mute = ObscuredPrefs.GetBool(CONST.SE_MUTE_KEY, CONST.SE_MUTE_DEFAULT);
    }

    public void PlaySE(string seName, float delay = 0.0f)
    {
        if (!seDic.ContainsKey(seName))
        {
            Debug.Log(seName + "There is no SE named");
            return;
        }

        nextSEName = seName;
        Invoke("DelayPlaySE", delay);
    }

    private void DelayPlaySE()
    {
        SESource.PlayOneShot(seDic[nextSEName] as AudioClip);
    }

    public void PlayBGM(string bgmName, float fadeSpeedRate = CONST.BGM_FADE_SPEED_RATE_HIGH)
    {
        if (!bgmDic.ContainsKey(bgmName))
        {
            Debug.Log(bgmName + "There is no BGM named");
            return;
        }

        //If BGM is not currently playing, play it as is
        if (!BGMSource.isPlaying)
        {
            nextBGMName = "";
            BGMSource.clip = bgmDic[bgmName] as AudioClip;
            BGMSource.Play();
        }
        //When a different BGM is playing, fade out the BGM that is playing before playing the next one.
        //Ignore when the same BGM is playing
        else if (BGMSource.clip.name != bgmName)
        {
            nextBGMName = bgmName;
            FadeOutBGM(fadeSpeedRate);
        }
    }

    public void FadeOutBGM(float fadeSpeedRate = CONST.BGM_FADE_SPEED_RATE_LOW)
    {
        bgmFadeSpeedRate = fadeSpeedRate;
        isFadeOut = true;
    }

    private void Update()
    {
        if (!isFadeOut)
        {
            return;
        }

        //Gradually lower the volume, and when the volume reaches 0
        //return the volume and play the next song
        BGMSource.volume -= Time.deltaTime * bgmFadeSpeedRate;
        if (BGMSource.volume <= 0)
        {
            BGMSource.Stop();
            BGMSource.volume = ObscuredPrefs.GetFloat(CONST.BGM_VOLUME_KEY, CONST.BGM_VOLUME_DEFAULT);
            isFadeOut = false;

            if (!string.IsNullOrEmpty(nextBGMName))
            {
                PlayBGM(nextBGMName);
            }
        }
    }

    public void ChangeBGMVolume(float BGMVolume)
    {
        BGMSource.volume = BGMVolume;
        ObscuredPrefs.SetFloat(CONST.BGM_VOLUME_KEY, BGMVolume);
    }

    public void ChangeSEVolume(float SEVolume)
    {
        SESource.volume = SEVolume;
        ObscuredPrefs.SetFloat(CONST.SE_VOLUME_KEY, SEVolume);
    }

    public void MuteBGM(bool isMute)
    {
        BGMSource.mute = isMute;
        ObscuredPrefs.SetBool(CONST.BGM_MUTE_KEY, isMute);
    }

    public void MuteSE(bool isMute)
    {
        SESource.mute = isMute;
        ObscuredPrefs.SetBool(CONST.SE_MUTE_KEY, isMute);
    }



    public static event Action<bool> OnSoundStatusChangedEvent;
    public static event Action<bool> OnMusicStatusChangedEvent;

    [HideInInspector] public bool isSoundEnabled = true;
    [HideInInspector] public bool isMusicEnabled = true;

    public AudioSource audioSource; //	Source of the audio
    public AudioClip clickSound;    //  Plays this sound on each button click.
    public AudioClip gameOverSound; //	This sound will play on loading gameover screen.

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        initAudioStatus();
    }

    /// <summary>
    /// Inits the audio status.
    /// </summary>
    public void initAudioStatus()
    {
        isSoundEnabled = (PlayerPrefs.GetInt("isSoundEnabled", 0) == 0) ? true : false;
        isMusicEnabled = (PlayerPrefs.GetInt("isMusicEnabled", 0) == 0) ? true : false;

        if ((!isSoundEnabled) && (OnSoundStatusChangedEvent != null))
        {
            OnSoundStatusChangedEvent.Invoke(isSoundEnabled);
        }
        if ((!isMusicEnabled) && (OnMusicStatusChangedEvent != null))
        {
            OnMusicStatusChangedEvent.Invoke(isMusicEnabled);
        }
    }

    /// <summary>
    /// Toggles the sound status.
    /// </summary>
    public void ToggleSoundStatus()
    {
        isSoundEnabled = (isSoundEnabled) ? false : true;
        PlayerPrefs.SetInt("isSoundEnabled", (isSoundEnabled) ? 0 : 1);

        if (OnSoundStatusChangedEvent != null)
        {
            OnSoundStatusChangedEvent.Invoke(isSoundEnabled);
        }
    }

    /// <summary>
    /// Toggles the music status.
    /// </summary>
    public void ToggleMusicStatus()
    {
        isMusicEnabled = (isMusicEnabled) ? false : true;
        PlayerPrefs.SetInt("isMusicEnabled", (isMusicEnabled) ? 0 : 1);

        if (OnMusicStatusChangedEvent != null)
        {
            OnMusicStatusChangedEvent.Invoke(isMusicEnabled);
        }
    }

    /// <summary>
    /// Plaies the button click sound.
    /// </summary>
    public void PlayButtonClickSound()
    {
        if (AudioManager.Instance.isSoundEnabled && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    /// <summary>
    /// Plaies the game over sound.
    /// </summary>
    public void PlayGameOverSound()
    {
        if (AudioManager.Instance.isSoundEnabled)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
    }

    /// <summary>
    /// Plays the sound given.
    /// </summary>
    /// <param name="clip">Clip.</param>
    public void PlaySound(AudioClip clip)
    {
        if (AudioManager.Instance.isSoundEnabled)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}