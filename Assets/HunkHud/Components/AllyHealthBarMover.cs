using System.Linq;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace HunkHud.Components
{
    public class AllyHealthBarMover : DisplayMover
    {
        private HealthComponent targetHealthComponent;
        private AllyCardController cardController;
        private AllyCardManager cardManager;

        protected override void Awake()
        {
            base.Awake();
            this.cardManager = this.GetComponentInParent<AllyCardManager>();
            this.cardController = this.GetComponent<AllyCardController>();
            this.targetHud = this.transform.root.GetComponent<HUD>();
            this.offset = new Vector3(-450f, 0f, 0f);
        }

        public override void CheckForActivity()
        {
            if (!this.cardController)
                return;

            this.targetBody = this.cardController.cachedSourceCharacterBody;
            this.targetMaster = this.cardController.cachedSourceMaster;
            this.targetHealthComponent = this.cardController.cachedHealthComponent;

            if (this.cardController.ShouldWeUpdate() || (this.targetHealthComponent && this.targetHealthComponent.missingCombinedHealth > 0f))
                this.SetActive();

            if (0f < this.activeTimer)
                return;

            var alloc = this.cardManager ? this.cardManager.cardAllocator?.elementControllerComponentsList : null;
            if (alloc?.Any() == true)
            {
                alloc.Remove(this.cardController);
            }
        }

        public override void SetActive()
        {
            base.SetActive();

            var alloc = this.cardManager ? this.cardManager.cardAllocator?.elementControllerComponentsList : null;
            if (alloc?.Any() == true && !alloc.Contains(this.cardController))
            {
                alloc.Remove(this.cardController);
            }
        }
    }
}