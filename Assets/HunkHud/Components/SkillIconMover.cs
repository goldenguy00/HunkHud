using RoR2;
using RoR2.UI;

namespace HunkHud.Components
{
    public class SkillIconMover : DisplayMover
    {
        private int prevStocks;

        protected override void Awake()
        {
            base.Awake();
            this.offset = new UnityEngine.Vector3(0f, 0f, 0f);
            this.activeInterval = 2.25f;
        }

        public override void CheckForActivity()
        {
            var newStocks = this.targetBody?.equipmentSlot?.stock ?? 0;
            if (newStocks != prevStocks)
            {
                this.prevStocks = newStocks;
                this.SetActive();
            }
        }

        protected override void HUD_onHudTargetChangedGlobal(HUD newHud)
        {
            if (this._prevBody)
                this._prevBody.onSkillActivatedAuthority -= this.OnSkillActivatedAuthority;

            base.HUD_onHudTargetChangedGlobal(newHud);

            if (this.targetBody)
                this.targetBody.onSkillActivatedAuthority += this.OnSkillActivatedAuthority;
        }

        private void OnSkillActivatedAuthority(GenericSkill skill)
        {
            if (skill.stock < skill.maxStock)
                SetActive();
        }
    }
}
