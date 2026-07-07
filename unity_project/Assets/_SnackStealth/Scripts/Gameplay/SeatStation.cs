using System.Collections.Generic;
using SnackStealth.Items;
using UnityEngine;

namespace SnackStealth.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SeatStation : MonoBehaviour
    {
        private static readonly List<SeatStation> Stations = new List<SeatStation>();

        [SerializeField] private Transform sitPoint;
        [SerializeField] private Transform snackPoint;
        [SerializeField] private SnackStash snackStash;
        [SerializeField] private ClassmateActor occupant;
        [SerializeField] private bool playerSeated;

        public Transform SitPoint => sitPoint;
        public Transform SnackPoint => snackPoint;
        public SnackStash SnackStash => snackStash;
        public ClassmateActor Occupant => occupant;
        public bool HasOccupant => occupant != null && occupant.IsSeated;
        public bool IsPlayerSeated => playerSeated;
        public bool IsAvailableForClassmate => !playerSeated && !HasOccupant;
        public bool HasSnack => snackStash != null && snackStash.HasSnack;
        public string SnackName => snackStash != null ? snackStash.SnackName : "Nothing";
        public float SnackRemaining => snackStash != null ? snackStash.FullnessRemaining : 0f;

        public void Configure(Transform newSitPoint, Transform newSnackPoint, SnackStash newSnackStash, ClassmateActor newOccupant)
        {
            sitPoint = newSitPoint;
            snackPoint = newSnackPoint;
            snackStash = newSnackStash;
            occupant = newOccupant;
            playerSeated = false;
        }

        private void OnEnable()
        {
            if (!Stations.Contains(this))
            {
                Stations.Add(this);
            }
        }

        private void OnDisable()
        {
            Stations.Remove(this);
        }

        public float ConsumeSnack(float fullnessAmount)
        {
            return snackStash != null ? snackStash.Consume(fullnessAmount) : 0f;
        }

        public void SetPlayerSeated(bool seated)
        {
            playerSeated = seated;
        }

        public void AssignOccupant(ClassmateActor newOccupant)
        {
            occupant = newOccupant;
        }

        public void EjectOccupant(Transform podiumSpot, GameplayEffectSpawner effects)
        {
            if (!HasOccupant)
            {
                return;
            }

            ClassmateActor ejected = occupant;
            occupant = null;

            SeatStation fallbackSeat = podiumSpot == null ? FindFirstAvailableForClassmate(this) : null;
            if (fallbackSeat != null)
            {
                fallbackSeat.AssignOccupant(ejected);
            }

            ejected.EjectFromSeat(podiumSpot, fallbackSeat, effects);
        }

        public static SeatStation FindNearest(Vector3 position, float radius)
        {
            SeatStation nearest = null;
            float nearestSqrDistance = radius * radius;

            foreach (SeatStation station in Stations)
            {
                if (station == null || station.sitPoint == null)
                {
                    continue;
                }

                float sqrDistance = (station.sitPoint.position - position).sqrMagnitude;
                if (sqrDistance <= nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = station;
                }
            }

            return nearest;
        }

        public static SeatStation FindFirstAvailableForClassmate(SeatStation excludedSeat)
        {
            foreach (SeatStation station in Stations)
            {
                if (station != null && station != excludedSeat && station.IsAvailableForClassmate)
                {
                    return station;
                }
            }

            return null;
        }
    }
}
