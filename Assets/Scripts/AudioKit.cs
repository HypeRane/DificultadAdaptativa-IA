using System.Collections.Generic;
using UnityEngine;

// Generador de efectos de sonido 100% por código (osciladores simples + ruido), para que el
// juego tenga audio propio sin depender de archivos externos.
public static class AudioKit
{
    private const int SampleRate = 44100;
    private static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    private static AudioClip GetOrBuild(string key, System.Func<AudioClip> builder)
    {
        if (!cache.TryGetValue(key, out AudioClip clip) || clip == null)
        {
            clip = builder();
            cache[key] = clip;
        }
        return clip;
    }

    // shape: 0 = seno, 1 = cuadrada, 2 = diente de sierra
    private static float Wave(float phase, int shape)
    {
        phase -= Mathf.Floor(phase);
        switch (shape)
        {
            case 1: return phase < 0.5f ? 1f : -1f;
            case 2: return phase * 2f - 1f;
            default: return Mathf.Sin(phase * Mathf.PI * 2f);
        }
    }

    public static AudioClip Note(float startFreq, float endFreq, float duration, int shape, float volume, float decay = 8f)
    {
        int samples = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        float[] data = new float[samples];
        float phase = 0f;
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(startFreq, endFreq, t);
            phase += freq / SampleRate;
            float envelope = Mathf.Exp(-decay * t);
            data[i] = Wave(phase, shape) * volume * envelope;
        }
        AudioClip clip = AudioClip.Create("Note", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip Noise(float duration, float volume, float decay = 6f)
    {
        int samples = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-decay * t);
            data[i] = (Random.value * 2f - 1f) * volume * envelope;
        }
        AudioClip clip = AudioClip.Create("Noise", samples, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip Sequence(params AudioClip[] notes)
    {
        int total = 0;
        foreach (AudioClip n in notes) total += n.samples;

        float[] data = new float[total];
        int offset = 0;
        foreach (AudioClip n in notes)
        {
            float[] buf = new float[n.samples];
            n.GetData(buf, 0);
            System.Array.Copy(buf, 0, data, offset, buf.Length);
            offset += n.samples;
        }
        AudioClip clip = AudioClip.Create("Sequence", total, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip Mix(AudioClip a, AudioClip b)
    {
        int len = Mathf.Max(a.samples, b.samples);
        float[] da = new float[a.samples]; a.GetData(da, 0);
        float[] db = new float[b.samples]; b.GetData(db, 0);
        float[] outData = new float[len];
        for (int i = 0; i < len; i++)
        {
            float va = i < da.Length ? da[i] : 0f;
            float vb = i < db.Length ? db[i] : 0f;
            outData[i] = Mathf.Clamp(va + vb, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("Mix", len, 1, SampleRate, false);
        clip.SetData(outData, 0);
        return clip;
    }

    // ---------- Efectos concretos del juego ----------

    public static AudioClip Shoot() => GetOrBuild("shoot", () => Note(900, 300, 0.08f, 1, 0.5f, 20f));
    public static AudioClip ShotgunShoot() => GetOrBuild("shotgun", () => Mix(Note(300, 120, 0.12f, 1, 0.5f, 12f), Noise(0.1f, 0.35f, 14f)));
    public static AudioClip RocketShoot() => GetOrBuild("rocket", () => Note(200, 80, 0.25f, 2, 0.5f, 6f));
    public static AudioClip Explosion() => GetOrBuild("explosion", () => Mix(Noise(0.4f, 0.6f, 5f), Note(150, 40, 0.4f, 0, 0.4f, 5f)));
    public static AudioClip Hit() => GetOrBuild("hit", () => Noise(0.06f, 0.3f, 30f));
    public static AudioClip EnemyDeath() => GetOrBuild("enemyDeath", () => Note(500, 100, 0.18f, 1, 0.35f, 10f));
    public static AudioClip PlayerHurt() => GetOrBuild("playerHurt", () => Mix(Note(180, 90, 0.15f, 0, 0.4f, 8f), Noise(0.12f, 0.25f, 12f)));
    public static AudioClip Pickup() => GetOrBuild("pickup", () => Sequence(Note(600, 600, 0.06f, 0, 0.35f, 4f), Note(900, 900, 0.09f, 0, 0.35f, 5f)));
    public static AudioClip DifficultyUp() => GetOrBuild("diffUp", () => Sequence(Note(400, 400, 0.08f, 0, 0.3f, 4f), Note(600, 600, 0.08f, 0, 0.3f, 4f), Note(900, 900, 0.12f, 0, 0.3f, 5f)));
    public static AudioClip DifficultyDown() => GetOrBuild("diffDown", () => Sequence(Note(500, 500, 0.08f, 0, 0.3f, 4f), Note(350, 350, 0.08f, 0, 0.3f, 4f), Note(220, 220, 0.14f, 0, 0.3f, 5f)));
    public static AudioClip LevelUp() => GetOrBuild("levelUp", () => Sequence(Note(523, 523, 0.08f, 0, 0.3f, 4f), Note(659, 659, 0.08f, 0, 0.3f, 4f), Note(784, 784, 0.08f, 0, 0.3f, 4f), Note(1047, 1047, 0.18f, 0, 0.35f, 4f)));
    public static AudioClip UIClick() => GetOrBuild("uiClick", () => Note(700, 700, 0.03f, 1, 0.25f, 40f));
    public static AudioClip BossAlarm() => GetOrBuild("bossAlarm", () => Sequence(Note(300, 300, 0.15f, 1, 0.4f, 3f), Note(220, 220, 0.15f, 1, 0.4f, 3f)));
    public static AudioClip GameOverSting() => GetOrBuild("gameOver", () => Sequence(Note(400, 400, 0.2f, 0, 0.35f, 3f), Note(320, 320, 0.2f, 0, 0.35f, 3f), Note(220, 220, 0.4f, 0, 0.35f, 2f)));
    public static AudioClip PerkSelect() => GetOrBuild("perkSelect", () => Sequence(Note(700, 700, 0.06f, 0, 0.3f, 5f), Note(1000, 1000, 0.1f, 0, 0.3f, 5f)));
    public static AudioClip FlameLoop() => GetOrBuild("flameLoop", () => Noise(1f, 0.2f, 0f));
}
