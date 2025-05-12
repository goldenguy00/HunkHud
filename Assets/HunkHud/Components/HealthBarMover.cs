using UnityEngine;

namespace HunkHud.Components
{
    public class HealthBarMover : DisplayMover
    {
        private int prevLevel;

        public override void CheckForActivity()
        {
            if (!this.targetBody || !this.targetBody.healthComponent)
                return;

            if (this.targetBody.healthComponent.missingCombinedHealth > 0f)
                this.SetActive();

            var newLevel = Mathf.RoundToInt(this.targetBody.level);
            if (newLevel != this.prevLevel)
            {
                this.prevLevel = newLevel;
                this.SetActive();
            }
        }
    }
}