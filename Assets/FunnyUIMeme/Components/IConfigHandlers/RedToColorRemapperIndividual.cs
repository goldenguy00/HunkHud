using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZioConfigFile;

namespace MaterialHud
{
    public class RedToColorRemapperIndividual : MonoBehaviour, IConfigHandler
    {
        public string configKey;
        public string configDesc;
        private Color defaultColor; // = new Color32(240, 91, 91, 255); //"#F05B5B";
        private ZioConfigEntry<Color> _configEntry;
        public MonoBehaviour target;
        private bool _rainbow;

        public void Startup()
        {
            defaultColor = GetColor();
            _configEntry = ConfigHelper.Bind("Recoloring", configKey, defaultColor, configDesc);
        }
        public void Awake()
        {
            Startup();
            _configEntry.SettingChanged += UpdateColor;
        }

        private void OnEnable()
        {
            UpdateColor();
        }

        private void OnDestroy()
        {
            _configEntry.SettingChanged -= UpdateColor;
        }

        private void UpdateColor(ZioConfigEntryBase config, object oldValue, bool ignoreSave)
        {
            UpdateColor();
        }

        private void SetColor(Color color)
        {
            switch (target)
            {
                case TextMeshProUGUI text:
                    text.color = color;
                    break;
                case Image image:
                    image.color = color;
                    break;
                case HealthBar healthBar:
                    healthBar.style.trailingOverHealthBarStyle.baseColor = color;
                    break;
            }
        }

        private Color GetColor()
        {
            switch (target)
            {
                case TextMeshProUGUI text:
                    return text.color;
                case Image image:
                    return image.color;
                case HealthBar healthBar:
                    return healthBar.style.trailingOverHealthBarStyle.baseColor;
            }
            return Color.black;
        }

        private void UpdateColor()
        {
            if (_configEntry.Value == Color.clear)
            {
                _rainbow = true;
                return;
            }
            //if (!ColorUtility.TryParseHtmlString(_configEntry.Value.Trim(), out var color)) return;
            SetColor(_configEntry.Value);
        }

        private void Update()
        {
            if (!_rainbow) return;
            SetColor(Color.HSVToRGB(Mathf.Sin(Time.time) * 0.5f + 0.5f, 1, 1));
        }
    }
}
