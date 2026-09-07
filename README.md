# Sistema de Dificultad Adaptativa (Adaptive Difficulty AI)

Prototipo de un shooter top-down 2D en Unity donde la dificultad se ajusta en tiempo real según el desempeño del jugador — inspirado en sistemas como el AI Director de *Left 4 Dead* o el balance dinámico de *Resident Evil* y *Mario Kart*.

![Gameplay](docs/gameplay.gif)
<!-- TODO: reemplazar por un gif corto (10-15s) mostrando el juego y el cambio de dificultad -->

## 🎮 Sobre el proyecto

La mayoría de proyectos de "IA de videojuegos" en portafolios se quedan en un enemigo con árbol de comportamiento básico. Este proyecto va por otro lado: en vez de IA para *un* enemigo, es un sistema que analiza el desempeño del jugador en tiempo real (precisión, daño recibido, enemigos eliminados) y ajusta parámetros del juego para mantener el desafío en un punto justo — ni tan fácil que aburra, ni tan difícil que frustre.

Es también el punto de partida de un proyecto más grande: la versión actual usa un sistema de reglas (baseline), y el siguiente paso es loggear partidas reales y entrenar un modelo que reemplace o complemente esas reglas.

## 🕹️ Cómo jugar

- **Movimiento:** WASD / flechas
- **Apuntar:** mouse
- **Disparar:** click izquierdo (mantener presionado)
- **Cambiar de arma:** teclas 1-4 (solo si tenés munición de esa arma)
- **Reiniciar tras morir:** R
- **Objetivo:** sobrevive todo lo que puedas, sumá puntaje con combos de kills seguidas. Los enemigos aparecen sin parar y la dificultad se reajusta cada 15 segundos según cómo te esté yendo — un panel en pantalla muestra en vivo qué está ajustando la IA.

## 🧠 Cómo funciona el sistema de dificultad

Cada 15 segundos, el `DifficultyManager` evalúa una "ventana" de desempeño:

```
performance_score = enemigos_eliminados - (daño_recibido / 10)
```

- **Score alto** → sube la dificultad: los enemigos aparecen más seguido, se mueven más rápido y tienen más vida. Sube también un "nivel de dificultad" (1-10) visible en el HUD.
- **Score bajo** → baja la dificultad: spawns más espaciados, enemigos más lentos y débiles.

Todos los parámetros están limitados dentro de un rango (`min`/`max`) para que el juego nunca se vuelva imposible ni trivial. El nivel de dificultad no solo escala números: también **desbloquea contenido**, para que quede claro (y sea más divertido) que el juego está reaccionando:

- **Variedad de enemigos:** empezás solo con el Rastreador (nivel 1+); con más nivel se suman el Corredor (2+, rápido y frágil), el Tirador (3+, ataca a distancia) y el Bruto (4+, lento pero tanque).
- **Variedad de armas:** los pickups de Escopeta aparecen desde el inicio, el Lanzallamas desde nivel 3, y el Lanzacohetes recién desde nivel 5.

## 📊 Datos

Cada evaluación se guarda en un CSV local (`difficulty_sessions.csv`, en la carpeta de datos persistentes de Unity) con: kills y daño de la ventana, precisión acumulada, el score calculado, y los parámetros de dificultad resultantes. Esto es el dataset que va a alimentar la siguiente fase del proyecto — reemplazar las reglas fijas por un modelo entrenado con partidas reales.

## ✨ Qué hay en el juego

- **HUD en vivo:** vida, puntaje/combo, cronómetro, y un panel de telemetría que muestra en tiempo real lo que la IA de dificultad está haciendo (nivel, velocidad/vida de enemigos, intervalo de spawn, precisión, enemigos activos), con avisos cuando la dificultad sube o baja.
- **4 armas:** Pistola (infinita), Escopeta (más daño de cerca por perdigón, cae con la distancia), Lanzallamas (daño continuo en cono) y Lanzacohetes (explota en área). Las especiales se consiguen recogiéndolas del mapa.
- **Mascota aliada:** un compañero que orbita al jugador y dispara solo a los enemigos cercanos. Se recoge del suelo (aparece al azar o como premio por una buena racha de combo) y recogerla de nuevo renueva su duración.
- **Obstáculos y mapa:** rocas/cajas repartidas por todo el mapa que bloquean el paso, para darle forma táctica a la arena.
- **Juice visual:** screen shake, flashes de impacto, partículas de golpe/muerte, texto de daño flotante, animación de aparición de enemigos — todo generado por código en tiempo de ejecución, sin depender de assets de arte.

## 🏗️ Arquitectura

| Categoría | Scripts | Responsabilidad |
|---|---|---|
| Core | `GameBootstrapper`, `GameManager`, `DifficultyManager` | Arman la partida al vuelo (HUD, fondo, spawners), llevan puntaje/combo/game-over, y centralizan las métricas + reglas de dificultad (con logging a CSV) |
| Jugador | `PlayerMovement`, `PlayerShooting`, `PlayerHealth`, `CameraFollow` | Movimiento y apuntado, disparo por arma, vida, cámara con seguimiento y screen shake |
| Armas | `WeaponData`, `Projectile`, `WeaponPickup`, `WeaponPickupSpawner` | Stats de cada arma (daño, caída por distancia, si explota), proyectiles, y sus recogibles en el mapa |
| Enemigos | `EnemyData`, `Enemy`, `EnemySpawner`, `EnemyProjectile` | Arquetipos (rastreador/corredor/tirador/bruto), spawn y desbloqueo según dificultad, IA de persecución/distancia y disparo a distancia |
| Mascota | `PetCompanion`, `PetProjectile`, `PetPickup`, `PetPickupSpawner` | Compañero que sigue y dispara solo, y sus recogibles (aleatorios o por combo) |
| Mapa | `ArenaBackground`, `ObstacleField`, `MapUtility` | Fondo procedural y obstáculos repartidos por toda el área visible |
| UI y efectos | `HUDController`, `UIKit`, `ProceduralSprites`, `HitEffects`, `DebrisFX`, `FloatingUIText`, `BlinkText` | HUD completo construido por código, generación de sprites/paneles en runtime, partículas y texto flotante |

## 🛠️ Tech stack

- Unity 6 (C#)
- Git LFS para assets
- *(Próximamente)* Python + pandas para análisis de datos, scikit-learn para el modelo

## 🚀 Roadmap

- [x] Movimiento, disparo y combate básico
- [x] Enemigos y sistema de oleadas
- [x] Dificultad adaptativa basada en reglas
- [x] Logging de datos de sesión a CSV
- [x] HUD en pantalla mostrando la dificultad ajustándose en vivo
- [x] Sistema de armas, mascota aliada y variedad de enemigos
- [ ] Jefes cada cierto nivel de dificultad / tiempo, con HUD de encuentro especial
- [ ] Sistema de perks tipo roguelite (elegir mejora al subir de nivel o matar un jefe)
- [ ] Audio: SFX generados por código para disparos, impactos, pickups y cambios de dificultad
- [ ] Menú principal + high score guardado localmente
- [ ] Tinte/iluminación de escena que reacciona al nivel de dificultad (usando la Global Light 2D)
- [ ] Análisis exploratorio de los datos con Python
- [ ] Modelo de ML que prediga el nivel de habilidad del jugador
- [ ] Build jugable en itch.io

## ▶️ Cómo correrlo

1. Clonar el repo
2. Abrir con Unity Hub (versión 6000.5.8f1 o superior)
3. Abrir la escena en `Assets/Scenes/SampleScene.unity`
4. Play

*(Próximamente: link a build jugable en el navegador)*

## 👤 Autor

Fabrizio — [LinkedIn](#) · [GitHub](#)
<!-- TODO: agregar tus links reales -->
