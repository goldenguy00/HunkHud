namespace HunkHud.Components
{
    public class SkillIconMover : DisplayMover
    {
        protected override void Awake()
        {
            base.Awake();
            this.offset = new UnityEngine.Vector3(0f, -300f, 0f);
        }

        public override void CheckForActivity()
        {
            if (!this.targetBody)
                return;

            if (this.targetBody.equipmentSlot && this.targetBody.equipmentSlot.cooldownTimer > 0f)
            {
                this.SetActive();
            }
            else if (this.targetBody.skillLocator)
            {
                for (int i = 0; i < this.targetBody.skillLocator.allSkills.Length; i++)
                {
                    var skill = this.targetBody.skillLocator.allSkills[i];
                    if (skill && skill.cooldownRemaining != 0f)
                    {
                        this.SetActive();
                        return;
                    }
                }
            }
        }
    }
}
