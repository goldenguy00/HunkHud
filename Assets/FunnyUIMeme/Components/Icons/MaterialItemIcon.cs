using System;
using RoR2;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using RoR2.UI;

namespace MaterialHud
{
    [RequireComponent(typeof(RectTransform))]
    public class MaterialItemIcon : MonoBehaviour
    {
        public InspectPanelLocator inspectPanelLocator;

        public UserProfile userProfile;

        public RawImage glowImage;

        public RawImage image;

        public TextMeshProUGUI stackText;

        public TooltipProvider tooltipProvider;

        public ItemIndex itemIndex;

        public int itemCount;

        public Action OnItemClicked;

        public RectTransform rectTransform { get; set; }

        public InspectPanelController inspectPanel
        {
            get
            {
                if ((bool)inspectPanelLocator)
                {
                    return inspectPanelLocator.InspectPanel;
                }

                return null;
            }
        }

        public void Awake()
        {
            CacheRectTransform();
            inspectPanelLocator = GetComponent<InspectPanelLocator>();
            userProfile = GetComponentInParent<HUD>().localUserViewer.userProfile;
        }

        public void CacheRectTransform()
        {
            if ((object)rectTransform == null)
            {
                rectTransform = (RectTransform)base.transform;
            }
        }

        public void SetItemIndex(ItemIndex newItemIndex, int newItemCount)
        {
            if (itemIndex == newItemIndex && itemCount == newItemCount)
            {
                return;
            }

            itemIndex = newItemIndex;
            itemCount = newItemCount;
            string titleToken = "";
            string bodyToken = "";
            Color titleColor = Color.white;
            Color bodyColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
            if (itemDef != null)
            {
                image.texture = itemDef.pickupIconTexture;
                if (itemCount > 1)
                {
                    stackText.text = "x" + itemCount;
                    stackText.enabled = true;
                }
                else
                {
                    stackText.text = "";
                    stackText.enabled = false;
                }

                titleToken = itemDef.nameToken;
                bodyToken = itemDef.pickupToken;
                titleColor = ColorCatalog.GetColor(itemDef._itemTierDef ? itemDef._itemTierDef.darkColorIndex : itemDef.darkColorIndex);
            }

            if ((bool)glowImage)
            {
                glowImage.color = new Color(titleColor.r, titleColor.g, titleColor.b, 0.75f);
            }

            if ((bool)tooltipProvider)
            {
                tooltipProvider.titleToken = titleToken;
                tooltipProvider.bodyToken = bodyToken;
                tooltipProvider.titleColor = titleColor;
                tooltipProvider.bodyColor = bodyColor;
            }
        }

        public void ItemClicked()
        {
            if (!userProfile.useInspectFeature)
            {
                return;
            }

            ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
            if (itemDef != null)
            {
                if ((bool)inspectPanel)
                {
                    inspectPanel.Show(itemDef);
                }

                OnItemClicked?.Invoke();
            }
        }
    }
}
