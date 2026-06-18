# Inkronin Blender Source

This folder contains the Blender source pipeline for the main character.

## Generate The Source Model

Install Blender, then run:

```powershell
blender --background --python create_inkronin_model.py
```

That creates:

```text
Inkronin.blend
```

## Export To Unity

From Blender:

1. Open `Inkronin.blend`.
2. Inspect the model and adjust proportions.
3. Export as FBX into:

   `Assets/_Project/Art/Models/Inkronin.fbx`

For now, this is a stylized source model with separate parts and a starter armature. The final gameplay version should become one optimized skinned mesh with a humanoid rig.
