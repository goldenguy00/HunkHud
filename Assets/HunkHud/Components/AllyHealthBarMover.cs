using RoR2.UI;
using UnityEngine;

namespace HunkHud.Components
{
    public class AllyHealthBarMover : DisplayMover
    {
        private AllyCardController cardController;

        protected override void Awake()
        {
            base.Awake();

            this.offset = new Vector3(-500f, 0f, 0f);
            this.cardController = this.GetComponent<AllyCardController>();
        }

        public override void CheckForActivity()
        {
            if (!this.cardController)
                return;

            var body = this.cardController.cachedSourceCharacterBody;
            if (!body || !body.healthComponent)
                return;

            if (body.healthComponent.missingCombinedHealth > 0f)
                this.SetActive();
        }
    }
}