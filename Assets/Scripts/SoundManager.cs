using UnityEngine;

public enum Sfx
{
    Shoot, ShotgunShoot, RocketShoot, Explosion, Hit, EnemyDeath, PlayerHurt,
    Pickup, DifficultyUp, DifficultyDown, LevelUp, UIClick, BossAlarm, GameOver, PerkSelect
}

// Reproduce los efectos de AudioKit con un pool de AudioSources 2D, para que varios sonidos
// se puedan superponer sin cortarse entre sí (varios disparos + impactos al mismo tiempo, etc).
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private const int PoolSize = 8;
    private AudioSource[] pool;
    private int nextSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        pool = new AudioSource[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            pool[i] = src;
        }
    }

    public static void Play(Sfx sfx, float volume = 1f, float pitch = 1f)
    {
        if (Instance == null) return;
        Instance.PlayInternal(sfx, volume, pitch);
    }

    private void PlayInternal(Sfx sfx, float volume, float pitch)
    {
        AudioClip clip = GetClip(sfx);
        if (clip == null) return;

        AudioSource src = pool[nextSource];
        nextSource = (nextSource + 1) % pool.Length;
        src.pitch = pitch;
        src.PlayOneShot(clip, volume);
    }

    private AudioClip GetClip(Sfx sfx)
    {
        switch (sfx)
        {
            case Sfx.Shoot: return AudioKit.Shoot();
            case Sfx.ShotgunShoot: return AudioKit.ShotgunShoot();
            case Sfx.RocketShoot: return AudioKit.RocketShoot();
            case Sfx.Explosion: return AudioKit.Explosion();
            case Sfx.Hit: return AudioKit.Hit();
            case Sfx.EnemyDeath: return AudioKit.EnemyDeath();
            case Sfx.PlayerHurt: return AudioKit.PlayerHurt();
            case Sfx.Pickup: return AudioKit.Pickup();
            case Sfx.DifficultyUp: return AudioKit.DifficultyUp();
            case Sfx.DifficultyDown: return AudioKit.DifficultyDown();
            case Sfx.LevelUp: return AudioKit.LevelUp();
            case Sfx.UIClick: return AudioKit.UIClick();
            case Sfx.BossAlarm: return AudioKit.BossAlarm();
            case Sfx.GameOver: return AudioKit.GameOverSting();
            case Sfx.PerkSelect: return AudioKit.PerkSelect();
            default: return null;
        }
    }
}
