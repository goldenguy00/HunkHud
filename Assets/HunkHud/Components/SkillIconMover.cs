namespace HunkHud.Components
{
    public class SkillIconMover : DisplayMover
    {
        private void Awake()
        {
            this.offset = new UnityEngine.Vector3(0f, -300f, 0f);
        }

        public override void CheckForActivity()
        {
            base.CheckForActivity();

            if (!this.targetBody)
                return;

            if (this.targetBody.equipmentSlot)
            {
                if (this.targetBody.equipmentSlot.cooldownTimer > 0f)
                {
                    this.activeTimer = this.refreshTimer;
                    return;
                }
            }

            if (this.targetBody.skillLocator)
            {
                for (int i = 0; i < this.targetBody.skillLocator.allSkills.Length; i++)
                {
                    var skill = this.targetBody.skillLocator.allSkills[i];
                    if (skill && skill.cooldownRemaining != 0f)
                    {
                        this.activeTimer = this.refreshTimer;
                        return;
                    }
                }
            }
        }
    }
}
