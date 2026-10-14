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

            if (this.targetCharacterBody && this.targetCharacterBody.keyItemSlot && this.targetCharacterBody.keyItemSlot.keyItem != KeyItemIndex.None)
            { 
                if (this.targetCharacterBody.keyItemSlot.BehaviorIsReadyToExecute())
                    SetActive();
            }
        }
        private void CheckEquipment()
        {
            var inventory = this.targetCharacterBody ? this.targetCharacterBody.inventory : null;
            if (!inventory)
                return;
            
            for (int i = 0; i < inventory._equipmentSlots?.Length; i++)
            {
                var slot = inventory._equipmentSlots[i];
                for (int j = 0; j < slot._sets?.Length; j++)
                {
                    if (slot._sets[j].isPerfomingRecharge)
                    {
                        this.SetActive();
                        return;
                    }
                }
            }
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
            {
                this.targetCharacterBody.onSkillActivatedAuthority += this.OnSkillActivatedAuthority;
            }
        }

        private void OnSkillActivatedAuthority(GenericSkill skill)
        {
            if (skill.stock < skill.maxStock)
                SetActive();
        }
    }
}
