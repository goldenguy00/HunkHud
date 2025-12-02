using RoR2;
using RoR2.UI;
using UnityEngine;

namespace HunkHud.Components.UI
{
    public class BandDisplayController : CustomHudElement
    {
        public struct BandDisplayInfo
        {
            public ItemIndex[] itemsToDisplay;
            public GameObject displayPrefab;
            public string prefabName;
        }

        public HealthBarMover healthBar;
        public static BandDisplayInfo[] bandInfos = new BandDisplayController.BandDisplayInfo[0];

        private Inventory inventory;

        protected override void HUD_onHudTargetChangedGlobal(HUD newHud)
        {
            if (!ReferenceEquals(this._prevBody, null))
                this._prevBody.onInventoryChanged -= this.CheckInventory;

            base.HUD_onHudTargetChangedGlobal(newHud);

            if (this.targetBody)
                this.targetBody.onInventoryChanged += this.CheckInventory;
        }

        private void CheckInventory()
        {
            this.inventory = this.targetMaster ? this.targetMaster.inventory : null;
            Util.NullifyIfInvalid(ref inventory);

            for (int i = 0; i < bandInfos.Length; ++i)
            {
                ref var info = ref bandInfos[i];
                AddOrRemovePrefab(ref info);
            }
        }

        private void AddOrRemovePrefab(ref BandDisplayInfo bandInfo)
        {
            bool hasItem = false;
            if (!ReferenceEquals(inventory, null))
            {
                for (int i = 0; i < bandInfo.itemsToDisplay.Length; i++)
                {
                    if (inventory.GetItemCountEffective(bandInfo.itemsToDisplay[i]) > 0)
                    {
                        hasItem = true;
                        break;
                    }
                }
            }

            var existingDisplay = this.transform.Find(bandInfo.prefabName);
            bool hasDisplay = existingDisplay != null;

            if (hasItem != hasDisplay)
            {
                if (hasDisplay)
                {
                    GameObject.Destroy(existingDisplay.gameObject);
                }
                else
                {
                    var child = GameObject.Instantiate(bandInfo.displayPrefab, this.transform);
                    child.name = bandInfo.prefabName;

                    var display = child.GetComponent<BandDisplay>();
                    display.healthBar = this.healthBar;
                    display.hud = this.hud;
                }
            }
        }
    }
}
