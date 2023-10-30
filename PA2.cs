using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV
{
    public partial class INFOIBV
    {
        /*
* createStructuringElement: takes as input the structure element shape(plus or square) and size and outputs
* the corresponding structure element. For the plus-shaped element, the result should be the same as an 
* iterative dilation of a 3x3 plus-shaped structuring element with a 3x3 plus-shaped structuring element.
* input:   shape               enum type of shape for SE
*          size                int size of the SE
*          binary              true if we want to create a SE for a binary image
* output:                      byte[,] matrix of SE
*/
        byte[,] createStructuringElement(int size, SEShape shape)
        {
            if (shape == SEShape.Square)
            {
                byte[,] SE = new byte[size, size];
                for (int r = 0; r < size; r++)
                    for (int c = 0; c < size; c++)
                        SE[r, c] = 1;                   // fill the square with ones and return
                return SE;
            }
            // Plus shaped 
            byte[,] tmp_SE = { { 1 } };                 // base element
            int tmp_SE_dim = 1;
            int n_repetition = (size - 1) / 2;          // compucte n of time to dilate
            for (int i = 0; i < n_repetition; i++)      //for n times
            {
                int new_SE_dim = tmp_SE_dim + 2;        //prepare dilation result
                byte[,] newSE = new byte[new_SE_dim, new_SE_dim];
                for (int r = 0; r < tmp_SE_dim; r++)
                    for (int c = 0; c < tmp_SE_dim; c++)
                    {
                        if (tmp_SE[r, c] == 0) continue; // dilate
                        newSE[r + 1, c + 1] = 1; // corresponding pixel
                        newSE[r + 2, c + 1] = 1; //pixel below
                        newSE[r, c + 1] = 1; //pixel over
                        newSE[r + 1, c + 2] = 1; // pixel on the right
                        newSE[r + 1, c] = 1; // pixel on the left
                    }
                tmp_SE = newSE;
                tmp_SE_dim = new_SE_dim;        // reassign result
            }
            return tmp_SE;
        }


        /*
        * erosion: takes as input an image and the structure element and returns the result of the erosion
        * input:   inputImage          single channel image
                   SE                  general Structural Element
                   binary              true if the input picture is a binary picture
        * output:                      byte[,] matrix of SE
        */
        byte[,] erodeImage(byte[,] inputImage, byte[,] SE, bool binary)
        {
            byte[,] temp = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            List<Tuple<int, int>> cords = new List<Tuple<int, int>>();
            List<byte> tmp;

            //Setup progress bar.
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // Collecting the values of the SE
            for (int x = 0; x < SE.GetLength(0); x++)
            {
                for (int y = 0; y < SE.GetLength(1); y++)
                {
                    if (SE[x, y] > 0)
                    {
                        cords.Add(new Tuple<int, int>(x - (SE.GetLength(0) / 2), y - (SE.GetLength(1) / 2)));
                    }
                }
            }

            //A check on if the image is binairy and followup on what to do in both cases.
            if (binary)
            {
                for (int x = 0; x < inputImage.GetLength(0); x++)
                    for (int y = 0; y < inputImage.GetLength(1); y++)
                    {
                        temp[x, y] = inputImage[x, y];

                        if (inputImage[x, y] == 0) continue;

                        for (int z = 0; z < cords.Count; z++)
                        {
                            if (x + cords[z].Item1 < 0 || y + cords[z].Item2 < 0) continue;

                            else if (x + cords[z].Item1 >= inputImage.GetLength(0) || y + cords[z].Item2 >= inputImage.GetLength(1))
                            {
                                continue;
                            }
                            if (inputImage[x + cords[z].Item1, y + cords[z].Item2] == 0)
                            {
                                temp[x, y] = 0;
                                break;
                            }


                        }
                        progressBar.PerformStep();
                    }

            }
            else
            {
                // Eroding the image using a double forloop
                for (int x = 0; x < inputImage.GetLength(0); x++)
                {
                    for (int y = 0; y < inputImage.GetLength(1); y++)
                    {
                        tmp = new List<byte>();
                        for (int z = 0; z < cords.Count; z++)
                        {
                            if (x + cords[z].Item1 < 0 || y + cords[z].Item2 < 0)
                            {
                                continue;
                            }
                            else if (x + cords[z].Item1 >= inputImage.GetLength(0) || y + cords[z].Item2 >= inputImage.GetLength(1))
                            {
                                continue;
                            }
                            else
                            {
                                tmp.Add(inputImage[x + cords[z].Item1, y + cords[z].Item2]);
                            }

                        }
                        temp[x, y] = tmp.AsQueryable().Min();
                        progressBar.PerformStep();
                    }
                }
            }

            progressBar.Visible = false;

            return temp;
        }


        /*
        * dilation: takes as input an image and the structure element and returns the result of the dilation
        * input:   inputImage          single channel image
                   SE                  general Structural Element
                   binary              true if the input picture is a binary picture
        * output:                      byte[,] matrix of SE
        */
        byte[,] dilateImage(byte[,] inputImage, byte[,] SE, bool binary)
        {
            byte[,] temp = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            List<Tuple<int, int>> cords = new List<Tuple<int, int>>();
            List<byte> tmp;

            //Setup progress bar.
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            //gathering the values of the SE.
            for (int x = 0; x < SE.GetLength(0); x++)
            {
                for (int y = 0; y < SE.GetLength(1); y++)
                {
                    if (SE[x, y] > 0)
                    {
                        cords.Add(new Tuple<int, int>(x - (SE.GetLength(0) / 2), y - (SE.GetLength(1) / 2)));
                    }
                }
            }

            //check if the image is bainary and the code for either case
            if (binary)
            {
                //double forlood to dialate the image
                if (!isBinary(inputImage)) throw new Exception("Image is not binary");
                for (int x = 0; x < inputImage.GetLength(0); x++)
                    for (int y = 0; y < inputImage.GetLength(1); y++)
                    {
                        if (inputImage[x, y] == 0) { continue; }

                        for (int z = 0; z < cords.Count; z++)
                        {
                            if (x + cords[z].Item1 < 0 || y + cords[z].Item2 < 0) continue;
                            else if (x + cords[z].Item1 >= inputImage.GetLength(0) || y + cords[z].Item2 >= inputImage.GetLength(1))
                            {
                                continue;
                            }
                            else temp[x + cords[z].Item1, y + cords[z].Item2] = 255;

                        }
                        progressBar.PerformStep();
                    }
            }
            else
            {
                // double forloop to dialte the image
                for (int x = 0; x < inputImage.GetLength(0); x++)
                {
                    for (int y = 0; y < inputImage.GetLength(1); y++)
                    {
                        tmp = new List<byte>();
                        for (int z = 0; z < cords.Count; z++)
                        {
                            if (x + cords[z].Item1 < 0 || y + cords[z].Item2 < 0)
                            {
                                continue;
                            }
                            else if (x + cords[z].Item1 >= inputImage.GetLength(0) || y + cords[z].Item2 >= inputImage.GetLength(1))
                            {
                                continue;
                            }
                            else
                            {
                                tmp.Add(inputImage[x + cords[z].Item1, y + cords[z].Item2]);
                            }

                        }
                        progressBar.PerformStep();
                        temp[x, y] = tmp.AsQueryable().Max();
                    }
                }
            }

            progressBar.Visible = false;

            return temp;
        }


        /*
        * countValues: takes an input image and returns the amount of distinct values and histogram of the values.
        * input:   inputImage          single channel image
        * output:                      int[] histogram of values
        *                              byte amount of distinct values.
        */
        Tuple<byte, List<int>> countValues(byte[,] inputImage)
        {
            //innitialising the values.
            List<int> hist = new List<int>();
            for (int i = 0; i < 256; i++) hist.Add(0);

            //Setup progress bar.
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            byte ammo = 0;
            // making all values in hist 0 so later it can be a simle addition method.
            for (int i = 0; i < 256; i++)
            {
                hist[i] = 0;
            }

            //going over every pixel and seeing it's value
            for (int x = 0; x < inputImage.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    if (hist[inputImage[x, y]] == 0) //if statement ot check if the value is anew distinct value.
                    {
                        ammo++;
                    }
                    hist[inputImage[x, y]]++;
                    progressBar.PerformStep();
                }
            }

            progressBar.Visible = false;

            return new Tuple<byte, List<int>>(ammo, hist);
        }


        /*
        * largest: takes an input bit image and return an image with only the largest item
        * input:   inputImage          single channel image
        * output:                      single channel image
        */
        byte[,] largest(byte[,] inputImage)
        {
            //first a floodfil to mark each diistinct item.
            byte[,] labels = floodFill(inputImage);

            //Setup progress bar.
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            //using countvalues and then making the array not have any logged for value 0 so the most common value, or the value 
            //of the largest object can be found and logged.
            (byte ammo, List<int> hist) = countValues(labels);
            hist[0] = 0;
            byte counter = (byte)hist.IndexOf(hist.Max());
            byte[,] temp = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];


            //double forloop that return a binairy image only showing the area with the value previously logged.
            for (int x = 0; x < inputImage.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    if (labels[x, y] == counter)
                    {
                        temp[x, y] = 255;
                    }
                    else
                    {
                        temp[x, y] = 0;
                    }
                    progressBar.PerformStep();
                }
            }

            progressBar.Visible = false;

            return temp;
        }


        /*
        * andImages: takes 2 input byte images and does a pointwise and funtion on each
        * input:   inputImage1         single channel bit image
        *          inputImage2         single channel bit image
        * output:                      single channel bit image
        */
        byte[,] andImages(byte[,] inputImage1, byte[,] inputImage2)
        {
            if (!isBinary(inputImage1)) throw new Exception("Image1 is not binary");
            if (!isBinary(inputImage2)) throw new Exception("Image2 is not binary");

            //Setup progress bar.
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            byte[,] temp = new byte[inputImage1.GetLength(0), inputImage1.GetLength(1)];

            // double forloop that generated a new image where only the values that are non 0 in both images get shown.
            for (int x = 0; x < inputImage1.GetLength(0); x++)
                for (int y = 0; y < inputImage1.GetLength(1); y++)
                {
                    byte p1 = (byte)inputImage1[x, y];
                    byte p2 = (byte)inputImage2[x, y];
                    if (inputImage1[x, y] != 0 && inputImage2[x, y] != 0)
                    {
                        temp[x, y] = inputImage1[x, y];
                    }
                    else
                    {
                        temp[x, y] = 0;
                    }
                    progressBar.PerformStep();
                }

            progressBar.Visible = false;

            return temp;
        }


        /*
        * orImages: takes 2 input byte images and does a pointwise or funtion on each
        * input:   inputImage1         single channel bit image
        *          inputImage2         single channel bit image
        * output:                      single channel bit image
        */
        byte[,] orImages(byte[,] inputImage1, byte[,] inputImage2)
        {
            if (!isBinary(inputImage1)) throw new Exception("Image1 is not binary");
            if (!isBinary(inputImage2)) throw new Exception("Image2 is not binary");

            //Setup progress bar.
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            byte[,] temp = new byte[inputImage1.GetLength(0), inputImage1.GetLength(1)];

            //double forloop that generates the new image, only showing he pixels that are non 0 in either of the images
            for (int x = 0; x < inputImage1.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage1.GetLength(1); y++)
                {
                    byte p1 = (byte)inputImage1[x, y];
                    byte p2 = (byte)inputImage2[x, y];
                    if (inputImage1[x, y] != 0 || inputImage2[x, y] != 0)
                    {
                        temp[x, y] = 255;
                    }
                    else
                    {
                        temp[x, y] = 0;
                    }
                    progressBar.PerformStep();
                }

            }

            progressBar.Visible = false;

            return temp;
        }


        /*
        * openImage: takes as input an image and the structure element and returns the result of the opening operation
        * input:   inputImage          single channel image
                   SE                  general Structural Element
                   binary              true if the input picture is a binary picture
        * output:                      byte[,] matrix of SE
        */
        byte[,] openImage(byte[,] inputImage, byte[,] SE, bool binary)
        {
            return dilateImage(erodeImage(inputImage, SE, binary), SE, binary); // erode image, than dilate
        }

        /*
        * closeImage: takes as input an image and the structure element and returns the result of the closing operation
        * input:   inputImage          single channel image
                   SE                  general Structural Element
                   binary              true if the input picture is a binary picture
        * output:                      byte[,] matrix of SE
        */
        byte[,] closeImage(byte[,] inputImage, byte[,] SE, bool binary)
        {
            return erodeImage(dilateImage(inputImage, SE, binary), SE, binary); // dilate image, then erode
        }
        //Boundary trace: implement a function(traceBoundary) that, given a binary image, 
        //    traces the outer boundary of a foreground shape in that image.The output of 
        //        the function is a list (choose your implementation) of(x, y)-pairs, each
        //    corresponding to a boundary pixel. Subsequent pairs in the list should be neighboring
        //        pixels in the image. Have a look at Book II - Chapter 2 and the code in 
        //        Book II - B.1.4. You may assume there is only a single shape in the image,
        //        or return only the list of the first boundary that you encounter. (10 points).

        List<int> traceBoundary(byte[,] inputImage)
        {
            (int, int) pixel = (-10, -10);
            int direction = 6;
            List<int> boundary = new List<int>();


            for (int r = 0; r < inputImage.GetLength(1); r++)
            {
                bool stop = false;
                for (int c = 0; c < inputImage.GetLength(0); c++)
                    if (inputImage[c, r] == 255)
                    {
                        pixel = (r, c); // find the first pixel in the area
                        stop = true;
                        break;
                    }
                if (stop) break;
            }
            (int, int) end_pixel = (-10, -10);      // end pixel and end direction are there for exiting the function whant the tracing is complete
            int ending_direction = -1;
            int dir_to_look;
            int coming_direction = -1;
            for (int i = 0; i < 8; i++)             // look for the next pixel to set as the ending pixel together with the direction
            {
                dir_to_look = (direction + i) % 8;  // compute direction
                (int, int) movement_matrix_r_c = contourDict[dir_to_look];
                int r = pixel.Item1 + movement_matrix_r_c.Item2;
                int c = pixel.Item2 + movement_matrix_r_c.Item1; // compute new coords
                if (r < 0 || r > inputImage.GetLength(1) || c < 0 || c > inputImage.GetLength(0)) continue; //check validity
                if (inputImage[c, r] == 0) continue; // if pixel is bg it is not part of the boundary
                if (inputImage[c, r] != 255) throw new Exception("Image is not binary");
                boundary.Add(dir_to_look);          // second pixel found, storing direction and end_pixel
                ending_direction = dir_to_look;
                coming_direction = ending_direction;
                direction = (dir_to_look + 6) % 8;
                pixel = (r, c);
                end_pixel = (r, c);
                break;
            }
            if (ending_direction == -1) return boundary; // single pixel instead of an area

            int cont = 0;
            while (true)
            {
                if (end_pixel == pixel && ending_direction == coming_direction) // first time the condition is true is because contour tracing just started
                {
                    if (cont == 0) cont = 1;
                    else
                    {
                        boundary.Remove(boundary.ElementAt(boundary.Count() - 1)); // second time the condition is true we drop the last element, equal to the first one, and return
                        return boundary;
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    dir_to_look = (direction + i) % 8; // compute direction to look at
                    (int, int) movement_matrix_r_c = contourDict[dir_to_look];
                    int r = pixel.Item1 + movement_matrix_r_c.Item2;
                    int c = pixel.Item2 + movement_matrix_r_c.Item1; // compute new coord
                    if (r < 0 || r >= inputImage.GetLength(1) || c < 0 || c >= inputImage.GetLength(0)) continue; // check validity
                    if (inputImage[c, r] == 0) continue; // if the pixel is not fg it is not boundary
                    if (inputImage[c, r] != 255) throw new Exception("Image is not binary");
                    boundary.Add(dir_to_look); // add pixel to contour and update direction
                    direction = (dir_to_look + 6) % 8;
                    pixel = (r, c);
                    break;
                }
            }
        }


        /*
        * floodFill: takes as input a binary image and returns a new image with 0 value for bg pixel and id value for pixels labeled with id
        * input:   inputImage          single channel binary image
        * output:                      byte[,] matrix with pixel label
        */
        byte[,] floodFill(byte[,] inputImage)
        {
            if (!isBinary(inputImage)) throw new Exception("Image is not binary");
            byte id = 1;   // initialize label
            byte[,] res = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            for (int r = 0; r < inputImage.GetLength(1); r++)
                for (int c = 0; c < inputImage.GetLength(0); c++)
                {
                    if (inputImage[c, r] == 0 || res[c, r] != 0) continue; // looking for fg unlabeld pixels
                    Queue<(int, int)> q = new Queue<(int, int)>();
                    q.Enqueue((c, r));
                    while (q.Any()) // start labeling new area
                    {
                        (int col, int row) = q.Dequeue(); // get pixel coords
                        if (col < 0 || col >= inputImage.GetLength(0) || row < 0 || row >= inputImage.GetLength(1))
                        {       // check if coords are valid
                            continue;
                        }
                        if (inputImage[col, row] == 0 || res[col, row] == id)
                        { // skip bg or already labeled pixels
                            continue;
                        }
                        res[col, row] = id; // label and add neighbours to the queue
                        q.Enqueue((col + 1, row));
                        q.Enqueue((col - 1, row));
                        q.Enqueue((col, row + 1));
                        q.Enqueue((col, row - 1));
                    }
                    id++; // when labeling is finished, change label number
                }
            return res;
        }
    }
}
