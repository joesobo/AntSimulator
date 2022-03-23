using UnityEngine;
using System.Collections;

public class Sensor : MonoBehaviour {
  public float value = 0;
  public float radius = 1;

  private void OnDrawGizmos() {
    Gizmos.color = new Color(0.75f, 0.5f, 0.5f, 0.5f);
    Gizmos.DrawSphere(transform.position, radius);
  }
}
