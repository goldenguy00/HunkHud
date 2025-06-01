using RoR2;
using RoR2.UI;

namespace HunkHud.Components
{
    public class MoneyDisplayMover : DisplayMover
    {
        public CostTypeIndex costType;
        private int cachedMoney;
        private MoneyText moneyText;
        private InteractionDriver interactionDriver;

        protected override void Awake()
        {
            base.Awake();

            this.moneyText = this.GetComponent<MoneyText>();
            this.activeInterval = 2.25f;
        }

        public override void CheckForActivity()
        {
            if (this.moneyText)
            {
                var newMoney = this.moneyText.displayAmount;
                if (newMoney != this.cachedMoney)
                {
                    this.cachedMoney = newMoney;
                    this.SetActive();
                }
            }

            if (this.interactionDriver)
            {
                var interactable = this.interactionDriver.currentInteractable ? this.interactionDriver.currentInteractable.GetComponent<PurchaseInteraction>() : null;
                if (interactable && interactable.costType == this.costType)
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
