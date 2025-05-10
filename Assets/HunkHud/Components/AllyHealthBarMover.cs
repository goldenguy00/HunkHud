using RoR2.UI;
using UnityEngine;

namespace HunkHud.Components
{
    public class AllyHealthBarMover : DisplayMover
    {
        private AllyCardController cardController;

        private void Awake()
        {
            this.cardController = this.GetComponent<AllyCardController>();
            this.targetHud = this.transform.root.GetComponent<HUD>();
            this.offset = new Vector3(-450f, 0f, 0f);
        }

        public override void CheckForActivity()
        {
            base.CheckForActivity();

            if (!this.cardController)
                return;

            this.targetBody = this.cardController.cachedSourceCharacterBody;
            this.targetMaster = this.cardController.cachedSourceMaster;

            if (this.cardController.ShouldWeUpdate() || (this.cardController.cachedHealthComponent && this.cardController.cachedHealthComponent.missingCombinedHealth > 0f))
                this.SetActive();
        }
    }
}