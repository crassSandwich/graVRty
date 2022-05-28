using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraVRty.Loading;

namespace GraVRty.CorePhysics
{
    [CreateAssetMenu(menuName = "GraVRty/Gravity Manager", fileName = "newGravityManager.asset")]
    public class Gravity : Loadable
    {
        [SerializeField] float m_GravityAcceleration, m_FluxDragMax;
        [SerializeField] AnimationCurve m_FluxDragLerpByPressStrength, m_FluxDragLerpSpeedByPressStrength;

        [Range(0, 1)]
        [SerializeField] float m_FluxPressThreshold, m_FluxHardStopThreshold;

        public GravityState State { get; private set; }
        public Vector3 Direction { get; private set; }
        public Quaternion Rotation { get; private set; }

        float dragLerp;

        public override IEnumerator LoadRoutine ()
        {
            State = GravityState.Active;
            setOrientation(Vector3.up, Quaternion.identity);
            yield break;
        }

        public void SetGravity (Transform directionSource, float strength)
        {
            if (strength >= m_FluxPressThreshold)
            {
                updateState(GravityState.Flux);

                float targetDragLerp = m_FluxDragLerpByPressStrength.Evaluate(strength),
                    dragLerpSpeed = m_FluxDragLerpSpeedByPressStrength.Evaluate(strength);

                if (dragLerp < targetDragLerp)
                    dragLerp = Mathf.Min(targetDragLerp, dragLerp + dragLerpSpeed * Time.deltaTime);
                else
                    dragLerp = Mathf.Max(targetDragLerp, dragLerp - dragLerpSpeed * Time.deltaTime);

                setOrientation(directionSource);

                if (strength >= m_FluxHardStopThreshold) Physics.gravity = Vector3.zero;
            }
            else
            {
                // ensure that gravity is properly set when the player lets go of their action button. especially useful for preventing situations where letting go of the action button too fast keeps gravity stuck at 0
                if (State == GravityState.Flux) setOrientation(directionSource);

                updateState(GravityState.Active);
                dragLerp = 0;
            }
        }

        public float GetGravitizerDrag (RigidbodyGravitizer gravitizer)
        {
            return Mathf.Lerp(gravitizer.BaseDrag, m_FluxDragMax, dragLerp);
        }

        void setOrientation (Transform source)
        {
            setOrientation(source.up, source.rotation);
        }

        void setOrientation (Vector3 up, Quaternion rotation)
        {
            Direction = -up;
            Rotation = rotation;

            Physics.gravity = Direction * m_GravityAcceleration;
        }

        void updateState (GravityState newState)
        {
            if (State != newState) State = newState;
        }
    }
}
