using RoR2;
using RoR2.UI;

namespace HunkHud.Components
{
    public class HealthBarMover : DisplayMover
    {
        private uint prevLevel;
        private InteractionDriver interactionDriver;

        public override void CheckForActivity()
        {
            var source = this.targetBody ? this.targetBody.healthComponent : null;
            if (!source)
                return;

            var currentHp = source.health + source.shield;
            var maxHp = source.fullHealth + source.fullShield;
            if (currentHp / maxHp < 0.99f)
                this.SetActive();

            var newLevel = HG.Convert.FloorToUIntClamped(this.targetBody.level);
            if (newLevel != this.prevLevel)
            {
                this.prevLevel = newLevel;
                this.SetActive();
            }

            if (this.interactionDriver)
            {
                var interactable = this.interactionDriver.currentInteractable ? this.interactionDriver.currentInteractable.GetComponent<PurchaseInteraction>() : null;
                if (interactable && interactable.costType is CostTypeIndex.PercentHealth or CostTypeIndex.SoulCost)
                {
                    SetActive();
                }
            }
        }

        protected override void HUD_onHudTargetChangedGlobal(HUD newHud)
        {
            base.HUD_onHudTargetChangedGlobal(newHud);

            this.interactionDriver = this.targetBody ? this.targetBody.GetComponent<InteractionDriver>() : null;
        }
    }
}