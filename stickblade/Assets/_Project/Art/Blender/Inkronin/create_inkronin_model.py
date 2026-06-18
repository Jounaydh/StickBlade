import math
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parent
BLEND_PATH = ROOT / "Inkronin.blend"


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


def make_material(name, color, roughness=0.65, metallic=0.0, emission=None):
    material = bpy.data.materials.new(name)
    material.use_nodes = True

    bsdf = material.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = color
        bsdf.inputs["Roughness"].default_value = roughness
        bsdf.inputs["Metallic"].default_value = metallic
        if emission:
            bsdf.inputs["Emission Color"].default_value = emission
            bsdf.inputs["Emission Strength"].default_value = 0.45

    return material


def shade_flat(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.shade_flat()
    obj.select_set(False)


def add_uv_sphere(name, loc, scale, material, segments=16, rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=loc)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    obj.data.materials.append(material)
    shade_flat(obj)
    return obj


def add_cube(name, loc, scale, material, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    obj.data.materials.append(material)
    shade_flat(obj)
    return obj


def add_cylinder(name, loc, radius, depth, material, vertices=10, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    shade_flat(obj)
    return obj


def add_cone(name, loc, radius1, radius2, depth, material, vertices=8, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius1, radius2=radius2, depth=depth, location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    shade_flat(obj)
    return obj


def add_bezier_curve(name, points, bevel_depth, material):
    curve = bpy.data.curves.new(name, "CURVE")
    curve.dimensions = "3D"
    curve.resolution_u = 2
    curve.bevel_depth = bevel_depth
    curve.bevel_resolution = 1

    spline = curve.splines.new("BEZIER")
    spline.bezier_points.add(len(points) - 1)
    for point, co in zip(spline.bezier_points, points):
        point.co = Vector(co)
        point.handle_left_type = "AUTO"
        point.handle_right_type = "AUTO"

    obj = bpy.data.objects.new(name, curve)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    return obj


def add_label_empty(name, loc):
    empty = bpy.data.objects.new(name, None)
    empty.empty_display_type = "PLAIN_AXES"
    empty.empty_display_size = 0.15
    empty.location = loc
    bpy.context.collection.objects.link(empty)
    return empty


def build_inkronin():
    clear_scene()

    black = make_material("Inkronin_Black", (0.006, 0.006, 0.008, 1))
    dark = make_material("Inkronin_DarkCloth", (0.025, 0.025, 0.03, 1))
    cyan = make_material("Inkronin_Cyan", (0.0, 0.65, 0.9, 1), emission=(0.0, 0.65, 0.9, 1))
    blade = make_material("Inkronin_BlackSteel", (0.025, 0.03, 0.035, 1), roughness=0.38, metallic=0.25)
    sole = make_material("Inkronin_Sole", (0.012, 0.012, 0.014, 1))

    rig_root = add_label_empty("Inkronin_Root", (0, 0, 0))

    body = add_uv_sphere("torso_shadow_robes", (0, 0, 1.32), (0.43, 0.27, 0.58), dark)
    body.parent = rig_root
    add_cube("upper_layered_gi", (0, -0.02, 1.48), (0.48, 0.31, 0.24), dark).parent = rig_root
    add_cube("wide_shoulder_cowl", (0, -0.01, 1.74), (0.63, 0.36, 0.08), black).parent = rig_root
    add_cube("front_cyan_diagonal_strap", (-0.12, -0.28, 1.47), (0.035, 0.025, 0.43), cyan, (0, 0, math.radians(-26))).parent = rig_root
    add_cube("waist_cyan_sash", (0, 0, 1.0), (0.55, 0.32, 0.055), cyan).parent = rig_root
    add_cube("waist_black_belt", (0, -0.01, 1.08), (0.59, 0.33, 0.035), black).parent = rig_root

    for i, x in enumerate([-0.36, -0.25, -0.14, -0.03, 0.08, 0.19, 0.3]):
        length = 0.38 if i % 2 else 0.52
        panel = add_cube(f"torn_robe_panel_{i}", (x, -0.08, 0.72), (0.04, 0.03, length), black, (0, 0, math.radians(-8 + i * 3)))
        panel.parent = rig_root

    head = add_uv_sphere("round_shadow_head", (0, -0.01, 2.15), (0.31, 0.29, 0.31), black, 20, 10)
    head.parent = rig_root
    add_uv_sphere("left_glow_eye", (-0.105, -0.265, 2.16), (0.032, 0.012, 0.095), cyan, 8, 4).parent = rig_root
    add_uv_sphere("right_glow_eye", (0.105, -0.265, 2.16), (0.032, 0.012, 0.095), cyan, 8, 4).parent = rig_root

    for i in range(24):
        angle = math.radians(-130 + i * 11)
        radius = 0.22 + (0.05 if i % 3 == 0 else 0)
        length = 0.38 + (0.14 if i % 4 == 0 else 0)
        loc = (math.sin(angle) * radius, math.cos(angle) * 0.1 - 0.01, 2.42 + (i % 4) * 0.015)
        hair = add_cone(f"hair_spike_{i}", loc, 0.055, 0.012, length, black, 7, (math.radians(72), 0, -angle))
        hair.parent = rig_root

    add_cube("cyan_headband_front", (0, -0.285, 2.27), (0.31, 0.025, 0.028), cyan).parent = rig_root
    add_cube("cyan_headband_side", (-0.25, -0.04, 2.25), (0.26, 0.022, 0.026), cyan, (0, 0, math.radians(16))).parent = rig_root
    add_cube("headband_tail_long", (-0.42, 0.08, 2.13), (0.035, 0.018, 0.38), cyan, (math.radians(15), 0, math.radians(36))).parent = rig_root
    add_cube("headband_tail_short", (-0.33, 0.1, 2.08), (0.03, 0.018, 0.3), cyan, (math.radians(5), 0, math.radians(48))).parent = rig_root

    add_bezier_curve("back_cyan_moon_mark", [(-0.18, 0.29, 1.58), (-0.02, 0.34, 1.68), (0.17, 0.28, 1.55), (0.12, 0.29, 1.38), (-0.06, 0.33, 1.34)], 0.018, cyan).parent = rig_root

    for side, x in [("left", -0.52), ("right", 0.52)]:
        add_cylinder(f"{side}_upper_arm", (x, -0.01, 1.37), 0.075, 0.48, black, 10, (math.radians(10), 0, math.radians(8 if x < 0 else -8))).parent = rig_root
        add_cylinder(f"{side}_forearm", (x * 1.02, -0.005, 1.02), 0.06, 0.38, black, 10, (math.radians(4), 0, math.radians(4 if x < 0 else -4))).parent = rig_root
        add_cube(f"{side}_cyan_wrist_wrap", (x * 1.03, -0.01, 0.89), (0.09, 0.07, 0.025), cyan).parent = rig_root
        add_uv_sphere(f"{side}_hand", (x * 1.04, -0.01, 0.78), (0.075, 0.06, 0.075), black, 10, 5).parent = rig_root

    for side, x in [("left", -0.22), ("right", 0.22)]:
        add_uv_sphere(f"{side}_baggy_pants", (x, 0, 0.58), (0.17, 0.14, 0.38), black, 12, 6).parent = rig_root
        add_cylinder(f"{side}_shin", (x, 0, 0.25), 0.055, 0.38, black, 10).parent = rig_root
        add_cube(f"{side}_ankle_cyan_wrap", (x, -0.005, 0.18), (0.13, 0.09, 0.025), cyan).parent = rig_root
        add_cube(f"{side}_ninja_shoe", (x, -0.085, 0.05), (0.16, 0.26, 0.055), sole).parent = rig_root
        add_cube(f"{side}_shoe_cyan_trim", (x, -0.25, 0.08), (0.12, 0.02, 0.018), cyan).parent = rig_root

    add_cylinder("katana_grip_on_back", (0.37, 0.2, 1.26), 0.04, 0.38, black, 8, (math.radians(56), 0, math.radians(-42))).parent = rig_root
    add_cube("katana_guard_on_back", (0.28, 0.12, 1.16), (0.15, 0.035, 0.025), cyan, (math.radians(56), 0, math.radians(-42))).parent = rig_root
    add_cube("katana_sheath_on_back", (0.48, 0.17, 0.83), (0.045, 0.05, 0.55), blade, (math.radians(56), 0, math.radians(-42))).parent = rig_root

    blade_curve = add_bezier_curve(
        "drawn_black_katana_blade",
        [(0.63, -0.2, 0.98), (0.82, -0.34, 0.68), (0.9, -0.42, 0.25), (0.82, -0.38, -0.05)],
        0.025,
        blade,
    )
    blade_curve.parent = rig_root
    edge_curve = add_bezier_curve(
        "drawn_cyan_katana_edge",
        [(0.65, -0.23, 0.92), (0.81, -0.36, 0.62), (0.86, -0.43, 0.2)],
        0.009,
        cyan,
    )
    edge_curve.parent = rig_root

    add_basic_armature()
    add_lighting_and_camera()

    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))


def add_basic_armature():
    bpy.ops.object.armature_add(enter_editmode=True, location=(0, 0, 0))
    armature = bpy.context.object
    armature.name = "Inkronin_Gameplay_Armature"
    armature.data.name = "Inkronin_Gameplay_ArmatureData"

    root = armature.data.edit_bones[0]
    root.name = "root"
    root.head = (0, 0, 0)
    root.tail = (0, 0, 1.0)

    spine = armature.data.edit_bones.new("spine")
    spine.head = (0, 0, 0.95)
    spine.tail = (0, 0, 1.75)
    spine.parent = root

    head = armature.data.edit_bones.new("head")
    head.head = (0, 0, 1.75)
    head.tail = (0, 0, 2.3)
    head.parent = spine

    for side, x in [("L", -0.42), ("R", 0.42)]:
        upper_arm = armature.data.edit_bones.new(f"upper_arm.{side}")
        upper_arm.head = (x * 0.6, 0, 1.65)
        upper_arm.tail = (x, 0, 1.18)
        upper_arm.parent = spine

        forearm = armature.data.edit_bones.new(f"forearm.{side}")
        forearm.head = upper_arm.tail
        forearm.tail = (x * 1.05, 0, 0.82)
        forearm.parent = upper_arm

        thigh = armature.data.edit_bones.new(f"thigh.{side}")
        thigh.head = (x * 0.45, 0, 0.95)
        thigh.tail = (x * 0.55, 0, 0.48)
        thigh.parent = root

        shin = armature.data.edit_bones.new(f"shin.{side}")
        shin.head = thigh.tail
        shin.tail = (x * 0.55, 0, 0.08)
        shin.parent = thigh

    bpy.ops.object.mode_set(mode="OBJECT")


def add_lighting_and_camera():
    bpy.ops.object.light_add(type="AREA", location=(0, -3.5, 4))
    key = bpy.context.object
    key.name = "Inkronin_Key_Light"
    key.data.energy = 550
    key.data.size = 4

    bpy.ops.object.camera_add(location=(0, -5.2, 1.7), rotation=(math.radians(76), 0, 0))
    camera = bpy.context.object
    bpy.context.scene.camera = camera
    camera.name = "Inkronin_Preview_Camera"
    camera.data.lens = 55


if __name__ == "__main__":
    build_inkronin()
