using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ant : MonoBehaviour {
  public float maxSpeed = 2;
  public float steerStrength = 2;
  public float wanderStrength = 1;
  public float viewRadius = 3;
  public float viewAngle = 45;
  public float interactRadius = 0.05f;
  public LayerMask foodLayer;
  public Transform home;
  public Transform head;
  public Transform pheromonesParent;
  public Pheromone homePheromonePrefab;
  public Pheromone foodPheromonePrefab;

  public AntCoreScriptableObject AntCore;

  public Sensor leftSensor;
  public Sensor centerSensor;
  public Sensor rightSensor;

  private Vector2 position;
  private Vector2 velocity;
  private Vector2 desiredDirection = Vector2.right;
  private Transform targetFood;
  private bool searchingForFood = true;

  private void Start() {
    AntCore.ants.Add(this);
    StartCoroutine(SpawnPheromone());
  }

  private void Update() {
    HandlePheromoneSteering();
    if (searchingForFood) {
      HandleFood();
    } else {
      HandleHome();
    }

    desiredDirection = (desiredDirection + Random.insideUnitCircle * wanderStrength).normalized;

    Vector2 desireVelocity = desiredDirection * maxSpeed;
    Vector2 desiredSteeringForce = (desireVelocity - velocity) * steerStrength;
    Vector2 acceleration = Vector2.ClampMagnitude(desiredSteeringForce, steerStrength) / 1;

    velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
    position += velocity * Time.deltaTime;

    float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
    transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));
  }

  private void HandleFood() {
    if (targetFood == null) {
      Collider2D[] allFood = Physics2D.OverlapCircleAll(position, viewRadius, foodLayer);

      if (allFood.Length > 0) {
        Transform food = allFood[Random.Range(0, allFood.Length)].transform;
        Vector2 dirToFood = (food.position - head.position).normalized;

        // only target food in view angle
        if (Vector2.Angle(transform.right, dirToFood) < viewAngle / 2) {
          food.gameObject.layer = LayerMask.NameToLayer("TakenFood");
          targetFood = food;
        }
      }
    } else {
      desiredDirection = (targetFood.position - head.position).normalized;


      if (Vector2.Distance(targetFood.position, head.position) < interactRadius) {
        targetFood.position = head.position;
        targetFood.parent = head;
        targetFood = null;
        searchingForFood = false;
      }
    }
  }

  private void HandlePheromoneSteering() {
    UpdateSensor(leftSensor);
    UpdateSensor(centerSensor);
    UpdateSensor(rightSensor);

    if (centerSensor.value > Mathf.Max(leftSensor.value, rightSensor.value)) {
      desiredDirection = transform.right * 1.5f;
    } else if (leftSensor.value > rightSensor.value) {
      desiredDirection = transform.right + transform.up * 0.75f;
    } else {
      desiredDirection = transform.right + transform.up * -0.75f;
    }
  }

  private void UpdateSensor(Sensor sensor) {
    sensor.value = 0;

    PheromoneMap map = (searchingForFood) ? AntCore.foodMarkers : AntCore.homeMarkers;
    List<Pheromone> pheromones = map.GetAllInCircle(sensor.transform.position, sensor.radius);

    foreach (Pheromone pheromone in pheromones) {
      float lifeTime = Time.time - pheromone.createTime;
      float evaporationAmount = Mathf.Min(1, lifeTime / AntCore.pheromoneEvaporationTime);
      sensor.value += 1 - evaporationAmount;
    }
  }

  private void HandleHome() {
    if (Vector2.Distance(home.position, head.position) < 1) {
      foreach (Transform child in head) {
        Destroy(child.gameObject);
      }
      searchingForFood = true;
    }
  }

  IEnumerator SpawnPheromone() {
    while (true) {
      if (searchingForFood) {
        Pheromone homePheromone = Instantiate(homePheromonePrefab, transform.position, Quaternion.identity, pheromonesParent);
        homePheromone.createTime = Time.time;
        AntCore.homeMarkers.Add(homePheromone);
      } else {
        Pheromone foodPheromone = Instantiate(foodPheromonePrefab, transform.position, Quaternion.identity, pheromonesParent);
        foodPheromone.createTime = Time.time;
        AntCore.foodMarkers.Add(foodPheromone);
      }
      yield return new WaitForSeconds(.25f);
    }
  }
}
