# Sistema de Dificultad Adaptativa (Adaptive Difficulty AI)

Prototipo de un shooter top-down 2D en Unity donde la dificultad se ajusta en tiempo real según el desempeño del jugador — inspirado en sistemas como el AI Director de *Left 4 Dead* o el balance dinámico de *Resident Evil* y *Mario Kart*.

![Gameplay](docs/gameplay.gif)
<!-- TODO: reemplazar por un gif corto (10-15s) mostrando el juego y el cambio de dificultad -->

## 🎮 Sobre el proyecto

La mayoría de proyectos de "IA de videojuegos" en portafolios se quedan en un enemigo con árbol de comportamiento básico. Este proyecto va por otro lado: en vez de IA para *un* enemigo, es un sistema que analiza el desempeño del jugador en tiempo real (precisión, daño recibido, enemigos eliminados) y ajusta parámetros del juego para mantener el desafío en un punto justo — ni tan fácil que aburra, ni tan difícil que frustre.

Es también el punto de partida de un proyecto más grande: la versión actual usa un sistema de reglas (baseline), y el siguiente paso es loggear partidas reales y entrenar un modelo que reemplace o complemente esas reglas.

## 🕹️ Cómo jugar

- **Movimiento:** WASD
- **Apuntar:** mouse
- **Disparar:** click izquierdo (mantener presionado)
- **Objetivo:** sobrevive todo lo que puedas. Los enemigos aparecen en oleadas y la dificultad se reajusta cada 15 segundos según cómo te esté yendo.

## 🧠 Cómo funciona el sistema de dificultad

Cada 15 segundos, el `DifficultyManager` evalúa una "ventana" de desempeño:

```
performance_score = enemigos_eliminados - (daño_recibido / 10)
```

- **Score alto** → sube la dificultad: los enemigos aparecen más seguido, se mueven más rápido y tienen más vida.
- **Score bajo** → baja la dificultad: spawns más espaciados, enemigos más lentos y débiles.

Todos los parámetros están limitados dentro de un rango (`min`/`max`) para que el juego nunca se vuelva imposible ni trivial.

## 📊 Datos

Cada evaluación se guarda en un CSV local (`difficulty_sessions.csv`) con: kills y daño de la ventana, precisión acumulada, el score calculado, y los parámetros de dificultad resultantes. Esto es el dataset que va a alimentar la siguiente fase del proyecto — reemplazar las reglas fijas por un modelo entrenado con partidas reales.

## 🏗️ Arquitectura

| Script | Responsabilidad |
|---|---|
| `PlayerMovement` | Movimiento y apuntado hacia el mouse |
| `PlayerShooting` | Disparo con cooldown |
| `Projectile` | Movimiento del proyectil y detección de impacto |
| `Enemy` | Persecución, vida, daño por contacto |
| `EnemySpawner` | Spawn de enemigos según el intervalo actual |
| `PlayerHealth` | Vida del jugador |
| `DifficultyManager` | Singleton que centraliza métricas, aplica las reglas de dificultad y loggea a CSV |

## 🛠️ Tech stack

- Unity 6 (C#)
- Git LFS para assets
- *(Próximamente)* Python + pandas para análisis de datos, scikit-learn para el modelo

## 🚀 Roadmap

- [x] Movimiento, disparo y combate básico
- [x] Enemigos y sistema de oleadas
- [x] Dificultad adaptativa basada en reglas
- [x] Logging de datos de sesión a CSV
- [ ] Análisis exploratorio de los datos con Python
- [ ] Modelo de ML que prediga el nivel de habilidad del jugador
- [ ] HUD en pantalla mostrando la dificultad ajustándose en vivo
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
