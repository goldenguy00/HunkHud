using RoR2;
using UnityEngine;
using HunkHud.Modules;
using RoR2.UI;
using System;

namespace HunkHud.Components.UI
{
    public class BandDisplayController : MonoBehaviour
    {
        public HealthBarMover healthBar;

        [NonSerialized]
        public HUD targetHud;

        [NonSerialized]
        public CharacterBody targetBody;

        [NonSerialized]
        public CharacterMaster targetMaster;

        private void OnEnable()
        {
            InstanceTracker.Add(this);
        }

        private void OnDisable()
        {
            InstanceTracker.Remove(this);

            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged -= this.Inventory_OnInventoryChanged;
        }

        public void UpdateReferences(HUD hud, CharacterBody body)
        {
            var inventory = hud?.targetMaster ? hud.targetMaster.inventory : null;

            if (this.targetMaster != inventory)
            {
                if (this.targetMaster && this.targetMaster.inventory)
                    this.targetMaster.inventory.onInventoryChanged -= this.Inventory_OnInventoryChanged;

                if (inventory)
                    inventory.onInventoryChanged += this.Inventory_OnInventoryChanged;
            }

            this.targetHud = hud;
            this.targetMaster = hud?.targetMaster;
            this.targetBody = body;

            this.Inventory_OnInventoryChanged();
        }

        private void Inventory_OnInventoryChanged()
        {
            var inventory = this.targetMaster ? this.targetMaster.inventory : null;
            AddOrRemovePrefab("BandDisplay", "FireRing", "IceRing");
            AddOrRemovePrefab("BandDisplayVoid", "ElementalRingVoid");
            AddOrRemovePrefab("BandDisplayHealing", "ITEM_HEALING_BAND", "ITEM_BARRIER_BAND");
            AddOrRemovePrefab("BandDisplayNova", "ITEM_NOVA_BAND");
            AddOrRemovePrefab("BandDisplaySacrificial", "ITEM_SANDSWEPT_SACRIFICIAL_BAND");

            void AddOrRemovePrefab(string prefabName, params string[] itemName)
            {
                bool hasItem = false;
                if (inventory)
                {
                    for (int i = 0; i < itemName.Length; i++)
                    {
                        var itemIndex = ItemCatalog.FindItemIndex(itemName[i]);
                        if (inventory.GetItemCount(itemIndex) > 0)
                        {
                            hasItem = true;
                            break;
                        }
                    }
                }

                var childTransform = this.transform.Find(prefabName);
                bool hasDisplay = childTransform != null;

                if (hasItem != hasDisplay)
                {
                    if (hasDisplay)
                    {
                        GameObject.Destroy(childTransform.gameObject);
                    }
                    else
                    {
                        var child = GameObject.Instantiate(HudAssets.mainAssetBundle.LoadAsset<GameObject>(prefabName), this.transform);
                        child.name = prefabName;
                        child.GetComponent<BandDisplay>().UpdateReferences(this.targetBody, this.healthBar);
                    }
                }
            }
        }
    }
}
