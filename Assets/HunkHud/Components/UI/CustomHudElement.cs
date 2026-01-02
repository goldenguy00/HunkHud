using RoR2;
using RoR2.UI;

namespace HunkHud
{
    public class CustomHudElement : HudElement
    {
        public CharacterMaster targetMaster => _hud?.targetMaster;

        protected virtual void OnEnable()
        {
            if (!this.hud)
                this.hud = this.transform.root.GetComponent<HUD>();

            HUD.onHudTargetChangedGlobal += this.HUD_onHudTargetChangedGlobal;

            InstanceTracker.Add(this);
        }

        protected virtual void HUD_onHudTargetChangedGlobal(HUD newHud)
        {
            this.hud = newHud;
        }

        protected virtual void OnDisable()
        {
            InstanceTracker.Remove(this);

            HUD.onHudTargetChangedGlobal -= this.HUD_onHudTargetChangedGlobal;
        }
    }
}
