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
        public string[] HanziSimp { get; set; } // Simplified Chinese character
        public string[] HanziTrad { get; set; } // Traditional Chinese character
        public string[] Pinyin { get; set; }    // Pinyin for the Word
        public string[] English { get; set; }   // English translation of the Word
        public string[] POS { get; set; }       // Part of speech of the Word
        public string ImagePath { get; set; }   // Path leading to the appropriate image for the Hanzi

        /// <summary>
        /// Word CTOR with all properties
        /// </summary>
        /// <param name="hanziSimp">Simplified Hanzi</param>
        /// <param name="hanziTrad">Traditional Hanzi</param>
        /// <param name="pinyin">Pinyin transliteration</param>
        /// <param name="english">English translation</param>
        /// <param name="pOS">Part of speech of the Word</param>
        /// <param name="imagePath">The path leading to the image representing this Word</param>
        public Word(string[] hanziSimp, string[] hanziTrad, string[] pinyin, string[] english, string[] pOS, string imagePath)
        {
            HanziSimp = hanziSimp;
            HanziTrad = hanziTrad;
            Pinyin = pinyin;
            English = english;
            POS = pOS;
            ImagePath = imagePath;
        }

        /// <summary>
        /// Word CTOR for Words without an image.
        /// </summary>
        /// <param name="hanziSimp">Simplified Hanzi</param>
        /// <param name="hanziTrad">Traditional Hanzi</param>
        /// <param name="pinyin">Pinyin transliteration</param>
        /// <param name="english">English translation</param>
        /// <param name="pOS">Part of speech of the Word</param>
        public Word(string[] hanziSimp, string[] hanziTrad, string[] pinyin, string[] english, string[] pOS)
        {
            HanziSimp = hanziSimp;
            HanziTrad = hanziTrad;
            Pinyin = pinyin;
            English = english;
            POS = pOS;
        }

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

        private static void CreateWord(string s)
        {
            /* Traditional = all characters up to first " " character.
            * Simplified = all characters after first " " character and before " ["
            * Pinyin = all characters between "[" and "]"
            * Translation = all characters between "/" and "/"
            * Cannot get POS yet
            */

            Regex r = new Regex(@"(?<trad>[\u4E00-\u9FCC\u3400-\u4DB5\uFA0E\uFA0F\uFA11\uFA13\uFA14\uFA1F\uFA21\uFA23\uFA24\uFA27-\uFA29]|[\ud840-\ud868][\udc00-\udfff]|\ud869[\udc00-\uded6\udf00-\udfff]|[\ud86a-\ud86c][\udc00-\udfff]|\ud86d[\udc00-\udf34\udf40-\udfff]|\ud86e[\udc00-\udc1d]/+) (?<simp>[[\u4E00-\u9FCC\u3400-\u4DB5\uFA0E\uFA0F\uFA11\uFA13\uFA14\uFA1F\uFA21\uFA23\uFA24\uFA27-\uFA29]|[\ud840-\ud868][\udc00-\udfff]|\ud869[\udc00-\uded6\udf00-\udfff]|[\ud86a-\ud86c][\udc00-\udfff]|\ud86d[\udc00-\udf34\udf40-\udfff]|\ud86e[\udc00-\udc1d]/]+) (?<pinyin>\[([\w ]+)\]) (?<english>\/([\w ]+)\/)");

            foreach (var x in r.GetGroupNames())
            {
                Console.WriteLine(x);
            }
            foreach (Match match in Regex.Matches(s, r.ToString(), RegexOptions.IgnoreCase))
            {
                Console.WriteLine("Trad: {0}", match.Groups["trad"].Value);
            }



        }

        private static void SplitCeDict(string dictPath)
        {
            // I want : [[一月份 一月份 [yi1 yue4 fen4] /January/], [一望無垠 一望无垠 [yi1 wang4 wu2 yin2] /to stretch as far as the eye can see (idiom)/], ...]
            List<string> lineList = new List<string>();

            const Int32 BufferSize = 128;
            using (var fileStream = File.OpenRead(dictPath))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                string l;
                while ((l = streamReader.ReadLine()) != null)
                {
                    lineList.Add(l);
                    //words.Add(CreateWord(l));
                }
            }
                    

            

            //string hanzi = lines[0]
            
            /*
            File.WriteAllText(@"C:\_repo\han-zidian\utilities\ParseCedict\Parsed.json", PrettyJson(JsonConvert.SerializeObject(new Word("Jeff", 22))));
            Console.WriteLine(@"Created JSON file at C:\_repo\han-zidian\utilities\ParseCedict");*/



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
            //SplitCeDict(@"C:\_repo\han-zidian\utilities\ParseCedict\TestObj.txt");
            CreateWord("粢 粢 [zi1] /common millet/");
        }
    }
}
// TODO: Split cedict line by line, add each line to a list, add each list to one list
// TODO: Split each line into the appropriate groups (hanzi, pinyin, etc.)