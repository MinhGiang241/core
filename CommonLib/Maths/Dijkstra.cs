namespace CommonLibCore.CommonLib.Maths
{
    public class Dijkstra
    {
        public int V;

        public Dijkstra(int vertekNum)
        {
            V = vertekNum;
        }

        int minDistance(double[] dist, bool[] sptSet)
        {
            // Initialize min value 
            double min = int.MaxValue;
            int min_index = -1;
            //Console.WriteLine($"initial min value={min}");
            for (int v = 0; v < V; v++)
                if (sptSet[v] == false && dist[v] <= min)
                {
                    min = dist[v];
                    min_index = v;
                }
            //Console.WriteLine($"mindistance for vertex {min_index} is {min}");
            return min_index;
        }

        /// <summary>
        /// trả về danh sách các index định tuyến từ đỉnh nguồn tới đỉnh đích dựa trên các thằng hàng xóm đã biết từ thuật toán dijkstra kèm các khoảng cách của từng node tới node nguồn
        /// </summary>
        /// <param name="dest"></param>
        /// <returns></returns>
        public double[][] findRoute(int dest, double[][] spt)
        {
            //danh sách này là mutable
            List<double[]> result = new List<double[]>();
            double[] nextNode = null;
            while (dest >= 0)
            {
                nextNode = spt[dest];
                double[] nodeInfo = new double[] { dest, nextNode[1] };
                result.Add(nodeInfo);
                dest = (int)nextNode[0];
            }
            result.Reverse();
            return result.ToArray();
        }

        /// <summary>
        /// kết quả trả về là mảng các node liền kề+khoảng cách so với node gốc
        /// </summary>
        /// <param name="graph">vecto các khoảng cách giữa các node mạng</param>
        /// <param name="src">index của node gốc trong ma trận graph</param>
        /// <returns></returns>
        public double[][] dijkstra(double[,] graph, int src)
        {
            double[] dist = new double[V]; // The output array. dist[i] 
                                           // will hold the shortest 
                                           // distance from src to i 

            //thêm thằng này để tính toán ông hàng xóm khi tính được kết quả cuối cùng, mặc định ông hàng xóm có giá trị -1 (tức là chưa tìm ra) giá trị -1 (tức là chưa tìm ra)tìm ra) giá trị -1 (tức là chưa tìm ra)
            double[][] neighbor = new double[V][];
            // sptSet[i] will true if vertex 
            // i is included in shortest path 
            // tree or shortest distance from 
            // src to i is finalized 
            bool[] sptSet = new bool[V];

            // Initialize all distances as 
            // INFINITE and stpSet[] as false 
            for (int i = 0; i < V; i++)
            {
                dist[i] = int.MaxValue;
                sptSet[i] = false;
            }
            for (int i = 0; i < V; i++)
            {
                neighbor[i] = new double[] { -1, int.MaxValue };
            }

            // Distance of source vertex 
            // from itself is always 0 
            dist[src] = 0;
            neighbor[src] = new double[] { -1, 0 };
            // Find shortest path for all vertices 
            for (int count = 0; count < V - 1; count++)
            {
                // Pick the minimum distance vertex 
                // from the set of vertices not yet 
                // processed. u is always equal to 
                // src in first iteration. 
                int u = minDistance(dist, sptSet);

                // Mark the picked vertex as processed 
                sptSet[u] = true;

                // Update dist value of the adjacent 
                // vertices of the picked vertex. 
                for (int v = 0; v < V; v++)

                    // Update dist[v] only if is not in 
                    // sptSet, there is an edge from u 
                    // to v, and total weight of path 
                    // from src to v through u is smaller 
                    // than current value of dist[v] 
                    if (!sptSet[v] && graph[u, v] != 0 &&
                         dist[u] != int.MaxValue && dist[u] + graph[u, v] < dist[v])
                    {
                        dist[v] = dist[u] + graph[u, v];
                        neighbor[v][0] = u;
                        neighbor[v][1] = dist[u] + graph[u, v];
                    }
            }
            return neighbor;

        }


    }
}
