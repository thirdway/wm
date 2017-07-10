using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TycoonTerrain {
    public class ResourceManager : MonoBehaviour {
        public static ResourceManager Instance;
        public List<ResourceInstance> startupResources;
        public List<Resource> resources;
        public List<int> amount;

        public ResourceInstance changeTerrainTileHeightCost;
        public ResourceInstance clearTerrainTileCost;
        [Tooltip("Link to The text display to draw current cash")]
        public UnityEngine.UI.Text txtCash;
        private bool hasBeenSetup = false;

        void Awake() {
            Instance = this;
        }


        // Use this for initialization
        void Start() {
            if (!hasBeenSetup) Reset();

        }

        // Update is called once per frame
        void Update() {

        }

        public void Reset() {
            SyncListSize();
            for (int i = 0; i < amount.Count; i++) {
                amount[i] = 0;
            }
            foreach (var item in startupResources) {
                amount[resources.IndexOf(item.resource)] = item.amount;
            }
            
            UpdateResourceDisplay();
        }

        public void Save(string key) {
            SaveDataManagement.SaveData<List<int>>(amount, key);
        }

        public void Load(string key) {
            if (SaveDataManagement.HasSavedData(key))
                amount = SaveDataManagement.LoadData<List<int>>(key);
            else
                Reset();
            SyncListSize();
            UpdateResourceDisplay();
        }

        private void SyncListSize() {
            while (amount.Count < resources.Count)
                amount.Add(0);
            while (amount.Count > resources.Count)
                amount.RemoveAt(amount.Count - 1);
            hasBeenSetup = true;
        }
        
        public bool CanAfford(IEnumerable<ResourceInstance> cost) {
            
            foreach (var item in cost) {
                if (!CanAfford(item)) {
                    return false;
                }
            }
            return true;
        }

        public bool CanAfford(ResourceInstance cost) {
            return amount[resources.IndexOf(cost.resource)] >= cost.amount;
        }

        public bool CanAfford(WorldBehaviour.TerraformCostCalculation cost) {
            return CanAfford(CalculateCost(cost));
        }

        public void Pay(IEnumerable<ResourceInstance> cost) {
            foreach (var item in cost) {
                Pay(item);
            }
        }

        public void Add(ResourceInstance item) {
            amount[resources.IndexOf(item.resource)] += item.amount;

            UpdateResourceDisplay();
        }

        public void Pay(ResourceInstance cost) {
            amount[resources.IndexOf(cost.resource)] -= cost.amount;
            
            UpdateResourceDisplay();
        }

        public void Pay(WorldBehaviour.TerraformCostCalculation cost) {
            Pay(CalculateCost(cost));
        }
        
        
        private ResourceInstance CalculateCost(WorldBehaviour.TerraformCostCalculation cost) {
            if (cost.heightChangedTiles.Count > 0) {
                ResourceInstance resources = changeTerrainTileHeightCost;
                resources.amount *= cost.heightChangedTiles.Count;
                return resources;
            } else {
                ResourceInstance resources = clearTerrainTileCost;
                resources.amount *= cost.bulldozedTiles.Count;
                return resources;
            }
        }

        public string GetCostString(WorldBehaviour.TerraformCostCalculation cost) {
            
            return CalculateCost(cost).ToString();
        }

        public string GetCostString(IEnumerable<ResourceInstance> cost) {
            string costStr = "";
            foreach (var item in cost) {
                costStr += item.ToString() + "\n";
            }
            return costStr;
        }
        
        #region UI
        protected virtual void UpdateResourceDisplay() {
            string resourceDisplay = "";
            for (int i = 0; i < resources.Count; i++) {
                resourceDisplay += resources[i].currencyPrefix + amount[i].ToString()+ resources[i].currencySuffix + "\n";
            }
            txtCash.text = resourceDisplay;
        }
        #endregion UI
    }
}