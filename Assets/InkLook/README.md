# Ink Look

Sumi-e (ink wash) rendering for Unity URP, taken out of *Before the Ink Runs* so it can be used in any project.

**Requires:** Unity 6 (built and tested on 6000.3) with URP 17. No other packages.

## What is inside

| Folder | Contents |
|---|---|
| `Shaders/` | `Ink/InkLit` (object shading), `Ink/InkPost` (full-screen pass), `Ink/Brush` (stroke meshes), `Ink/Stroke` (particles: rain, sparks), `Ink/Transition` (scene change) |
| `Runtime/` | `InkWorld`, `BrushFx`, `SceneTransition`, `InkTransitionSettings`, `InkLookDemo` (assembly `InkLook.Runtime`) |
| `Editor/` | `InkLookSetup`: the Tools > Ink Look menu (assembly `InkLook.Editor`) |
| `Textures/` | `ink_grain.png` (r = bristle streaks, g = wash blots, b = paper fibre), brush stroke and splat alphas |
| `Materials/` | Default post material, brush materials, transition material |
| `Resources/InkLook/` | `InkTransitionSettings.asset` |

## Setting up a project

1. Import `InkLook.unitypackage`, or copy this folder into `Assets/` (or into `Packages/` as an embedded package; `package.json` is included).
2. **Tools > Ink Look > Add Ink Pass to Renderer.** It uses the renderer selected in the Project window, otherwise the URP asset's default renderer. Your editable copy of the post material is made at `Assets/InkLookSettings/M_InkPost.mat`.
3. In each scene: **Tools > Ink Look > Add Ink World to Scene.** It adds the object that feeds the shaders (paper colour, grain, distance fade) and the brush-stroke spawner, and paints the camera background paper-coloured.
4. Give objects ink materials: **Assets > Create > Ink Look > Ink Lit Material** (three tones: light, mid, shade).

**Tools > Ink Look > Create Demo Scene** builds a small scene with buttons for every effect.

## Using it from code

```csharp
using InkLook;

// Painted scene change (works with nothing in the scene)
SceneTransition.Go("Level01");
SceneTransition.Run(() => player.Respawn());     // cover, do something, reveal
SceneTransition.CoverStarted += () => audio.Play("brush");   // hook your own sounds

// Full-screen effects, 0..1
InkWorld.Flash = 1f;        // flash of bare paper (fade it out yourself)
InkWorld.Invert = 1f;       // ink negative
InkWorld.ScreenBleed = 0.5f;// wet stains from the edges
InkWorld.Dissolve = 1f;     // picture washes into the paper

// Brush strokes in the world (side-on, drawn at BrushFx.zPlane)
BrushFx.Arc(centre, radius, fromDeg, toDeg, width, BrushFx.Ink, life);
BrushFx.Ring(centre, radius, width, BrushFx.Ink, life, grow);
BrushFx.Streak(a, b, width, BrushFx.Wash, life);
BrushFx.Burst(centre, count, inner, outer, width, BrushFx.Ink, life);
BrushFx.Splat(centre, size, BrushFx.Ink, life);
```

## Tuning

- **Paper, outlines, wash, wobble, vignette:** `Assets/InkLookSettings/M_InkPost.mat`.
- **Paper colour and distance fade:** the `InkWorld` component.
- **Transition timings and optional sounds:** Tools > Ink Look > Edit Transition Settings.

The outline thresholds (`Depth Threshold`, `Line Fade`) were tuned for a 30° camera 15 to 20 m from the action. Retune them for other cameras.

## Credits

Everything here (shaders, code, generated textures) was written for *Before the Ink Runs*. No third-party assets.
