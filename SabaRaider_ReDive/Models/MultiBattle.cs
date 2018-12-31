using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SabaRaider_ReDive.Models
{
    public class MultiBattle
    {
        public int BattleID { get; set; }

        public string DisplayBattleName { get; set; }

        public string BattleNameJP { get; set; }

        public string BattleNameEn { get; set; }

        public static List<MultiBattle> GetMultiBattles()
        {
            // csvファイル名
            string csvName = "RaidBattleList.csv";

            try
            {
                List<MultiBattle> multiBattles = new List<MultiBattle>();

                // マルチバトルリスト読み込み
                using (Stream stream = new FileStream(csvName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (TextFieldParser parser = new TextFieldParser(stream, Encoding.Default))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.Delimiters = new[] { "," };
                        parser.HasFieldsEnclosedInQuotes = true;
                        parser.TrimWhiteSpace = true;

                        while (!parser.EndOfData)
                        {
                            string[] fields = parser.ReadFields();

                            MultiBattle battle = new MultiBattle()
                            {
                                BattleID = int.TryParse(fields[0], out int id) ? id : 0,
                                DisplayBattleName = fields[1].ToString(),
                                BattleNameJP = fields[2].ToString(),
                                BattleNameEn = fields[3].ToString()
                            };

                            multiBattles.Add(battle);
                        }
                    }
                }

                return multiBattles;
            }
            catch (Exception)
            {
                return new List<MultiBattle>();
            }
        }
    }
}
