using RoR2;
using UnityEngine;
using HunkHud.Modules;
using RoR2.UI;

namespace HunkHud.Components.UI
{
    public class BandDisplayController : CustomHudElement
    {
        public HealthBarMover healthBar;

        protected override void HUD_onHudTargetChangedGlobal(HUD newHud)
        {
            if (this._prevBody)
                this._prevBody.onInventoryChanged -= this.CheckInventory;

            base.HUD_onHudTargetChangedGlobal(newHud);

            if (this.targetBody)
                this.targetBody.onInventoryChanged += this.CheckInventory;
        }

        private void CheckInventory()
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
                        var display = child.GetComponent<BandDisplay>();
                        display.healthBar = this.healthBar;
                        display.hud = this.hud;
                    }
                }
            }
        }
    }
}
