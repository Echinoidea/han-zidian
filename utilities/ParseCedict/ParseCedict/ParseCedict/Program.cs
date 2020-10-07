using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;
using System.Linq;
using System.Text.Unicode;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Reflection;

namespace ParseCedict
{
    class Word
    {
        // All words must contain a hanzi, pinyin, english, part of speech, but may not have image path
        public string HanziSimp; // Simplified Chinese character
        public string HanziTrad; // Traditional Chinese character
        public string Pinyin;    // Pinyin for the Word
        public string English;   // English translation of the Word
        public string POS;       // Part of speech of the Word
        public string ImagePath; // Path leading to the appropriate image for the Hanzi


        /// <summary>
        /// Default CTOR
        /// </summary>
        public Word() { }

        // TODO: Add common words list containing this Word
        // TODO: Add example sentences containing this Word
        // TODO: Locate image to fit Word

    }

    class Program
    {
        private static List<Word> words = new List<Word>();

        private static Word CreateWord(string s)
        {
            /* Traditional = all characters up to first " " character.
            * Simplified = all characters after first " " character and before " ["
            * Pinyin = all characters between "[" and "]"
            * Translation = all characters between "/" and "/"
            * Cannot get POS yet
            */

            Word word = new Word();

            string hanziTrad;
            string hanziSimp;
            string pinyin;
            string english;

            Regex r = new Regex(@"^(?<trad>[\u4E00-\u9FA5]+) (?<simp>[\u4E00-\u9FA5]+) (?<pinyin>\[([\w ]+)\]) (?<english>\/(.+)\/)$");

            foreach (Match match in Regex.Matches(s, r.ToString(), RegexOptions.IgnoreCase))
            {
                Console.WriteLine("Found: {0} : {1}", match.Groups["simp"].Value, match.Groups["english"]);

                hanziTrad = match.Groups["trad"].Value;
                hanziSimp = match.Groups["simp"].Value;
                pinyin = match.Groups["pinyin"].Value;
                english = match.Groups["english"].Value;

                word.HanziTrad = hanziTrad;
                word.HanziSimp = hanziSimp;
                word.Pinyin = pinyin;
                word.English = english;
            }

            return word;
        }

        private static void SplitCeDict(string dictPath)
        {
            List<string> lineList = new List<string>();

            const Int32 BufferSize = 128;
            using (var fileStream = File.OpenRead(dictPath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                string l;
                while ((l = streamReader.ReadLine()) != null)
                {
                    lineList.Add(l);
                    words.Add(CreateWord(l));
                }
            }
        }

        private static string PrettyJson(string inJson)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            var jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(inJson);

            return System.Text.Json.JsonSerializer.Serialize(jsonElement, options);
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            SplitCeDict(@"C:\_repo\han-zidian\utilities\ParseCedict\TestDict.txt");
            foreach (var l in words)
            {
                File.WriteAllText(@"C:\_repo\han-zidian\utilities\ParseCedict\Parsed.json", PrettyJson(JsonConvert.SerializeObject(l)));
            }
            
            Console.WriteLine(@"Created JSON file at C:\_repo\han-zidian\utilities\ParseCedict");
        }
    }
}

// TODO: Make JSON file support Hanzi
// TODO: Exception handling when parsing 118,000 lines of text. Something will probably happen.