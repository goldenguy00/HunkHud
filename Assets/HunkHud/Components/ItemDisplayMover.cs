using RoR2;
using RoR2.UI;
using UnityEngine;

namespace HunkHud.Components
{
    public class ItemDisplayMover : DisplayMover
    {
        protected override void Awake()
        {
            base.Awake();
            this.offset = new Vector3(0f, 250f, 0f);
        }

        protected override void Start()
        {
            var pos = this.transform.localPosition;
            pos.x = 0f;
            this.transform.localPosition = pos;

            base.Start();
        }

        public override void CheckForActivity() { }

        public override void UpdateReferences(HUD hud, CharacterBody body)
        {
            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged -= this.SetActive;

            base.UpdateReferences(hud, body);

            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged += this.SetActive;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged -= this.SetActive;
        }
    }
}
