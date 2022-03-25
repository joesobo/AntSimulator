using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PheromoneMap {
  public List<Pheromone> pheromones = new List<Pheromone>();

  public List<Pheromone> GetAllInCircle(Vector2 position, float radius) {
    List<Pheromone> foundPheromones = new List<Pheromone>();

    foreach (Pheromone pheromone in pheromones) {
      if (Vector2.Distance(position, pheromone.transform.position) < radius + pheromone.radius) {
        foundPheromones.Add(pheromone);
      }
    }

    return foundPheromones;
  }

  public void Add(Pheromone pheromone) {
    pheromones.Add(pheromone);
  }

  public void Remove(Pheromone pheromone) {
    if (pheromones.Contains(pheromone)) {
      pheromones.Remove(pheromone);
    }
  }
}
