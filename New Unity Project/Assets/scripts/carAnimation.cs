﻿using System;
using UnityEngine;

namespace MLcar.carSystems
{
    /// <summary>
    /// This class handles all the canned and procedural animation for the car, giving it a more pleasing appearance.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class carAnimation : MonoBehaviour
    {
        /// <summary>
        /// A class representing an individual wheel on a car.  This can be used to represent either front or back wheels.
        /// </summary>
        [Serializable]
        public class Wheel
        {
            [Tooltip("A reference to the transform of the wheel.")]
            public Transform wheelTransform;
            [Tooltip("A vector representing the axel around which the wheel turns as the kart moves forward and backward.  This is relative to the wheel.")]
            public Vector3 axelAxis;
            [Tooltip("A vector around which the wheel will turn as the kart steers.  This is relative to the kart and will be ignored for the back wheels.")]
            public Vector3 steeringAxis;

            Vector3 m_NormalizedAxelAxis;
            Vector3 m_NormalizedSteeringAxis;
            Quaternion m_SteerlessLocalRotation;

            /// <summary>
            /// This initialises the cached values that the Wheel uses for various operations and should be called once before any other methods.
            /// </summary>
            public void Setup()
            {
                m_NormalizedAxelAxis = axelAxis.normalized;
                m_NormalizedSteeringAxis = steeringAxis.normalized;
                m_SteerlessLocalRotation = wheelTransform.localRotation;
            }

            /// <summary>
            /// Some rotations are made relative to a default rotation.  This should be called to store that rotation.
            /// </summary>
            public void StoreDefaultRotation()
            {
                m_SteerlessLocalRotation = wheelTransform.localRotation;
            }

            /// <summary>
            /// Some rotations are made relative to a default rotation.  This restores that default rotation.
            /// </summary>
            public void SetToDefaultRotation()
            {
                wheelTransform.localRotation = m_SteerlessLocalRotation;
            }

            /// <summary>
            /// Rotates the wheel around its axel.
            /// </summary>
            /// <param name="rotationAngle">The angle in degrees by which the wheel rotates.</param>
            public void TurnWheel(float rotationAngle)
            {
                wheelTransform.Rotate(m_NormalizedAxelAxis, rotationAngle, Space.Self);
            }

            /// <summary>
            /// Rotates the wheel around its steering axis.
            /// </summary>
            /// <param name="rotationAngle">The angle from a neutral position that the wheel should face.</param>
            public void SteerWheel(float rotationAngle)
            {
                wheelTransform.Rotate(m_NormalizedSteeringAxis, rotationAngle, Space.World);
            }
        }

        [Tooltip("What car do we want to listen to?")]
        public macar carController;

        [Space]
        [Tooltip("The damping for the appearance of steering compared to the input.  The higher the number the less damping.")]
        public float steeringAnimationDamping = 10f;

        [Space]
        [Tooltip("The radius of the front wheels of the car.  Used to calculate how far the front wheels need to turn given the speed of the car.")]
        public float frontWheelRadius;
        [Tooltip("The radius of the rear wheels of the car.  Used to calculate how far the rear wheels need to turn given the speed of the car.")]
        public float rearWheelRadius;
        [Tooltip("The maximum angle in degrees that the front wheels can be turned away from their default positions, when the Steering input is either 1 or -1.")]
        public float maxSteeringAngle;
        [Tooltip("Information referring to the front left wheel of the car.")]
        public Wheel frontLeftWheel;
        [Tooltip("Information referring to the front right wheel of the car.")]
        public Wheel frontRightWheel;
        [Tooltip("Information referring to the rear left wheel of the car.")]
        public Wheel rearLeftWheel;
        [Tooltip("Information referring to the rear right wheel of the car.")]
        public Wheel rearRightWheel;

        float m_InverseFrontWheelRadius;
        float m_InverseRearWheelRadius;
        float m_SmoothedSteeringInput;

        void Start()
        {
            frontLeftWheel.Setup();
            frontRightWheel.Setup();
            rearLeftWheel.Setup();
            rearRightWheel.Setup();

            m_InverseFrontWheelRadius = 1f / frontWheelRadius;
            m_InverseRearWheelRadius = 1f / rearWheelRadius;
        }

        void FixedUpdate()
        {
            m_SmoothedSteeringInput = Mathf.MoveTowards(m_SmoothedSteeringInput, carController.Input.x,
                steeringAnimationDamping * Time.deltaTime);
        }

        void LateUpdate()
        {
            RotateWheels();
        }

        void RotateWheels()
        {
            frontLeftWheel.SetToDefaultRotation();
            frontRightWheel.SetToDefaultRotation();

            float speed = carController.LocalSpeed() * 10f;
            float rotationAngle = speed * Time.deltaTime * m_InverseFrontWheelRadius * Mathf.Rad2Deg;
            frontLeftWheel.TurnWheel(rotationAngle);
            frontRightWheel.TurnWheel(rotationAngle);

            rotationAngle = speed * Time.deltaTime * m_InverseRearWheelRadius * Mathf.Rad2Deg;

            rearLeftWheel.TurnWheel(rotationAngle);
            rearRightWheel.TurnWheel(rotationAngle);

            frontLeftWheel.StoreDefaultRotation();
            frontRightWheel.StoreDefaultRotation();

            rotationAngle = m_SmoothedSteeringInput * maxSteeringAngle;
            frontLeftWheel.SteerWheel(rotationAngle);
            frontRightWheel.SteerWheel(rotationAngle);
        }
    }
}
