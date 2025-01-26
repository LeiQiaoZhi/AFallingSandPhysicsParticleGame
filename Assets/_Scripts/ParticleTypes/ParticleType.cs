using System;
using MyBox;
using MyHelpers;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _Scripts.ParticleTypes
{
    public enum ColorType
    {
        SingleColor,
        RandomBetweenTwoColors,
    }

    public abstract class ParticleType : ScriptableObject
    {
        public string particleName;
        [Header("Color")] public ColorType colorType;
        [SerializeField] private Color color;

        [ConditionalField(nameof(colorType), false, ColorType.RandomBetweenTwoColors)] [SerializeField]
        private Color color2;

        [Header("Physics Settings")] public float speedMultiplier = 1;

        [FormerlySerializedAs("airResistance")]
        public float resistance = 1;

        public float gravity = 9.81f;
        public int horizontalSpeed = 2;

        public Color Color
        {
            get
            {
                if (colorType == ColorType.SingleColor)
                    return color;
                else
                    return Random.value < 0.5f ? color : color2;
            }
        }


        public abstract void Step(Particle _particle, Vector2Int _position,
            ParticleEfficientContainer _particleContainer, ParticleTypeSet _particleTypeSet, float _dt);

        protected static Vector2Int TryMoveToTarget(Vector2Int _position, Vector2Int _targetPosition,
            ParticleEfficientContainer _particleContainer)
        {
            return TryMoveToTarget(_position, _targetPosition, _particleContainer,
                _particle => _particle.ParticleType is not EmptyParticle);
        }

        protected static Vector2Int TryMoveToTarget(Vector2Int _position, Vector2Int _targetPosition,
            ParticleEfficientContainer _particleContainer, Predicate<Particle> _stopCondition)

        {
            var points = Helpers.LazyLinePoints(_position, _targetPosition);

            Vector2Int lastValidPoint = _position; // Default to starting position if no points are valid
            var index = 0;

            using (var pointEnumerator = points.GetEnumerator())
            {
                while (pointEnumerator.MoveNext())
                {
                    Vector2Int point = pointEnumerator.Current;

                    // Skip the first point, continue with the rest
                    if (index > 0)
                    {
                        Particle particle = _particleContainer.GetParticleByLocalPosition(point);
                        if (particle == null || _stopCondition(particle))
                        {
                            break; // Stop evaluating and break the loop as soon as a condition fails
                        }

                        lastValidPoint = point; // Update last valid point
                    }

                    index++; // Increment index
                }
            }

            return lastValidPoint; // Return the last valid point processed
        }

        protected void UpdateVelocity(Particle _particle, float _dt, float _resistance)
        {
            var currentVelocity = _particle.Velocity.y;
            var acc = -_resistance * currentVelocity * currentVelocity + gravity;
            var newVelocity = Mathf.Clamp(_particle.Velocity.y + acc * _dt, 0, 10);
            _particle.Velocity = new Vector2(_particle.Velocity.x, newVelocity);
        }
    }
}