using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Ant))]
public class AntEditor : Editor {

  public void OnSceneGUI() {
    Ant ant = target as Ant;

    Handles.color = new Color(0.8f, 0.8f, 1, 0.05f);

    Handles.DrawSolidArc(ant.transform.position, ant.transform.forward, ant.transform.right, ant.viewAngle / 2f, ant.viewRadius);
    Handles.DrawSolidArc(ant.transform.position, ant.transform.forward, ant.transform.right, -ant.viewAngle / 2f, ant.viewRadius);
  }
}
