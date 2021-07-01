namespace consoleWheelchair
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    /// <summary>
    /// 분석용 함수
    /// </summary>
    public class AnalyticsUtils
    {
        /// <summary>
        /// 선형 분석
        /// 기울기만 쓰세요
        /// </summary>
        /// <param name="xVals">X축 데이터</param>
        /// <param name="yVals">Y축 데이터</param>
        /// <param name="inclusiveStart">측정시작점</param>
        /// <param name="exclusiveEnd">측정 종료점</param>
        /// <param name="rsquared">결정계수</param>
        /// <param name="yintercept">y 절편</param>
        /// <param name="slope">기울기</param>
        public static void LinearRegression(double[] xVals, double[] yVals,
                                            int inclusiveStart, int exclusiveEnd,
                                            out double rsquared, out double yintercept,
                                            out double slope)
        {
            Debug.Assert(xVals.Length == yVals.Length);
            double sumOfX = 0;
            double sumOfY = 0;
            double count = exclusiveEnd - inclusiveStart;
            rsquared = 0;
            yintercept = 0;
            slope = 0;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                sumOfX += xVals[ctr];
                sumOfY += yVals[ctr];
            }

            var avgX = sumOfX / count;
            var avgY = sumOfY / count;

            double sumxy = 0;
            double sumxsq = 0;
            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                double x = xVals[ctr];
                double y = yVals[ctr];
                sumxy += (x - avgX) * (y - avgY);
                sumxsq += Math.Pow(x - avgX, 2);
            }
            slope = sumxy / sumxsq;
        }
    }
}
