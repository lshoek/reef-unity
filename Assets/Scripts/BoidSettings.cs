using UnityEngine;

public class BoidSettings
{
    // Settings
    public float minSpeed = 2f;
    public float maxSpeed = 8f;
    public float perceptionRadius = 5f;
    public float avoidanceRadius = 5f;
    public float maxSteerForce = 5f;

    public float alignWeight = 1f;
    public float cohesionWeight = 0.5f;
    public float seperateWeight = 1.5f;
    public float targetWeight = 1.125f;

    public int layer = 2;

    // collisions
    public LayerMask obstacleMask;
    public float boundsRadius = 0.5f;
    public float avoidCollisionWeight = 8f;
    public float collisionAvoidDist = 2f;
}

/*
public float minSpeed = 2f;
public float maxSpeed = 5f;
public float perceptionRadius = 2.5f;
public float avoidanceRadius = 2f;
public float maxSteerForce = 3f;

public float alignWeight = 1f;
public float cohesionWeight = 1f;
public float seperateWeight = 1f;

public float targetWeight = 1f;
public int layer = 2;

// collisions
public LayerMask obstacleMask;
public float boundsRadius = 0.27f;
public float avoidCollisionWeight = 10f;
public float collisionAvoidDist = 5f;
*/
