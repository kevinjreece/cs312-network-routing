using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetworkRouting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            int randomSeed = int.Parse(randomSeedBox.Text);
            int size = int.Parse(sizeBox.Text);

            Random rand = new Random(randomSeed);
            seedUsedLabel.Text = "Random Seed Used: " + randomSeed.ToString();

            this.adjacencyList = generateAdjacencyList(size, rand);
            List<PointF> points = generatePoints(size, rand);
            resetImageToPoints(points);
            this.points = points;
            src = -1;
            dst = -1;
            sourceNodeBox.Text = "";
            targetNodeBox.Text = "";
        }

        // Generates the distance matrix.  Values of -1 indicate a missing edge.  Loopbacks are at a cost of 0.
        private const int MIN_WEIGHT = 1;
        private const int MAX_WEIGHT = 100;
        private const double PROBABILITY_OF_DELETION = 0.35;

        private const int NUMBER_OF_ADJACENT_POINTS = 3;

        private List<HashSet<int>> generateAdjacencyList(int size, Random rand)
        {
            List<HashSet<int>> adjacencyList = new List<HashSet<int>>();

            for (int i = 0; i < size; i++)
            {
                HashSet<int> adjacentPoints = new HashSet<int>();
                while (adjacentPoints.Count < 3)
                {
                    int point = rand.Next(size);
                    if (point != i) adjacentPoints.Add(point);
                }
                adjacencyList.Add(adjacentPoints);
            }

            return adjacencyList;
        }

        private List<PointF> generatePoints(int size, Random rand)
        {
            List<PointF> points = new List<PointF>();
            for (int i = 0; i < size; i++)
            {
                points.Add(new PointF((float) (rand.NextDouble() * pictureBox.Width), (float) (rand.NextDouble() * pictureBox.Height)));
            }
            return points;
        }

        private void resetImageToPoints(List<PointF> points)
        {
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics graphics = Graphics.FromImage(pictureBox.Image);
            foreach (PointF point in points)
            {
                graphics.DrawEllipse(new Pen(Color.Blue), point.X, point.Y, 2, 2);
            }

            this.graphics = graphics;
            pictureBox.Invalidate();
        }

        // These variables are instantiated after the "Generate" button is clicked
        private List<PointF> points = new List<PointF>();
        private Graphics graphics;
        private List<HashSet<int>> adjacencyList;

        private void TestDrawing()
        {
            Pen pen = new Pen(Color.Black);
            for (int i = 0; i < points.Count(); i++)
            {
                PointF p1 = points[i];
                for (int j = 0; j < adjacencyList[i].Count(); j++)
                {
                    PointF p2 = points[adjacencyList[i].ElementAt(j)];
                    graphics.DrawLine(pen, p1, p2);
                }
            }
            pictureBox.Refresh();
        }

        private void TestDist()
        {
            Pen pen = new Pen(Color.Black);
            Font font = new Font("Arial", 8);
            SolidBrush brush = new SolidBrush(Color.Black);
            PointF p0 = points.ElementAt(src);
            for (int i = 0; i < adjacencyList[src].Count(); i++)
            {
                int dest_id = adjacencyList[src].ElementAt(i);
                PointF p1 = points[dest_id];
                double dist = GetDist(p0, p1);
                Console.WriteLine("Distance between node " + src + " " + p0 + " and node " + dest_id + " " + p1 + " is " + dist);
                graphics.DrawLine(pen, p0, p1);
                graphics.DrawString(dist.ToString("#.##"), font, brush, GetMidPoint(p0, p1));
            }
            pictureBox.Refresh();
        }

        private double GetDist(PointF p1, PointF p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private PointF GetMidPoint(PointF p1, PointF p2)
        {
            PointF left;
            PointF right;
            PointF top;
            PointF bottom;

            if (p1.X < p2.X)
            {
                left = p1;
                right = p2;
            }
            else
            {
                left = p2;
                right = p1;
            }

            if (p1.Y < p2.Y)
            {
                top = p1;
                bottom = p2;
            }
            else
            {
                top = p2;
                bottom = p1;
            }

            return new PointF(left.X + ((right.X - left.X) / 2), top.Y + ((bottom.Y - top.Y) / 2));
        }

        public void OnePathDijkstra()
        {
            Console.WriteLine("OnePathDijkstra");
            int n_nodes = points.Count();
            PriorityQueue q = new PriorityQueue(n_nodes);
            double[] src_dist = new double[n_nodes];
            int[] prev = new int[n_nodes];
            int i;

            for (i = 0; i < n_nodes; i++)
            {
                src_dist[i] = double.MaxValue;
                prev[i] = -1;
            }

            src_dist[src] = 0;
            prev[src] = -1;

            q.Insert(src, src_dist[src]);

            while (!q.IsEmpty())
            {
                int id = q.PopMin();
                if (id == dst) { break; }
                HashSet<int> adj_nodes = adjacencyList[id];
                for (i = 0; i < adj_nodes.Count(); i++)
                {
                    int temp_id = adj_nodes.ElementAt(i);
                    double temp_dist = src_dist[id] + GetDist(points[id], points[temp_id]);
                    //Console.WriteLine("Distance from " + id + " to " + temp_id + " is " + temp_dist);

                    if (src_dist[temp_id] == double.MaxValue) // If the node has NOT been visited
                    {
                        src_dist[temp_id] = temp_dist;
                        prev[temp_id] = id;

                        if (!q.Insert(temp_id, src_dist[temp_id]))
                        {
                            Console.WriteLine("ERROR inserting id " + temp_id + " connected to id " + id);
                            return;
                        }
                        //Console.WriteLine("queue:\n" + q.ToString());
                    }
                    else if (temp_dist < src_dist[temp_id]) // If the node HAS been visited and the temp distance is less than the previous distance
                    {
                        src_dist[temp_id] = temp_dist;
                        prev[temp_id] = id;

                        if (!q.ReduceVal(temp_id, temp_dist))
                        {
                            Console.WriteLine("ERROR reducing id " + temp_id + " connected to id " + id);
                            return;
                        }
                        //Console.WriteLine("queue:\n" + q.ToString());
                    }
                }
            }

            if (src_dist[dst] != double.MaxValue)
            {
                Pen pen = new Pen(Color.Red);
                int temp = dst;
                while (prev[temp] != -1)
                {
                    graphics.DrawLine(pen, points[temp], points[prev[temp]]);
                    temp = prev[temp];
                }
                pictureBox.Refresh();
                Console.WriteLine("Distance to destination is " + src_dist[dst]);
            }
            else
            {
                Console.WriteLine("Destination is unreachable");
            }
        }

        public void AllPathDijkstra()
        {

        }

        // Use this to generate routing tables for every node
        private void solveButton_Click(object sender, EventArgs e)
        {
            // *** Implement this method, use the variables "startNodeIndex" and "stopNodeIndex" as the indices for your start and stop points, respectively ***
            //TestDrawing();
            //TestDist();

            OnePathDijkstra();

            

            //PriorityQueue.TestPriorityQueue();
        }

        private void sourceNodeBox_LostFocus(object sender, EventArgs e)
        {
            if (sourceNodeBox.Text == "") { return; }

            src = int.Parse(sourceNodeBox.Text);
            resetImageToPoints(points);
            paintStartStopPoints();
        }

        private void targetNodeBox_LostFocus(object sender, EventArgs e)
        {
            if (targetNodeBox.Text == "") { return; }

            dst = int.Parse(targetNodeBox.Text);
            resetImageToPoints(points);
            paintStartStopPoints();
        }

        private Boolean startStopToggle = true;
        private int src = -1;
        private int dst = -1;
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (points.Count > 0)
            {
                Point mouseDownLocation = new Point(e.X, e.Y);
                int index = ClosestPoint(points, mouseDownLocation);
                if (startStopToggle)
                {
                    src = index;
                    sourceNodeBox.Text = "" + index;
                }
                else
                {
                    dst = index;
                    targetNodeBox.Text = "" + index;
                }
                startStopToggle = !startStopToggle;

                resetImageToPoints(points);
                paintStartStopPoints();
            }
        }

        private void paintStartStopPoints()
        {
            if (src > -1)
            {
                Graphics graphics = Graphics.FromImage(pictureBox.Image);
                graphics.DrawEllipse(new Pen(Color.Green, 6), points[src].X, points[src].Y, 1, 1);
                this.graphics = graphics;
                pictureBox.Invalidate();
            }

            if (dst > -1)
            {
                Graphics graphics = Graphics.FromImage(pictureBox.Image);
                graphics.DrawEllipse(new Pen(Color.Red, 2), points[dst].X - 3, points[dst].Y - 3, 8, 8);
                this.graphics = graphics;
                pictureBox.Invalidate();
            }
        }

        private int ClosestPoint(List<PointF> points, Point mouseDownLocation)
        {
            double minDist = double.MaxValue;
            int minIndex = 0;

            for (int i = 0; i < points.Count; i++)
            {
                double dist = Math.Sqrt(Math.Pow(points[i].X-mouseDownLocation.X,2) + Math.Pow(points[i].Y - mouseDownLocation.Y,2));
                if (dist < minDist)
                {
                    minIndex = i;
                    minDist = dist;
                }
            }

            return minIndex;
        }
    }
}
