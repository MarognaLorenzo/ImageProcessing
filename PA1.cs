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
         * convertToGrayScale: convert a three-channel color image to a single channel grayscale image
         * input:   inputImage          three-channel (Color) image
         * output:                      single-channel (byte) image
         */
        private byte[,] convertToGrayscale(Color[,] inputImage)
        {
            // create temporary grayscale image of the same size as input, with a single channel
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < InputImage1.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage1.Size.Height; y++)            // loop over rows
                {
                    Color pixelColor = inputImage[x, y];                    // get pixel color
                    byte average = (byte)((pixelColor.R + pixelColor.B + pixelColor.G) / 3); // calculate average over the three channels
                    tempImage[x, y] = average;                              // set the new pixel color at coordinate (x,y)
                    progressBar.PerformStep();                              // increment progress bar
                }

            progressBar.Visible = false;                                    // hide progress bar

            return tempImage;
        }

        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 1 GO HERE ==============
        // ====================================================================
        private byte[,] invertImage(byte[,] inputImage)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                {
                    // get pixel color
                    tempImage[x, y] = (byte)(255 - inputImage[x, y]);      // invert pixel
                    progressBar.PerformStep();                              // increment progress bar
                }

            progressBar.Visible = false;                                    // hide progress bar

            return tempImage;
        }

        private byte[,] brightImage(byte[,] inputImage, int value)
        {
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                {
                    // get pixel color
                    tempImage[x, y] = (byte)(Math.Min(255,Math.Max(0, inputImage[x, y] + value)));      // add or take down
                    progressBar.PerformStep();                              // increment progress bar
                }

            progressBar.Visible = false;                                    // hide progress bar

            return tempImage;
        }


        /*
         * adjustContrast: create an image with the full range of intensity values used
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        private byte[,] adjustContrast(byte[,] inputImage)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];


            // Calculating the histogram

            int[] histogram = new int[256];
            for (int a = 0; a < histogram.Length; a++) histogram[a] = 0;
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                    histogram[inputImage[x, y]]++;

            // Calculating actual contrast
            int lower_bound = 0;
            while (histogram[lower_bound] == 0 && lower_bound < 256) lower_bound++;
            int upper_bound = 255;
            while (histogram[upper_bound] == 0 && upper_bound >= 0) upper_bound--;

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = inputImage.GetLength(0) * inputImage.GetLength(1);
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                {
                    // get pixel color
                    tempImage[x, y] = (byte)((inputImage[x, y] - lower_bound) * ((float)255 / (float)(upper_bound - lower_bound)));
                    progressBar.PerformStep();                              // increment progress bar
                }
            for (int a = 0; a < histogram.Length; a++) histogram[a] = 0;
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                    histogram[tempImage[x, y]]++;
            progressBar.Visible = false;

            return tempImage;
        }

        /*
 * adjustContrast: create an image with the full range of intensity values used
 * input:   inputImage          single-channel (byte) image
 * output:                      single-channel (byte) image
 */
        private byte[,] adjustContrast(int[,] inputImage)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            int max_value = 0;
            for (int x = 0; x < inputImage.GetLength(0); x++)
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    max_value = Math.Max(max_value, inputImage[x, y]);
                }
            max_value += 2;


            // Calculating the histogram

            int[] histogram = new int[max_value];
            for (int a = 0; a < histogram.Length; a++) histogram[a] = 0;
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                    histogram[inputImage[x, y]]++;

            // Calculating actual contrast
            int lower_bound = 0;
            while (histogram[lower_bound] == 0 && lower_bound < max_value) lower_bound++;
            int upper_bound = max_value;

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = inputImage.GetLength(0) * inputImage.GetLength(1);
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                {
                    // get pixel color
                    tempImage[x, y] = (byte)((inputImage[x, y] - lower_bound) * ((float)256 / (float)(upper_bound - lower_bound)));
                    progressBar.PerformStep();                              // increment progress bar
                }
            histogram = new int[256];
            for (int a = 0; a < histogram.Length; a++) histogram[a] = 0;
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                    histogram[tempImage[x, y]]++;
            progressBar.Visible = false;

            return tempImage;
        }

        /*
         * createGaussianFilter: create a Gaussian filter of specific square size and with a specified sigma
         * input:   size                length and width of the Gaussian filter (only odd sizes)
         *          sigma               standard deviation of the Gaussian distribution
         * output:                      Gaussian filter
         */
        private float[,] createGaussianFilter(int size, float sigma)
        {
            // create temporary grayscale image
            float[,] filter = new float[size, size];
            byte half = (byte)(size / 2);
            float sum = 0;
            for (byte x = 0; x < size; x++)
            {
                for (byte y = 0; y < size; y++)
                {
                    filter[x, y] = (float)Math.Exp(
                        -(
                        (Math.Pow(x - half, 2) + Math.Pow(y - half, 2))
                        / (2 * Math.Pow(sigma, 2))));
                    sum += filter[x, y];
                }
            }
            for (byte x = 0; x < size; x++)
                for (byte y = 0; y < size; y++)
                    filter[x, y] /= sum;

            return filter;
        }


        /*
         * convolveImage: apply linear filtering of an input image
         * input:   inputImage          single-channel (byte) image
         *          filter              linear kernel
         *          convolve            boolean option (true = convolution / false = correlation)
         * output:                      single-channel (byte) image
         */
        private byte[,] convolveImage(byte[,] inputImage, float[,] filter, bool convolve)
        {
            // Kernel processing
            int M = inputImage.GetLength(0); //Width
            int N = inputImage.GetLength(1); // Height
            int k_dim = kernel_processing(filter);
            int hotspot = (k_dim - 1) / 2; // Center of the kernel

            // kernel 180 rotation
            if (convolve) rotate_filter(filter, k_dim);


            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            //For out of the border Pixels
            float mean_grey_value = calc_mean_value(inputImage);

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            //Correlation filtering
            for (int row = 0; row < N; row++)
                for (int col = 0; col < M; col++)
                {
                    // Apply filter on single pixel
                    float h_sum = 0; // sum all the partial values of the kernel on the image

                    for (int frow = -hotspot; frow <= hotspot; frow++)// for each coordinate of the kernel
                        for (int fcol = -hotspot; fcol <= hotspot; fcol++)
                        {
                            int r_tot = row + frow; // computing position of the pixel around hotspot
                            int c_tot = col + fcol;

                            h_sum += filter[frow + hotspot, fcol + hotspot] *  // sum the value to h_sum (if pixel is out of bound we use mean gray value)
                                ((r_tot < 0 || r_tot >= N || c_tot < 0 || c_tot >= M) ? mean_grey_value : inputImage[c_tot, r_tot]);
                        }
                    h_sum = Math.Max(Math.Min(h_sum, 255), 0); // map values out of range
                    tempImage[col, row] = (byte)h_sum; // assign the value
                    progressBar.PerformStep();
                }
            progressBar.Visible = false;
            return tempImage;
        }


        /*
        * rotate_filter: apply mirroring on vertical and horizontal axis on a filter.
        * input:   filter              odd size squared filter of floats
        *          k_dim               filter dimension
        */
        void rotate_filter(float[,] filter, int k_dim)
        {
            float swap;
            int hotspot = (k_dim - 1) / 2;
            for (int r = 0; r <= hotspot; r++)
                for (int c = 0; c < k_dim; c++)
                {
                    swap = filter[c, r];
                    filter[c, r] = filter[k_dim - c - 1, k_dim - r - 1];
                    filter[k_dim - c - 1, k_dim - r - 1] = swap;
                }
        }


        /*
        * calc_mean_value: computes the mean grey value of an image.
        * input:   inputImage          single channel (byte) image
        * output:  float               mean_value intensity (float)
        */
        private float calc_mean_value(byte[,] inputImage)
        {
            int M = inputImage.GetLength(0);
            int N = inputImage.GetLength(1);
            float sum = 0;
            for (int i = 0; i < M; i++)
                for (int j = 0; j < N; j++)
                    sum += inputImage[i, j];
            float res = sum / (N * M);
            return res;
        }


        /*
         * medianFilter: apply median filtering on an input image with a kernel of specified size
         * input:   inputImage          single-channel (byte) image
         *          size                length/width of the median filter kernel
         * output:                      single-channel (byte) image
         */
        private byte[,] medianFilter(byte[,] inputImage, byte size)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            byte half = (byte)(size / 2), fx, fy;
            byte[] ar;
            int counter;

            //Setup progress bar.
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            //double forloop for functionality.
            for (int x = 0; x < inputImage.GetLength(0); x++)
            {
                //boundries of the area for which the median will be decided.
                if (x < half)
                {
                    fx = (byte)(size - half + x);
                }
                else if (x >= inputImage.GetLength(0) - half)
                {
                    fx = (byte)(size - (half + x - inputImage.GetLength(0) - 1));
                }
                else
                {
                    fx = size;
                }
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    if (y < half)
                    {
                        fy = (byte)(size - half + y);
                    }
                    else if (y >= inputImage.GetLength(1) - half)
                    {
                        fy = (byte)(size - (half + y - inputImage.GetLength(1) - 1));
                    }
                    else
                    {
                        fy = size;
                    }
                    ar = new byte[(fx * fy)];
                    counter = 0;
                    //double forloop, collect values within searcharea.
                    for (int l = -half; l <= half; l++)
                    {
                        for (int h = -half; h <= half; h++)
                        {
                            if (x < -l || y < -h)
                            {
                                continue;
                            }
                            else if (x + l >= inputImage.GetLength(0) || y + h >= inputImage.GetLength(1))
                            {
                                continue;
                            }
                            else
                            {
                                ar[counter] = inputImage[(x + l), (y + h)];
                                counter++;
                            }
                        }
                    }

                    //Sort array with the values from the searcharea and determine the median
                    Array.Sort(ar);
                    tempImage[x, y] = ar[ar.Length / 2];
                    progressBar.PerformStep();
                }
            }

            progressBar.Visible = false;
            return tempImage;
        }


        /*
         * Normalise: Equalises the histogram of an image.
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        private byte[,] normalize(byte[,] inputImage)
        {
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            int pixls = inputImage.GetLength(0) * inputImage.GetLength(1);
            int[] pix = new int[256]; //pix: Array used to represent the histgram.

            //Setup progress bar.
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            //orloop to set all values in pix to 0.
            for (int i = 0; i < 256; i++)
            {
                pix[i] = 0;
            }

            //Double for-loop to collect the values for the histogram.
            for (int x = 0; x < inputImage.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    pix[inputImage[x, y]]++;
                }
            }

            //Two consecutive for-loops to turn pix into a cumulative histogram.
            for (int i = 0; i < 256; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                else
                {
                    pix[i] += pix[i - 1];
                }
            }
            for (int i = 0; i < 256; i++)
            {
                pix[i] = (pix[i] * 255) / pixls;
            }

            //double for-loop yo generate the new image.
            for (int x = 0; x < inputImage.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    tempImage[x, y] = (byte)(pix[inputImage[x, y]]);
                    progressBar.PerformStep();

                }
            }

            progressBar.Visible = false;

            return tempImage;
        }


        /*
         * edgeMagnitude: calculate the image derivative of an input image and a provided edge kernel
         * input:   inputImage          single-channel (byte) image
         *          horizontalKernel    horizontal edge kernel
         *          virticalKernel      vertical edge kernel
         * output:                      single-channel (byte) image
         */
        private byte[,] edgeMagnitude(byte[,] inputImage, sbyte[,] horizontalKernel, sbyte[,] verticalKernel)
        {
            // Kernel processing
            int M = inputImage.GetLength(0); //Width
            int N = inputImage.GetLength(1); // Height

            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            //kernel processing
            float[,] hKernel = convertKernel(horizontalKernel);
            float[,] vKernel = convertKernel(verticalKernel);
            int k_dim = kernel_processing(hKernel);
            if (k_dim != kernel_processing(vKernel)) throw new ArgumentException(" Kernels has to be the same size");

            int hotspot = (k_dim - 1) / 2; // Center of the filter

            //For piels out of the border
            float mean_grey_value = calc_mean_value(inputImage);

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;


            //Correlation filtering
            for (int row = 0; row < N; row++)
                for (int col = 0; col < M; col++)
                {
                    // Apply filter on single pixel
                    float h_sum = 0; // sum all the partial values of the kernel on the image
                    float v_sum = 0;

                    for (int frow = -hotspot; frow <= hotspot; frow++)// for each coordinate of the kernel
                        for (int fcol = -hotspot; fcol <= hotspot; fcol++)
                        {
                            int r_tot = row + frow;
                            int c_tot = col + fcol;

                            h_sum += hKernel[frow + hotspot, fcol + hotspot] *  // sum the value to h_sum (if pixel is out of bound we use mean gray value)
                                ((r_tot < 0 || r_tot >= N || c_tot < 0 || c_tot >= M) ? mean_grey_value : inputImage[c_tot, r_tot]);

                            v_sum += vKernel[frow + hotspot, fcol + hotspot] *  // sum the value to v_sum (if pixel is out of bound we use mean gray value)
                               ((r_tot < 0 || r_tot >= N || c_tot < 0 || c_tot >= M) ? mean_grey_value : inputImage[c_tot, r_tot]);


                        }

                    float vect_sum = (float)Math.Sqrt(Math.Pow(h_sum, 2) + Math.Pow(v_sum, 2));
                    vect_sum = Math.Max(Math.Min(vect_sum, 255), 0);
                    tempImage[col, row] = (byte)vect_sum;
                    if (row == 0 || row == N - 1 || col == 0 || col == M - 1) tempImage[col, row] = 0;
                    progressBar.PerformStep();
                }

            progressBar.Visible = false;

            return tempImage;
        }

        

        /*
         * kernel_processing: checks if the kernel is squared and with odd size and 
         * input:   kernel              float matrix
         * output:                      dimension (int) of the kernel
         */
        int kernel_processing(float[,] kernel)
        {
            if ((kernel.GetLength(0) + kernel.GetLength(1)) % 2 != 0) throw new ArgumentException("Filter size must be even");
            if (kernel.GetLength(0) != kernel.GetLength(1)) throw new ArgumentException("Filter must be a square");

            return kernel.GetLength(0);
        }


        /*
        * convert_kernel: casts every value to a floar kernel starting from a sbyte 
        * input:   kernel              sbyte matrix
        * output:                      float matrix
        */
        float[,] convertKernel(sbyte[,] kernel)
        {
            float[,] newk = new float[kernel.GetLength(0), kernel.GetLength(1)];
            for (int i = 0; i < kernel.GetLength(0); i++)
                for (int j = 0; j < kernel.GetLength(1); j++)
                    newk[i, j] = (float)kernel[i, j];
            return newk;
        }


        /*
         * thresholdImage: threshold a grayscale image
         * input:   inputImage          single-channel (byte) image
         *          th                  threshold value to use
         * output:                      single-channel (byte) image with on/off values
         */
        private byte[,] thresholdImage(byte[,] inputImage, int th)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // TODO: add your functionality and checks, think about how to represent the binary values

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = inputImage.GetLength(0) * inputImage.GetLength(1);
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                {
                    // get pixel color
                    tempImage[x, y] = (byte)(inputImage[x, y] >= th ? 255 : 0);
                    progressBar.PerformStep();                              // increment progress bar
                }

            progressBar.Visible = false;

            return tempImage;
        }

        private byte[,] thresholdMapToZeroImage(byte[,] inputImage, int th)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // TODO: add your functionality and checks, think about how to represent the binary values

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = inputImage.GetLength(0) * inputImage.GetLength(1);
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
                for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
                {
                    // get pixel color
                    tempImage[x, y] = (byte)(inputImage[x, y] >= th ? inputImage[x,y] : 0);
                    progressBar.PerformStep();                              // increment progress bar
                }

            progressBar.Visible = false;

            return tempImage;
        }

        private byte[,] edge_sharpening(byte[,] inputImage, int a, int sigma)
        {
            // Create smothered image
            byte[,] smooth = convolveImage(inputImage, createGaussianFilter(3, sigma), false);

            //Create temp_image to be returned
            byte[,] temp_image = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage1.Size.Width * InputImage1.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            int mask, tmp;
            for (int c = 0; c < inputImage.GetLength(0); c++)
                for (int r = 0; r < inputImage.GetLength(1); r++) // iterate over columns and rows
                {
                    mask = inputImage[c, r] - smooth[c, r]; // subtract smoothered value from the original one
                    tmp = (int)inputImage[c, r] + a * mask;
                    temp_image[c, r] = (byte)Math.Max(Math.Min(tmp, 255), 0); // tmp value is the sum of the original image with a mask
                    progressBar.PerformStep();
                }
            progressBar.Visible = false;
            return temp_image;

        }

        private byte[,] canny_edge_detection(byte[,] inputImage, int sizeGaussiagn, int sigma, int low_th, int high_th)
        {
            inputImage = convolveImage(inputImage, createGaussianFilter(sizeGaussiagn, sigma), false);
            byte[,] Gx = convolveImage(inputImage,convertKernel(xSobelfilter), false); // gradiente orizzontale, linee verticali
            byte[,] Gy= convolveImage(inputImage,convertKernel(ySobelfilter), false);  // gradiente verticale, linee orizzontali
            int[,] magnitude = new int[inputImage.GetLength(0), inputImage.GetLength(1)];
            int[,] non_max_sup = new int[inputImage.GetLength(0), inputImage.GetLength(1)];

            double theta = Math.PI / 8; // Angolo di rotazione in radianti (π/8)
            double cosTheta = Math.Cos(theta);
            double sinTheta = Math.Sin(theta);

            // Calcolo delle nuove componenti ruotate
            byte[,] direction = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            for(int c = 0; c < inputImage.GetLength(0); c++)
                for(int r = 0; r < inputImage.GetLength(1); r++)
                {
                    magnitude[c,r] = (int) Math.Sqrt(Gx[c,r] * Gx[c,r] + Gy[c, r] * Gy[c, r]);
                    if (magnitude[c, r] == 0)
                    {
                        direction[c, r] = 9;
                        continue;
                    }
                    double vx = Gx[c, r] * cosTheta - Gy[c, r] * sinTheta;
                    double vy = Gx[c, r] * sinTheta + Gy[c, r] * cosTheta;

                    if(vx >= 0)
                    {
                        if(vy >= 0)
                        {
                            if (vx >= vy) direction[c, r] = 0;
                            else direction[c, r] = 1;
                        }
                        else
                        {
                            if (vx >= -vy) direction[c, r] = 7;
                            else direction[c, r] = 6;
                        }
                    }
                    else
                    {
                        if (vy >= 0)
                        {
                            if (-vx >= vy) direction[c, r] = 3;
                            else direction[c, r] = 2;
                        }
                        else
                        {
                            if (-vx >= -vy) direction[c, r] = 4;
                            else direction[c, r] = 5;
                        }
                    }
                }

            for (int c = 0; c < inputImage.GetLength(0); c++)
                for (int r = 0; r < inputImage.GetLength(1); r++)
                {
                    int dir = direction[c, r];
                    if (dir > 3) continue;
                    int x_off, y_off;
                    switch (dir)
                    {
                        case 0:
                            x_off = 1;
                            y_off = 0;
                            break;

                        case 1:
                            x_off = 1;
                            y_off = 1;
                            break;

                        case 2:
                            x_off = 0;
                            y_off = 1;
                            break;

                        case 3:
                            x_off = -1;
                            y_off = 1;
                            break;

                        default:
                            throw new Exception("I should never reach this point!");
                            // Gestione del caso in cui dir non corrisponde a nessuna delle casistiche sopra
                    }
                    if (c + x_off >= inputImage.GetLength(0) || c + x_off < 0 || r + y_off >= inputImage.GetLength(1) || r + y_off < 0) continue;
                    non_max_sup[c,r] = (magnitude[c,r] <= magnitude[c + x_off, r + y_off]) ? 0 : magnitude[c,r];
                    x_off = - x_off; y_off = - y_off;
                    if (c + x_off >= inputImage.GetLength(0) || c + x_off < 0 || r + y_off >= inputImage.GetLength(1) || r + y_off < 0) continue;
                    non_max_sup[c, r] = (magnitude[c, r] <= magnitude[c + x_off, r + y_off]) ? 0 : magnitude[c, r];

                }

            byte[,] contr = adjustContrast(non_max_sup);


            Queue<PixelPoint> pixel_to_check = new Queue<PixelPoint>() ;
            for(int c = 0; c < non_max_sup.GetLength(0); c++)
                for(int r = 0; r < non_max_sup.GetLength(1); r++)
                    if (non_max_sup[c,r] >= high_th) pixel_to_check.Enqueue(new PixelPoint(c, r));

            byte[,] result_image = new byte[non_max_sup.GetLength(0), non_max_sup.GetLength(1)];

            while (pixel_to_check.Any())
            {
                PixelPoint pixel = pixel_to_check.Dequeue();
                int px = pixel.x;
                int py = pixel.y;
                if (result_image[px, py] == 255) continue;
                if (px < 0 || px >= inputImage.GetLength(0) || py < 0 || py >= inputImage.GetLength(1)) continue;
                
                int value = non_max_sup[px, py];

                if(low_th <= value)
                {
                    result_image[px, py] = 255;
                    if(value < high_th) // they actually need to be added because the pixel is not a top pixel
                    {
                        pixel_to_check.Enqueue(pixel.Up());
                        pixel_to_check.Enqueue(pixel.Down());
                        pixel_to_check.Enqueue(pixel.Left());
                        pixel_to_check.Enqueue(pixel.Right());
                    }
                }
            }
        
            return result_image;
        }
    }
}
