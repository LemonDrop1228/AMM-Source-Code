﻿using System;
using System.Collections.Generic;
using System.Text;
using Anno1800ModLauncher.Helpers;
using Anno1800ModLauncher.Helpers.Enums;

namespace SerializableModinfo
{
    public class Localized
    {
        public Localized() { }
        public String Chinese { get; set; }
        public String English { get; set; }
        public String French { get; set; }
        public String German { get; set; }
        public String Italian { get; set; }
        public String Japanese { get; set; }
        public String Korean { get; set; }
        public String Polish { get; set; }
        public String Russian { get; set; }
        public String Spanish { get; set; }
        public String Taiwanese { get; set; }

        public String getText() {
            switch (LanguageManager.Instance.GetLanguage()) {
                case HelperEnums.Language.English: return English;
                case HelperEnums.Language.German: return German;
                default: return English; 
            }
        }
    }
}
