using RoR2;
using UnityEngine;
using HunkHud.Modules;

namespace HunkHud.Components.UI
{
    public class BandDisplayController : CustomHudElement
    {
        public HealthBarMover healthBar;

        private CharacterMaster _prevMaster;

        private bool isInventoryCheckDirty;

        private void FixedUpdate()
        {
            if (this._prevMaster != this.targetMaster)
            {
                if (this._prevMaster?.inventory)
                    this._prevMaster.inventory.onInventoryChanged -= OnInventoryChanged;

                this._prevMaster = this.targetMaster;

                if (this.targetMaster?.inventory)
                    this.targetMaster.inventory.onInventoryChanged += OnInventoryChanged;

                OnInventoryChanged();
            }

            if (this.isInventoryCheckDirty)
            {
                CheckInventory();
            }
        }

        public void OnInventoryChanged()
        {
            isInventoryCheckDirty = true;
        }

        private void CheckInventory()
        {
            this.isInventoryCheckDirty = false;
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
                        var display = child.GetComponent<BandDisplay>();
                        display.healthBar = this.healthBar;
                        display.hud = this.hud;
                    }
                }
            }
        }
    }
}
