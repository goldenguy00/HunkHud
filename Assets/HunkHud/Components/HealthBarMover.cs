using UnityEngine;

namespace HunkHud.Components
{
    public class HealthBarMover : DisplayMover
    {
        private int prevLevel;

        public override void CheckForActivity()
        {
            base.CheckForActivity();

            if (!this.targetBody)
                return;

            if (this.targetBody.healthComponent && this.targetBody.healthComponent.combinedHealth != this.targetBody.healthComponent.fullCombinedHealth)
                this.activeTimer = this.refreshTimer;

            var newLevel = Mathf.RoundToInt(this.targetBody.level);
            if (newLevel != this.prevLevel)
            {
                this.prevLevel = newLevel;
                this.activeTimer = this.refreshTimer;
            }
        }
    }
}