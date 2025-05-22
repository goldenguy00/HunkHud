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

        protected override void Update()
        {
            base.Update();

            if (this.cardController && this.cardController.layoutElement)
                this.cardController.layoutElement.ignoreLayout = this.activeTimer < -0.5f;
        }

        public override void CheckForActivity()
        {
            var source = this.cardController ? this.cardController.cachedHealthComponent : null;
            if (!source)
                return;

            var currentHp = source.health + source.shield;
            var maxHp = source.fullHealth + source.fullShield;
            if (currentHp / maxHp < 0.99f)
                this.SetActive();
        }
    }
}