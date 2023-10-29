using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;


namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage1;
        private Bitmap InputImage2;
        private Bitmap OutputImage;
        private const double HOUGH_STEP = 0.3;
        private sbyte[,] vPrewittfilter = new sbyte[3, 3] { { -1, -1, -1 },
                                             {  0 , 0,  0 },
                                             {  1,  1,  1 } };

        private sbyte[,] hPrewittfilter = new sbyte[3, 3] { { -1, 0, 1 },
                                             { -1, 0, 1 },
                                             { -1, 0, 1 } };
        private sbyte[,] vSobelfilter = new sbyte[3, 3] { { -1, -2, -1 },
                                             {  0 , 0,  0 },
                                             {  1,  2,  1 } };

        private sbyte[,] hSobelfilter = new sbyte[3, 3] { { -1, 0, 1 },
                                             { -2, 0, 2 },
                                             { -1, 0, 1 } };

        private double THRESHOLD = 0.6;
        private int CLOSE_DIM = 11;
        private int MINIMUM_THRESHOLD = 70;
        private int MINIMUM_LENGHT = 20;
        private int MAXIMUM_GAP = 5;
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
                if (InputImage1 != null) InputImage1.Dispose();               // reset image
                InputImage1 = new Bitmap(file);                              // create new Bitmap from file
                if (InputImage1.Size.Height <= 0 || InputImage1.Size.Width <= 0 ||
                    InputImage1.Size.Height > 512 || InputImage1.Size.Width > 512) // dimension check (may be removed or altered)
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBoxIn1.Image = (Image)InputImage1;                 // display input image
            }
        }
        private void loadImageButton2_Click(object sender, EventArgs e)
        {
            if (openImageDialog.ShowDialog() == DialogResult.OK)             // open file dialog
            {
                string file = openImageDialog.FileName;                     // get the file name
                image2FileName.Text = file;                                  // show file name
                if (InputImage2 != null) InputImage2.Dispose();               // reset image
                InputImage2 = new Bitmap(file);                              // create new Bitmap from file
                if (InputImage2.Size.Height <= 0 || InputImage2.Size.Width <= 0 ||
                    InputImage2.Size.Height > 512 || InputImage2.Size.Width > 512) // dimension check (may be removed or altered)
                    MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
                else
                    pictureBoxIn2.Image = (Image)InputImage2;                 // display input image
            }
        }


        /*
         * applyButton_Click: process when user clicks "image_b" button
         */
        private void houghLineDetectionClick(object sender, EventArgs e)
        {
            if (InputImage1 == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            OutputImage = new Bitmap(InputImage1.Size.Width, InputImage1.Size.Height); // create new output image
            Color[,] Image = new Color[InputImage1.Size.Width, InputImage1.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage1.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage1.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage1.GetPixel(x, y);                // set pixel color in array at (x,y)


            byte[,] g_scale_image = convertToGrayscale(Image);          // convert image to grayscale
            byte[,] workingImage = g_scale_image;


            // copy array to output Bitmap
            List<(double, double)> lines = peakFinding(g_scale_image, THRESHOLD); // find lines

            List<List<Segment>> segments = new List<List<Segment>>();
            List<List<(int, int)>> pixel_in_lines = new List<List<(int, int)>>();

            foreach ((double r, double theta) line in lines) // for every line detect segments and all pixel on that line ( for precision and easier computation)
            {
                (List<Segment> segmentsToAdd, List<(int, int)> pixels_on_line) = hough_line_detection(workingImage, line.r, line.theta, MINIMUM_THRESHOLD, MINIMUM_LENGHT, MAXIMUM_GAP);
                Console.WriteLine("Line - r: " + line.r + "theta: " + line.theta);
                segments.Add(segmentsToAdd);
                pixel_in_lines.Add(pixels_on_line);
            }

            List<(int, int)> crossing_coords = hough_crossing_line(lines, workingImage); // detect crossing points


            OutputImage = new Bitmap(workingImage.GetLength(0), workingImage.GetLength(1)); // create new output image

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            //VISUALIZE BASE LINES
            foreach (var line in pixel_in_lines)
                foreach ((int c, int r) pixel in line)
                {
                    OutputImage.SetPixel(pixel.c, pixel.r, Color.Yellow);
                }

            // VISUALIZE RED LINES
            for (int i = 0; i < segments.Count; i++)
                hough_visualization(segments[i], ref OutputImage, pixel_in_lines[i], Color.Red);

            //VISUALIZE GREEN DOTS
            hough_visualize_crossing(crossing_coords, ref OutputImage);

            pictureBoxOut.Image = (Image)OutputImage;                         // display output image

        }



        private void FindSquaresClick(object sender, EventArgs e)
        {
            if (InputImage1 == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            OutputImage = new Bitmap(InputImage1.Size.Width, InputImage1.Size.Height); // create new output image
            Color[,] Image = new Color[InputImage1.Size.Width, InputImage1.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage1.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage1.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage1.GetPixel(x, y);                // set pixel color in array at (x,y)


            byte[,] g_scale_image = convertToGrayscale(Image);          // convert image to grayscale
            byte[,] contrast = adjustContrast(g_scale_image);
            byte[,] edge_image = edgeMagnitude(contrast, hSobelfilter, vSobelfilter);
            byte[,] workingImage = contrast;
            List<(double, double)> lines = peakFinding(edge_image, THRESHOLD); // find peaks



            (List<(double, double)> square, List<int> square_index) = findSquare(lines, ref workingImage);

            List<(int, int)> vertices = hough_crossing_line(square, workingImage);

            // blob detection
            List<(int x, int y)> blobs = new List<(int x, int y)>();
            if (vertices.Count > 1)
            {
                (int top, int right, int bottom, int left) region = region_square(vertices);
                byte[,] crop = cropImage(contrast, region);
                blobs = blobFinding(crop, 0.2, ref workingImage);
                for (int i = 0; i < blobs.Count; i++)
                    blobs[i] = (blobs[i].x + region.left, blobs[i].y + region.bottom);
            }




            List<List<Segment>> segments = new List<List<Segment>>();
            List<List<(int, int)>> pixel_in_lines = new List<List<(int, int)>>();

            foreach ((double r, double theta) line in lines) // for every detected lines find segments and whole line ( easier for computation and precision)
            {
                (List<Segment> segmentsToAdd, List<(int, int)> pixels_on_line) = hough_line_detection(workingImage, line.r, line.theta, MINIMUM_THRESHOLD, MINIMUM_LENGHT, MAXIMUM_GAP);
                segments.Add(segmentsToAdd);
                pixel_in_lines.Add(pixels_on_line);
            }

            List<(int, int)> crossing_coords = hough_crossing_line(lines, workingImage); // find crossing points


            OutputImage = new Bitmap(workingImage.GetLength(0), workingImage.GetLength(1)); // create new output image

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            // VISUALIZE LINES
            for (int i = 0; i < segments.Count; i++)
                hough_visualization(segments[i], ref OutputImage, pixel_in_lines[i], square_index.Exists(x => x == i) ? Color.Blue : Color.Red);

            //VISUALIZE DOTS
            hough_visualize_crossing(crossing_coords, ref OutputImage);

            hough_visualize_crossing(vertices, ref OutputImage, false, Color.Orange);

            hough_visualize_crossing(blobs, ref OutputImage, false, Color.YellowGreen);

            pictureBoxOut.Image = (Image)OutputImage;                         // display output image

        }

        private void LineDetectionClick(object sender, EventArgs e)
        {
            if (InputImage1 == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            OutputImage = new Bitmap(InputImage1.Size.Width, InputImage1.Size.Height); // create new output image
            Color[,] Image = new Color[InputImage1.Size.Width, InputImage1.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage1.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage1.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage1.GetPixel(x, y);                // set pixel color in array at (x,y)


            byte[,] g_scale_image = convertToGrayscale(Image);          // convert image to grayscale
            byte[,] workingImage = g_scale_image;


            // copy array to output Bitmap
            List<(double, double)> lines = peakFinding(g_scale_image, THRESHOLD); // find peaks

            List<List<Segment>> segments = new List<List<Segment>>();
            List<List<(int, int)>> pixel_in_lines = new List<List<(int, int)>>();

            foreach ((double r, double theta) line in lines) // for every detected lines find segments and whole line ( easier for computation and precision)
            {
                (List<Segment> segmentsToAdd, List<(int, int)> pixels_on_line) = hough_line_detection(workingImage, line.r, line.theta, MINIMUM_THRESHOLD, MINIMUM_LENGHT, MAXIMUM_GAP);
                segments.Add(segmentsToAdd);
                pixel_in_lines.Add(pixels_on_line);
            }

            List<(int, int)> crossing_coords = hough_crossing_line(lines, workingImage); // find crossing points


            OutputImage = new Bitmap(workingImage.GetLength(0), workingImage.GetLength(1)); // create new output image

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            // VISUALIZE RED LINES
            for (int i = 0; i < segments.Count; i++)
                hough_visualization(segments[i], ref OutputImage, pixel_in_lines[i], Color.Red);

            //VISUALIZE GREEN DOTS
            hough_visualize_crossing(crossing_coords, ref OutputImage);

            pictureBoxOut.Image = (Image)OutputImage;                         // display output image

        }

        private void HoughCirclesClick(object sender, EventArgs e)
        {
            if (InputImage1 == null) return; // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            OutputImage = new Bitmap(InputImage1.Size.Width, InputImage1.Size.Height); // create new output image
            Color[,] Image1 = new Color[InputImage1.Size.Width, InputImage1.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)
            // copy array to output Bitmap
            for (int x = 0; x < InputImage1.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage1.Size.Height; y++)
                {
                    Image1[x, y] = InputImage1.GetPixel(x, y);                // set pixel color in array at (x,y)
                }// loop over rows

            if (NumberBox.Text.Length > 0)
            {
                int r = Convert.ToInt32(Convert.ToString(NumberBox.Text));
                byte[,] g_scale_image = convertToGrayscale(Image1);          // convert image to grayscale
                byte[,] newImg = thresholdImage(g_scale_image, 175);
                byte[,] workingImage = adjustContrast(HoughCircles(newImg, r));

                for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                    for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                    {
                        Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                        OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                    }

                pictureBoxOut.Image = (Image)OutputImage;                         // display output image                         // display output image
            }


        }

        private void houghTransformAngleLimitClick(object sender, EventArgs e)
        {
            if (InputImage1 == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            Color[,] Image = new Color[InputImage1.Size.Width, InputImage1.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage1.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage1.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage1.GetPixel(x, y);                // set pixel color in array at (x,y)


            byte[,] g_scale_image = convertToGrayscale(Image);          // convert image to grayscale

            // copy array to output Bitmap

            byte[,] workingImage = adjustContrast(houghAngleLimit(g_scale_image, 45, 90));
            OutputImage = new Bitmap(workingImage.GetLength(0), workingImage.GetLength(1)); // create new output image

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            pictureBoxOut.Image = (Image)OutputImage;                         // display output image

        }

        private void houghTransformClick(object sender, EventArgs e)
        {
            if (InputImage1 == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            Color[,] Image = new Color[InputImage1.Size.Width, InputImage1.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage1.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage1.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage1.GetPixel(x, y);                // set pixel color in array at (x,y)


            byte[,] g_scale_image = convertToGrayscale(Image);          // convert image to grayscale

            // copy array to output Bitmap

            byte[,] workingImage = adjustContrast(houghTransform(g_scale_image));
            OutputImage = new Bitmap(workingImage.GetLength(0), workingImage.GetLength(1)); // create new output image

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            pictureBoxOut.Image = (Image)OutputImage;                         // display output image

        }

        private void showStudyThresholdClick(object sender, EventArgs e)
        {
            if (InputImage1 == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            Color[,] Image = new Color[InputImage1.Size.Width, InputImage1.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage1.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage1.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage1.GetPixel(x, y);                // set pixel color in array at (x,y)


            byte[,] g_scale_image = convertToGrayscale(Image);          // convert image to grayscale
            int[,] transf = houghTransform(g_scale_image);
            byte[,] contrast = adjustContrast(transf);
            byte[,] thresholded = thresholdImage(contrast, (int)(THRESHOLD * 255.0));
            byte[,] close = closeImage(thresholded, createStructuringElement(CLOSE_DIM, SEShape.Plus), isBinary(thresholded));

            byte[,] workingImage = thresholded;



            OutputImage = new Bitmap(workingImage.GetLength(0), workingImage.GetLength(1)); // create new output image

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            pictureBoxOut.Image = (Image)OutputImage;                         // display output image
        }

        private void showStudyCloseImageClick(object sender, EventArgs e)
        {
            if (InputImage1 == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            Color[,] Image = new Color[InputImage1.Size.Width, InputImage1.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage1.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage1.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage1.GetPixel(x, y);                // set pixel color in array at (x,y)


            byte[,] g_scale_image = convertToGrayscale(Image);          // convert image to grayscale
            int[,] transf = houghTransform(g_scale_image);
            byte[,] contrast = adjustContrast(transf);
            byte[,] thresholded = thresholdImage(contrast, (int)(THRESHOLD * 255.0));
            byte[,] close = closeImage(thresholded, createStructuringElement(3, SEShape.Plus), isBinary(thresholded));

            byte[,] workingImage = close;



            OutputImage = new Bitmap(workingImage.GetLength(0), workingImage.GetLength(1)); // create new output image

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            pictureBoxOut.Image = (Image)OutputImage;                         // display output image
        }

        private void edgedetectionClick(object sender, EventArgs e)
        {
            if (InputImage1 == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            OutputImage = new Bitmap(InputImage1.Size.Width, InputImage1.Size.Height); // create new output image
            Color[,] Image = new Color[InputImage1.Size.Width, InputImage1.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage1.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage1.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage1.GetPixel(x, y);                // set pixel color in array at (x,y)


            byte[,] g_scale_image = convertToGrayscale(Image);          // convert image to grayscale

            byte[,] edge = edgeMagnitude(g_scale_image, hPrewittfilter, vPrewittfilter);
            byte[,] workingImage = edge;

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            pictureBoxOut.Image = (Image)OutputImage;                         // display output image
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

        private byte[,] cropImage(byte[,] inputImage, (int top, int right, int bottom, int left) coords)
        {
            int top = coords.top; int right = coords.right; int bottom = coords.bottom;int left = coords.left;
            byte[,] res = new byte[right - left, top - bottom];

            for (int c = left; c < right; c++)
                for (int r = bottom; r < top; r++)
                    res[c - left, r - bottom] = inputImage[c, r];
            return res;
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

        /*
        * isBinary: takes as input a single channel image and return true only if all values of the image are either 0 or 255
        * input:   inputImage          single channel  image
        * output:                      bool value, true if image is binary
        */
        bool isBinary(byte[,] inputImage)
        {
            for (int c = 0; c < inputImage.GetLength(0); c++)
                for (int r = 0; r < inputImage.GetLength(1); r++)
                    if (inputImage[c, r] != 0 && inputImage[c, r] != 255)
                        return false;
            return true;
        }

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


        List<(double r, double theta)> peakFinding(byte[,] inputImage, double threshold)
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


            List<(double, double)> lines = new List<(double, double)>();
            foreach (var kv in map)// for each kea value in the dictionary with centroids sums and count of pixels get the average and add it to the result
            {
                double T = ((double)kv.Value.sumx) / kv.Value.count * 180 / 500;
                double R = (((double)kv.Value.sumy) / kv.Value.count * maxR / 500) - maxR / 2;
                Console.WriteLine("R:" + R + " - T: " + T + "count:" + kv.Value.count);
                lines.Add((R, T));
            }
            return lines;
        }

        List<(int x, int y)> blobFinding(byte[,] inputImage, double threshold, ref byte[,] workingImage)
        {

            byte[,] thresholded = thresholdImage(inputImage, (int)(threshold * 255));
            byte[,] invert = invertImage(thresholded);
            byte[,] open_invert = openImage(invert, createStructuringElement(5, SEShape.Plus), true);

            byte[,] flood = floodFill(open_invert); // label all regions with an incremental ID

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


            List<(int, int)> blobs = new List<(int, int)>();
            foreach (var kv in map)// for each kea value in the dictionary with centroids sums and count of pixels get the average and add it to the result
            {
                int x = kv.Value.sumx / kv.Value.count;
                int y = kv.Value.sumy / kv.Value.count;
                Console.WriteLine("Blob: x: " + x + " y: " + y);
                blobs.Add((x, y));
            }
            return blobs;
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
        List<(int, int)> hough_crossing_line(List<(double, double)> lines, byte[,] inputImage)
        {
            (int c, int r) image_center = (inputImage.GetLength(0) / 2, inputImage.GetLength(1) / 2);
            List<(int c, int r)> res = new List<(int, int)>();

            for (int i = 0; i < lines.Count; i++)
            {
                (double r, double theta) line1 = lines[i];
                for (int j = i + 1; j < lines.Count; j++) // for every combination of lines
                {
                    (double r, double theta) line2 = lines[j];
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

                    res.Add(pixel_coord);

                }
            }

            return res;

        }

        void hough_visualize_crossing(List<(int c, int r)> crossing_coords, ref Bitmap inputImage)
        {
            foreach ((int c, int r) point in crossing_coords) // visualize every crossing point with a plus-shaped marker
            {
                if (point.c < 0 || point.c >= inputImage.Width || point.r < 0 || point.r >= inputImage.Height) continue;

                if (inputImage.GetPixel(point.c, point.r).R != 255) continue;
                inputImage.SetPixel(point.c, point.r, Color.Green);
                if (point.c + 1 < inputImage.Width) inputImage.SetPixel(point.c + 1, point.r, Color.Green);
                if (point.c - 1 >= 0) inputImage.SetPixel(point.c - 1, point.r, Color.Green);
                if (point.r + 1 < inputImage.Height) inputImage.SetPixel(point.c, point.r + 1, Color.Green);
                if (point.r - 1 >= 0) inputImage.SetPixel(point.c, point.r - 1, Color.Green);
            }
        }
        void hough_visualize_crossing(List<(int c, int r)> crossing_coords, ref Bitmap inputImage, bool only_on_segment, Color color)
        {
            foreach ((int c, int r) point in crossing_coords) // visualize every crossing point with a plus-shaped marker
            {
                if (point.c < 0 || point.c >= inputImage.Width || point.r < 0 || point.r >= inputImage.Height) continue;

                if (only_on_segment && inputImage.GetPixel(point.c, point.r).R != 255) continue;
                inputImage.SetPixel(point.c, point.r, color);
                if (point.c + 1 < inputImage.Width) inputImage.SetPixel(point.c + 1, point.r, color);
                if (point.c - 1 >= 0) inputImage.SetPixel(point.c - 1, point.r, color);
                if (point.r + 1 < inputImage.Height) inputImage.SetPixel(point.c, point.r + 1, color);
                if (point.r - 1 >= 0) inputImage.SetPixel(point.c, point.r - 1, color);
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

        double degree_to_rad(double degree)
        {
            return degree * Math.PI / 180;
        }

        /*
         * math_to_image: converts some coordinates from the mathematical space to the image coordinates, using the position of the center of the image
         */
        (int, int) math_to_image((int c, int r) image_center, (double x, double y) xy)
        {
            return (image_center.c + (int)xy.x, image_center.r - (int)xy.y);
        }

        internal enum SEShape
        {
            Square,
            Plus
        }

        public struct Segment
        {
            public (int c, int r) start;
            public (int c, int r) end;
            public void clear()
            {
                this.start = this.end = (-1, -1);
            }
            public void set_start((int, int) start)
            {
                this.start = start;

            }
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

        private void NumberBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private (int top, int right, int bottom, int left) region_square(List<(int x, int y)> vertices)
        {
            if (vertices.Count == 0) throw new Exception("No vertices");
            int top = vertices[0].y;
            int bottom = vertices[0].y;
            int left = vertices[0].x;
            int right = vertices[0].x;
            foreach ((int x, int y) point in vertices)
            {
                top = Math.Max(top, point.y);
                bottom = Math.Min(bottom, point.y);
                left = Math.Min(left, point.x);
                right = Math.Max(right, point.x);
            }
            return (top, right, bottom, left);
        }

        (List<(double, double)> square, List<int> square_index) findSquare(List<(double r, double theta)> lines, ref byte[,] workingImage)
        {
            List<(double, double)> square = new List<(double, double)>();
            List<int> square_index = new List<int>();


            //List<var> parallel_lines = new List<var>();

            List<double> used_thetas = new List<double>();

            for (int i = 0; i < lines.Count; i++)
            {
                (double r, double theta) line1 = lines[i];
                if (square.Exists(line_in_square => areAlmostSame(line_in_square, line1))) continue;

                for (int j = i + 1; j < lines.Count; j++)
                {
                    (double r, double theta) line2 = lines[j];
                    if (square.Exists(line_in_square => areAlmostSame(line_in_square, line2))) continue;
                    
                    if (areDistantParallel(line1, line2))
                    {
                        Console.WriteLine("Parallel line: " + line1.ToString() + "with " + line2.ToString());
                        square.Add(line1);
                        square.Add(line2);
                        square_index.Add(i);
                        square_index.Add(j);
                        used_thetas.Add((line1.theta + line2.theta) / 2);
                    }
                }
            }
            return (square, square_index);
        }

        private bool areAlmostSame((double r, double theta) line1, (double r, double theta) line2)
        {
            double diff = Math.Abs(line1.theta - line2.theta);
            if (diff < 5 && Math.Abs(line1.r - line2.r) < 4) return true;
            if (diff > 175 && Math.Abs(line1.r + line2.r) < 5) return true;

            return false;
        }
        private bool areDistantParallel((double r, double theta) line1, (double r, double theta) line2)
        {
            double diff = Math.Abs(line1.theta - line2.theta);
            if (diff < 5 && Math.Abs(line1.r - line2.r ) > 25) return true;
            if (diff > 175 && Math.Abs(line1.r + line2.r ) > 25) return true;
            return false;
        }
    }
}