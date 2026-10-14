using RoR2;
using RoR2.UI;
using UnityEngine;

namespace HunkHud
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HudElement))]
    public class CustomHudElement : MonoBehaviour
    {
        protected HudElement hudElement;

        public CharacterMaster targetMaster => hudElement._hud?.targetMaster;

        public HUD hud
        {
            get => hudElement.hud;
            set => hudElement.hud = value;
        }

        public CharacterBody targetCharacterBody
        {
            get => hudElement.targetCharacterBody;
            set => hudElement.targetCharacterBody = value;
        }
        
        public GameObject targetBodyObject
        {
            get => hudElement.targetBodyObject;
            set => hudElement.targetBodyObject = value;
        }

        protected virtual void Awake()
        {
            hudElement = GetComponent<HudElement>();
        }

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
