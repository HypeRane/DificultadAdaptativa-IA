# Sistema de Dificultad Adaptativa (Adaptive Difficulty AI)

Shoot 'em up vertical en Unity (C#) cuyo objetivo central no es el gameplay en sí, sino el sistema que lo dirige: un director de dificultad que observa el desempeño del jugador en tiempo real y ajusta la presión del juego para mantenerlo en una banda de desafío objetivo, en vez de escalar la dificultad de forma lineal por tiempo transcurrido.

![Gameplay](docs/gameplay.gif)
<!-- TODO: reemplazar por un gif corto (10-15s) mostrando el juego y el cambio de dificultad -->

## Planteamiento

La mayoría de proyectos de portafolio sobre "IA de videojuegos" se limitan a un enemigo con árbol de comportamiento. Este proyecto va por otro lado: en vez de IA para *un* enemigo, es un sistema de dos capas que analiza al jugador (precisión, salud, ritmo de eliminaciones, movilidad) y modula tanto la progresión estructural de la partida como el ritmo momento a momento — inspirado en el AI Director de *Left 4 Dead* y en sistemas de balance dinámico como los de *Resident Evil* o *Mario Kart*.

La implementación actual es un modelo basado en reglas (baseline) con una máquina de estados explícita. El siguiente paso planeado es loggear partidas reales y entrenar un modelo que reemplace o complemente esas reglas — el CSV que ya se genera en cada sesión está pensado con ese fin.

## Controles

- Movimiento: WASD / flechas direccionales, dentro del área visible de la cámara.
- Disparo: mantener click izquierdo (la nave dispara siempre hacia adelante, sin apuntado).
- Cambio de arma: teclas 1-5 (solo si hay munición de esa arma).
- Reinicio tras morir: R.
- Objetivo: sobrevivir el mayor tiempo posible, encadenar combos de eliminaciones, elegir mejoras al subir de nivel y superar los jefes que aparecen periódicamente.

## Sistema de dificultad adaptativa

El sistema tiene dos capas independientes que se alimentan de la misma telemetría pero operan en escalas de tiempo distintas.

### Capa 1: progresión estructural (`DifficultyManager`)

Cada `evaluationInterval` (15 s) se evalúa una ventana de desempeño:

```
performance_score = eliminaciones_en_la_ventana - (daño_recibido_en_la_ventana / 10)
blended_score      = performance_score + (skill_factor - 0.5) * 6
```

- `blended_score >= 5` sube el nivel de dificultad (1-10): los enemigos aparecen más seguido, se mueven más rápido y tienen más vida, dentro de límites configurables (`min`/`max`) para que el juego nunca sea imposible ni trivial.
- `blended_score <= 1` lo baja.

El nivel de dificultad no solo escala números: también desbloquea contenido, para que el ajuste sea perceptible y no solo numérico.

| Nivel | Se desbloquea |
|---|---|
| 1 | Devorador (enemigo base), Pistola (infinita) |
| 2 | Enjambre |
| 3 | Escupidor, pickups de Lanzallamas |
| 4 | Behemoth, pickups de Rifle de Francotirador |
| 5 | Explosivo, pickups de Lanzacohetes |
| 7 | Élite |

### Capa 2: director de ritmo (`DifficultyManager.DirectorState`)

Una máquina de estados explícita — `Valley -> Rising -> Climax -> Falling -> Valley` — reevaluada cada 2 s (no en cada frame), que modula `SpawnIntervalMultiplier` sobre el intervalo de aparición base:

| Estado | Multiplicador de spawn | Condición de salida |
|---|---|---|
| Valley | x1.35 (respiro) | `skill_factor >= 0.55` tras un mínimo de 10 s |
| Rising | x1.0 | 20 s transcurridos, o `skill_factor >= 0.8` |
| Climax | x0.65 (pico, +3 al techo de enemigos concurrentes, composición sesgada a tipos más amenazantes) | 25 s transcurridos |
| Falling | x1.1 | 12 s transcurridos |

Desde `Rising` o `Climax`, si `skill_factor <= 0.25` el director salta directo a `Falling` — una válvula de alivio para no ahogar a un jugador que está sufriendo, independiente del temporizador de estado.

### Telemetría (`PlayerTelemetryTracker`)

Calcula `skill_factor` (0-1, suavizado por interpolación para evitar saltos bruscos) cada `windowSeconds` (12 s), combinando:

- Precisión acumulada (proyectiles disparados vs. impactos).
- Salud actual como fracción del máximo.
- Ritmo de eliminaciones respecto a un valor de referencia por ventana.
- Movilidad: distancia recorrida por segundo, calculada sobre un buffer circular de 8 muestras de posición (sin `List<>` ni LINQ, para no generar basura en `Update`). Un valor bajo marca al jugador como "campeando" (`IsPlayerCamping`), señal que consumen directamente algunos enemigos y el jefe (ver más abajo) sin pasar por `DifficultyManager`.

`skill_factor` se reporta a `DifficultyManager` (`ReportSkillFactor`), que lo usa tanto para las transiciones de estado como para afinar `blended_score` en la capa 1.

## Sistemas de juego

### Armas (5, tecla 1-5)

| Arma | Comportamiento | Desbloqueo |
|---|---|---|
| Pistola | Munición infinita, daño base | Desde el inicio |
| Escopeta | 6 perdigones en abanico, daño con caída por distancia recorrida | Nivel 3 (pickup) |
| Lanzallamas | Daño continuo por tick dentro de un cono frente a la nave | Nivel 3 (pickup) |
| Rifle de francotirador | Un solo proyectil, alto daño, cadencia lenta | Nivel 4 (pickup) |
| Lanzacohetes | Proyectil explosivo con daño en área | Nivel 5 (pickup) |

### Enemigos (6 arquetipos)

Cada arquetipo aplica multiplicadores sobre las stats base que ya calcula `DifficultyManager` (`EnemyArchetype`), de forma que la IA de dificultad sigue controlando la potencia general y el arquetipo solo cambia el comportamiento.

| Enemigo | Rasgo |
|---|---|
| Devorador | Persecución directa, arquetipo base |
| Enjambre | Alta velocidad, baja vida |
| Escupidor | Mantiene distancia y dispara proyectiles; si detecta al jugador campeando, dispara en abanico de 3 en vez de un tiro directo |
| Behemoth | Alta vida y daño de contacto, baja velocidad |
| Explosivo | Corre hacia el jugador y detona en área al llegar (o al morir por cualquier otra causa) |
| Élite | Velocidad, vida y daño por encima del promedio; combina rasgos de varios arquetipos |

### Jefes (`BossDirector`, `BossController`)

Aparecen cada 90 s (60 s la primera vez), con aviso previo. Sus stats escalan sobre los valores actuales de `DifficultyManager`. Además del estallido de proyectiles radial periódico, si detectan camping disparan una andanada apuntada directo a la posición del jugador. Otorgan una bonificación fija de puntaje al ser derrotados y disparan la oferta de un perk.

### Perks (`PerkDatabase`, `PerkManager`, `PerkEffects`)

Cada vez que el nivel de dificultad sube a un número par, o se derrota un jefe, la partida se pausa y se ofrecen 3 mejoras al azar de un catálogo de 10 (cadencia de disparo, daño, vida máxima, curación, velocidad de movimiento, daño del dron, vampirismo, multiplicador de puntaje, reducción de daño recibido, munición extra por recogida). Los efectos son multiplicadores globales acumulables entre sí, reseteados al empezar una partida nueva.

### Dron aliado (`PetCompanion`, `PetPickup`, `PetPickupSpawner`)

Recogible del suelo (aparición periódica o como premio por una racha de combo). Hasta 3 drones simultáneos, orbitando al jugador y disparando solos al enemigo más cercano dentro de su rango. Cada dron tiene una duración limitada; recoger otro mientras ya se está en el máximo renueva la duración de todos en vez de sumar uno nuevo.

### Economía de recompensas

Los recogibles de armas y del dron ajustan su intervalo de aparición según `skill_factor`: más frecuentes si el jugador está sufriendo, al ritmo base si está dominando — una capa simple de riesgo/recompensa sobre el mismo director de dificultad.

## Notas de implementación

- **Sin assets externos.** Todos los sprites (`ProceduralSprites`), el fondo de estrellas con paralaje (`ArenaBackground`) y los efectos de sonido (`AudioKit`, osciladores + ruido generados por código) se construyen en tiempo de ejecución. No hay archivos de imagen ni de audio en el proyecto.
- **Todo se arma por código.** `GameBootstrapper` construye HUD, spawners y sistemas al iniciar la escena; no depende de objetos preconfigurados a mano en el editor. Como `RuntimeInitializeOnLoadMethod` solo se ejecuta una vez por sesión del juego (no una vez por escena), también se suscribe a `SceneManager.sceneLoaded` para reconstruir todo al reiniciar tras morir.
- **Bajo acoplamiento por eventos.** La comunicación entre sistemas usa `System.Action` (`PlayerHealth.OnHealthChanged`, `DifficultyManager.OnDifficultyChanged`/`OnStateChanged`, `Enemy.OnDied`, `GameManager.OnScoreChanged`, etc.) o métodos de registro directo (`RegisterShotFired`, `ReportSkillFactor`) sobre los singletons existentes, en vez de referencias cruzadas entre componentes.
- **Disciplina de asignaciones.** El muestreo de movilidad de `PlayerTelemetryTracker` usa un array circular de tamaño fijo en vez de listas dinámicas o LINQ, para no generar basura en cada frame.
- **Cámara y disparo.** La cámara hace scroll continuo hacia arriba (sin seguir al jugador); el movimiento queda acotado al área visible. El disparo no depende del mouse: todas las armas apuntan hacia adelante por diseño.

## Datos

Cada evaluación de `DifficultyManager` se agrega a un CSV local (`difficulty_sessions.csv`, en `Application.persistentDataPath`) con: eliminaciones y daño de la ventana, precisión acumulada, `performance_score`, los parámetros de dificultad resultantes, el estado del director de ritmo y el `skill_factor` del momento. Es el dataset planeado para la siguiente fase del proyecto: reemplazar las reglas fijas por un modelo entrenado con partidas reales.

## Arquitectura de scripts

| Categoría | Scripts | Responsabilidad |
|---|---|---|
| Core | `GameBootstrapper`, `GameManager`, `HighScoreManager` | Arman la partida al vuelo; puntaje, combo y estado de partida; mejores resultados en `PlayerPrefs` |
| Dificultad | `DifficultyManager`, `PlayerTelemetryTracker`, `DifficultyAmbiance` | Progresión estructural + director de ritmo; cálculo de `skill_factor`; tinte de iluminación según nivel |
| Jugador | `PlayerMovement`, `PlayerShooting`, `PlayerHealth`, `CameraFollow` | Movimiento acotado a pantalla, disparo por arma, vida, scroll automático de cámara |
| Armas | `WeaponData`, `Projectile`, `WeaponPickup`, `WeaponPickupSpawner` | Stats de cada arma, proyectiles, recogibles |
| Enemigos | `EnemyData`, `Enemy`, `EnemySpawner`, `EnemyProjectile` | Arquetipos, spawn y desbloqueo según dificultad, comportamiento reactivo a telemetría |
| Jefes | `BossDirector`, `BossController` | Cadencia de aparición, patrón de disparo radial y andanada dirigida |
| Perks | `PerkEffects`, `PerkDatabase`, `PerkManager` | Multiplicadores de la run, catálogo de mejoras, cuándo ofrecerlas |
| Dron | `PetCompanion`, `PetProjectile`, `PetPickup`, `PetPickupSpawner` | Compañero que sigue y dispara solo; recogibles |
| Mapa / ambiente | `ArenaBackground`, `MapUtility`, `ObstacleMarker` | Fondo de estrellas con paralaje; punto de spawn relativo a cámara |
| Audio | `AudioKit`, `SoundManager` | Síntesis de efectos por código; pool de reproducción |
| UI y efectos | `HUDController`, `UIKit`, `ProceduralSprites`, `HitEffects`, `DebrisFX`, `FloatingUIText`, `BlinkText` | HUD (menú, panel de telemetría, elección de perks, barra de jefe), generación de sprites/paneles/botones en runtime, partículas |

## Stack técnico

- Unity 6 (6000.5.8f1), C#
- Universal Render Pipeline (2D)
- Sin dependencias de terceros para arte o audio

## Roadmap

- [x] Movimiento, disparo y combate básico
- [x] Enemigos y aparición continua
- [x] Dificultad adaptativa basada en reglas
- [x] Logging de datos de sesión a CSV
- [x] HUD con panel de telemetría en vivo
- [x] Sistema de armas, dron aliado y variedad de enemigos
- [x] Jefes con aviso previo, barra de vida y bonificación de puntaje
- [x] Perks tipo roguelite
- [x] Audio generado por código
- [x] Menú principal y mejores resultados locales
- [x] Iluminación reactiva al nivel de dificultad
- [x] Director de ritmo (Tensión/Valle/Clímax) impulsado por telemetría del jugador
- [x] Conversión a shoot 'em up vertical con scroll de cámara automático
- [ ] Análisis exploratorio de los datos con Python
- [ ] Modelo de ML que prediga el nivel de habilidad del jugador y reemplace las reglas actuales
- [ ] Build jugable en itch.io

## Cómo ejecutarlo

1. Clonar el repositorio.
2. Abrir con Unity Hub (versión 6000.5.8f1 o superior).
3. Abrir la escena en `Assets/Scenes/SampleScene.unity`.
4. Play.

## Autor

Fabrizio — [LinkedIn](#) · [GitHub](#)
<!-- TODO: agregar los links reales -->
