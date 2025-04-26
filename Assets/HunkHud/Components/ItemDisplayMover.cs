using RoR2;
using RoR2.UI;
using UnityEngine;

namespace HunkHud.Components
{
    public class ItemDisplayMover : DisplayMover
    {
        private void Awake()
        {
            this.offset = new Vector3(0f, 250f, 0f);
        }

        public override void UpdateReferences(HUD hud, CharacterBody body)
        {
            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged -= this.Inventory_OnInventoryChanged;

            base.UpdateReferences(hud, body);

            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged += this.Inventory_OnInventoryChanged;
        }

        private void OnDestroy()
        {
            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged -= this.Inventory_OnInventoryChanged;
        }

        private void Inventory_OnInventoryChanged()
        {
            this.activeTimer = this.refreshTimer;
        }
    }
}
