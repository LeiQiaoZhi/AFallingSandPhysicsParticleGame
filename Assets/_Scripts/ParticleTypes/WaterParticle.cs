using UnityEngine;
using UnityEngine.Assertions;

namespace _Scripts.ParticleTypes
{
    [CreateAssetMenu(fileName = "WaterParticle", menuName = "Particles/WaterParticle", order = 3)]
    public class WaterParticle : ParticleType
    {
        public GasParticle steamParticle;
        public override void Step(Particle _particle, Vector2Int _position,
            ParticleEfficientContainer _particleContainer, ParticleTypeSet _particleTypeSet, float _dt)
        {
            // update speed
            _dt *= speedMultiplier;

            Vector2Int[] pointsToTest =
            {
                _position + Vector2Int.down,
                _position + Vector2Int.down + Vector2Int.left,
                _position + Vector2Int.down + Vector2Int.right,
                _position + Vector2Int.left,
                _position + Vector2Int.right
            };
            if (Random.value < 0.5f)
                (pointsToTest[1], pointsToTest[2]) = (pointsToTest[2], pointsToTest[1]);
            if (Random.value < 0.5f)
                (pointsToTest[^2], pointsToTest[^1]) = (pointsToTest[^1], pointsToTest[^2]);

            foreach (Vector2Int pointToTest in pointsToTest)
            {
                Particle particleToTest = _particleContainer.GetParticleByLocalPosition(pointToTest);
                if (particleToTest != null && particleToTest.ParticleType is EmptyParticle)
                {
                    // Friction
                    var f = CalculateFriction(_particleContainer, _position);

                    UpdateVelocity(_particle, _dt, particleToTest.ParticleType.resistance, f, horizontalSpeed);

                    var verticalOffset = (int)(_particle.Velocity.y * _dt);
                    var horizontalOffset = (int)(_particle.Velocity.x * _dt);
                    Assert.IsTrue(verticalOffset >= 0, "Vertical offset must be positive");
                    Assert.IsTrue(horizontalOffset >= 0, "Horizontal offset must be positive");

                    var offset = new Vector2Int(
                        (pointToTest.x - _position.x) * horizontalOffset,
                        (pointToTest.y - _position.y) * verticalOffset
                    );
                    Vector2Int target = _position + offset;
                    Vector2Int destination = TryMoveToTarget(pointToTest, target, _particleContainer,
                        _p => _p.ParticleType is not (EmptyParticle or FireParticle)
                    );
                    
                    Particle destinationParticle = _particleContainer.GetParticleByLocalPosition(destination);
                    if (destinationParticle != null && destinationParticle.ParticleType is FireParticle)
                    {
                        _particle.SetType(_particleTypeSet.GetInstanceByType(typeof(EmptyParticle)));
                        destinationParticle.SetType(steamParticle);
                        return;
                    }

                    if (offset.sqrMagnitude > 0.9)
                        _particleContainer.Swap(_position, destination);

                    return;
                }
            }
        }
    }
}