using UnityEngine;
using System.Collections;

public class Pheromone : MonoBehaviour {
  public float radius = 0.2f;
  public float createTime;
  public AntCoreScriptableObject AntCore;
  private SpriteRenderer spriteRenderer;

  private void Awake() {
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  private void Update() {
    float aliveTime = Time.time - createTime;
    float alpha = 1 - aliveTime / AntCore.pheromoneEvaporationTime;
    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);

    if (alpha <= 0.01f) {
      AntCore.RemovePheromone(this);
      Destroy(gameObject);
    }

    transform.localScale = Vector3.one * radius;
    radius = Mathf.Max(aliveTime / AntCore.pheromoneEvaporationTime, 0.2f);
  }
}
