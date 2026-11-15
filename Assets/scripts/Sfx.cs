using System.Collections.Generic;
using UnityEngine;

// Quản lý âm thanh toàn game. Tự khởi tạo khi game chạy, không cần đặt vào scene.
// Âm thanh nằm trong Assets/Resources/Audio2 (ưu tiên) hoặc Assets/Resources/Audio.
// Phím M: bật/tắt nhạc nền.
public class Sfx : MonoBehaviour
{
    static Sfx instance;

    // Chỉnh âm lượng tổng ở đây (0 đến 1)
    public float sfxVolume = 0.7f;
    public float musicVolume = 0.22f;

    AudioSource musicSource;
    AudioSource[] pool;
    int poolIndex;
    readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        if (instance != null) return;
        GameObject go = new GameObject("Sfx");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<Sfx>();
    }

    void Awake()
    {
        pool = new AudioSource[10];
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = gameObject.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
            pool[i].spatialBlend = 0f;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
        musicSource.clip = Load("music_ambient");
        if (musicSource.clip != null) musicSource.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            musicSource.mute = !musicSource.mute;
    }

    AudioClip Load(string clipName)
    {
        AudioClip clip;
        if (cache.TryGetValue(clipName, out clip)) return clip;

        clip = Resources.Load<AudioClip>("Audio2/" + clipName);
        if (clip == null) clip = Resources.Load<AudioClip>("Audio/" + clipName);

        if (clip == null)
            UnityEngine.Debug.LogWarning("Sfx: không tìm thấy âm thanh '" + clipName + "' trong Assets/Resources/Audio2 hoặc Audio");

        cache[clipName] = clip;
        return clip;
    }

    // Âm thanh 2D (không có vị trí)
    public static void Play(string clipName, float volume = 1f, float pitch = 1f)
    {
        if (instance == null) return;

        AudioClip clip = instance.Load(clipName);
        if (clip == null) return;

        AudioSource s = instance.pool[instance.poolIndex];
        instance.poolIndex = (instance.poolIndex + 1) % instance.pool.Length;

        s.pitch = pitch;
        s.volume = volume * instance.sfxVolume;
        s.clip = clip;
        s.Play();
    }

    // Âm thanh 3D tại một vị trí: nhỏ dần theo khoảng cách (2m đến 30m)
    public static void PlayAt(string clipName, Vector3 position, float volume = 1f)
    {
        if (instance == null) return;

        AudioClip clip = instance.Load(clipName);
        if (clip == null) return;

        GameObject go = new GameObject("Sfx3D");
        go.transform.position = position;

        AudioSource a = go.AddComponent<AudioSource>();
        a.clip = clip;
        a.volume = volume * instance.sfxVolume;
        a.spatialBlend = 0.85f;
        a.rolloffMode = AudioRolloffMode.Linear;
        a.minDistance = 2f;
        a.maxDistance = 30f;
        a.Play();

        Destroy(go, clip.length + 0.3f);
    }

    // Dùng cho vòng lặp của vật thể (ví dụ tiếng ù của laser)
    public static AudioClip GetClip(string clipName)
    {
        return instance != null ? instance.Load(clipName) : null;
    }
}
