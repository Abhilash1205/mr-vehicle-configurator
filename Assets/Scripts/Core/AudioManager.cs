using UnityEngine;

/// <summary>
/// Manages all sound effects for the vehicle configurator
/// Centralized audio system for UI sounds, car sounds, and ambient audio
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource uiAudioSource;      // For UI clicks
    [SerializeField] private AudioSource carAudioSource;     // For car sounds (doors, engine)
    [SerializeField] private AudioSource ambientAudioSource; // For background/ambient sounds
    
    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;     // Optional
    
    [Header("Car Door Sounds")]
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip doorCloseSound;
    
    [Header("Engine Sounds")]
    [SerializeField] private AudioClip engineStartSound;
    [SerializeField] private AudioClip engineStopSound;
    [SerializeField] private AudioClip engineIdleLoop;       // Optional - for continuous idle
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float uiVolume = 0.5f;
    [Range(0f, 1f)]
    [SerializeField] private float carVolume = 0.8f;
    [Range(0f, 1f)]
    [SerializeField] private float ambientVolume = 0.3f;
    
    // Singleton instance
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional - keeps audio manager across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        SetupAudioSources();
    }
    
    private void SetupAudioSources()
    {
        // Create audio sources if they don't exist
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
            uiAudioSource.volume = uiVolume;
        }
        
        if (carAudioSource == null)
        {
            carAudioSource = gameObject.AddComponent<AudioSource>();
            carAudioSource.playOnAwake = false;
            carAudioSource.volume = carVolume;
            carAudioSource.spatialBlend = 0f; // 2D sound (not 3D positioned)
        }
        
        if (ambientAudioSource == null)
        {
            ambientAudioSource = gameObject.AddComponent<AudioSource>();
            ambientAudioSource.playOnAwake = false;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.loop = true; // Ambient sounds usually loop
        }
    }
    
    #region UI Sounds
    
    /// <summary>
    /// Play button click sound
    /// </summary>
    public void PlayButtonClick()
    {
        if (buttonClickSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSound, uiVolume);
        }
    }
    
    /// <summary>
    /// Play button hover sound (optional)
    /// </summary>
    public void PlayButtonHover()
    {
        if (buttonHoverSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(buttonHoverSound, uiVolume * 0.5f); // Quieter than click
        }
    }
    
    #endregion
    
    #region Car Door Sounds
    
    /// <summary>
    /// Play door opening sound
    /// </summary>
    public void PlayDoorOpen()
    {
        if (doorOpenSound != null && carAudioSource != null)
        {
            carAudioSource.PlayOneShot(doorOpenSound, carVolume);
            Debug.Log("Playing door open sound");
        }
    }
    
    /// <summary>
    /// Play door closing sound
    /// </summary>
    public void PlayDoorClose()
    {
        if (doorCloseSound != null && carAudioSource != null)
        {
            carAudioSource.PlayOneShot(doorCloseSound, carVolume);
            Debug.Log("Playing door close sound");
        }
    }
    
    #endregion
    
    #region Engine Sounds
    
    /// <summary>
    /// Play engine start sound
    /// </summary>
    public void PlayEngineStart()
    {
        if (engineStartSound != null && carAudioSource != null)
        {
            carAudioSource.PlayOneShot(engineStartSound, carVolume);
            Debug.Log("Playing engine start sound");
            
            // Optional: Start idle loop after start sound finishes
            if (engineIdleLoop != null)
            {
                Invoke(nameof(StartEngineIdle), engineStartSound.length);
            }
        }
    }
    
    /// <summary>
    /// Play engine stop sound
    /// </summary>
    public void PlayEngineStop()
    {
        // Stop idle loop if playing
        StopEngineIdle();
        
        if (engineStopSound != null && carAudioSource != null)
        {
            carAudioSource.PlayOneShot(engineStopSound, carVolume * 0.7f);
            Debug.Log("Playing engine stop sound");
        }
    }
    
    /// <summary>
    /// Start engine idle loop (continuous sound)
    /// </summary>
    private void StartEngineIdle()
    {
        if (engineIdleLoop != null && carAudioSource != null && !carAudioSource.isPlaying)
        {
            carAudioSource.clip = engineIdleLoop;
            carAudioSource.loop = true;
            carAudioSource.volume = carVolume * 0.5f; // Quieter for ambient idle
            carAudioSource.Play();
            Debug.Log("Engine idle loop started");
        }
    }
    
    /// <summary>
    /// Stop engine idle loop
    /// </summary>
    public void StopEngineIdle()
    {
        if (carAudioSource != null && carAudioSource.isPlaying && carAudioSource.clip == engineIdleLoop)
        {
            carAudioSource.Stop();
            carAudioSource.loop = false;
            Debug.Log("Engine idle loop stopped");
        }
    }
    
    #endregion
    
    #region Volume Control
    
    /// <summary>
    /// Set UI sounds volume
    /// </summary>
    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        if (uiAudioSource != null)
        {
            uiAudioSource.volume = uiVolume;
        }
    }
    
    /// <summary>
    /// Set car sounds volume
    /// </summary>
    public void SetCarVolume(float volume)
    {
        carVolume = Mathf.Clamp01(volume);
        if (carAudioSource != null)
        {
            carAudioSource.volume = carVolume;
        }
    }
    
    /// <summary>
    /// Mute all sounds
    /// </summary>
    public void MuteAll(bool mute)
    {
        if (uiAudioSource != null) uiAudioSource.mute = mute;
        if (carAudioSource != null) carAudioSource.mute = mute;
        if (ambientAudioSource != null) ambientAudioSource.mute = mute;
    }
    
    #endregion
}
