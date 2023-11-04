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
        internal enum SEShape
        {
            Square,
            Plus
        }

        private class Segment
        {
            public PixelPoint start;
            public PixelPoint end;
            public void clear()
            {
                start = end = new PixelPoint(-1, -1);
            }
            public void set_start(PixelPoint start)
            {
                this.start = start;

            }
        }

        private class RectangularRegion
        {
            public int top;
            public int bottom;
            public int right;
            public int left;

            private List<PixelPoint> creation_points;
            public List<PixelPoint> blobs;

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

            public RectangularRegion(int value)
            {
                top = bottom = left = right = value;
            }
            public RectangularRegion(List<PixelPoint> vertices)
            {
                if (vertices.Count == 0) throw new Exception("No vertices");
                this.top = this.bottom = vertices[0].y;
                this.right = this.left = vertices[0].x;
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

            public PixelPoint get_baricenter()
            {
                return new PixelPoint((right + left) / 2, (top + bottom) / 2);
            }

            public String toString()
            {
                return "Rectangular region => top: " + top + " bottom: " + bottom + "right: " + right + " left: " + left;
            }
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

            public Socket toSocket()
            {
                Socket res = new Socket(this);
                res.blobs = blobs;
                return res;
            }
        }

        private class Socket : RectangularRegion
        {
            public SocketType type;
            public Socket() : base(0)
            {

            }
            public Socket(SocketType type) : base(0)
            {
                this.type = type;
            }
            public Socket(RectangularRegion region) : base(new List<PixelPoint>() { new PixelPoint(region.left, region.top), new PixelPoint(region.right, region.bottom) })
            {
                type = SocketType.Unknown;
            }
            public void findType()
            {
                switch (blobs.Count)
                {
                    case 2:
                        {
                            type = SocketType.Unknown;
                            double avgy = blobs.Select(b => b.y).Average();
                            if (blobs.All(blob => Math.Abs(blob.y - avgy) < 3))
                            {
                                type = SocketType.GermanFrench;
                                break;
                            }

                        }

                        break;
                    case 3:
                        {
                            type = SocketType.Unknown;
                            double avgx = blobs.Select(b => b.x).Average();
                            double avgy = blobs.Select(b => b.y).Average();
                            if (blobs.All(blob => Math.Abs(blob.x - avgx) < 3))
                            {
                                type = SocketType.Italian;
                                break;
                            }
                            if (blobs.All(blob => Math.Abs(blob.y - avgy) < 3))
                            {
                                type = SocketType.HorizontalItalian;
                                break;
                            }
                            double base_distance = blobs[0].euclidian_distance(blobs[1]);
                            if (Math.Abs(blobs[1].euclidian_distance(blobs[2]) - base_distance) < base_distance * 0.15)
                            {
                                if (Math.Abs(blobs[0].euclidian_distance(blobs[2]) - base_distance) < base_distance * 0.15)
                                {
                                    type = SocketType.British;
                                    break;
                                }
                            }
                        }
                        break;

                    default:

                        break;

                }

            }

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
        private class Line
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
                if (diff > 175 && Math.Abs(line1.r + this.r) < 4) return true;

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
                if (diff < 5 && Math.Abs(line1.r - this.r) > 10) return true;
                if (diff > 175 && Math.Abs(line1.r + this.r) > 10) return true;
                return false;
            }
            public bool isAlmostVertical()
            {
                return theta < 5 || theta > 175;
            }
            public bool isAlmostHorizontal()
            {
                return 75 < theta && theta < 85;
            }
            public bool isAxisAligned()
            {
                return this.isAlmostVertical() || this.isAlmostHorizontal();
            }


        }
        private class PixelPoint
        {
            public int x { get; set; }
            public int y { get; set; }
            public PixelPoint(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public double euclidian_distance(PixelPoint p2)
            {
                return Math.Sqrt(Math.Pow((x - p2.x), 2) + Math.Pow((y - p2.y), 2));
            }

            public String toString()
            {
                return "Point => x: " + x + ", " + y + ")";
            }

            public PixelPoint Up()
            {
                return new PixelPoint(x, y + 1);
            }
            public PixelPoint Down()
            {
                return new PixelPoint(x, y - 1);
            }
            public PixelPoint Left()
            {
                return new PixelPoint(x + 1, y);
            }
            public PixelPoint Right()
            {
                return new PixelPoint(x - 1, y);
            }
        }

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
