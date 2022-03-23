using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "AntCoreData", menuName = "ScriptableObjects/AntCoreScriptableObject", order = 1)]
public class AntCoreScriptableObject : SerializedScriptableObject {
  public List<Ant> ants = new List<Ant>();
  public PheromoneMap homeMarkers = new PheromoneMap();
  public PheromoneMap foodMarkers = new PheromoneMap();
  public float pheromoneEvaporationTime = 5;

  private void OnEnable() {
    ants.Clear();
    homeMarkers = new PheromoneMap();
    foodMarkers = new PheromoneMap();
  }

  public void RemovePheromone(Pheromone pheromone) {
    homeMarkers.Remove(pheromone);
    foodMarkers.Remove(pheromone);
  }
}
