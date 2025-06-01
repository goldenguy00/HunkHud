using RoR2;
using RoR2.UI;

namespace HunkHud.Components
{
    public class ObjectiveDisplayMover : DisplayMover
    {
        private int prevCount;
        private ObjectivePanelController objectivePanel;

        protected override void Awake()
        {
            base.Awake();
            this.offset = new UnityEngine.Vector3(400f, 100f, 0f);

            this.objectivePanel = this.GetComponentInChildren<ObjectivePanelController>();
            GlobalEventManager.onTeamLevelUp += this.GlobalEventManager_onTeamLevelUp;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GlobalEventManager.onTeamLevelUp -= this.GlobalEventManager_onTeamLevelUp;
        }

        private void GlobalEventManager_onTeamLevelUp(TeamIndex team)
        {
            this.SetActive();
        }

        public override void CheckForActivity()
        {
            if (!this.objectivePanel)
                return;

            var newCount = this.objectivePanel.objectiveTrackers.Count;
            if (newCount != this.prevCount)
            {
                this.prevCount = newCount;
                this.SetActive();
            }
        }
    }
}
