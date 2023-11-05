using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV
{
    public partial class INFOIBV

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// EVERY PARAMETER IN THIS FILE AND IN THE REST OF THE PROJECT HAS BEEN CHOSEN EMPIRICALLY TO GET THE BEST RESULT FROM THE ANALYZED IMAGES///////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    {
        // define step for hough transforation from the line equation to the discrete version in the accumulator
        private const double HOUGH_STEP = 0.3;

        // define vertical and horizontal version of Prewitt and Sobel filters
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
        // threshold for binary images
        private double THRESHOLD = 0.6;

        //dimension of structural element for closing operation
        private int CLOSE_DIM = 11;

        //define minimum threshold on a gray scale image to draw a segment
        private int MINIMUM_THRESHOLD = 70;

        // define minimum lenght of a segment
        private int MINIMUM_LENGHT = 20;

        // define maximum gap of consecutive pixels in a line
        private int MAXIMUM_GAP = 5;

        // define Gaussian kernel size in the canny edge detection
        private int CANNY_EDGE_SIZE = 3;

        // define sigma value for the Gaussian kernel in the canny edge detection
        private int CANNY_SIGMA = 3;

        // define low threshold for canny edge detection
        private int LOW_TH = 50;

        // define high threshold for canny edge detection
        private int HIGH_TH = 100;

        // dictionary for contour tracing, to each direction there is the corresponding x and y shift of the index from the (0,0) pixel
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
