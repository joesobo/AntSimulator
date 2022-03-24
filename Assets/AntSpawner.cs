using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AntSpawner : MonoBehaviour {
  public GameObject antPrefab;
  public Transform pheromonesParent;
  public int numAnts = 10;
  public TextMeshProUGUI foodCountText;

  private int foodCount = 0;

  private void Start() {
    SpawnAnts();
  }

  public void SpawnAnts() {
    for (int i = 0; i < numAnts; i++) {
      Ant ant = Instantiate(antPrefab, transform.position, Quaternion.Euler(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), 0), transform).GetComponent<Ant>();
      ant.home = transform;
      ant.antSpawner = this;
      ant.pheromonesParent = pheromonesParent;
    }
  }

  public void AddFood(int amount) {
    foodCount += amount;

    foodCountText.text = foodCount.ToString();
  }
}
