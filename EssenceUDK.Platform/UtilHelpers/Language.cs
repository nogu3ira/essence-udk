﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EssenceUDK.Platform.UtilHelpers
{
    public sealed class Language
    {
        public string InterName { get; private set; }
        public string LocalName { get; private set; }
        public string Extension { get; private set; }
        public ushort EncodPage { get; private set; }

        private readonly Encoding _Encoding;
        private readonly byte[]   _StrBufer;

        // Ok, this are official languages
        public static readonly Language English = new Language("English",  "English",  "enu", 1250);
        public static readonly Language German  = new Language("German",   "Deutsch",  "deu", 1250);
        public static readonly Language French  = new Language("France",   "Français", "fra", 1250);
        public static readonly Language Spanish = new Language("Spanish",  "Español",  "esp", 1250); 

        // For thees one we use english codepage as we cant use ansi for theme (or maybe can?) 
        public static readonly Language Japanese= new Language("Japanese", "日本人",   "jpn", 1250); 
        public static readonly Language Korean  = new Language("Korean",   "한국의",   "kor", 1250); 
        public static readonly Language Chinese = new Language("Chinese",  "中国的",   "cht", 1250); // is it really chinese???

        // Lets olso add some custom unofficial languages
        public static readonly Language Russian = new Language("Russian",  "Русский",  "rus", 1251);
        public static readonly Language Italian = new Language("Italian",  "Italiano", "ita", 1250);


        public Language(string intername, string localname, string extension, ushort encodpage)
        {
            InterName = intername;
            LocalName = localname;
            Extension = extension.ToLower();
            EncodPage = encodpage;
            _Encoding = Encoding.GetEncoding(EncodPage);
            _StrBufer = new byte[256];
        }

        public string ReadAnsiString(byte[] chars)
        {
            int count;
            for (count = 0; count < chars.Length && chars[count] != 0; ++count) ;
            return _Encoding.GetString(chars, 0, count);
        }

        public unsafe string ReadAnsiString(byte* chars, uint count)
        {
            var buffer = count <= _StrBufer.Length ? _StrBufer : new byte[count];
            for (count = 0; count < buffer.Length && chars[count] != 0; ++count) buffer[count] = chars[count];
            return _Encoding.GetString(buffer, 0, (int)count);
        }
    }
}
