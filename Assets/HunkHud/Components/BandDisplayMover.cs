using RoR2.UI;
using RoR2;
using UnityEngine;
using HunkHud.Modules;

namespace HunkHud.Components
{
    public class BandDisplayMover : DisplayMover
    {
        private void OnDestroy()
        {
            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged -= this.Inventory_OnInventoryChanged;
        }

        public override void UpdateReferences(HUD hud, CharacterBody body)
        {
            if (this.targetMaster && this.targetMaster.inventory)
                this.targetMaster.inventory.onInventoryChanged -= this.Inventory_OnInventoryChanged;

            base.UpdateReferences(hud, body);

            if (this.targetMaster && this.targetMaster.inventory)
            {
                this.targetMaster.inventory.onInventoryChanged += this.Inventory_OnInventoryChanged;
                this.Inventory_OnInventoryChanged();
            }
        }

        private void Inventory_OnInventoryChanged()
        {
            AddOrRemovePrefab("BandDisplay", "FireRing", "IceRing");
            AddOrRemovePrefab("BandDisplayVoid", "ElementalRingVoid");
            AddOrRemovePrefab("BandDisplayHealing", "ITEM_HEALING_BAND");
            AddOrRemovePrefab("BandDisplayNova", "ITEM_NOVA_BAND");
            AddOrRemovePrefab("BandDisplaySacrificial", "ITEM_SANDSWEPT_SACRIFICIAL_BAND");
            void AddOrRemovePrefab(string prefabName, params string[] itemName)
            {
                bool hasItem = false;
                for (int i = 0; i < itemName.Length; i++)
                {
                    var itemIndex = ItemCatalog.FindItemIndex(itemName[i]);
                    if (itemIndex != ItemIndex.None)
                        hasItem |= this.targetMaster.inventory.GetItemCount(itemIndex) > 0;
                }

                var childTransform = this.transform.Find(prefabName);
                bool hasChild = childTransform != null;

                if (hasItem != hasChild)
                {
                    if (hasChild)
                    {
                        GameObject.Destroy(childTransform.gameObject);
                    }
                    else
                    {
                        var child = GameObject.Instantiate(HudAssets.mainAssetBundle.LoadAsset<GameObject>(prefabName), this.transform);
                        child.name = prefabName;
                    }
                }
            }
        }
    }
}
