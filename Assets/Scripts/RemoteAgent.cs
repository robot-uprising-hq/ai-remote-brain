//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using MLAgents;
using MLAgents.Sensor;

public class RemoteAgent : Agent
{
    PushBlockSettings m_PushBlockSettings;

    public RemoteRayPerceptionSensorComponent lowerSensor;
    public RemoteRayPerceptionSensorComponent upperSensor;
    public bool useVectorObs;

    private float action = -1;


    void Awake()
    {
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();
    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void AgentAction(float[] vectorAction)
    {
        action = vectorAction[0];
    }

    /// <summary>
    /// Set agent's observations.
    /// </summary>
    public void SetObservations(float[] lowerObservations, float[] upperObservations)
    {
        lowerSensor.m_RaySensor.SetObservations(lowerObservations);
        upperSensor.m_RaySensor.SetObservations(upperObservations);
    }

    /// <summary>
    /// External scripts can get the agents new action.
    /// </summary>
    public int GetDecidedAction()
    {
        return (int)action;
    }
}
