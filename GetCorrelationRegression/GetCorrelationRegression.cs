using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class GetCorrelationRegression : Robot
    {
        [Parameter("CSV出力パス", DefaultValue = "c:\\tmp")]
        public string para_filePath { get; set; }

        [Parameter("遡及日数", DefaultValue = 365)]
        public int para_retroactiveDates { get; set; }

        private TimeFrame[] timeFrames =    
        {
            TimeFrame.Weekly,
            TimeFrame.Daily,
            TimeFrame.Hour4,
            TimeFrame.Hour,
            TimeFrame.Minute30,
            TimeFrame.Minute10,
            TimeFrame.Minute5,
            TimeFrame.Minute
        };

        string fileFullPath;
        private string fileName = DateTime.Now.ToString("yyyyMMddHHmmss_") + "GetCorrelationRegression.csv";
        private const string csvHeader = "通貨ペア1" + "," + "通貨ペア2" + "," + "Weekly" + "," + "Daily" + "," + "Hour4" + "," + "Hour1" + "," + "Minute30" + "," + "Minute10" + "," + "Minuts5" + "," + "Minuts1";
        protected override void OnStart()
        {
            Print("開始");

            fileFullPath = System.IO.Path.Combine(para_filePath, fileName);
            WriteCsvFile(csvHeader);
            Print(csvHeader);

            foreach (var baseSymbol in Symbols)
            {
                foreach (var subSymbol in Symbols)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.Append(baseSymbol);
                    sb.Append(",");
                    sb.Append(subSymbol);
                    sb.Append(",");

                    foreach (var timeFrame in timeFrames)
                    {
                        //メインシンボルとサブシンボルのバーの数を少ないほうに合わせる
                        int barCount = System.Math.Min(MarketData.GetBars(timeFrame, baseSymbol).Count, MarketData.GetBars(timeFrame, subSymbol).Count);

                        //メインシンボルとサブシンボルの終値のリストを作成する
                        var closePriceOfBaseSymbol = new List<double>();
                        var closePriceOfSubSymbol = new List<double>();

                        for (int i = 0; i < barCount; i++)
                        {
                            closePriceOfBaseSymbol.Add(MarketData.GetBars(timeFrame, baseSymbol).ClosePrices.Last(i));
                            closePriceOfSubSymbol.Add(MarketData.GetBars(timeFrame, subSymbol).ClosePrices.Last(i));
                        }
                        //相関係数を算出する
                        sb.Append(ComputeCoeff(closePriceOfBaseSymbol, closePriceOfSubSymbol));
                        sb.Append(",");
                    }
                    WriteCsvFile(sb.ToString());
                }
            }
            Print("終了");

        }
        /// <summary>
        /// 相関係数の導出
        /// </summary>
        /// <param name="values1">通貨ペア1</param>
        /// <param name="values2">通貨ペア2</param>
        /// <returns></returns>
        public double ComputeCoeff(List<double> values1, List<double> values2)
        {
            /*
            if (values1.Count != values2.Count)
                throw new ArgumentException("values must be the same length");
                */
            var avg1 = values1.Average();
            var avg2 = values2.Average();

            var sum1 = values1.Zip(values2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

            var sumSqr1 = values1.Sum(x => Math.Pow((x - avg1), 2.0));
            var sumSqr2 = values2.Sum(y => Math.Pow((y - avg2), 2.0));

            var result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);

            return Math.Round(result, 2);
        }
        private void WriteCsvFile(string text)
        {
            try
            {
                var append = true;
                using (var sw = new System.IO.StreamWriter(fileFullPath, append))
                {
                    sw.WriteLine(text);
                }
            } catch (System.Exception e)
            {
                // ファイルを開くのに失敗したときエラーメッセージを表示
                System.Console.WriteLine(e.Message);
            }
        }

    }
}
