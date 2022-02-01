using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ActionInspector {
  private struct ActionDrawer {
    public bool Open;
    public bool[] StepsOpen; // TODO: make steps fold
    public ActionDrawer (Action action) {
      Open = false;
      StepsOpen = new bool[action.Steps.Length];
      for (int i = 0; i < action.Steps.Length; ++i)
        StepsOpen[i] = false;
    }
  }
  private ActionDrawer[] drawers;

  public void Draw (Action[] actions) {
    if (drawers == null) {
      drawers = new ActionDrawer[actions.Length];
      for (int i = 0; i < actions.Length; ++i)
        drawers[i] = new ActionDrawer(actions[i]);
    }
      
    for (int i = 0; i < actions.Length; ++i) {
      var action = actions[i];  

      drawers[i].Open = EditorGUILayout.BeginFoldoutHeaderGroup(drawers[i].Open, action.Name);
      if (drawers[i].Open) {
        for (int j = 0; j < action.Steps.Length; ++j) {
          var step = action.Steps[j];

          EditorGUILayout.BeginHorizontal();
          EditorGUILayout.LabelField($"Step {j}", GUILayout.Width(50));
          step.Freeze = step.Freeze
          ? !GUILayout.Button("Unfreeze")
          : GUILayout.Button("Freeze");
          EditorGUILayout.EndHorizontal();

          step.Duration = EditorGUILayout.IntField("Duration", step.Duration);

          EditorGUILayout.LabelField("Sprite Offset", EditorStyles.boldLabel);
          step.SpriteOffset = EditorGUILayout.Vector2Field(GUIContent.none, step.SpriteOffset);

          EditorGUILayout.LabelField("Hit-Box Offset and Size", EditorStyles.boldLabel);
          var hitbox_offset = EditorGUILayout.Vector2Field(GUIContent.none, new Vector2(step.MovementBox.OffsetX, step.MovementBox.OffsetY));
          step.MovementBox.OffsetX = hitbox_offset.x;
          step.MovementBox.OffsetY = hitbox_offset.y;
          var size = EditorGUILayout.Vector2Field(GUIContent.none, new Vector2(step.MovementBox.W, step.MovementBox.H));
          step.MovementBox.W = size.x;
          step.MovementBox.H = size.y;

          EditorGUILayout.LabelField("Start and Update Displacement", EditorStyles.boldLabel);
          step.StartDisplacement = EditorGUILayout.Vector2Field(GUIContent.none, step.StartDisplacement);
          step.UpdateDisplacement = EditorGUILayout.Vector2Field(GUIContent.none, step.UpdateDisplacement);

          if (action.GetType() == typeof(Attack) && ((AttackStep) step).AttackShape != null) {
            var attack_step = ((AttackStep) step);

            EditorGUILayout.LabelField("Attack Offset and Size", EditorStyles.boldLabel);
            if (attack_step.AttackShape.GetType() == typeof(HitBox)) {
              var attack_box = (HitBox) attack_step.AttackShape;
              var attack_offset = EditorGUILayout.Vector2Field(GUIContent.none, new Vector2(attack_box.OffsetX, attack_box.OffsetY));
              attack_box.OffsetX = attack_offset.x;
              attack_box.OffsetY = attack_offset.y;
              var attack_size = EditorGUILayout.Vector2Field(GUIContent.none, new Vector2(attack_box.W, attack_box.H));
              attack_box.W = attack_size.x;
              attack_box.H = attack_size.y;
            }
          }
          EditorGUILayout.Separator();
        }
      }
      EditorGUILayout.EndFoldoutHeaderGroup();
    }
  }
}
