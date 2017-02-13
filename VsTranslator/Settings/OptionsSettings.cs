﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace VsTranslator.Settings
{
    public class OptionsSettings
    {
        private static Settings _settings;

        public static Settings Settings
        {
            get { return _settings ?? (_settings = Settings.Instance()); }

            set
            {
                _settings = value;
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(_settings), Encoding.UTF8);
            }
        }

        private static readonly string LocalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VsTranslator");

        private static readonly string ConfigPath = Path.Combine(LocalPath, "settings.config");

        static OptionsSettings()
        {
            Init();
        }

        private static void Init()
        {
            if (!Directory.Exists(LocalPath))
            {
                Directory.CreateDirectory(LocalPath);
            }

            if (File.Exists(ConfigPath))
            {
                var settings = File.ReadAllText(ConfigPath, Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(settings))
                {
                    try
                    {
                        _settings = JsonConvert.DeserializeObject<Settings>(settings);
                        if (_settings.BaiduSettings == null) _settings.BaiduSettings = new TransSettings();
                        if (_settings.GoogleSettings == null) _settings.GoogleSettings = new TransSettings();
                        if (_settings.BingSettings == null) _settings.BingSettings = new TransSettings();
                        if (_settings.YoudaoSettings == null) _settings.YoudaoSettings = new TransSettings();
                        if (_settings.LetterSpliters == null) _settings.LetterSpliters = new List<Spliter>();
                        if (string.IsNullOrWhiteSpace(_settings.TranslateCachePath) || !System.IO.Directory.Exists(_settings.TranslateCachePath)) _settings.TranslateCachePath = Settings.TranslateCacheDefaultPath;
                    }
                    catch (Exception)
                    {
                        _settings = Settings.Instance();
                    }
                }
            }
        }

        public static void ShowOptions()
        {
            new TranslateOptions(Settings)
            {
                OnSave = SaveSettings
            }.ShowDialog();
            Init();
        }

        private static void SaveSettings(Settings settings)
        {
            Settings = settings;
        }

        public static void SaveSpliters(IList<Spliter> spliters)
        {
            Settings.LetterSpliters = spliters.ToList();

            SaveSettings(Settings);
        }

        public static string SpliteLetterByRules(string selectedText)
        {
            return Settings.LetterSpliters.Aggregate(selectedText, (current, letterSpliter) => new Regex(letterSpliter.MatchRegex).Replace(current, letterSpliter.ReplaceRegex));
        }
    }
}