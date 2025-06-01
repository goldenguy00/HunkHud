using System;
using RoR2;
using RoR2.UI;

namespace HunkHud
{
    public class CustomHudElement : HudElement
    {
        protected CharacterBody _prevBody;
        public CharacterBody targetBody => _targetCharacterBody;
        public CharacterMaster targetMaster => _hud?.targetMaster;

        protected virtual void OnEnable()
        {
            this.hud ??= this.transform.root.GetComponent<HUD>();

            HUD.onHudTargetChangedGlobal += this.HUD_onHudTargetChangedGlobal;

            InstanceTracker.Add(this);
        }

        protected virtual void HUD_onHudTargetChangedGlobal(HUD newHud)
        {
            this.hud = newHud;
            this._prevBody = this.targetBody;
        }

        protected virtual void OnDisable()
        {
            HUD.onHudTargetChangedGlobal -= this.HUD_onHudTargetChangedGlobal;

            InstanceTracker.Remove(this);
        }
    }
}
