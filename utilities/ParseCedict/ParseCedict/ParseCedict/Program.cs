using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;

namespace ParseCedict
{
    public class Word
    {
        // All words must contain a hanzi, pinyin, english, but may not have image path
        public string HanziSimp = ""; // Simplified Chinese character
        public string HanziTrad = ""; // Traditional Chinese character
        public string Pinyin = "";    // Pinyin for the Word
        public string English = "";   // English translation of the Word
        //public string POS;       // Part of speech of the Word
        public string ImagePath = ""; // Path leading to the appropriate image for the Hanzi


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

        static void ToXML(string path)
        {
            XDocument xdoc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                        // This is the root of the document
                        new XElement("Entries",
                        from w in words
                        select
                            new XElement("Entry",
                                new XElement("HanziSimp", w.HanziSimp),
                                new XElement("HanziTrad", w.HanziTrad),
                                new XElement("Pinyin", w.Pinyin),
                                new XElement("English", w.English),
                                new XElement("StrokeImagePath", w.ImagePath))));

            // Write the document to the file system            
            xdoc.Save(path);
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string dictPath = @"C:\_repo\han-zidian\utilities\ParseCedict\cedict_ts.u8";
            //dictPath = @"C:\_repo\han-zidian\utilities\ParseCedict\TestDict.txt";
            string parsedPath = @"C:\_repo\han-zidian\utilities\ParseCedict\ParsedTest.xml";

            File.Delete(parsedPath);

            SplitCeDict(dictPath);

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            ToXML(parsedPath);

            stopwatch.Stop();
            Console.WriteLine("Wrote {0} words in {1} milliseconds\nFound images for {2} words", words.Count, stopwatch.ElapsedMilliseconds, imageCount);
            Console.WriteLine(@"Created XML file at {0}", parsedPath);
            // RESULTS Wrote 118845 words in 339 milliseconds Found images for 1165 words
        }
    }
}
