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

        private float currentVelocity;
        private float desiredX;
        private bool isMoving;
        private bool isAttacking;
        private bool isDead;

        public float CurrentVelocity => currentVelocity;
        public float DesiredX => desiredX;
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

            desiredX = Mathf.Clamp(targetX, Mathf.Min(minX, maxX), Mathf.Max(minX, maxX));
            float deltaX = desiredX - transform.position.x;
            if (Mathf.Abs(deltaX) <= stoppingDistance)
            {
                Stop();
                transform.position = new Vector3(desiredX, laneY, transform.position.z);
                return false;
            }

            float targetVelocity = Mathf.Sign(deltaX) * moveSpeed;
            float rate = Mathf.Abs(targetVelocity) > Mathf.Abs(currentVelocity) ? acceleration : deceleration;
            currentVelocity = Mathf.MoveTowards(currentVelocity, targetVelocity, rate * Time.deltaTime);

            float nextX = transform.position.x + currentVelocity * Time.deltaTime;
            if (Mathf.Sign(desiredX - nextX) != Mathf.Sign(deltaX))
            {
                nextX = desiredX;
                currentVelocity = 0f;
            }

            transform.position = new Vector3(Mathf.Clamp(nextX, minX, maxX), laneY, transform.position.z);
            isMoving = Mathf.Abs(currentVelocity) > 0.01f;
            return isMoving;
        }

        public void Stop()
        {
            currentVelocity = Mathf.MoveTowards(currentVelocity, 0f, deceleration * Time.deltaTime);
            isMoving = Mathf.Abs(currentVelocity) > 0.01f;
        }

        public void SnapLaneY(float laneY)
        {
            transform.position = new Vector3(transform.position.x, laneY, transform.position.z);
        }
    }
}
