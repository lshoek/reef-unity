﻿using UnityEngine;

public class Boid : MonoBehaviour
{
    private BoidSettings settings;

    public Vector2 position;
    public Vector2 forward;
    private Vector2 velocity;

    public Vector2 avgFlockHeading;
    public Vector2 centreOfFlockmates;
    public Vector2 avgAvoidanceHeading;
    public float numPerceivedFlockmates;

    public Vector2 Target { get; set; }

    public Transform InnerTransform { get; set; }
    public Renderer Renderer { get; private set; }

    public Color TintColor = Color.white;

    public bool IsActive { get; set; }

    void Awake()
    {
        Renderer = GetComponentInChildren<Renderer>();
        InnerTransform = GetComponentInChildren<Transform>();
    }

    void Start()
    {
        Renderer.material.SetFloat("_Alpha", 0);

        position = transform.position;
        forward = transform.up;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = forward * startSpeed;

        IsActive = false;
    }

    public void Init(BoidSettings settings)
    {
        this.settings = settings;
    }

    public void UpdateBoid()
    {
        Vector2 acceleration = Vector2.zero;

        if (Target != Vector2.zero)
        {
            Vector2 offsetToTarget = new Vector2(Target.x, Target.y) - position;
            acceleration = SteerTowards(offsetToTarget) * settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector2 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            Vector2 alignmentForce = SteerTowards(avgFlockHeading) * settings.alignWeight;
            Vector2 cohesionForce = SteerTowards(offsetToFlockmatesCentre) * settings.cohesionWeight;
            Vector2 seperationForce = SteerTowards(avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision())
        {
            Vector2 collisionAvoidDir = ObstacleRays();
            Vector2 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        transform.position += new Vector3(velocity.x, velocity.y, 0) * Time.deltaTime;
        transform.up = dir;
        position = transform.position;
        forward = dir;
    }

    Vector2 SteerTowards(Vector2 vector)
    {
        Vector2 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector2.ClampMagnitude(v, settings.maxSteerForce);
    }

    bool IsHeadingForCollision()
    {
        RaycastHit hit;
        if (Physics.SphereCast(To3D(position), settings.boundsRadius, forward, out hit, settings.collisionAvoidDist, settings.obstacleMask))
        {
            return true;
        }
        return false;
    }

    Vector2 ObstacleRays()
    {
        Vector2[] rayDirections = BoidHelper.directions;
        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector2 worldSpaceDir = transform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(To3D(position), worldSpaceDir);
            if (!Physics.SphereCast(ray, settings.boundsRadius, settings.collisionAvoidDist, settings.obstacleMask))
                return worldSpaceDir;
        }
        return forward;
    }

    private Vector3 To3D(Vector2 vec2d)
    {
        return new Vector3(vec2d.x, vec2d.y, settings.layer);
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.Out, duration, 
            (x) => Renderer.material.SetFloat("_Alpha", x),
            () => IsActive = false));
    }

    public void FadeIn(float duration)
    {
        StartCoroutine(ReefHelper.FadeNormalized(ReefHelper.FadeType.In, duration, 
            (x) => Renderer.material.SetFloat("_Alpha", x),
            null));
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(To3D(position), settings.boundsRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(To3D(position), To3D(position + forward));
        Gizmos.DrawLine(To3D(position), To3D(position + Target.normalized));
    }
}

