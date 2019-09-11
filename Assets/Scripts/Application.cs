using UnityEngine;

public class Application : MonoBehaviour
{
    public static Application Instance { get; private set; }
    [HideInInspector] public Transform WorldParent;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        WorldParent = GameObject.FindGameObjectWithTag("WorldParent").transform;
        UnityEngine.Application.targetFrameRate = 60;
    }
}
