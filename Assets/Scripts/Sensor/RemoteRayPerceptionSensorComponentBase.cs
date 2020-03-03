using System;
using System.Collections.Generic;
using UnityEngine;

namespace MLAgents.Sensor
{
    public abstract class RemoteRayPerceptionSensorComponentBase : SensorComponent
    {
        public string sensorName = "RayPerceptionSensor";

        [Tooltip("List of tags in the scene to compare against.")]
        public List<string> detectableTags;

        [Range(0, 50)]
        [Tooltip("Number of rays to the left and right of center.")]
        public int raysPerDirection = 3;

        [Range(0, 180)]
        [Tooltip("Cone size for rays. Using 90 degrees will cast rays to the left and right. Greater than 90 degrees will go backwards.")]
        public float maxRayDegrees = 70;

        [Range(0f, 10f)]
        [Tooltip("Radius of sphere to cast. Set to zero for raycasts.")]
        public float sphereCastRadius = 0.5f;

        [Range(1, 1000)]
        [Tooltip("Length of the rays to cast.")]
        public float rayLength = 20f;

        [Tooltip("Controls which layers the rays can hit.")]
        public LayerMask rayLayerMask = Physics.DefaultRaycastLayers;

        [Range(1, 50)]
        [Tooltip("Whether to stack previous observations. Using 1 means no previous observations.")]
        public int observationStacks = 1;

        [Header("Debug Gizmos", order = 999)]
        public Color rayHitColor = Color.red;
        public Color rayMissColor = Color.white;
        [Tooltip("Whether to draw the raycasts in the world space of when they happened, or using the Agent's current transform'")]
        public bool useWorldPositions = true;


        [NonSerialized]
        public RemoteRayPerceptionSensor m_RaySensor;

        public abstract RemoteRayPerceptionSensor.CastType GetCastType();

        public virtual float GetStartVerticalOffset()
        {
            return 0f;
        }

        public virtual float GetEndVerticalOffset()
        {
            return 0f;
        }

        public override ISensor CreateSensor()
        {
            var rayAngles = GetRayAngles(raysPerDirection, maxRayDegrees);
            m_RaySensor = new RemoteRayPerceptionSensor(sensorName, rayLength, detectableTags, rayAngles,
                transform, GetStartVerticalOffset(), GetEndVerticalOffset(), sphereCastRadius, GetCastType(),
                rayLayerMask
            );

            if (observationStacks != 1)
            {
                var stackingSensor = new StackingSensor(m_RaySensor, observationStacks);
                return stackingSensor;
            }

            return m_RaySensor;
        }

        public static float[] GetRayAngles(int raysPerDirection, float maxRayDegrees)
        {
            // Example:
            // { 90, 90 - delta, 90 + delta, 90 - 2*delta, 90 + 2*delta }
            var anglesOut = new float[2 * raysPerDirection + 1];
            var delta = maxRayDegrees / raysPerDirection;
            anglesOut[0] = 90f;
            for (var i = 0; i < raysPerDirection; i++)
            {
                anglesOut[2 * i + 1] = 90 - (i + 1) * delta;
                anglesOut[2 * i + 2] = 90 + (i + 1) * delta;
            }
            return anglesOut;
        }

        public override int[] GetObservationShape()
        {
            var numRays = 2 * raysPerDirection + 1;
            var numTags = detectableTags == null ? 0 : detectableTags.Count;
            var obsSize = (numTags + 2) * numRays;
            var stacks = observationStacks > 1 ? observationStacks : 1;
            return new[] { obsSize * stacks };
        }
    }
}
