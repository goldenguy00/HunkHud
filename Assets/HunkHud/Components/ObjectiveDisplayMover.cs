using RoR2;
using RoR2.UI;

namespace HunkHud.Components
{
    public class ObjectiveDisplayMover : DisplayMover
    {
        private int prevCount;
        private ObjectivePanelController objectivePanel;

        private void Awake()
        {
            this.offset = new UnityEngine.Vector3(400f, 0f, 0f);

            this.objectivePanel = this.GetComponentInChildren<ObjectivePanelController>();
            GlobalEventManager.onTeamLevelUp += this.GlobalEventManager_onTeamLevelUp;
        }

        private void OnDestroy()
        {
            GlobalEventManager.onTeamLevelUp -= this.GlobalEventManager_onTeamLevelUp;
        }

        private void GlobalEventManager_onTeamLevelUp(TeamIndex team)
        {
            if (team == TeamIndex.Monster)
                this.activeTimer = this.refreshTimer;
        }

        public override void CheckForActivity()
        {
            base.CheckForActivity();

            if (!this.objectivePanel)
                return;

            var newCount = this.objectivePanel.objectiveTrackers.Count;
            if (newCount != this.prevCount)
            {
                this.prevCount = newCount;
                this.activeTimer = this.refreshTimer;
            }
        }
    }
}
