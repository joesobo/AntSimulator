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
  public float pheromoneInterval = 0.25f;
  public float turnTime = 0.75f;
  public LayerMask foodLayer;
  public LayerMask collisionLayer;
  [HideInInspector] public Transform home;
  [HideInInspector] public AntSpawner antSpawner;
  public Transform head;
  [HideInInspector] public Transform pheromonesParent;
  public Pheromone homePheromonePrefab;
  public Pheromone foodPheromonePrefab;

  public AntCoreScriptableObject AntCore;

  public Sensor leftSensor;
  public Sensor centerSensor;
  public Sensor rightSensor;

  private Vector2 position;
  private Vector2 velocity;
  private Vector2 desiredDirection;
  private Transform targetFood;
  private bool searchingForFood = true;
  private Vector2 lastCollisionNormal = Vector2.zero;

  private bool turnLeft = false;
  private bool turnRight = false;

  private void Start() {
    AntCore.ants.Add(this);
    desiredDirection = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)).normalized;
    StartCoroutine(SpawnPheromone());
  }

  private void Update() {
    HandlePheromoneSteering();

    // random direction offset
    desiredDirection = (desiredDirection + Random.insideUnitCircle * wanderStrength).normalized;

    if (searchingForFood) {
      HandleFood();
    } else {
      HandleHome();
    }

    HandleCollision();

    // movement calculation
    Vector2 desireVelocity = desiredDirection * maxSpeed;
    Vector2 desiredSteeringForce = (desireVelocity - velocity) * steerStrength;
    Vector2 acceleration = Vector2.ClampMagnitude(desiredSteeringForce, steerStrength) / 1;

    velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxSpeed);
    position += velocity * Time.deltaTime;

    // move
    float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
    transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, angle));
  }

  private void HandlePheromoneSteering() {
    UpdateSensor(leftSensor);
    UpdateSensor(centerSensor);
    UpdateSensor(rightSensor);

    if (leftSensor.value == 0 && centerSensor.value == 0 && rightSensor.value == 0) {
      return;
    }

    if (centerSensor.value > Mathf.Max(leftSensor.value, rightSensor.value)) {
      desiredDirection = transform.right * 1.5f;
    } else if (leftSensor.value > rightSensor.value) {
      desiredDirection = transform.right * 0.75f + transform.up * 0.75f;
    } else {
      desiredDirection = transform.right * 0.75f + transform.up * -0.75f;
    }
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

        // turn around after finding food
        StartCoroutine(TurnAround());
      }
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
    if (Vector2.Distance(home.position, head.position) < viewRadius) {
      Vector2 dirToHome = (home.position - head.position).normalized;

      // only target food in view angle
      if (Vector2.Angle(transform.right, dirToHome) < viewAngle / 2) {
        desiredDirection = dirToHome;
      }
    }

    if (Vector2.Distance(home.position, head.position) < interactRadius) {
      // drop off food
      antSpawner.AddFood(1);

      foreach (Transform child in head) {
        Destroy(child.gameObject);
      }
      searchingForFood = true;

      // turn around and start finding food
      StartCoroutine(TurnAround());
    }
  }

  private void HandleCollision() {
    Vector2 headPosition = (Vector2)(head.position);

    // cast 2 ray cast out at angle from front of ant
    RaycastHit2D hitLeft = Physics2D.Raycast(headPosition, (headPosition - (Vector2)transform.position + (Vector2)(transform.right + transform.up * 0.85f)) * 0.75f, 1f, collisionLayer);
    RaycastHit2D hitRight = Physics2D.Raycast(headPosition, (headPosition - (Vector2)transform.position + (Vector2)(transform.right + transform.up * -0.85f)) * 0.75f, 1f, collisionLayer);

    if (hitLeft.collider != null) {
      turnRight = true;
      turnLeft = false;
    } else if (hitRight.collider != null) {
      turnLeft = true;
      turnRight = false;
    }

    // if any of the rays hit a wall
    if (turnLeft || turnRight) {
      RaycastHit2D hit = turnLeft ? hitLeft : hitRight;
      // find the normal of the hit point
      lastCollisionNormal = hit.normal;

      Vector3 incomingVec = hit.point - headPosition;

      // Use the point's normal to calculate the reflection vector.
      Vector3 reflectVec = Vector3.Reflect(incomingVec, hit.normal).normalized;

      // Draw lines to show the incoming "beam" and the reflection.
      lastCollisionNormal = reflectVec;
    }

    // TODO: implement turning speed based on proximity to collision
    if (lastCollisionNormal != Vector2.zero) {
      // pick direction to turn
      desiredDirection = turnLeft ? transform.up : -transform.up;

      // continue turning until the last normal direction matches one of the 2 ray cast directions
      if (
        Vector2.Distance((Vector2)(transform.right + transform.up * 0.85f).normalized, lastCollisionNormal.normalized) < 0.1f ||
        Vector2.Distance((Vector2)(transform.right + transform.up * -0.85f).normalized, lastCollisionNormal.normalized) < 0.1f
      ) {
        // reset back to zero
        lastCollisionNormal = Vector2.zero;
        turnLeft = false;
        turnRight = false;
      }
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
      yield return new WaitForSeconds(pheromoneInterval);
    }
  }

  IEnumerator TurnAround() {
    float counter = 0;

    while (counter <= turnTime) {
      counter += Time.deltaTime;

      desiredDirection = -transform.up;

      yield return null;
    }
  }

  private void OnDrawGizmos() {
    Vector2 headPosition = (Vector2)(head.position);

    // antennae collision view
    Gizmos.color = Color.green;
    Gizmos.DrawRay(headPosition, (headPosition - (Vector2)transform.position + (Vector2)(transform.right + transform.up * 0.85f) * 0.75f));
    Gizmos.DrawRay(headPosition, (headPosition - (Vector2)transform.position + (Vector2)(transform.right + transform.up * -0.85f) * 0.75f));

    // area of avoidance
    Gizmos.color = Color.white;
    Gizmos.DrawWireSphere(transform.position, 0.4f);

    // last collision normal
    if (lastCollisionNormal != Vector2.zero) {
      Gizmos.color = Color.yellow;
      Gizmos.DrawRay(headPosition, (headPosition - (Vector2)transform.position + lastCollisionNormal) * 1.75f);
    }
  }
}
