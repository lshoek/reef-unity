using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Scheduler m_scheduler;

    void Start()
    {
        m_scheduler = Application.Instance.Scheduler;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            m_scheduler.QueueAction("Title_NelsonKwame", true);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            m_scheduler.QueueAction("Title_Bastienne", true);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            m_scheduler.QueueAction("Title_PitchControllerMitch", true);
        }
    }
}
