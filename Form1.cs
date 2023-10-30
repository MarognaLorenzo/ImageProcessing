using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using System.Windows.Forms;



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
            List<Line> lines = peakFinding(g_scale_image, THRESHOLD); // find lines

            List<List<Segment>> segments = new List<List<Segment>>();
            List<List<(int, int)>> pixel_in_lines = new List<List<(int, int)>>();

            foreach (Line line in lines) // for every line detect segments and all pixel on that line ( for precision and easier computation)
            {
                (List<Segment> segmentsToAdd, List<(int, int)> pixels_on_line) = hough_line_detection(workingImage, line.r, line.theta, MINIMUM_THRESHOLD, MINIMUM_LENGHT, MAXIMUM_GAP);
                Console.WriteLine("Line - r: " + line.r + "theta: " + line.theta);
                segments.Add(segmentsToAdd);
                pixel_in_lines.Add(pixels_on_line);
            }

            List<PixelPoint> crossing_coords = hough_crossing_line(lines, workingImage); // detect crossing points


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
            List<Line> lines = peakFinding(g_scale_image, THRESHOLD); // find peaks

            List<List<Segment>> segments = new List<List<Segment>>();
            List<List<(int, int)>> pixel_in_lines = new List<List<(int, int)>>();

            foreach (Line line in lines) // for every detected lines find segments and whole line ( easier for computation and precision)
            {
                (List<Segment> segmentsToAdd, List<(int, int)> pixels_on_line) = hough_line_detection(workingImage, line.r, line.theta, MINIMUM_THRESHOLD, MINIMUM_LENGHT, MAXIMUM_GAP);
                segments.Add(segmentsToAdd);
                pixel_in_lines.Add(pixels_on_line);
            }

            List<PixelPoint> crossing_coords = hough_crossing_line(lines, workingImage); // find crossing points


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

        private void FindSocketClick(object sender, EventArgs e)
        {
            bool detectedSocket = false;
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
            List<Line> lines = peakFinding(edge_image, THRESHOLD); // find peaks



            (List<Line> grid_lines, List<int> grid_lines_index) = findGrid(lines, ref workingImage);


            List<PixelPoint> vertices = hough_crossing_line(grid_lines, workingImage);

            List<RectangularRegion> regions = identify_regions(grid_lines, grid_lines_index, ref workingImage);

            // blob detection
            List<PixelPoint> blobs = new List<PixelPoint>();
            if (vertices.Count > 1)
            {
                RectangularRegion region = new RectangularRegion(vertices);
                byte[,] crop = cropImage(contrast, region);
                byte[,] reAdjust = adjustContrast(crop);
                blobs = blobFinding(reAdjust, 0.2, ref workingImage);
                for (int i = 0; i < blobs.Count; i++)
                {
                    blobs[i].x_shift(region.left);
                    blobs[i].y_shift(region.bottom);
                }
                if (vertices.Count == 4)
                {
                    if (blobs.Count < 2)
                    {
                        Console.WriteLine("Socket not found");
                        return;
                    }
                    if (blobs.Count == 3 || blobs.Count == 2)
                    {
                        if( blobs.All(bl => Math.Abs(bl.x - blobs[0].x) < 3) || blobs.All(bl => Math.Abs(bl.y - blobs[0].y) < 3))
                        {
                            detectedSocket = true;
                            Console.WriteLine("Socket found");
                        }
                    }


                }


            }



            List<List<Segment>> segments = new List<List<Segment>>();
            List<List<(int, int)>> pixel_in_lines = new List<List<(int, int)>>();

            foreach (Line line in lines) // for every detected lines find segments and whole line ( easier for computation and precision)
            {
                (List<Segment> segmentsToAdd, List<(int, int)> pixels_on_line) = hough_line_detection(workingImage, line.r, line.theta, MINIMUM_THRESHOLD, MINIMUM_LENGHT, MAXIMUM_GAP);
                segments.Add(segmentsToAdd);
                pixel_in_lines.Add(pixels_on_line);
            }

            List<PixelPoint> crossing_coords = hough_crossing_line(lines, workingImage); // find crossing points


            OutputImage = new Bitmap(workingImage.GetLength(0), workingImage.GetLength(1)); // create new output image

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }

            //// VISUALIZE LINES
            //for (int i = 0; i < segments.Count; i++)
            //    hough_visualization(segments[i], ref OutputImage, pixel_in_lines[i], square_index.Exists(x => x == i) ? Color.Blue : Color.Red);

            ////VISUALIZE DOTS
            //hough_visualize_crossing(crossing_coords, ref OutputImage);


            // VISUALIZE square
            for (int i = 0; i < segments.Count; i++)
                if(grid_lines_index.Exists(x => x == i))
                    hough_visualization(segments[i], ref OutputImage, pixel_in_lines[i], Color.Blue);


            //Vertices of the parallel lines
            hough_visualize_crossing(vertices, ref OutputImage, false, Color.Orange);

            //foreach (RectangularRegion region in regions)
            //    hough_visualize_crossing(region.get_creation_point(), ref OutputImage, false, Color.Red);



            //Blo
            hough_visualize_crossing(blobs, ref OutputImage, false, Color.YellowGreen);

            pictureBoxOut.Image = (Image)OutputImage;                         // display output image

        }
        private byte[,] cropImage(byte[,] inputImage, RectangularRegion target)
        {
            byte[,] res = new byte[target.get_width(), target.get_height()];
            (int top, int right, int bottom, int left) = target.get_tuple();

            for (int c = left; c < right; c++)
                for (int r = bottom; r < top; r++)
                    res[c - left, r - bottom] = inputImage[c, r];
            return res;
        }

        List<PixelPoint> blobFinding(byte[,] inputImage, double threshold, ref byte[,] workingImage)
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


            List<PixelPoint> blobs = new List<PixelPoint>();
            foreach (var kv in map)// for each kea value in the dictionary with centroids sums and count of pixels get the average and add it to the result
            {
                int x = kv.Value.sumx / kv.Value.count;
                int y = kv.Value.sumy / kv.Value.count;
                Console.WriteLine("Blob: x: " + x + " y: " + y);
                blobs.Add(new PixelPoint(x, y));
            }
            return blobs;
        }

        private List<RectangularRegion> identify_regions(List<Line> grid_lines, List<int> grid_lines_index, ref byte[,] workinImage)
        {
            if (grid_lines.Count == 0) return new List<RectangularRegion>();

            // dbsscan
            List<Line> axis1 = new List<Line> { grid_lines[0] };
            List<Line> axis2 = new List<Line>();
            List<RectangularRegion> res = new List<RectangularRegion>();
            foreach (Line line in grid_lines.Skip(1))
            {
                if (axis1.Any(horixontal_line => horixontal_line.isAlmostParallelWith(line))) axis1.Add(line);
                else axis2.Add(line);
            }

            List<Line> h_lines;
            List<Line> v_lines;
            double avg1 = axis1.Select(single_line => single_line.theta).Average();
            h_lines = avg1 < 45 || avg1 > 135 ? axis1 : axis2;
            v_lines = avg1 < 45 || avg1 > 135 ? axis2 : axis1;

            h_lines = h_lines.OrderBy(line => line.r).ToList();
            v_lines = v_lines.OrderBy(line => line.r).Reverse().ToList();
            List<Line> lines_to_use = new List<Line> {v_lines[0] };

            for (int i = 0; i < v_lines.Count - 1; i++)
            {
                lines_to_use.Add(v_lines[i + 1]);
                lines_to_use.Add(h_lines[0]);
                for (int j = 0; j < h_lines.Count -1; j++)
                {
                    lines_to_use.Add(h_lines[j + 1]);

                    List<PixelPoint> rectangle_vertices = hough_crossing_line(lines_to_use, workinImage);
                    if(rectangle_vertices.Count == 4) res.Add(new RectangularRegion(rectangle_vertices));

                    if(!lines_to_use.Remove(h_lines[j])) throw new Exception("element not found");
                    
                }
                lines_to_use.Remove(h_lines[h_lines.Count - 1]);
                if(!lines_to_use.Remove(v_lines[i])) throw new Exception("element not found");
            }
            return res;
        }


        (List<Line> grid, List<int> grid_index) findGrid(List<Line> lines, ref byte[,] workingImage)
        {
            ICollection<Line> pairable_lines = new HashSet<Line>();
            ICollection<int> pairable_lines_index = new HashSet<int>();

            for (int i = 0; i < lines.Count; i++)
            {
                Line line1 = lines[i];
                if (pairable_lines.Any(line_in_square => line_in_square.isAlmostSameAs(line1))) continue;
            
                for (int j = i + 1; j < lines.Count; j++)
                {
                    Line line2 = lines[j];
                    if (pairable_lines.Any(line_in_square => line_in_square.isAlmostSameAs(line2))) continue;
                    
                    if (line1.isDistantParallelWith(line2))
                    {
                        Console.WriteLine("Parallel line: " + line1.ToString() + "with " + line2.ToString());
                        pairable_lines.Add(line1);
                        pairable_lines.Add(line2);
                        pairable_lines_index.Add(j);
                        pairable_lines_index.Add(i);
                    }
                }
            }
            // Only lines with a parallel buddy distant from the line itself
            pairable_lines = pairable_lines.ToList();
            pairable_lines_index = pairable_lines_index.ToList();


            List<int> accum = new List<int> (lines.Count);
            while (accum.Count < accum.Capacity) accum.Add(0);

            for (int i = 0; i < pairable_lines.Count; i++)
            {
                Line line1 = lines[i];
                for (int j = i + 1; j < pairable_lines.Count; j++)
                {
                    Line line2 = lines[j];
                    if(line1.isAlmostParallelWith(line2) || line1.isAlmostPerpendicularWith(line2))
                    {
                        accum[pairable_lines_index.ElementAt(i)] += 1;
                        accum[pairable_lines_index.ElementAt(j)] += 1;
                    }
                }
            }

            int max = accum.Max();
            List<int> chosen_index = new List<int>();

            for (int i = 0; i < pairable_lines.Count; i++)
                if (accum[i] == max)
                    chosen_index.Add(i);


            List<Line> result = new List<Line>();

            foreach(int index in chosen_index) result.Add(lines[index]);
            
            return (result, chosen_index);
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


        private void NumberBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }


        internal enum SEShape
        {
            Square,
            Plus
        }

        private struct Segment
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

        private struct RectangularRegion
        {
            public int top;
            public int bottom;
            public int right;
            public int left;

            private List<PixelPoint> creation_points;
            public int get_height()
            {
                return this.top - this.bottom;
            }
            public int get_width()
            {
                return this.right - this.left;
            }
            public (int top, int right, int bottom, int left) get_tuple()
            {
                return (top, right, bottom, left);
            }

            public RectangularRegion (List<PixelPoint> vertices)
            {
                if (vertices.Count == 0) throw new Exception("No vertices");
                this.top = this.bottom = vertices[0].y;
                this.right = this.left = vertices[1].x;
                this.creation_points = vertices;
                foreach (PixelPoint point in vertices)
                {
                    this.top = Math.Max(this.top, point.y);
                    this.bottom = Math.Min(this.bottom, point.y);
                    this.left = Math.Min(this.left, point.x);
                    this.right = Math.Max(this.right, point.x);
                }
            }
            public List<PixelPoint> get_creation_point()
            {
                return creation_points;
            }
        }
        private struct Line
        {
            public double theta;
            public double r;
            public Line(double r, double theta)
            {
                this.theta = theta;
                this.r = r;
            }
            public double get_diff(Line l2)
            {
                double abs_diff = Math.Abs(l2.theta - this.theta);
                return abs_diff <= 90 ? abs_diff : (360 - 2 * abs_diff) / 2;
            }
            public bool isAlmostSameAs(Line line1)
            {
                double diff = Math.Abs(line1.theta - this.theta);
                if (diff < 5 && Math.Abs(line1.r - this.r) < 4) return true;
                if (diff > 175 && Math.Abs(line1.r + this.r) < 5) return true;

                return false;
            }
            public bool isAlmostParallelWith(Line line1)
            {
                return this.get_diff(line1) < 5;
            }
            public bool isAlmostPerpendicularWith(Line line1)
            {
                double diff = this.get_diff(line1);
                if (85 < diff && diff < 95) return true;
                return false;
            }
            public bool isDistantParallelWith(Line line1)
            {
                double diff = Math.Abs(line1.theta - this.theta);
                if (diff < 5 && Math.Abs(line1.r - this.r) > 25) return true;
                if (diff > 175 && Math.Abs(line1.r + this.r) > 25) return true;
                return false;
            }



        }
        private struct PixelPoint
        {
            public int x { get; set; }
            public int y { get; set; }
            public PixelPoint(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public void x_shift(int shift)
            {
                x += shift;
            }
            public void y_shift(int shift)
            {
                y += shift;
            }
        }
    }
}