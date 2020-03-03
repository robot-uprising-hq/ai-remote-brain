using System;
using UnityEngine;

namespace MLAgents.Sensor
{
    [AddComponentMenu("ML Agents/Remote Ray Perception Sensor", (int)MenuGroup.Sensors)]
    public class RemoteRayPerceptionSensorComponent : RemoteRayPerceptionSensorComponentBase
    {
        public override RemoteRayPerceptionSensor.CastType GetCastType()
        {
            return RemoteRayPerceptionSensor.CastType.Cast3D;
        }
    }
}
