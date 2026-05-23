


# MiJuegoAR URP - Laboratorio de Realidad Aumentada con Unity 6 y AR Foundation

## Descripción del Proyecto

Este proyecto es una adaptación de un laboratorio clásico de Google ARCore migrado a un proyecto nuevo en **Unity 6 (6000.4.7f1)** usando la plantilla **Universal 3D** con **URP (Universal Render Pipeline)**.

La aplicación permite detectar superficies reales mediante la cámara de Android, visualizar un piso arenoso sobre los planos detectados, colocar un carro sobre la superficie mediante una retícula y generar un paquete en la escena de realidad aumentada.

El objetivo principal del laboratorio es comprender la integración entre **AR Foundation**, **ARCore**, **URP**, detección de planos, raycasts en AR, prefabs interactivos y posicionamiento de objetos virtuales sobre superficies reales.

---

## Tecnologías Utilizadas

- Unity 6 `6000.4.7f1`
- Universal Render Pipeline `URP`
- AR Foundation
- ARCore XR Plugin
- XR Plug-in Management
- Input System
- Android Build Support
- Dispositivo de prueba: Lenovo Tab M11

---

## Funcionalidades Implementadas

- Visualización del feed de cámara real.
- Inicialización correcta de sesión AR mediante `AR Session`.
- Uso de `XR Origin` con cámara AR.
- Detección de planos horizontales mediante `AR Plane Manager`.
- Renderizado de un piso arenoso sobre superficies detectadas.
- Retícula ubicada en el centro de la pantalla mediante raycast AR.
- Colocación de un carro al tocar la pantalla.
- Generación de un paquete cerca del carro.
- Corrección de altura del paquete para evitar que aparezca debajo del suelo.
- Ocultamiento de planos no utilizados después de bloquear el plano principal.
- Compatibilidad con el Input System nuevo de Unity.

---

## Estructura Principal del Proyecto

```text
Assets/
├── Scenes/
│   └── SampleScene.unity
├── Starter Package/
│   ├── CarBehaviour.cs
│   ├── CarManager.cs
│   ├── DrivingSurfaceManager.cs
│   ├── PackageBehaviour.cs
│   ├── PackageSpawner.cs
│   ├── ReticleBehaviour.cs
│   ├── LightEstimation.cs
│   ├── Car Prefab.prefab
│   ├── Package Prefab.prefab
│   ├── Driving Surface Plane.prefab
│   └── Reticle Prefab.prefab
└── Settings/
    ├── Mobile_RPAsset.asset
    └── Mobile_Renderer.asset
```

---

## Configuración de AR

La escena contiene los siguientes objetos principales:

### AR Session

Objeto encargado de inicializar y controlar el ciclo de vida de ARCore en Android.

### XR Origin

Contiene los componentes principales de AR Foundation:

- `XR Origin`
- `AR Plane Manager`
- `AR Raycast Manager`
- `DrivingSurfaceManager`
- `PlaneDebugLogger`

El `AR Plane Manager` utiliza como prefab:

```text
Driving Surface Plane.prefab
```

Este prefab contiene:

- `ARPlane`
- `ARPlaneMeshVisualizer`
- `MeshFilter`
- `MeshRenderer`
- `MeshCollider`
- Material de suelo arenoso compatible con URP

### Main Camera

La cámara está dentro de:

```text
XR Origin -> Camera Offset -> Main Camera
```

Componentes importantes:

- `Camera`
- `AR Camera Manager`
- `AR Camera Background`
- `Tracked Pose Driver`
- `Universal Additional Camera Data`

El componente `AR Camera Background` permite mostrar el feed real de la cámara como fondo de la escena.

---

## Configuración de URP

El proyecto usa Universal Render Pipeline.

En el renderer móvil se agregó:

```text
AR Background Renderer Feature
```

Esto permite que el fondo de cámara AR se renderice correctamente en URP.

También se verificó que:

- La cámara tenga `Culling Mask = Everything`.
- El renderer activo sea el renderer móvil configurado.
- El material del suelo use un shader compatible con URP.
- El `AR Camera Background` esté activo.

---

## Scripts Principales

## ReticleBehaviour.cs

Este script posiciona la retícula en el centro de la pantalla mediante un raycast contra planos AR.

Funcionalidad principal:

- Usa `ARRaycastManager`.
- Lanza un raycast desde el centro de la pantalla.
- Detecta planos con `TrackableType.PlaneWithinBounds`.
- Mueve la retícula al punto de impacto.
- Aplica un pequeño desplazamiento vertical para evitar Z-fighting.

Fragmento importante:

```csharp
transform.position = hit.Value.pose.position + new Vector3(0f, 0.02f, 0f);
```

Este offset evita que la retícula se mezcle visualmente con el suelo arenoso.

---

## DrivingSurfaceManager.cs

Administra el plano principal sobre el que se colocan los objetos del juego.

Responsabilidades:

- Mantener referencia al `ARPlaneManager`.
- Mantener referencia al `ARRaycastManager`.
- Guardar el plano bloqueado en `LockedPlane`.
- Ocultar otros planos después de colocar el carro.

Esto evita que el suelo arenoso siga apareciendo en múltiples capas mientras ARCore continúa refinando la detección del entorno.

---

## CarManager.cs

Se encarga de instanciar el carro cuando el usuario toca la pantalla.

El proyecto usa el Input System nuevo de Unity, por lo que se agregó compatibilidad con:

```csharp
Pointer.current.press.wasPressedThisFrame
Touchscreen.current.touches
```

Flujo:

1. La retícula detecta un plano válido.
2. El usuario toca la pantalla.
3. Se instancia el prefab del carro.
4. El carro se coloca en la posición de la retícula.
5. Se bloquea el plano actual.
6. Se genera el paquete cerca del carro.

---

## CarBehaviour.cs

Controla el movimiento del carro hacia la retícula.

Funcionamiento:

- El carro sigue la posición actual de la retícula.
- Rota suavemente hacia el objetivo.
- Avanza con `Vector3.MoveTowards`.
- Detecta colisión con el paquete mediante `OnTriggerEnter`.

---

## PackageSpawner.cs

Se encarga de generar el paquete sobre el plano AR bloqueado.

Problema solucionado:

Durante las pruebas, el paquete aparecía debajo del suelo arenoso porque el pivot del prefab no coincidía exactamente con la parte inferior visible del modelo.

Solución implementada:

- Se calcula la parte más baja visible del paquete usando `Renderer.bounds`.
- Se proyecta la posición contra el plano AR.
- Se aplica un offset vertical de seguridad.

```csharp
public float SurfaceOffset = 0.02f;
```

Esto permite que el paquete quede visualmente encima del suelo y no enterrado en una capa inferior.

---

## Problemas Encontrados y Soluciones

## 1. Pantalla negra en AR

### Problema

Al iniciar la aplicación, no se mostraba el feed de la cámara.

### Causa

Faltaban componentes necesarios para inicializar y renderizar correctamente la cámara AR.

### Solución

Se agregó:

- `AR Session`
- `AR Camera Background`
- `AR Background Renderer Feature` en URP

---

## 2. El suelo arenoso no aparecía

### Problema

El `AR Plane Manager` no mostraba el prefab del suelo.

### Causas encontradas

- Existían referencias incorrectas en la escena.
- Había más de un objeto con configuración AR similar.
- El prefab asignado al `AR Plane Manager` no era el correcto.
- El material original dependía de un Shader Graph antiguo.

### Solución

- Se dejó una única ruta AR principal en el `XR Origin`.
- Se asignó correctamente el prefab `Driving Surface Plane`.
- Se cambió el material del suelo a un shader compatible con URP.
- Se agregó un logger para verificar los planos detectados.

---

## 3. La retícula aparecía, pero el carro no

### Problema

La retícula funcionaba, pero al tocar la pantalla no se instanciaba el carro.

### Causa

El proyecto usaba el Input System nuevo, mientras el código original del codelab usaba el sistema de input antiguo.

### Solución

Se actualizó `CarManager.cs` para soportar el Input System nuevo.

---

## 4. El paquete no aparecía

### Problema

El carro aparecía correctamente, pero el paquete no.

### Causas encontradas

- El `PackageSpawner` no estaba conectado correctamente.
- El prefab del paquete estaba mal referenciado.
- El paquete podía generarse fuera del área visible.

### Solución

- Se conectó `PackageSpawner` al `CarManager`.
- Se corrigió la referencia al prefab real del paquete.
- Se configuró el paquete para aparecer cerca del carro.

---

## 5. El paquete aparecía debajo del suelo

### Problema

El paquete se veía como si estuviera en una capa inferior al suelo arenoso.

### Causa

El pivot del prefab no coincidía con la base visual del modelo, y el plano AR seguía refinándose mientras se formaba la superficie.

### Solución

Se calculó la parte inferior visible del paquete mediante los bounds de sus renderers y se aplicó una corrección de altura sobre la normal del plano.

---

## Estado Actual

Actualmente el proyecto permite:

- Abrir la aplicación en Android.
- Ver el entorno real mediante la cámara.
- Detectar planos horizontales.
- Mostrar suelo arenoso sobre el plano.
- Mostrar una retícula sobre la superficie detectada.
- Tocar la pantalla para colocar el carro.
- Generar un paquete cerca del carro.
- Mantener el paquete sobre el suelo sin que quede enterrado.
- Ocultar planos adicionales después de seleccionar el plano principal.

---

## Cómo Probar

1. Conectar la tablet Android por USB.
2. Abrir el proyecto en Unity 6.
3. Ir a:

```text
File -> Build Profiles
```

4. Seleccionar Android.
5. Presionar:

```text
Build and Run
```

6. En el dispositivo:
   - Mover lentamente el dispositivo para escanear el piso.
   - Esperar a que aparezca el suelo arenoso.
   - Apuntar la retícula a una zona estable.
   - Tocar la pantalla.
   - Verificar que aparezca el carro.
   - Verificar que aparezca el paquete cerca del carro.

---

## Recomendaciones de Prueba

Para mejores resultados:

- Usar una superficie bien iluminada.
- Evitar pisos completamente lisos o brillantes.
- Mover la tablet lentamente.
- Esperar a que el plano esté estable antes de tocar.
- No colocar el carro mientras el piso todavía se está reajustando demasiado.
- Probar en un área con textura visible para que ARCore detecte mejor el plano.

---

## Conclusión

Este laboratorio permitió migrar una experiencia clásica de ARCore a una arquitectura moderna basada en Unity 6, AR Foundation y URP.

Durante el desarrollo se resolvieron problemas comunes en proyectos de realidad aumentada:

- Configuración de cámara AR en URP.
- Renderizado del fondo real.
- Detección y visualización de planos.
- Uso correcto de prefabs AR.
- Migración al Input System nuevo.
- Posicionamiento correcto de objetos virtuales sobre superficies reales.
- Corrección de profundidad visual en modelos con pivots no alineados.

El resultado final es una escena funcional de realidad aumentada donde el usuario puede detectar una superficie real, colocar un carro y visualizar un paquete interactivo sobre el entorno físico.
```
