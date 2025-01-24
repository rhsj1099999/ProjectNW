using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KinematicCharacterController
{
    [DefaultExecutionOrder(-100)]
    public class KinematicCharacterSystem : MMonoBehaviour
    {
        private static KinematicCharacterSystem _instance;

        public static List<KinematicCharacterMotor> CharacterMotors = new List<KinematicCharacterMotor>();
        //public static List<PhysicsMover> PhysicsMovers = new List<PhysicsMover>();

        private static float _lastCustomInterpolationStartTime = -1f;
        private static float _lastCustomInterpolationDeltaTime = -1f;

        public static KCCSettings Settings;

        /// <summary>
        /// Creates a KinematicCharacterSystem instance if there isn't already one
        /// </summary>
        public static void EnsureCreation()
        {
            if (_instance == null)
            {
                GameObject systemGameObject = new GameObject("KinematicCharacterSystem");
                _instance = systemGameObject.AddComponent<KinematicCharacterSystem>();

                systemGameObject.hideFlags = HideFlags.NotEditable;
                _instance.hideFlags = HideFlags.NotEditable;

                Settings = ScriptableObject.CreateInstance<KCCSettings>();

                GameObject.DontDestroyOnLoad(systemGameObject);
            }
        }

        /// <summary>
        /// Gets the KinematicCharacterSystem instance if any
        /// </summary>
        /// <returns></returns>
        public static KinematicCharacterSystem GetInstance()
        {
            return _instance;
        }

        /// <summary>
        /// Sets the maximum capacity of the character motors list, to prevent allocations when adding characters
        /// </summary>
        /// <param name="capacity"></param>
        public static void SetCharacterMotorsCapacity(int capacity)
        {
            if (capacity < CharacterMotors.Count)
            {
                capacity = CharacterMotors.Count;
            }
            CharacterMotors.Capacity = capacity;
        }

        /// <summary>
        /// Registers a KinematicCharacterMotor into the system
        /// </summary>
        public static void RegisterCharacterMotor(KinematicCharacterMotor motor)
        {
            CharacterMotors.Add(motor);
        }

        /// <summary>
        /// Unregisters a KinematicCharacterMotor from the system
        /// </summary>
        public static void UnregisterCharacterMotor(KinematicCharacterMotor motor)
        {
            CharacterMotors.Remove(motor);
        }

        private void OnDisable()
        {
            Destroy(this.gameObject);
        }

        private void Awake()
        {
            _instance = this;
        }

        private void KCCCall(float deltaTime)
        {
            PreSimulationInterpolationUpdate(deltaTime);
            Simulate(deltaTime, CharacterMotors);
            PostSimulationInterpolationUpdate(deltaTime);
        }

        private void Update()
        {
            SynchronizedUpdate(KCCCall);
        }

        public static void PreSimulationInterpolationUpdate(float deltaTime)
        {
            // Save pre-simulation poses and place transform at transient pose
            for (int i = 0; i < CharacterMotors.Count; i++)
            {
                KinematicCharacterMotor motor = CharacterMotors[i];

                motor.InitialTickPosition = motor.TransientPosition;
                motor.InitialTickRotation = motor.TransientRotation;

                motor.Transform.SetPositionAndRotation(motor.TransientPosition, motor.TransientRotation);
            }
        }

        public static void Simulate(float deltaTime, List<KinematicCharacterMotor> motors)
        {
            int characterMotorsCount = motors.Count;

            for (int i = 0; i < characterMotorsCount; i++)
            {
                motors[i].UpdatePhase1(deltaTime);
            }

            for (int i = 0; i < characterMotorsCount; i++)
            {
                KinematicCharacterMotor motor = motors[i];

                motor.UpdatePhase2(deltaTime);

                motor.Transform.SetPositionAndRotation(motor.TransientPosition, motor.TransientRotation);
            }
        }

        public static void PostSimulationInterpolationUpdate(float deltaTime)
        {
            _lastCustomInterpolationStartTime = Time.time;
            _lastCustomInterpolationDeltaTime = deltaTime;

            // Return interpolated roots to their initial poses
            for (int i = 0; i < CharacterMotors.Count; i++)
            {
                KinematicCharacterMotor motor = CharacterMotors[i];

                motor.Transform.SetPositionAndRotation(motor.InitialTickPosition, motor.InitialTickRotation);
            }
        }

        private static void CustomInterpolationUpdate()
        {
            float interpolationFactor = Mathf.Clamp01((Time.time - _lastCustomInterpolationStartTime) / _lastCustomInterpolationDeltaTime);

            // Handle characters interpolation
            for (int i = 0; i < CharacterMotors.Count; i++)
            {
                KinematicCharacterMotor motor = CharacterMotors[i];

                motor.Transform.SetPositionAndRotation(
                    Vector3.Lerp(motor.InitialTickPosition, motor.TransientPosition, interpolationFactor),
                    Quaternion.Slerp(motor.InitialTickRotation, motor.TransientRotation, interpolationFactor));
            }
        }
    }
}