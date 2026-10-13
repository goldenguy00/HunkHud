using RoR2;
using RoR2.UI;

namespace HunkHud.Components
{
    public class SkillIconMover : DisplayMover
    {
        protected override void Awake()
        {
            base.Awake();
            this.offset = new UnityEngine.Vector3(0f, 0f, 0f);
            this.activeInterval = 2.25f;
        }

        public override void CheckForActivity()
        {
            CheckEquipment();
            CheckSkills();
        }
        private void CheckEquipment()
        {
            var inventory = this.targetCharacterBody ? this.targetCharacterBody.inventory : null;
            if (!inventory)
                return;
            /*
            for (int i = 0; i < inventory._equipmentSlots?.Length; i++)
            {
                for (int j = 0; j < inventory._equipmentSlots[i]?.Length; j++)
                {
                    if (inventory._equipmentStateSlots[i][j].isPerfomingRecharge)
                    {
                        this.SetActive();
                        return;
                    }
                }
            }*/
        }

        private void CheckSkills()
        {
            var skillLoc = this.targetCharacterBody ? this.targetCharacterBody.skillLocator : null;
            if (!skillLoc)
                return;

            for (int i = 0; i < skillLoc.allSkills?.Length; i++)
            {
                if (skillLoc.allSkills[i].stock < skillLoc.allSkills[i].maxStock)
                {
                    this.SetActive();
                    return;
                }
            }
        }

        protected override void HUD_onHudTargetChangedGlobal(HUD newHud)
        {
            if (!ReferenceEquals(this.targetCharacterBody, null))
                this.targetCharacterBody.onSkillActivatedAuthority -= this.OnSkillActivatedAuthority;

            base.HUD_onHudTargetChangedGlobal(newHud);

            if (this.targetCharacterBody)
                this.targetCharacterBody.onSkillActivatedAuthority += this.OnSkillActivatedAuthority;
        }

        private void OnSkillActivatedAuthority(GenericSkill skill)
        {
            if (skill.stock < skill.maxStock)
                SetActive();
        }
    }
}
