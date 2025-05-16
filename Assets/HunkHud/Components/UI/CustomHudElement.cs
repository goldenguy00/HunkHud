using System;
using RoR2;
using RoR2.UI;

namespace HunkHud
{
    public class CustomHudElement : HudElement
    {
        public HUD targetHud => _hud;
        public CharacterBody targetBody => _targetCharacterBody;
        public CharacterMaster targetMaster => _hud?.targetMaster;

        protected virtual void OnEnable()
        {
            InstanceTracker.Add(this);
        }

        protected virtual void OnDisable()
        {
            InstanceTracker.Remove(this);
        }
    }
}
