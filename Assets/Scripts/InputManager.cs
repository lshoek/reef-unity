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
        else if (Input.GetKeyDown(KeyCode.Plus))
        {
            int layerIndex = ((int)m_scheduler.CurrentAquaticLayerMode + 1) % ReefHelper.NumAquaticLayers;
            m_scheduler.CurrentAquaticLayerMode = (ReefHelper.AquaticLayerMode)layerIndex;
        }
        else if (Input.GetKeyDown(KeyCode.Minus))
        {
            int layerIndex = ((int)m_scheduler.CurrentAquaticLayerMode - 1) % ReefHelper.NumAquaticLayers;
            m_scheduler.CurrentAquaticLayerMode = (ReefHelper.AquaticLayerMode)layerIndex;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            m_scheduler.QueueAction("EndCurrentShow", true);
        }
    }
}
