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

        protected override void Start()
        {
            base.Start();
            base.activePosition.x = 0f;
            base.inactivePosition.x = 0f;
        }

        public override void UpdateReferences(HUD hud, CharacterBody body)
        {
            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged -= this.SetActive;

            base.UpdateReferences(hud, body);

            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged += this.SetActive;
        }

        private void OnDestroy()
        {
            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged -= this.SetActive;
        }
    }
}
