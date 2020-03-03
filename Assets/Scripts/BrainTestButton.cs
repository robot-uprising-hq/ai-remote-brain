using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAgents;

public class BrainTestButton : MonoBehaviour
{
    public Button brainTestButton;

    RemoteAgent remoteAgent;
    // Start is called before the first frame update
    void Start()
    {
        remoteAgent = GameObject.FindWithTag("agent").GetComponent<RemoteAgent>();
        brainTestButton.onClick.AddListener(TestBrain);
    }

    void TestBrain()
    {
        remoteAgent.RequestDecision();
        var testObs = new float[] {0.0f, 1.0f, 0.0f, 0.0f, 0.6f, 0.0f, 1.0f, 0.0f, 0.0f, 0.7f, 1.0f, 0.0f, 0.0f, 0.0f, 0.1f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.3f, 0.0f, 0.0f, 1.0f, 0.0f, 0.9f, 1.0f, 0.0f, 0.0f, 0.0f, 0.2f};

        int action = remoteAgent.GetDecidedAction();
        Debug.Log("Action is: " + action);
    }
}
