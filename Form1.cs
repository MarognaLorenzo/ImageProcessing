using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage;
        private Bitmap OutputImage;
        sbyte[,] vPrewittfilter = new sbyte[3, 3] { { -1, -1, -1 },
                                             {  0 , 0,  0 },
                                             {  1,  1,  1 } };

        sbyte[,] hPrewittfilter = new sbyte[3, 3] { { -1, 0, 1 },
                                             { -1, 0, 1 },
                                             { -1, 0, 1 } };
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

        public INFOIBV()
        {
            
            byte[,] img = createStructuringElement(5,SEShape.Plus, true);
            for (int r = 0; r < 5; r++)
                for (int c = 0; c < 5; c++)
                {
                    if (img[r,c] != 0) img[r,c] = 255;
                }

            List<int> l = traceBoundary(img);
            Console.WriteLine(l);
            Console.WriteLine(l.Count);
            foreach (int el in l)  Console.Write(el +" ");
            
            InitializeComponent();
        }

        /*
         * loadButton_Click: process when user clicks "Load" button
         */
        private void loadImageButton_Click(object sender, EventArgs e)
        {
            if (openImageDialog.ShowDialog() == DialogResult.OK)             // open file dialog
            {
                string file = openImageDialog.FileName;                     // get the file name
                imageFileName.Text = file;                                  // show file name
                if (InputImage != null) InputImage.Dispose();               // reset image
                InputImage = new Bitmap(file);                              // create new Bitmap from file
                if (InputImage.Size.Height <= 0 || InputImage.Size.Width <= 0 ||
                    InputImage.Size.Height > 512 || InputImage.Size.Width > 512) // dimension check (may be removed or altered)
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBox1.Image = (Image)InputImage;                 // display input image
            }
        }


        /*
         * applyButton_Click: process when user clicks "image_b" button
         */
        private void image_b_ButtonClick(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage.GetPixel(x, y);                // set pixel color in array at (x,y)


            byte[,] g_scale_image = convertToGrayscale(Image);          // convert image to grayscale

            byte[,] workingImage = adjustContrast(g_scale_image);


            // ==================== END OF YOUR FUNCTION CALLS ====================
            // ====================================================================

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            pictureBox2.Image = (Image)OutputImage;                         // display output image
        }

        private void image_c_ButtonClick(object sender, EventArgs e)
        {
            if (InputImage == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            OutputImage = new Bitmap(InputImage.Size.Width, InputImage.Size.Height); // create new output image
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage.GetPixel(x, y);                // set pixel color in array at (x,y)

            // ====================================================================
            // =================== YOUR FUNCTION CALLS GO HERE ====================
            // Alternatively you can create buttons to invoke certain functionality
            // ====================================================================


            byte[,] g_scale_image = convertToGrayscale(Image);          // convert image to grayscale

            byte[,] med_filter = medianFilter(g_scale_image, 5);

            byte[,] edge_det = edgeMagnitude(med_filter, hPrewittfilter, vPrewittfilter);

            byte[,] image_c = thresholdImage(edge_det, 60);

            byte[,] workingImage = image_c;


            // ==================== END OF YOUR FUNCTION CALLS ====================
            // ====================================================================

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            pictureBox2.Image = (Image)OutputImage;                         // display output image
        }


        /*
         * saveButton_Click: process when user clicks "Save" button
         */
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (OutputImage == null) return;                                // get out if no output image
            if (saveImageDialog.ShowDialog() == DialogResult.OK)
                OutputImage.Save(saveImageDialog.FileName);                 // save the output image
        }


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
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
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

        /*
         * invertImage: invert a single channel (grayscale) image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        private byte[,] invertImage(byte[,] inputImage)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    // get pixel color
                    tempImage[x, y] = (byte)(255 - inputImage[x, y]);      // invert pixel
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
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                    histogram[inputImage[x, y]]++;

            // Calculating actual contrast
            int lower_bound = 0;
            while (histogram[lower_bound] == 0 && lower_bound < 256) lower_bound++;
            int upper_bound = 255;
            while (histogram[upper_bound] == 0 && upper_bound >= 0) upper_bound--;

            // setup progress bar
            progressBar.Visible = true;
            progressBar.Minimum = 1;
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    // get pixel color
                    tempImage[x, y] = (byte)((inputImage[x, y] - lower_bound) *  ((float) 255 / (float)(upper_bound - lower_bound)));
                    progressBar.PerformStep();                              // increment progress bar
                }
            for (int a = 0; a < histogram.Length; a++) histogram[a] = 0;
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
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
        private float[,] createGaussianFilter(byte size, float sigma)
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
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
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
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
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
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
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
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
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
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
            progressBar.Value = 1;
            progressBar.Step = 1;

            // process all pixels in the image
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                {
                    // get pixel color
                    tempImage[x, y] = (byte)(inputImage[x, y] >= th ? 255 : 0);
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
            progressBar.Maximum = InputImage.Size.Width * InputImage.Size.Height;
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


        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 2 GO HERE ==============
        // ====================================================================

        /*
        * createStructuringElement: takes as input the structure element shape(plus or square) and size and outputs
        * the corresponding structure element. For the plus-shaped element, the result should be the same as an 
        * iterative dilation of a 3x3 plus-shaped structuring element with a 3x3 plus-shaped structuring element.
        * input:   shape               enum type of shape for SE
        *          size                int size of the SE
        *          binary              true if we want to create a SE for a binary image
        * output:                      byte[,] matrix of SE
        */
        byte[,] createStructuringElement(int size, SEShape shape, bool binary)
        {
            if (shape == SEShape.Square)
            {
                byte[,] SE = new byte[size, size];
                for (int r = 0; r < size; r++)
                    for (int c = 0; c < size; c++)
                        SE[r, c] = 1;
                return SE;
            }
            byte[,] tmp_SE = { { 1 } };
            int tmp_SE_dim = 1;
            int n_repetition = (size - 1) / 2;
            for (int i = 0; i < n_repetition; i++)
            {
                int new_SE_dim = tmp_SE_dim + 2;
                byte[,] newSE = new byte[new_SE_dim, new_SE_dim];
                for (int r = 0; r < tmp_SE_dim; r++)
                    for (int c = 0; c < tmp_SE_dim; c++)
                    {
                        if (tmp_SE[r, c] == 0) continue;
                        newSE[r + 1, c + 1] = 1; // corresponding pixel
                        newSE[r + 2, c + 1] = 1; //pixel below
                        newSE[r, c + 1] = 1; //pixel over
                        newSE[r + 1, c + 2] = 1; // pixel on the right
                        newSE[r + 1, c] = 1; // pixel on the left
                    }
                tmp_SE = newSE;
                tmp_SE_dim = new_SE_dim;
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
            byte[,] temp = new byte[inputImage.GetLength(0),inputImage.GetLength(1)];
            List<Tuple<int, int>> cords = new List<Tuple<int, int>>();
            List<byte> tmp;
            for(int x = 0; x < SE.GetLength(0); x++)
            {
                for (int y = 0; y < SE.GetLength(1); y++)
                {
                    if (SE[x, y] > 0)
                    {
                        cords.Add(new Tuple<int, int>(x - (SE.GetLength(0)/2), y - (SE.GetLength(1) / 2)));
                    }
                }
            }

            if (binary)
            {
                for (int x = 0; x < inputImage.GetLength(0); x++)
                {
                    for (int y = 0; y < inputImage.GetLength(1); y++)
                    {
                        if(inputImage[x, y] < 1)
                        {
                            temp[x, y] = inputImage[x, y];
                            break;
                        }
                        else
                        {
                            temp[x, y] = inputImage[x, y];
                        }
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
                                if (inputImage[x + cords[z].Item1, y + cords[z].Item2] < 1)
                                {
                                    temp[x, y] = 0;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
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
                    }
                }
            }

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

            if (binary)
            {
                for (int x = 0; x < inputImage.GetLength(0); x++)
                {
                    for (int y = 0; y < inputImage.GetLength(1); y++)
                    {
                        if (inputImage[x, y] > 0)
                        {
                            temp[x, y] = inputImage[x, y];
                            break;
                        }
                        else
                        {
                            temp[x, y] = inputImage[x, y];
                        }
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
                                if (inputImage[x + cords[z].Item1, y + cords[z].Item2] > 0)
                                {
                                    temp[x, y] = 0;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
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
                        temp[x, y] = tmp.AsQueryable().Max();
                    }
                }
            }

            return temp;
        }


        /*
        * countValues: takes an input image and returns the amount of distinct values and histogram of the values.
        * input:   inputImage          single channel image
        * output:                      int[] histogram of values
        *                              byte amount of distinct values.
        */
        Tuple<byte, int[]> countValues(byte[,] inputImage)
        {
            int[] hist = new int[256];
            byte ammo = 0;

            for (int i = 0; i < 256; i++)
            {
                hist[i] = 0;
            }

            for (int x = 0; x < inputImage.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage.GetLength(1); y++)
                {
                    if(hist[inputImage[x, y]] == 0)
                    {
                        ammo++;
                    }
                    hist[inputImage[x, y]]++;
                }
            }

            return new Tuple<byte, int[]>(ammo, hist);
        }


        /*
        * andImages: takes 2 input byte images and does a pointwise and funtion on each
        * input:   inputImage1         single channel bit image
        *          inputImage2         single channel bit image
        * output:                      single channel bit image
        */
        byte[,] andImages(byte[,] inputImage1, byte[,] inputImage2)
        {
            byte[,] temp = new byte[inputImage1.GetLength(0), inputImage1.GetLength(1)];

            for (int x = 0; x < inputImage1.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage1.GetLength(1); y++)
                {
                    if (inputImage1[x,y] != 0 && inputImage2[x, y] != 0)
                    {
                        temp[x,y] = inputImage1[x,y];
                    }
                    else
                    {
                        temp[x, y] = 0;
                    }
                }
            }

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
            byte[,] temp = new byte[inputImage1.GetLength(0), inputImage1.GetLength(1)];

            for (int x = 0; x < inputImage1.GetLength(0); x++)
            {
                for (int y = 0; y < inputImage1.GetLength(1); y++)
                {
                    if (inputImage1[x, y] != 0 || inputImage2[x, y] != 0)
                    {
                        temp[x, y] = inputImage1[x, y];
                    }
                    else
                    {
                        temp[x, y] = 0;
                    }
                }
            }

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
            return erodeImage(dilateImage(inputImage, SE, binary), SE, binary);
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
            return dilateImage(erodeImage(inputImage, SE, binary), SE, binary);
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


            for(int r = 0; r < inputImage.GetLength(1); r++)
            {
                bool stop = false;
                for (int c = 0; c < inputImage.GetLength(0); c++)
                    if (inputImage[c, r] == 255)
                    {
                        pixel = (r, c);
                        stop = true;
                        break;
                    }
                if (stop) break;
            }
            (int, int) end_pixel = (-10, -10);
            int ending_direction = -1;
            int dir_to_look;
            int coming_direction = -1;
            for (int i = 0; i < 8; i++)
            {
                dir_to_look = (direction + i) % 8;
                (int, int) movement_matrix_r_c = contourDict[dir_to_look];
                int r = pixel.Item1 + movement_matrix_r_c.Item2;
                int c = pixel.Item2 + movement_matrix_r_c.Item1;
                if (r < 0 || r > inputImage.GetLength(1) || c < 0 || c > inputImage.GetLength(0)) continue;
                if (inputImage[c, r] == 0) continue;
                if (inputImage[c, r] != 255) throw new Exception("Image is not binary");
                boundary.Add(dir_to_look);
                ending_direction= dir_to_look;
                coming_direction = ending_direction;
                direction = (dir_to_look + 6) % 8;
                pixel = (r, c);
                end_pixel = (r, c);
                break;
            }
            if (ending_direction == -1) return boundary;

            int cont = 0;
            while (true)
            {
                if (end_pixel == pixel && ending_direction == coming_direction)
                {
                    if (cont == 0) cont = 1;
                    else return boundary;
                }
                for (int i = 0; i < 8; i++)
                {
                    dir_to_look = (direction + i) % 8;
                    (int, int) movement_matrix_r_c = contourDict[dir_to_look];
                    int r = pixel.Item1 + movement_matrix_r_c.Item2;
                    int c = pixel.Item2 + movement_matrix_r_c.Item1;
                    if (r < 0 || r >= inputImage.GetLength(1) || c < 0 || c >= inputImage.GetLength(0)) continue;
                    if (inputImage[c, r] == 0) continue;
                    if (inputImage[c, r] != 255) throw new Exception("Image is not binary");
                    boundary.Add(dir_to_look);
                    direction = ( dir_to_look + 6 ) % 8;
                    pixel = (r, c);
                    break;
                }
            }
        }

        // ====================================================================
        // ============= YOUR FUNCTIONS FOR ASSIGNMENT 3 GO HERE ==============
        // ====================================================================

    }

    internal enum SEShape
    {
        Square,
        Plus
    }
}