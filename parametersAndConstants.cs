using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV
{
    public partial class INFOIBV
    {
        private const double HOUGH_STEP = 0.3;
        private sbyte[,] vPrewittfilter = new sbyte[3, 3] { { -1, -1, -1 },
                                             {  0 , 0,  0 },
                                             {  1,  1,  1 } };

        private sbyte[,] hPrewittfilter = new sbyte[3, 3] { { -1, 0, 1 },
                                             { -1, 0, 1 },
                                             { -1, 0, 1 } };
        private sbyte[,] ySobelfilter = new sbyte[3, 3] { { -1, -2, -1 },
                                             {  0 , 0,  0 },
                                             {  1,  2,  1 } };

        private sbyte[,] xSobelfilter = new sbyte[3, 3] { { -1, 0, 1 },
                                             { -2, 0, 2 },
                                             { -1, 0, 1 } };

        private double THRESHOLD = 0.6;
        private int CLOSE_DIM = 11;
        private int MINIMUM_THRESHOLD = 70;
        private int MINIMUM_LENGHT = 20;
        private int MAXIMUM_GAP = 5;

        private int CANNY_EDGE_SIZE = 3;
        private int CANNY_SIGMA = 3;
        private int LOW_TH = 50;
        private int HIGH_TH = 100;

        IDictionary<int, (int, int)> contourDict = new Dictionary<int, (int, int)>()
        {
            { 0, (1,0)},
            { 7, (1,-1)},
            { 6, (0,-1)},
            { 5, (-1,-1)},
            { 4, (-1,0)},
            { 3, (-1,1)},
            { 2, (0,1)},
            { 1, (1,1)},
        };

    }
}
