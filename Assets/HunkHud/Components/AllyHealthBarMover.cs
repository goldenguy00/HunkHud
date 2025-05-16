using RoR2.UI;
using UnityEngine;

namespace HunkHud.Components
{
    public class AllyHealthBarMover : DisplayMover
    {
        private AllyCardController cardController;
        private CanvasGroup canvas;

        protected override void Awake()
        {
            base.Awake();

            this.offset = new Vector3(-500f, 0f, 0f);
            this.cardController = this.GetComponent<AllyCardController>();
            this.canvas = this.GetComponent<CanvasGroup>();
        }

        protected override void Update()
        {
            base.Update();

            if (this.canvas)
                this.canvas.alpha = Mathf.Clamp01(this.activeTimer + 1f);
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