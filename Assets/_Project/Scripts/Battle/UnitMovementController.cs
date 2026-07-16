using UnityEngine;

namespace RuneGate
{
    public sealed class UnitMovementController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 1.8f;
        [SerializeField] private float acceleration = 9f;
        [SerializeField] private float deceleration = 14f;
        [SerializeField] private float stoppingDistance = 0.05f;
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private float personalSpace = 0.42f;
        [SerializeField] private float leashRange = 1.5f;

        private Vector2 currentVelocity;
        private Vector2 desiredPosition;
        private bool isMoving;
        private bool isAttacking;
        private bool isDead;

        public float CurrentVelocity => currentVelocity.magnitude;
        public Vector2 CurrentVelocity2D => currentVelocity;
        public Vector2 DesiredPosition => desiredPosition;
        public float DesiredX => desiredPosition.x;
        public float MoveSpeed => moveSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float StoppingDistance => stoppingDistance;
        public float AttackRange => attackRange;
        public float PersonalSpace => personalSpace;
        public float LeashRange => leashRange;
        public bool IsMoving => isMoving;
        public bool IsAttacking => isAttacking;
        public bool IsDead => isDead;

        public void Configure(float nextMoveSpeed, float nextAttackRange, float nextPersonalSpace, float nextLeashRange)
        {
            moveSpeed = Mathf.Max(0.01f, nextMoveSpeed);
            attackRange = Mathf.Max(0.01f, nextAttackRange);
            personalSpace = Mathf.Max(0f, nextPersonalSpace);
            leashRange = Mathf.Max(0f, nextLeashRange);
        }

        public void SetMotionTuning(float nextAcceleration, float nextDeceleration, float nextStoppingDistance)
        {
            acceleration = Mathf.Max(0.01f, nextAcceleration);
            deceleration = Mathf.Max(0.01f, nextDeceleration);
            stoppingDistance = Mathf.Max(0f, nextStoppingDistance);
        }

        public void SetAttackState(bool value)
        {
            isAttacking = value;
            if (value)
            {
                Stop();
            }
        }

        public void SetDeadState(bool value)
        {
            isDead = value;
            if (value)
            {
                Stop();
            }
        }

        public bool MoveToX(float targetX, float laneY, float minX, float maxX)
        {
            if (isDead || isAttacking)
            {
                Stop();
                return false;
            }

            desiredPosition = new Vector2(
                Mathf.Clamp(targetX, Mathf.Min(minX, maxX), Mathf.Max(minX, maxX)),
                laneY);
            float deltaX = desiredPosition.x - transform.position.x;
            if (Mathf.Abs(deltaX) <= stoppingDistance)
            {
                Stop();
                transform.position = new Vector3(desiredPosition.x, laneY, transform.position.z);
                return false;
            }

            float targetVelocity = Mathf.Sign(deltaX) * moveSpeed;
            float rate = Mathf.Abs(targetVelocity) > Mathf.Abs(currentVelocity.x) ? acceleration : deceleration;
            currentVelocity.x = Mathf.MoveTowards(currentVelocity.x, targetVelocity, rate * Time.deltaTime);
            currentVelocity.y = Mathf.MoveTowards(currentVelocity.y, 0f, deceleration * Time.deltaTime);

            float nextX = transform.position.x + currentVelocity.x * Time.deltaTime;
            if (Mathf.Sign(desiredPosition.x - nextX) != Mathf.Sign(deltaX))
            {
                nextX = desiredPosition.x;
                currentVelocity.x = 0f;
            }

            transform.position = new Vector3(Mathf.Clamp(nextX, minX, maxX), laneY, transform.position.z);
            isMoving = currentVelocity.sqrMagnitude > 0.0001f;
            return isMoving;
        }

        public bool MoveTo(
            Vector2 target,
            BattlefieldBounds bounds,
            Vector2 halfExtents,
            Vector2 steeringOffset)
        {
            if (isDead || isAttacking || !bounds.IsValid)
            {
                Stop();
                return false;
            }

            desiredPosition = bounds.Clamp(target + steeringOffset, halfExtents);
            Vector2 delta = desiredPosition - (Vector2)transform.position;
            if (delta.sqrMagnitude <= stoppingDistance * stoppingDistance)
            {
                Stop();
                Vector2 snapped = bounds.Clamp(desiredPosition, halfExtents);
                transform.position = new Vector3(snapped.x, snapped.y, transform.position.z);
                return false;
            }

            Vector2 targetVelocity = delta.normalized * moveSpeed;
            float rate = targetVelocity.sqrMagnitude > currentVelocity.sqrMagnitude ? acceleration : deceleration;
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, rate * Time.deltaTime);

            Vector2 nextPosition = (Vector2)transform.position + currentVelocity * Time.deltaTime;
            if (Vector2.Dot(desiredPosition - nextPosition, delta) <= 0f)
            {
                nextPosition = desiredPosition;
                currentVelocity = Vector2.zero;
            }

            nextPosition = bounds.Clamp(nextPosition, halfExtents);
            transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
            isMoving = currentVelocity.sqrMagnitude > 0.0001f;
            return isMoving;
        }

        public void Stop()
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration * Time.deltaTime);
            isMoving = currentVelocity.sqrMagnitude > 0.0001f;
        }

        public void SnapLaneY(float laneY)
        {
            transform.position = new Vector3(transform.position.x, laneY, transform.position.z);
        }
    }
}
