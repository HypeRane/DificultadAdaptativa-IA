using UnityEngine;

// Decide cuándo aparece un jefe (cada cierto tiempo de partida) y arma el encuentro: aviso
// previo, spawn con stats escaladas según la dificultad actual, y un único jefe vivo a la vez.
public class BossDirector : MonoBehaviour
{
    [SerializeField] private float firstBossTime = 60f;
    [SerializeField] private float intervalBetweenBosses = 90f;
    [SerializeField] private float warningDuration = 2.5f;

    private float timer;
    private bool warningIssued;
    private int bossNumber;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;
        if (BossController.ActiveBoss != null) return; // ya hay un jefe vivo

        timer += Time.deltaTime;
        float threshold = bossNumber == 0 ? firstBossTime : intervalBetweenBosses;

        if (!warningIssued && timer >= threshold - warningDuration)
        {
            warningIssued = true;
            HUDController.Instance?.ShowBossWarning();
            SoundManager.Play(Sfx.BossAlarm);
        }

        if (timer >= threshold)
        {
            timer = 0f;
            warningIssued = false;
            bossNumber++;
            SpawnBoss();
        }
    }

    private void SpawnBoss()
    {
        DifficultyManager dm = DifficultyManager.Instance;
        int baseHealth = dm != null ? dm.EnemyHealth : 30;
        float baseSpeed = dm != null ? dm.EnemySpeed : 2f;
        int baseDamage = dm != null ? dm.EnemyContactDamage : 10;

        Vector2 spawnPos = MapUtility.RandomPointInPlayArea(9f, 1f);

        GameObject bossObj = new GameObject("Boss");
        bossObj.transform.position = spawnPos;
        bossObj.transform.localScale = Vector3.one * 2.4f;
        bossObj.tag = "Enemy";

        SpriteRenderer sr = bossObj.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSprites.Circle;
        sr.sharedMaterial = ProceduralSprites.WorldSpriteMaterial;
        sr.sortingOrder = 3;

        Rigidbody2D rb = bossObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        bossObj.AddComponent<CircleCollider2D>().radius = 0.5f;

        Enemy enemy = bossObj.AddComponent<Enemy>();
        enemy.ConfigureAsBoss(
            Mathf.RoundToInt(baseHealth * (6f + bossNumber * 2f)),
            baseSpeed * 0.6f,
            Mathf.RoundToInt(baseDamage * 1.8f),
            new Color(0.75f, 0.1f, 0.15f));
        enemy.BeginSpawnAnimation();

        BossController controller = bossObj.AddComponent<BossController>();
        controller.Init(enemy, $"JEFE {bossNumber}");

        HUDController.Instance?.ShowBossBar(enemy, controller.DisplayName);
    }
}
