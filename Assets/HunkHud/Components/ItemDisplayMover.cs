using RoR2;
using RoR2.UI;
using UnityEngine;

namespace HunkHud.Components
{
    public class ItemDisplayMover : DisplayMover
    {
        private InteractionDriver interactionDriver;

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

        public override void CheckForActivity()
        {
            if (this.interactionDriver)
            {
                var interactable = this.interactionDriver.currentInteractable ? this.interactionDriver.currentInteractable.GetComponent<PurchaseInteraction>() : null;
                if (interactable && CostTypeIndex.WhiteItem <= interactable.costType && interactable.costType <= CostTypeIndex.TreasureCacheVoidItem)
                {
                    SetActive();
                }
            }
        }

        protected override void HUD_onHudTargetChangedGlobal(HUD newHud)
        {
            if (this._prevBody)
                this._prevBody.onInventoryChanged -= this.SetActive;

            base.HUD_onHudTargetChangedGlobal(newHud);

            if (this.targetBody)
                this.targetBody.onInventoryChanged += this.SetActive;

            this.interactionDriver = this.targetBody ? this.targetBody.GetComponent<InteractionDriver>() : null;
        }
    }
}
