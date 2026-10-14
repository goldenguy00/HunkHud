using System;
using System.Collections.Generic;
using System.Reflection;
using RoR2.UI;
using UnityEngine;
using ZioConfigFile;

namespace MaterialHud
{
	public class HealthbarRecolor : MonoBehaviour, IConfigHandler
	{
		public string whichBar = "Player HealthBar";
		public HealthBar healthBar;
		private readonly Dictionary<string, HealthBarConfig> entries = new();
		
		public void Awake()
		{
			Startup(true);
			OnEnable();
		}

		private void OnEnable()
		{
			foreach (var entry in entries)
			{
				entry.Value.ConfigChanged(null, null, false);
			}
		}

		
		private void SetupConfig(string key, string desc, HealthBarStyle style, string field, bool b)
		{
			var entryKey = "Recoloring " + whichBar + key;
			var config = new HealthBarConfig("Recoloring " + whichBar, key, desc, field, style, b);
			entries[entryKey] = config;
		}

		private void OnDestroy()
		{
			foreach (var entry in entries)
			{
				entry.Value.Destroy();
			}
		}

		private void Update()
		{
			var color = Color.HSVToRGB(Mathf.Sin(Time.time) * 0.5f + 0.5f, 1, 1);
			foreach (var rainbow in entries)
			{
				rainbow.Value.Update(color);
			}
		}

		public class HealthBarConfig
		{
			private static readonly Type StyleType = typeof(HealthBarStyle);
			public HealthBarStyle style;
			private readonly FieldInfo _fieldInfo;
			private ZioConfigEntry<Color> _configValue;
			private bool rainbow;

			public Color Color
			{
				get => ((HealthBarStyle.BarStyle) _fieldInfo.GetValue(style)).baseColor;
				set
				{
					var sty = (HealthBarStyle.BarStyle) _fieldInfo.GetValue(style);
					sty.baseColor = value;
					_fieldInfo.SetValue(style, sty);
				}
			}

			public HealthBarConfig(string category, string key, string desc, string field, HealthBarStyle styleIn, bool b)
			{
				style = styleIn;
				_fieldInfo = StyleType.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				_configValue = ConfigHelper.Bind(category, key, Color, desc);
				if (b)
					_configValue.SettingChanged += ConfigChanged;
			}

			public void Destroy()
			{
				_configValue.SettingChanged -= ConfigChanged;
			}

			public void ConfigChanged(ZioConfigEntryBase zioConfigEntryBase, object o, bool arg3)
			{
				rainbow = _configValue.Value == Color.clear;
				Color = _configValue.Value;
			}

			public void Update(Color color)
			{
				if (rainbow)
					Color = color;
			}
		}

		public void Startup(bool b)
		{
			var style = healthBar.style;
			SetupConfig("Trailing Under", "When hurt the bit that is slow to catch up.", style, "trailingUnderHealthBarStyle", b);
			SetupConfig("Instant Health", "From medkits, etc.", style, "instantHealthBarStyle", b);
			SetupConfig("Trailing Over", "The general color of the healthbar.", style, "trailingOverHealthBarStyle", b);
			SetupConfig("Shield", "Shield", style, "shieldBarStyle", b);
			SetupConfig("Curse", "Curse refers to having Shaped glass / anything that temporarily lowers max HP & normally leaves you w/ the white outlined section.", style, "curseBarStyle", b);
			SetupConfig("Barrier", "Barrier", style, "barrierBarStyle", b);
			SetupConfig("Flash", "?", style, "flashBarStyle", b);
			SetupConfig("Cull", "Cull refers to things like guillotine / freeze, and is only seen when an allied minion (turret / zoea / drone, etc..) is hit by a Glacial Elite's death effect radius. Cull can't be applied to players however.", style, "cullBarStyle", b);
			SetupConfig("Low Health Over", "Color for delicate watches and things like that.", style, "lowHealthOverStyle", b);
			SetupConfig("Low Health Under", "?", style, "lowHealthUnderStyle", b);
			SetupConfig("OSP", "One shot protection color.", style, "ospStyle", b);
            SetupConfig("Echo Damage", "Pending warped echo damage", style, "echoDamageStyle", b);
            SetupConfig("Lunar Ruin", "Lunar Ruin recolor", style, "lunarRuinStyle", b);
            SetupConfig("Acolyte Regen", "Acolyte's potential healing", style, "regainableHealthStyle", b);
        }

		public void Startup()
		{
			Startup(false);
		}
	}
}