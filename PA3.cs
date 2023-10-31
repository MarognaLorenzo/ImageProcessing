using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV
{
    public partial class INFOIBV
    {
        /*
* houghTransform: a function that takes an image and returns the r-theta image that corresponds to it.
 * input: inputImage        Single channel image.
 * output                   Single channel image.
*/
        int[,] houghTransform(byte[,] inputImage)
        {
            int x_off = (inputImage.GetLength(0) / 2), y_off = (inputImage.GetLength(1) / 2);
            double maxR = Math.Sqrt((x_off * x_off) + (y_off * y_off));
            int[,] newImg2 = new int[501, 501];
            for (int r = 0; r < newImg2.GetLength(0); r++)
                for (int c = 0; c < newImg2.GetLength(1); c++)
                {
                    newImg2[r, c] = 0;
                }
            IDictionary<int, (double sin, double cos)> sincosTable = new Dictionary<int, (double, double)>();
            for (int t = 0; t <= 500; t++)
            {
                double T = t * Math.PI / 500;
                sincosTable[t] = (Math.Sin(T), Math.Cos(T));
            }

            for (int r = 1; r < inputImage.GetLength(0) - 1; r++)
            {
                for (int c = 1; c < inputImage.GetLength(1) - 1; c++) // for every pixel
                {
                    if (inputImage[r, c] != 0)
                    {
                        for (int t = 0; t <= 500; t++) // for every possible angle
                        {
                            double R = (r - x_off) * sincosTable[t].cos + ((y_off - c) * sincosTable[t].sin); // find corresponding radius
                            R += maxR;
                            int r2 = (int)(R * 500 / (maxR * 2));

                            newImg2[t, r2] += inputImage[r, c]; // increment accumulator

                            if (r2 >= 1)
                                newImg2[t, r2 - 1] += inputImage[r, c];// increment value with radius +- 1

                            if (r2 < newImg2.GetLength(1) - 1)
                                newImg2[t, r2 + 1] += inputImage[r, c];
                        }
                    }
                }
            }
            return newImg2;
        }


        List<Line> peakFinding(byte[,] inputImage, double threshold)
        {
            double maxR = Math.Sqrt(Math.Pow(inputImage.GetLength(0), 2) + Math.Pow(inputImage.GetLength(1), 2)); // half diagonal of the image

            int[,] transf = houghTransform(inputImage);
            byte[,] contrast = adjustContrast(transf);// to get value in range 0 - 255
            byte[,] thresholded = thresholdImage(contrast, (int)(threshold * 255));
            byte[,] close = closeImage(thresholded, createStructuringElement(CLOSE_DIM, SEShape.Square), isBinary(thresholded));
            byte[,] flood = floodFill(close); // label all regions with an incremental ID

            IDictionary<int, (int count, int sumx, int sumy)> map = new Dictionary<int, (int, int, int)>();// data structure for finding centroids x and y position

            for (int c = 0; c < flood.GetLength(0); c++) // find centroids
                for (int r = 0; r < flood.GetLength(1); r++)
                {
                    if (flood[c, r] == 0) continue; // bg pixels are not important
                    (int a, int b, int c) elem = (0, 0, 0);
                    if (map.ContainsKey(flood[c, r])) elem = map[flood[c, r]];
                    elem = (elem.a + 1, elem.b + c, elem.c + r);
                    map[flood[c, r]] = elem;
                }


            List<Line> lines = new List<Line>();
            foreach (var kv in map)// for each kea value in the dictionary with centroids sums and count of pixels get the average and add it to the result
            {
                double T = ((double)kv.Value.sumx) / kv.Value.count * 180 / 500;
                double R = (((double)kv.Value.sumy) / kv.Value.count * maxR / 500) - maxR / 2;
                Console.WriteLine("R:" + R + " - T: " + T + "count:" + kv.Value.count);
                lines.Add(new Line(R, T));
            }
            return lines;
        }


        /*
        * hough_line_detection: takes as input a single channel image and return true only if all values of the image are either 0 or 255
        * input:   inputImage                       single channel  image
        *          r                                radius of the segment from origin
        *          theta                            angle of the segment
        *          minimum_intensity_threshold      
        *          minimum_lengh                    
        *          maximum_gap                      
        * output:                                   list of line segmentsToAdd
        */
        (List<Segment>, List<(int, int)> pixel_to_check) hough_line_detection(byte[,] inputImage, double radius, double theta, int minimum_intensity_threshold, int minimum_lenght, int maximum_gap)
        {

            List<Segment> segment_list = new List<Segment>();
            HashSet<(int, int)> pixels_in_line_set = new HashSet<(int, int)>();
            bool binary = isBinary(inputImage);

            (int c, int r) image_center = (inputImage.GetLength(0) / 2, inputImage.GetLength(1) / 2);
            double cos_theta = Math.Cos(degree_to_rad(theta));
            double sen_theta = Math.Sin(degree_to_rad(theta));

            // I get to the point which is surely present in the image ( the distance point between the origin and the line)
            (double x, double y) math_starting_point = (radius * cos_theta, radius * sen_theta);

            double alpha = theta - 90; // getting the vector - direction of the line to calculate step increments to "walk" over the line until the end of the image, in both directions


            (double x, double y) = math_starting_point;
            double x_increment = HOUGH_STEP * Math.Cos(degree_to_rad(alpha));
            double y_increment = HOUGH_STEP * Math.Sin(degree_to_rad(alpha));
            int strikes = 0;
            while (true)
            {
                if (!(x < image_center.c && x > -image_center.c && y < image_center.r && y > -image_center.r)) // I need to stay in the image
                {
                    if (strikes == 1) break; // Once I get at the end of the image I need to restart from the starting point and go in the other direction
                    strikes++;
                    alpha += 180; // not really used but for completness
                    (x, y) = math_starting_point;
                    x_increment *= -1;
                    y_increment *= -1;
                    continue;
                }

                pixels_in_line_set.Add(math_to_image(image_center, (x, y < 0 ? y : y + 1)));
                //Console.WriteLine((x ,y));
                x += x_increment;
                y += y_increment;
            }

            List<(int, int)> pixels_in_line_list = pixels_in_line_set.ToList();
            pixels_in_line_list.Sort(); // it gets sorted on first element of the tuple first


            // Walk through the line from left to right (its sorted) with pen "up" or "down" (tracking state == true -> pen down) depending on the state. State can change depending on pixel intensity and parameters
            bool tracking_state = false; // knows if I am tracking a segment or looking for a new one
            int gap_cont = 0; // checks how long have I been in a gap for
            int seg_cont = 0; // checks how many pixels are in my segment
            Segment new_segment = new Segment();
            new_segment.clear();
            foreach ((int c, int r) line_pixel in pixels_in_line_list)
            {
                //inputImage[line_pixel.c, line_pixel.r] = 255;

                if (inputImage[line_pixel.c, line_pixel.r] >= (binary ? 255 : minimum_intensity_threshold))
                {       // pixel on
                    if (!tracking_state)
                    { // starting a new segment
                        new_segment.start = new_segment.end = line_pixel;
                        seg_cont = 1;
                        gap_cont = 0;
                        tracking_state = true;
                    }
                    else
                    {       // Continue an existing segment
                        new_segment.end = (line_pixel.c, line_pixel.r);
                        seg_cont++;
                        gap_cont = 0;
                    }

                }
                else
                {       //pixel off
                    if (!tracking_state) continue; // keep looking
                    else if (++gap_cont >= maximum_gap)
                    {
                        tracking_state = false;// close a segment
                        if (seg_cont > minimum_lenght) segment_list.Add(new_segment);
                        gap_cont = 0;
                        seg_cont = 0;
                        new_segment.clear();
                        new_segment.set_start(line_pixel);
                        continue;
                    }

                }
            }
            if (tracking_state && seg_cont > minimum_lenght) segment_list.Add(new_segment);

            return (segment_list, pixels_in_line_list);
        }

        /*
        * hough_visualization: takes as input a single channel image and a list of detected segmentsToAdd and colors in red the segment in a bitmap
        * input:   inputImage                       single channel  image
        *          segmentsToAdd                         the list of detected segmentsToAdd          
        * output:                                   Bitmap with red segmentsToAdd on it
        */

        void hough_visualization(List<Segment> segments, ref Bitmap inputImage, List<(int c, int r)> pixel_in_line, Color color)
        {
            foreach (Segment segment in segments)// for each segment
            {
                bool start = false;
                foreach (var pixel in pixel_in_line)//for each pixel in the line walk through the line, when I meet the start of the segment I start drawing until I find the end of the segment
                {
                    if (pixel == segment.start) start = true;
                    else if (start) inputImage.SetPixel(pixel.c, pixel.r, color);
                    if (pixel == segment.end) break;
                }
            }
        }


        /*
        * hough_crossing_line: takes as input a single channel image and a list of lines and give back a list of pairs (c, r) of crossing points
        * input:   inputImage                       single channel  image
        *          r-theta pairs                    list of detected r   
        * output:                                   A List with the coords of crossing points of different lines
        */
        List<PixelPoint> hough_crossing_line(List<Line> lines, byte[,] inputImage)
        {
            (int c, int r) image_center = (inputImage.GetLength(0) / 2, inputImage.GetLength(1) / 2);
            List<PixelPoint> res = new List<PixelPoint>();

            for (int i = 0; i < lines.Count; i++)
            {
                Line line1 = lines[i];
                for (int j = i + 1; j < lines.Count; j++) // for every combination of lines
                {
                    Line line2 = lines[j];
                    if (line1.theta == line2.theta) continue; // parallel lines do not have crossing points

                    // Detect crossing point
                    double cos1 = Math.Cos(degree_to_rad(line1.theta));
                    double cos2 = Math.Cos(degree_to_rad(line2.theta));
                    double sin1 = Math.Sin(degree_to_rad(line1.theta));
                    double sin2 = Math.Sin(degree_to_rad(line2.theta));
                    double r1 = line1.r;
                    double r2 = line2.r;

                    double x;
                    double y;

                    if (sin1 != 0 && sin2 != 0) // vertical lines are not functions so they don't solve the system of equaations between 2 lines, we take care of them individually
                    {
                        x = ((r2 / sin2) - (r1 / sin1)) / ((cos2 / sin2) - (cos1 / sin1)); // solution of the system of equations between 2 lines
                        y = (r1 - x * cos1) / sin1;
                    }
                    else
                    {
                        if (sin1 == 0)
                        {
                            x = r1;
                            y = (r2 - x * cos2) / sin2;
                        }
                        else
                        {
                            x = r2;
                            y = (r1 - x * cos1) / sin1;
                        }
                    }

                    (int c, int r) pixel_coord = math_to_image(image_center, (x, (y < 0 ? y : y + 1))); // finding pixel coords in the image and check if the pixel is inside the boundaries of the image
                    if (pixel_coord.c < 0 || pixel_coord.c >= inputImage.GetLength(0) || pixel_coord.r < 0 || pixel_coord.r >= inputImage.GetLength(1)) continue;

                    res.Add(new PixelPoint(pixel_coord.c, pixel_coord.r));

                }
            }

            return res;

        }

        void hough_visualize_crossing(List<PixelPoint> crossing_coords, ref Bitmap inputImage)
        {
            foreach (PixelPoint point in crossing_coords) // visualize every crossing point with a plus-shaped marker
            {
                if (point.x < 0 || point.x >= inputImage.Width || point.y < 0 || point.y >= inputImage.Height) continue;

                if (inputImage.GetPixel(point.x, point.y).R != 255) continue;
                inputImage.SetPixel(point.x, point.y, Color.Green);
                if (point.x + 1 < inputImage.Width) inputImage.SetPixel(point.x + 1, point.y, Color.Green);
                if (point.x - 1 >= 0) inputImage.SetPixel(point.x - 1, point.y, Color.Green);
                if (point.y + 1 < inputImage.Height) inputImage.SetPixel(point.x, point.y + 1, Color.Green);
                if (point.y - 1 >= 0) inputImage.SetPixel(point.x, point.y - 1, Color.Green);
            }
        }
        void hough_visualize_crossing(List<PixelPoint> crossing_coords, ref Bitmap inputImage, bool only_on_segment, Color color)
        {
            foreach (PixelPoint point in crossing_coords) // visualize every crossing point with a plus-shaped marker
            {
                if (point.x < 0 || point.x >= inputImage.Width || point.y < 0 || point.y >= inputImage.Height) continue;

                if (only_on_segment && inputImage.GetPixel(point.x, point.y).R != 255) continue;
                inputImage.SetPixel(point.x, point.y, color);
                if (point.x + 1 < inputImage.Width) inputImage.SetPixel(point.x + 1, point.y, color);
                if (point.x - 1 >= 0) inputImage.SetPixel(point.x - 1, point.y, color);
                if (point.y + 1 < inputImage.Height) inputImage.SetPixel(point.x, point.y + 1, color);
                if (point.y - 1 >= 0) inputImage.SetPixel(point.x, point.y - 1, color);
            }
        }

        /*
         * houghTransform: a function that takes an image and returns the r-theta image that corresponds to it.
         * input: inputImage        Single channel image.
         *        theta1            first degree in degree in range 0--180
         * output                   second degree in degree.
         */
        int[,] houghAngleLimit(byte[,] inputImage, double theta1, double theta2)
        {
            int x_off = (int)(inputImage.GetLength(0) / 2), y_off = (int)(inputImage.GetLength(1) / 2);
            double maxR = Math.Sqrt((x_off * x_off) + (y_off * y_off));
            int[,] newImg2 = new int[501, 501];
            for (int r = 0; r < newImg2.GetLength(0); r++)
                for (int c = 0; c < newImg2.GetLength(1); c++)
                {
                    newImg2[r, c] = 0;
                }
            IDictionary<int, (double sin, double cos)> sincosTable = new Dictionary<int, (double, double)>();
            for (int t = 0; t <= 500; t++)
            {
                double T = t * Math.PI / 500;
                sincosTable[t] = (Math.Sin(T), Math.Cos(T));
            }
            int T1 = (int)(theta1 * 500 / 180);
            int T2 = (int)(theta2 * 500 / 180);

            for (int r = 0; r < inputImage.GetLength(0); r++)
            {
                for (int c = 0; c < inputImage.GetLength(1); c++) // for every pixel
                {
                    if (inputImage[r, c] != 0)
                    {
                        for (int t = T1; t <= T2; t++) // for every possible angle between corresponding angles theta 1 and theta2 in the new range
                        {
                            double R = (r - x_off) * sincosTable[t].cos + ((y_off - c) * sincosTable[t].sin); // find corresponding radius
                            R += maxR;
                            int r2 = (int)(R * 500 / (maxR * 2));
                            newImg2[t, r2] += inputImage[r, c]; // increment accumulator

                            if (r2 >= 1)
                                newImg2[t, r2 - 1] += inputImage[r, c];// increment value with radius +- 1

                            if (r2 < newImg2.GetLength(1))
                                newImg2[t, r2] += inputImage[r, c];
                        }
                    }
                }
            }
            return newImg2;
        }

        int[,] HoughCircles(byte[,] inputImage, int r)
        {
            int[,] temp = new int[inputImage.GetLength(0), inputImage.GetLength(1)];
            Tuple<int, int> last = null;
            int x, y, max = 1;
            for (int i = 0; i < inputImage.GetLength(0); i++)
                for (int j = 0; j < inputImage.GetLength(1); j++)
                {
                    temp[i, j] = 0;
                }
            for (int i = 0; i < inputImage.GetLength(0); i++)
                for (int j = 0; j < inputImage.GetLength(1); j++)
                {
                    if (inputImage[i, j] > 0)
                    {
                        last = null;
                        for (int a = 0; a < 360; a++)
                        {
                            x = (int)(r * Math.Cos(a * Math.PI / 180));
                            x += i;
                            y = (int)(r * Math.Sin(a * Math.PI / 180));
                            y += j;
                            if (last == null)
                            {
                                if ((x < 0) || (x > temp.GetLength(0) - 1) || (y < 0) || (y > temp.GetLength(1) - 1))
                                {

                                }
                                else
                                {
                                    temp[x, y]++;
                                    if (max < temp[x, y])
                                    {
                                        x = temp[x, y];
                                    }
                                    last = new Tuple<int, int>(x, y);
                                }
                            }
                            else if ((x == last.Item1 && y == last.Item2) || (x < 0) || (x > temp.GetLength(0) - 1) || (y < 0) || (y > temp.GetLength(1) - 1))
                            {
                                continue;
                            }
                            else
                            {
                                temp[x, y]++;
                                if (max < temp[x, y])
                                {
                                    x = temp[x, y];
                                }
                                last = new Tuple<int, int>(x, y);
                            }
                        }
                    }

                }
            return temp;
        }

        List<PixelPoint> findCentroids(byte[,] flood)
        {
            IDictionary<int, (int count, int sumx, int sumy)> map = new Dictionary<int, (int, int, int)>();// data structure for finding centroids x and y position
            for (int c = 0; c < flood.GetLength(0); c++) // find centroids
                for (int r = 0; r < flood.GetLength(1); r++)
                {
                    if (flood[c, r] == 0) continue; // bg pixels are not important
                    (int a, int b, int c) elem = (0, 0, 0);
                    if (map.ContainsKey(flood[c, r])) elem = map[flood[c, r]];
                    elem = (elem.a + 1, elem.b + c, elem.c + r);
                    map[flood[c, r]] = elem;
                }


            List<PixelPoint> centroids = new List<PixelPoint>();
            foreach (var kv in map)// for each kea value in the dictionary with centroids sums and count of pixels get the average and add it to the result
            {
                int x = kv.Value.sumx / kv.Value.count;
                int y = kv.Value.sumy / kv.Value.count;
                Console.WriteLine("Blob: x: " + x + " y: " + y);
                centroids.Add(new PixelPoint(x, y));
            }
            return centroids;
        }
    }
}
