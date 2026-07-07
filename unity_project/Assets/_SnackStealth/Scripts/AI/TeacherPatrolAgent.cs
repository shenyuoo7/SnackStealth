using UnityEngine;
using UnityEngine.AI;

namespace SnackStealth.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class TeacherPatrolAgent : MonoBehaviour
    {
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField, Min(0f)] private float waitTimeAtPoint = 0.8f;
        [SerializeField, Min(0.01f)] private float arrivalDistance = 0.35f;

        private NavMeshAgent agent;
        private int patrolIndex;
        private float waitTimer;

        public void Configure(Transform[] newPatrolPoints)
        {
            patrolPoints = newPatrolPoints;
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            EnsureOnNavMesh();
            MoveToCurrentPoint();
        }

        private void Update()
        {
            if (patrolPoints == null || patrolPoints.Length == 0 || !agent.isOnNavMesh)
            {
                return;
            }

            if (agent.pathPending || agent.remainingDistance > arrivalDistance)
            {
                return;
            }

            waitTimer += Time.deltaTime;
            if (waitTimer < waitTimeAtPoint)
            {
                return;
            }

            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            waitTimer = 0f;
            MoveToCurrentPoint();
        }

        private void MoveToCurrentPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0 || !agent.isOnNavMesh)
            {
                return;
            }

            Transform destination = patrolPoints[patrolIndex];
            if (destination != null && NavMesh.SamplePosition(destination.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        private void EnsureOnNavMesh()
        {
            if (agent.isOnNavMesh)
            {
                return;
            }

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }
    }
}
