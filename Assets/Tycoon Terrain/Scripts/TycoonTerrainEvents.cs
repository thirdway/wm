using UnityEngine.Events;
using System;

namespace TycoonTerrain {
    [Serializable]
    public class BuildingEvent : UnityEvent<SimpleBuildingBehaviour> {}
}