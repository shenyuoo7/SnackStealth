using UnityEngine;
using UnityEngine.AI;

namespace SnackStealth.Navigation
{
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public sealed class NavMeshDataLoader : MonoBehaviour
    {
        [SerializeField] private NavMeshData navMeshData;

        private NavMeshDataInstance navMeshDataInstance;

        public void Configure(NavMeshData newNavMeshData)
        {
            navMeshData = newNavMeshData;
        }

        private void OnEnable()
        {
            if (navMeshData != null)
            {
                navMeshDataInstance = NavMesh.AddNavMeshData(navMeshData, transform.position, transform.rotation);
            }
        }

        private void OnDisable()
        {
            if (navMeshDataInstance.valid)
            {
                navMeshDataInstance.Remove();
            }
        }
    }
}
