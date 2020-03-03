using System;
using System.Collections.Generic;
using UnityEngine;

namespace MLAgents.Sensor
{
    public class RemoteRayPerceptionSensor : ISensor
    {
        public enum CastType
        {
            Cast2D,
            Cast3D,
        }

        float[] m_Observations;
        int[] m_Shape;
        string m_Name;

        float m_RayDistance;
        List<string> m_DetectableObjects;
        float[] m_Angles;

        float m_StartOffset;
        float m_EndOffset;
        float m_CastRadius;
        CastType m_CastType;
        Transform m_Transform;
        int m_LayerMask;

        /// <summary>
        /// Debug information for the raycast hits. This is used by the RayPerceptionSensorComponent.
        /// </summary>
        public class DebugDisplayInfo
        {
            public struct RayInfo
            {
                public Vector3 localStart;
                public Vector3 localEnd;
                public Vector3 worldStart;
                public Vector3 worldEnd;
                public bool castHit;
                public float hitFraction;
                public float castRadius;
            }

            public void Reset()
            {
                m_Frame = Time.frameCount;
            }

            /// <summary>
            /// "Age" of the results in number of frames. This is used to adjust the alpha when drawing.
            /// </summary>
            public int age
            {
                get { return Time.frameCount - m_Frame; }
            }

            public RayInfo[] rayInfos;

            int m_Frame;
        }

        DebugDisplayInfo m_DebugDisplayInfo;

        public DebugDisplayInfo debugDisplayInfo
        {
            get { return m_DebugDisplayInfo; }
        }

        public RemoteRayPerceptionSensor(string name, float rayDistance, List<string> detectableObjects, float[] angles,
                                   Transform transform, float startOffset, float endOffset, float castRadius, CastType castType,
                                   int rayLayerMask)
        {
            var numObservations = (detectableObjects.Count + 2) * angles.Length;
            m_Shape = new[] { numObservations };
            m_Name = name;

            m_Observations = new float[numObservations];

            m_RayDistance = rayDistance;
            m_DetectableObjects = detectableObjects;
            // TODO - preprocess angles, save ray directions instead?
            m_Angles = angles;
            m_Transform = transform;
            m_StartOffset = startOffset;
            m_EndOffset = endOffset;
            m_CastRadius = castRadius;
            m_CastType = castType;
            m_LayerMask = rayLayerMask;

            if (Application.isEditor)
            {
                m_DebugDisplayInfo = new DebugDisplayInfo();
            }
        }

        public int Write(WriteAdapter adapter)
        {
            using (TimerStack.Instance.Scoped("RemoteRayPerceptionSensor.Perceive"))
            {
                adapter.AddRange(m_Observations);
            }
            return m_Observations.Length;
        }

        public void Update()
        {
        }

        public void SetObservations(float[] observations)
        {
            m_Observations = observations;
        }

        public int[] GetObservationShape()
        {
            return m_Shape;
        }

        public string GetName()
        {
            return m_Name;
        }

        public virtual byte[] GetCompressedObservation()
        {
            return null;
        }

        public virtual SensorCompressionType GetCompressionType()
        {
            return SensorCompressionType.None;
        }
    }
}
