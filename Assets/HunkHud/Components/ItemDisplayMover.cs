using RoR2;
using UnityEngine;

namespace HunkHud.Components
{
    public class ItemDisplayMover : DisplayMover
    {
        private CharacterMaster _prevMaster;

        protected override void Awake()
        {
            base.Awake();
            this.offset = new Vector3(0f, 250f, 0f);
        }

        protected override void Start()
        {
            var pos = this.transform.localPosition;
            pos.x = 0f;
            this.transform.localPosition = pos;

            base.Start();
        }

        public override void CheckForActivity()
        {
            if (this._prevMaster != this.targetMaster)
            {
                if (this._prevMaster && this._prevMaster.inventory)
                    this._prevMaster.inventory.onInventoryChanged -= this.SetActive;
                
                this._prevMaster = this.targetMaster;

                if (this.targetMaster && this.targetMaster.inventory)
                    this.targetMaster.inventory.onInventoryChanged += this.SetActive;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged -= this.SetActive;
        }
    }
}
