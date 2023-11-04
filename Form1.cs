using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Permissions;
using System.Windows.Forms;



namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap InputImage1;
        private Bitmap InputImage2;
        private Bitmap OutputImage;
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
            List<List<PixelPoint>> pixel_in_lines = new List<List<PixelPoint>>();

            foreach (Line line in lines) // for every line detect segments and all pixel on that line ( for precision and easier computation)
            {
                (List<Segment> segmentsToAdd, List<PixelPoint> pixels_on_line) = hough_line_detection(workingImage, line.r, line.theta, MINIMUM_THRESHOLD, MINIMUM_LENGHT, MAXIMUM_GAP);
                //Console.WriteLine("Line - r: " + line.r + "theta: " + line.theta);
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
                foreach (PixelPoint pixel in line)
                {
                    OutputImage.SetPixel(pixel.x, pixel.y, Color.Yellow);
                }

            // VISUALIZE RED LINES
            for (int i = 0; i < segments.Count; i++)
                hough_visualization(segments[i], ref OutputImage, pixel_in_lines[i], Color.Red);

            //VISUALIZE GREEN DOTS
            hough_visualize_crossing(crossing_coords, ref OutputImage, Color.Green);

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
            byte[,] contrast = adjustContrast(g_scale_image);
            byte[,] edge_image = edgeMagnitude(contrast, xSobelfilter, ySobelfilter);
            byte[,] workingImage = contrast;
            List<Line> lines = peakFinding(edge_image, THRESHOLD); // find peaks

            List<List<Segment>> segments = new List<List<Segment>>();
            List<List<PixelPoint>> pixel_in_lines = new List<List<PixelPoint>>();

            foreach (Line line in lines) // for every detected lines find segments and whole line ( easier for computation and precision)
            {
                (List<Segment> segmentsToAdd, List<PixelPoint> pixels_on_line) = hough_line_detection(workingImage, line.r, line.theta, MINIMUM_THRESHOLD, MINIMUM_LENGHT, MAXIMUM_GAP);
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
            hough_visualize_crossing(crossing_coords, ref OutputImage, Color.Green);

            pictureBoxOut.Image = (Image)OutputImage;                         // display output image

        }

        private void CannyLineDetectionClick(object sender, EventArgs e)
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
            byte[,] edge_image = canny_edge_detection(contrast, CANNY_EDGE_SIZE, CANNY_SIGMA, LOW_TH, HIGH_TH);
            byte[,] workingImage = contrast;
            List<Line> lines = peakFinding(edge_image, THRESHOLD); // find peaks

            List<List<Segment>> segments = new List<List<Segment>>();
            List<List<PixelPoint>> pixel_in_lines = new List<List<PixelPoint>>();

            foreach (Line line in lines) // for every detected lines find segments and whole line ( easier for computation and precision)
            {
                (List<Segment> segmentsToAdd, List<PixelPoint> pixels_on_line) = hough_line_detection(workingImage, line.r, line.theta, MINIMUM_THRESHOLD, MINIMUM_LENGHT, MAXIMUM_GAP);
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
            hough_visualize_crossing(crossing_coords, ref OutputImage, Color.Green);

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

        private void CannyEdgeDetectionClick(object sender, EventArgs e)
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

            byte[,] edge = canny_edge_detection(g_scale_image, CANNY_EDGE_SIZE, CANNY_SIGMA, LOW_TH, HIGH_TH);
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

        private void edgeMagnitudeClick(object sender, EventArgs e)
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
            byte[,] workingImage = adjustContrast(brightImage(edge, -50));

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
            if (InputImage1 == null) return;                                 // get out if no input image
            if (OutputImage != null) OutputImage.Dispose();                 // reset output image
            OutputImage = new Bitmap(InputImage1.Size.Width, InputImage1.Size.Height); // create new output image
            Color[,] Image = new Color[InputImage1.Size.Width, InputImage1.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage1.Size.Width; x++)                 // loop over columns
                for (int y = 0; y < InputImage1.Size.Height; y++)            // loop over rows
                    Image[x, y] = InputImage1.GetPixel(x, y);                // set pixel color in array at (x,y)

            // START OF THE PIPELINE
            // PREPOCESSING
            byte[,] g_scale_image = convertToGrayscale(Image);          // convert image to grayscale
            byte[,] contrast = adjustContrast(g_scale_image);

            // EDGE DETECTION - 2 different methods
            byte[,] edge_image = edgeMagnitude(contrast, xSobelfilter, ySobelfilter);
            byte[,] canny_edge_image = canny_edge_detection(contrast, CANNY_EDGE_SIZE, CANNY_SIGMA, LOW_TH, HIGH_TH);

            // LINE DETECTION
            List<Line> canny_detected_lines = peakFinding(canny_edge_image, THRESHOLD); 
            List<Line> withouth_canny_detected_lines = peakFinding(edge_image, THRESHOLD); 
            List<Line> lines = canny_detected_lines.Concat(withouth_canny_detected_lines).ToList();

            // SETTING UP OUTPUT IMAGE
            byte[,] workingImage = contrast;

            // GRID BUILDING
            List<Line> grid_lines = findGrid(lines, ref workingImage);

            // RECTANGLES IDENTIFICATION
            List<RectangularRegion> regions = identify_regions(grid_lines, ref workingImage);

            // some of the RectangularRegions can be converted in a Socket
            List<Socket> sockets = new List<Socket>();

            foreach (RectangularRegion region in regions)
            {
                //FIND BLOBS

                // Crop image
                byte[,] crop = cropImage(contrast, region);

                region.blobs = blobFinding(crop, 0.2);
                
                // Adjsut blob position with on uncropped image
                for (int i = 0; i < region.blobs.Count; i++)
                {
                    region.blobs[i].x += region.left;
                    region.blobs[i].y += region.bottom;
                }


                // SOCKET DETECTION
                if (region.blobs.Count < 2) continue; // Socket not found in the region


                // Comparison of barycentres
                PixelPoint region_barycentre = region.get_barycentre();

                double euclidian_distance = barycentreOf(region.blobs).euclidian_distance(region_barycentre);

                if (euclidian_distance < 30){ // SOCKET FOUND

                    //it is possible to show which blobs have been detected
                    //foreach(var blob in region.blobs) Console.WriteLine(blob.toString());

                    // Convert the region to a socket
                    Socket socket = region.toSocket();

                    // SOCKET RECOGNITION
                    socket.findType();

                    sockets.Add(socket);

                    Console.WriteLine("Socket found: " + socket.type);
                }
            }

            // PLOT RESULTS

            OutputImage = new Bitmap(workingImage.GetLength(0), workingImage.GetLength(1)); // create new output image

            // copy array to output Bitmap
            for (

                int x = 0; x < workingImage.GetLength(0); x++)             // loop over columns
                for (int y = 0; y < workingImage.GetLength(1); y++)         // loop over rows
                {
                    Color newColor = Color.FromArgb(workingImage[x, y], workingImage[x, y], workingImage[x, y]);
                    OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
                }


            // plot bounding box
            foreach (var socket in sockets)
            {
                // Color of the box depends on the type of socket
                Color color = socket.typeToColor(); 

                // plot box
                foreach (var pixel in socket.generate_rectangle())
                    OutputImage.SetPixel(pixel.x, pixel.y, color);

                // plot blobs if socket is Unknown
                if (socket.type == SocketType.Unknown)
                {
                    hough_visualize_crossing(socket.blobs, ref OutputImage, false, Color.YellowGreen);
                }
            }

            pictureBoxOut.Image = (Image)OutputImage;                         // display output image
        }

        // cropImage: returns a new image from the old one given a rectangular region inside the starting image
        private byte[,] cropImage(byte[,] inputImage, RectangularRegion target)
        {
            byte[,] res = new byte[target.get_width(), target.get_height()];

            // find the borders
            (int top, int right, int bottom, int left) = target.get_tuple();

            // build the new image
            for (int c = left; c < right; c++)
                for (int r = bottom; r < top; r++)
                    res[c - left, r - bottom] = inputImage[c, r];
            return res;
        }

        /* blobFinding: pipeline to find the blobs inside a socket
         * input:
         * inputImage: single channel greyscale image
         * threshold: maximum value of intensity for black ( percentage )
         * output: List of pixels corresponding to the barycentres of every single blob
         */
        List<PixelPoint> blobFinding(byte[,] inputImage, double threshold)
        {
            // find real blacks
            byte[,] thresholded = thresholdImage(inputImage, (int)(threshold * 255));
            
            // invert image
            byte[,] invert = invertImage(thresholded);

            // open image to get rid of single white pixels, I only want to keep bigger areas
            byte[,] open_invert = openImage(invert, createStructuringElement(5, SEShape.Plus), true);

            // label all regions with an incremental ID
            byte[,] flood = floodFill(open_invert); 
            
            return findCentroids(flood);
        }

        /* identify regions: returns the list of rectangles described by 4 lines of the grid
         * input:
         * grid_lines: a list of Lines that compose the grid, every line is either parallel or perpendicular to all the others in the list
         * workingImage: singles channel greyscale 
         */
        private List<RectangularRegion> identify_regions(List<Line> grid_lines, ref byte[,] workinImage)
        {
            if (grid_lines.Count == 0) return new List<RectangularRegion>();

            // First step - divide parallel and perpendicular lines in 2 groups

            List<Line> axis1 = new List<Line> { grid_lines[0] };
            List<Line> axis2 = new List<Line>();

            foreach (Line line in grid_lines.Skip(1)) // first element already in a group
            {
                // if the line is parallel with axis 1 it belongs to axis 1, otherwise to axis 2
                if (axis1.Any(horizontal_line => horizontal_line.isAlmostParallelWith(line))) axis1.Add(line);
                else axis2.Add(line);
            }

            // if the grid only have parallel line there is no rectangle
            if (axis2.Count == 0 || axis1.Count == 0) return new List<RectangularRegion>();

            // detect which group is more horizontal or more vertical
            List<Line> h_lines;
            List<Line> v_lines;
            double theta_avg = axis1.Select(single_line => single_line.theta).Average();
            h_lines = theta_avg < 45 || theta_avg > 135 ? axis1 : axis2;
            v_lines = theta_avg < 45 || theta_avg > 135 ? axis2 : axis1;

            h_lines = h_lines.OrderBy(line => line.r).ToList();
            v_lines = v_lines.OrderBy(line => line.r).Reverse().ToList();



            List<Line> lines_to_use = new List<Line> {v_lines[0] };

            List<RectangularRegion> res = new List<RectangularRegion>();
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


        List<Line> findGrid(List<Line> lines, ref byte[,] workingImage)
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
                        //Console.WriteLine("Parallel line: " + line1.ToString() + "with " + line2.ToString());
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

            for (int i = 0; i < accum.Count; i++)
                if (accum[i] == max)
                    chosen_index.Add(i);


            List<Line> result = new List<Line>();

            foreach(int index in chosen_index) result.Add(lines[index]);

            return result;
        }

        PixelPoint barycentreOf(List<PixelPoint> points)
        {
               return new PixelPoint((int) points.Select(point => point.x).Average(), (int) points.Select(point => point.y) .Average());
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
        PixelPoint math_to_image(PixelPoint image_center, (double x, double y) xy)
        {
            return new PixelPoint(image_center.x + (int)xy.x, image_center.y - (int)xy.y);
        }


        private void NumberBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}