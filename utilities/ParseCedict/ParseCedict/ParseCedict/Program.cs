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
using System.Diagnostics;

namespace ParseCedict
{
    class Word
    {
        // All words must contain a hanzi, pinyin, english, but may not have image path
        public string HanziSimp; // Simplified Chinese character
        public string HanziTrad; // Traditional Chinese character
        public string Pinyin;    // Pinyin for the Word
        public string English;   // English translation of the Word
        //public string POS;       // Part of speech of the Word
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
        private static int imageCount = 0;
        private static void FindImagePath(Word word)
        {
            string imageSourceDir = @"C:\_repo\han-zidian\assets\stroke-orders";
            DirectoryInfo directory = new DirectoryInfo(imageSourceDir);

            foreach (var f in directory.GetFiles())
            {
                if (f.Name.Contains(word.HanziSimp) || f.Name.Contains(word.HanziTrad)) 
                {
                    word.ImagePath = f.DirectoryName + @"\\" + f.Name;
                    imageCount++;
                }
            }
        }

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

            Regex r = new Regex(@"^(?<trad>[\p{Lo}\p{N}]+) (?<simp>[\p{Lo}\p{N}]+) (?<pinyin>\[([\w ]+)\]) (?<english>\/(.+)\/)$");


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

                FindImagePath(word);
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

            string dictPath = @"C:\_repo\han-zidian\utilities\ParseCedict\cedict_ts.u8";
            string parsedPath = @"C:\_repo\han-zidian\utilities\ParseCedict\ParsedCedict.json";

            File.Delete(parsedPath);

            SplitCeDict(dictPath);

            int wordCount = 0;
            int skipCount = 0;
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            foreach (Word w in words)
            {
                if (w.HanziSimp == null && w.HanziTrad == null && w.Pinyin == null && w.English == null)
                {
                    Console.WriteLine("Word did not contain any data! Skipped");
                    skipCount++;
                }
                else
                {
                    File.AppendAllText(parsedPath, PrettyJson(JsonConvert.SerializeObject(w)));
                    Console.WriteLine("Wrote {0} to {1}", w.HanziSimp, parsedPath);
                    wordCount++;
                }
            }
            stopwatch.Stop();

            Console.WriteLine("Wrote {0} words and skipped {1} words in {2} milliseconds\nFound images for {3} words", wordCount, skipCount, stopwatch.ElapsedMilliseconds, imageCount);
            Console.WriteLine(@"Created JSON file at C:\_repo\han-zidian\utilities\ParseCedict");
            // RESULTS Wrote 116747 words and skipped 2098 words in 430098 milliseconds Found images for 1165 words
        }
    }
}

// TODO: It's fine that the JSON file isn't showing the Hanzi, just convert the encoding to UTF8 when deserializing in Dart