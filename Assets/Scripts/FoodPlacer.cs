using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPlacer : MonoBehaviour {
  public GameObject FoodPrefab;

  private void Update() {
    if (Input.GetMouseButtonDown(0)) {
      Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      mousePos.z = 0;
      Instantiate(FoodPrefab, mousePos, Quaternion.identity);
    }
  }
}
