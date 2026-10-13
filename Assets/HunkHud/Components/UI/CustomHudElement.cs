using HG;
using RoR2;
using RoR2.UI;
using System;
using UnityEngine;

namespace HunkHud
{
    [DisallowMultipleComponent]
    public class CustomHudElement : MonoBehaviour
    {
        [NonSerialized]
        public HUD _hud;

        [NonSerialized]
        public GameObject _targetBodyObject;

        [NonSerialized]
        public CharacterBody _targetCharacterBody;

        public HUD hud
        {
            get
            {
                return _hud;
            }
            set
            {
                if ((object)_hud != value)
                {
                    if ((object)_hud != null)
                    {
                        OnHudLost(_hud);
                    }

                    _hud = value;
                    if ((object)_hud != null)
                    {
                        OnHudDiscovered(_hud);
                    }
                }
            }
        }

        public GameObject targetBodyObject
        {
            get
            {
                return _targetBodyObject;
            }
            set
            {
                if ((object)_targetBodyObject != value)
                {
                    if ((object)_targetBodyObject != null)
                    {
                        OnHudTargetBodyObjectLost(_targetBodyObject);
                    }

                    _targetBodyObject = value;
                    if ((object)_targetBodyObject != null)
                    {
                        OnHudTargetBodyObjectDiscovered(_targetBodyObject);
                    }
                }
            }
        }

        public CharacterBody targetCharacterBody
        {
            get
            {
                return _targetCharacterBody;
            }
            set
            {
                if ((object)_targetCharacterBody != value)
                {
                    if ((object)_targetCharacterBody != null)
                    {
                        OnHudTargetBodyLost(_targetCharacterBody);
                    }

                    _targetCharacterBody = value;
                    if ((object)_targetCharacterBody != null)
                    {
                        OnHudTargetBodyDiscovered(_targetCharacterBody);
                    }
                }
            }
        }

        public event Action<HUD> onHudDiscovered;

        public event Action<HUD> onHudLost;

        public event Action<GameObject> onTargetBodyObjectDiscovered;

        public event Action<GameObject> onTargetBodyObjectLost;

        public event Action<CharacterBody> onTargetBodyDiscovered;

        public event Action<CharacterBody> onTargetBodyLost;

        public void OnHudDiscovered(HUD hud)
        {
            this.onHudDiscovered?.Invoke(hud);
            hud.onTargetBodyObjectDiscovered += OnHudTargetBodyObjectDiscovered;
            hud.onTargetBodyObjectLost += OnHudTargetBodyObjectLost;
            targetBodyObject = hud.targetBodyObject;
        }

        public void OnHudLost(HUD hud)
        {
            hud.onTargetBodyObjectDiscovered -= OnHudTargetBodyObjectDiscovered;
            hud.onTargetBodyObjectLost -= OnHudTargetBodyObjectLost;
            targetBodyObject = null;
            this.onHudLost?.Invoke(hud);
        }

        public void OnHudTargetBodyObjectDiscovered(GameObject hudTargetBodyObject)
        {
            this.onTargetBodyObjectDiscovered?.Invoke(hudTargetBodyObject);
            targetCharacterBody = hudTargetBodyObject.AsValidOrNull()?.GetComponent<CharacterBody>();
        }

        public void OnHudTargetBodyObjectLost(GameObject hudTargetBodyObject)
        {
            targetCharacterBody = null;
            this.onTargetBodyObjectLost?.Invoke(hudTargetBodyObject);
        }

        public void OnHudTargetBodyDiscovered(CharacterBody hudTargetBody)
        {
            this.onTargetBodyDiscovered?.Invoke(hudTargetBody);
        }

        public void OnHudTargetBodyLost(CharacterBody hudTargetBody)
        {
            this.onTargetBodyLost?.Invoke(hudTargetBody);
        }

        public void Awake()
        {
            hud = GetComponentInParent<HUD>();
        }

        public void OnTransformParentChanged()
        {
            hud = GetComponentInParent<HUD>();
        }

        public void OnDestroy()
        {
            hud = null;
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
        public CharacterMaster targetMaster => _hud?.targetMaster;
    }
}
