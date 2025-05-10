using RoR2;
using UnityEngine;
using HunkHud.Modules;

namespace HunkHud.Components.UI
{
    public class BandDisplayController : MonoBehaviour
    {
        public static BandDisplayController instance;
        private Inventory targetInventory;
        private void Awake()
        {
            instance = this;
        }
        private void OnDestroy()
        {
            if (this.targetInventory)
                this.targetInventory.onInventoryChanged -= this.Inventory_OnInventoryChanged;
        }

        public void UpdateReferences(Inventory inventory)
        {
            if (this.targetInventory)
                this.targetInventory.onInventoryChanged -= this.Inventory_OnInventoryChanged;

            this.targetInventory = inventory;

            if (this.targetInventory)
            {
                this.targetInventory.onInventoryChanged += this.Inventory_OnInventoryChanged;
                this.Inventory_OnInventoryChanged();
            }
        }

        private void Inventory_OnInventoryChanged()
        {
            AddOrRemovePrefab("BandDisplay", "FireRing", "IceRing");
            AddOrRemovePrefab("BandDisplayVoid", "ElementalRingVoid");
            AddOrRemovePrefab("BandDisplayHealing", "ITEM_HEALING_BAND", "ITEM_BARRIER_BAND");
            AddOrRemovePrefab("BandDisplayNova", "ITEM_NOVA_BAND");
            AddOrRemovePrefab("BandDisplaySacrificial", "ITEM_SANDSWEPT_SACRIFICIAL_BAND");

            void AddOrRemovePrefab(string prefabName, params string[] itemName)
            {
                bool hasItem = false;
                if (this.targetInventory)
                {
                    for (int i = 0; i < itemName.Length; i++)
                    {
                        var itemIndex = ItemCatalog.FindItemIndex(itemName[i]);
                        if (this.targetInventory.GetItemCount(itemIndex) > 0)
                        {
                            hasItem = true;
                            break;
                        }
                    }
                }

                var childTransform = this.transform.Find(prefabName);
                bool hasChild = childTransform != null;

                if (hasItem != hasChild)
                {
                    if (hasChild)
                    {
                        GameObject.Destroy(childTransform.gameObject);
                    }
                    else
                    {
                        var child = GameObject.Instantiate(HudAssets.mainAssetBundle.LoadAsset<GameObject>(prefabName), this.transform);
                        child.name = prefabName;
                    }
                }
            }
        }
    }
}
