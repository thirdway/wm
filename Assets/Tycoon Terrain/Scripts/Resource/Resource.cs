using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonTerrain {
    /// <summary>
    /// A resource that can be used to construct buildings in game
    /// </summary>
    [CreateAssetMenu(fileName = "New resource type.asset", menuName = "Tycoon Terrain/Create new resource type")]
    public class Resource : ScriptableObject {
        public string currencyPrefix = "$";
        public string currencySuffix = "";

        public override string ToString() {
            return name;
        }
    }
    [System.Serializable]
    public struct ResourceInstance {
        public Resource resource;
        public int amount;

        public override string ToString() {
            if (resource == null) return "None";
            return resource.currencyPrefix + amount.ToString() + resource.currencySuffix;
        }
    }
}