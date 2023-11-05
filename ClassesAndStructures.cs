using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV
{
    // The main class for the program, INFOIBV
    public partial class INFOIBV
    {
        // Enum to represent different shapes
        internal enum SEShape
        {
            Square,
            Plus
        }

        // Private class to represent a line segment
        private class Segment
        {
            public PixelPoint start;
            public PixelPoint end;

            // Clear the segment by setting start and end to (-1, -1)
            public void clear()
            {
                start = end = new PixelPoint(-1, -1);
            }

            // Set the start point of the segment
            public void set_start(PixelPoint start)
            {
                this.start = start;
            }
        }

        // Private class to represent a rectangular region
        private class RectangularRegion
        {
            public int top;
            public int bottom;
            public int right;
            public int left;

            private List<PixelPoint> creation_points;
            public List<PixelPoint> blobs;

            // Get the height of the region
            public int get_height()
            {
                return this.top - this.bottom;
            }

            // Get the width of the region
            public int get_width()
            {
                return this.right - this.left;
            }

            // Get a tuple representing the top, right, bottom, and left values
            public (int top, int right, int bottom, int left) get_tuple()
            {
                return (top, right, bottom, left);
            }

            // Constructor with a single value for initialization
            public RectangularRegion(int value)
            {
                top = bottom = left = right = value;
            }

            // Constructor that defines a rectangular region based on a list of vertices.
            public RectangularRegion(List<PixelPoint> vertices)
            {
                // Ensure the list of vertices is not empty
                if (vertices.Count == 0)
                {
                    throw new Exception("No vertices");
                }

                // Calculate the top, bottom, right, and left boundaries of the region.
                this.creation_points = vertices;
                this.top = vertices.Select(v => v.y).Max();
                this.bottom = vertices.Select(v => v.y).Min();
                this.right = vertices.Select(v => v.x).Max();
                this.left = vertices.Select(v => v.x).Min();
            }


            // Get the list of creation points
            public List<PixelPoint> get_creation_point()
            {
                return creation_points;
            }

            // Calculate the barycenter of the region
            public PixelPoint get_barycentre()
            {
                return new PixelPoint((right + left) / 2, (top + bottom) / 2);
            }

            // Generate and return a list of PixelPoints forming a rectangle
            public List<PixelPoint> generate_rectangle()
            {
                List<PixelPoint> list = new List<PixelPoint>();
                for (int y = bottom; y <= top; y++)
                {
                    list.Add(new PixelPoint(right, y));
                    list.Add(new PixelPoint(left, y));
                }
                for (int x = left; x <= right; x++)
                {
                    list.Add(new PixelPoint(x, top));
                    list.Add(new PixelPoint(x, bottom));
                }
                return list;
            }

            // Convert the region to a Socket (inherited class) with associated blobs
            public Socket toSocket()
            {
                Socket res = new Socket(this);
                res.blobs = blobs;
                return res;
            }
        }

        // Private class representing a Socket (inherits from RectangularRegion)
        private class Socket : RectangularRegion
        {
            public SocketType type;

            // Default constructor
            public Socket() : base(0)
            {
            }

            // Constructor with a SocketType
            public Socket(SocketType type) : base(0)
            {
                this.type = type;
            }

            // Constructor to create a Socket from a RectangularRegion using top left and bottom right corners
            public Socket(RectangularRegion region) : base(new List<PixelPoint>() { new PixelPoint(region.left, region.top), new PixelPoint(region.right, region.bottom) })
            {
                type = SocketType.Unknown;
            }

            // Method to determine the type of the socket based on the number of blobs and their positions
            public void findType()
            {
                switch (blobs.Count)
                {
                    case 2:
                        {
                            // If there are two blobs, initially set the type to Unknown
                            type = SocketType.Unknown;

                            // Calculate the average Y-coordinate of the blobs
                            double avgy = blobs.Select(b => b.y).Average();

                            // Check if all blobs have Y-coordinates close to the average, i.e they are horizontally aligned
                            if (blobs.All(blob => Math.Abs(blob.y - avgy) < 3))
                            {
                                type = SocketType.GermanFrench;// Set the type to German/French
                                break;
                            }
                        }
                        break;
                    case 3:
                        {
                            // If there are three blobs, initially set the type to Unknown
                            type = SocketType.Unknown;

                            // Calculate the average X and Y coordinates of the blobs
                            double avgx = blobs.Select(b => b.x).Average();
                            double avgy = blobs.Select(b => b.y).Average();

                            // Check if all blobs have X-coordinates close to the average, i.e aligned vertically
                            if (blobs.All(blob => Math.Abs(blob.x - avgx) < 3))
                            {
                                type = SocketType.Italian; // Set the type to Italian
                                break;
                            }

                            // Check if all blobs have Y-coordinates close to the average, i.e aligned horizontally
                            if (blobs.All(blob => Math.Abs(blob.y - avgy) < 3))
                            {
                                type = SocketType.HorizontalItalian;
                                break;
                            }

                            // Calculate the distance between the first and second blob
                            double base_distance = blobs[0].euclidian_distance(blobs[1]);

                            // Check if the distances between all pairs of blobs are consistent, i.e equilateral tringle
                            if (Math.Abs(blobs[1].euclidian_distance(blobs[2]) - base_distance) < base_distance * 0.15)
                            {
                                if (Math.Abs(blobs[0].euclidian_distance(blobs[2]) - base_distance) < base_distance * 0.15)
                                {
                                    type = SocketType.British;   // Set the type to British
                                    break;
                                }
                            }
                        }
                        break;
                    default:
                        // For other cases, leave the type as Unknown
                        break;
                }
            }

            // Convert the SocketType to a corresponding color
            public Color typeToColor()
            {
                switch (type)
                {
                    case SocketType.Unknown: return Color.Coral;
                    case SocketType.British: return Color.Green;
                    case SocketType.GermanFrench: return Color.Blue;
                    case SocketType.Italian: return Color.Red;
                    case SocketType.HorizontalItalian: return Color.Violet;
                    default: return Color.DarkOliveGreen;
                }
            }
        }

        // Private class to represent a line (used in some calculations)
        private class Line
        {
            public double theta;
            public double r;

            // Constructor for a line with r and theta
            public Line(double r, double theta)
            {
                this.theta = theta;
                this.r = r;
            }

            // Calculate the absolute difference in theta values between two lines
            public double get_diff(Line l2)
            {
                double abs_diff = Math.Abs(l2.theta - this.theta);
                return abs_diff <= 90 ? abs_diff : (360 - 2 * abs_diff) / 2;
            }

            // Check if two lines are almost the same based on theta and r values
            public bool isAlmostSameAs(Line line1)
            {
                double diff = Math.Abs(line1.theta - this.theta);
                if (diff < 5 && Math.Abs(line1.r - this.r) < 4) return true;
                if (diff > 175 && Math.Abs(line1.r + this.r) < 4) return true;

                return false;
            }

            // Check if two lines are almost parallel based on theta values
            public bool isAlmostParallelWith(Line line1)
            {
                return this.get_diff(line1) < 5;
            }

            // Check if two lines are almost perpendicular based on theta values
            public bool isAlmostPerpendicularWith(Line line1)
            {
                double diff = this.get_diff(line1);
                if (85 < diff && diff < 95) return true;
                return false;
            }

            // Check if two lines are distant and parallel based on theta and r values
            public bool isDistantParallelWith(Line line1)
            {
                double diff = Math.Abs(line1.theta - this.theta);
                if (diff < 5 && Math.Abs(line1.r - this.r) > 10) return true;
                if (diff > 175 && Math.Abs(line1.r + this.r) > 10) return true;
                return false;
            }

            // Check if the line is almost vertical based on theta value
            public bool isAlmostVertical()
            {
                return theta < 5 || theta > 175;
            }

            // Check if the line is almost horizontal based on theta value
            public bool isAlmostHorizontal()
            {
                return 75 < theta && theta < 85;
            }

            // Check if the line is axis-aligned (either vertical or horizontal)
            public bool isAxisAligned()
            {
                return this.isAlmostVertical() || this.isAlmostHorizontal();
            }
        }

        // Private class to represent a 2D pixel point with x and y coordinates
        private class PixelPoint
        {
            public int x { get; set; }
            public int y { get; set; }

            // Constructor for a PixelPoint with x and y coordinates
            public PixelPoint(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            // Calculate the Euclidean distance between two PixelPoints
            public double euclidian_distance(PixelPoint p2)
            {
                return Math.Sqrt(Math.Pow((x - p2.x), 2) + Math.Pow((y - p2.y), 2));
            }

            // Convert the PixelPoint to a string for display
            public String toString()
            {
                return "Point => x: " + x + ", " + y + ")";
            }

            // Move the PixelPoint one unit up
            public PixelPoint Up()
            {
                return new PixelPoint(x, y + 1);
            }

            // Move the PixelPoint one unit down
            public PixelPoint Down()
            {
                return new PixelPoint(x, y - 1);
            }

            // Move the PixelPoint one unit to the left
            public PixelPoint Left()
            {
                return new PixelPoint(x + 1, y);
            }

            // Move the PixelPoint one unit to the right
            public PixelPoint Right()
            {
                return new PixelPoint(x - 1, y);
            }
        }

        // Enum to represent different socket types
        private enum SocketType
        {
            Italian,
            HorizontalItalian,
            British,
            GermanFrench,
            Unknown
        }
    }
}
