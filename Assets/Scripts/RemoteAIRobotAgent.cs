using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RemoteAIRobotAgent : Agent
{
    public volatile int m_ArucoMarkerID;

    private volatile float[] lowerSensor;
    private volatile float[] upperSensor;
    private volatile float action = -1;

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(lowerSensor);
        sensor.AddObservation(upperSensor);
    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void OnActionReceived(float[] vectorAction)
    {
        action = vectorAction[0];
    }

    /// <summary>
    /// Set agent's observations.
    /// </summary>
    public void SetObservations(float[] lowerObservations, float[] upperObservations)
    {
        action = -1;
        lowerSensor = lowerObservations;
        upperSensor = upperObservations;
    }

    /// <summary>
    /// External scripts can get the agents new action.
    /// </summary>
    public int GetDecidedAction()
    {
        return (int)action;
    }
}
