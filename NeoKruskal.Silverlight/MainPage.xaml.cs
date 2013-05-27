using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ICSharpCode.SharpZipLib.Zip;
using NeoGraph;


namespace NeoKruskal_Silverlight
{
	public partial class MainPage : UserControl
    {
        #region Private Variables
        private const string filterString = "Graph Data Markup Language (*.gdml)|*.gdml|Compressed Graph Data Markup Language (*.gdmlz)|*.gdmlz";

        private Graph graph;
        private NeoKruskalState state;
        private bool isStartPointSet;
        private Dictionary<Ellipse, Vertex> vertices;
        private Vertex startVertex;
        private Vertex endVertex;
        private Dictionary<Line, Edge> edges;
        private List<Line> solutionEdges;
        private Edge selectedEdge;
        private Line selectedLine;
        private Ellipse selectedEllipse;
        private int iter = 1;
        private double avr;
        private Random rand = new Random();

        #endregion

        public MainPage()
		{
			// Required to initialize variables
			InitializeComponent();

            Init();
		}

        #region Private Methods

        private void Init()
        {
            graph = new Graph();
            state = NeoKruskalState.VertexDrawing;
            isStartPointSet = false;
            vertices = new Dictionary<Ellipse, Vertex>();
            edges = new Dictionary<Line, Edge>();
            solutionEdges = new List<Line>();
        }

        private void DoEdgeDrawing(Vertex point)
        {
            if (!isStartPointSet)
            {
                isStartPointSet = true;
                startVertex = point;
            }
            else
            {
                isStartPointSet = false;
                endVertex = point;
                DoEdgeDrawing(startVertex, endVertex);
            }
        }

        private void DoEdgeDrawing(Vertex a, Vertex b)
        {
            Line line = BuildLine(a, b, Colors.Gray);
            
            Edge edge = new Edge(a, b);
            if (!edges.ContainsValue(edge))
            {
                graph.AddEdge(a, b);
                edges.Add(line, edge);
                AddUIElement(line, -10);
            }
        }

        private void DoVertexDrawing(Point point)
        {
            Ellipse elps = BuildEllipse(point);
            Vertex vert = new Vertex((int)point.X, (int)point.Y);
            if (!vertices.ContainsValue(vert))
            {
                
                graph.AddVertex(vert);
                vertices.Add(elps, vert);

                AddUIElement(elps, 10);
            }
            
        }

        private Ellipse BuildEllipse(Point point)
        {
            Ellipse elps = new Ellipse();
            elps.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Ellipse_MouseLeftButtonDown);
            elps.Width = 20;
            elps.Height = 20;
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            Point pt = point;
            // Describes the brush's color using RGB values. 
            // Each value has a range of 0-255.
            mySolidColorBrush.Color = Colors.Red;
            elps.Fill = mySolidColorBrush;
            Canvas.SetTop(elps, pt.Y - 10);
            Canvas.SetLeft(elps, pt.X - 10);

            return elps;
        }

        private Line BuildLine(Vertex a, Vertex b, Color color)
        {
            Line myLine = new Line();
            myLine.Stroke = new SolidColorBrush(color);
            //myLine.MouseEnter += new System.Windows.Input.MouseEventHandler(myLine_MouseEnter);
            if(color == Colors.Gray)
                myLine.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Line_MouseLeftButtonDown);
            myLine.StrokeThickness = 7;
            myLine.X1 = a.X;
            myLine.Y1 = a.Y;

            myLine.X2 = b.X;
            myLine.Y2 = b.Y;

            return myLine;
        }

        private void DrawGraph()
        {
            foreach (Vertex v in graph.VertexList)
            {
                Ellipse el = BuildEllipse(v.Location);
                vertices.Add(el, v);
                AddUIElement(el, 10);
            }
            foreach (Edge e in graph.EdgeList)
            {
                Line line = BuildLine(e.VertexFirst, e.VertexSecond, Colors.Gray);
                edges.Add(line, e);
                AddUIElement(line, -10);
            }
        }

        private void ClearSolution()
        {
            foreach (Line line in solutionEdges)
            {
                GraphCanvas.Children.Remove(line);
            }
            solutionEdges.Clear();
        }

        private void ShowCost()
        {
            double total = 0.0;
            foreach (Edge e in graph.EdgeSolution)
                total += e.Length;
            txtCost.Text = total.ToString();
        }

        private void DrawSolutionEdges()
        {
            ClearSolution();
            foreach (Line line in edges.Keys)
                line.Stroke = new SolidColorBrush(Colors.Transparent);

            foreach (Edge edge in graph.EdgeSolution)
            {
                Line line = BuildLine(edge.VertexFirst, edge.VertexSecond, Colors.Blue);
                solutionEdges.Add(line);
                AddUIElement(line, -1);
            }
        }

        private void AddUIElement(UIElement element, int index)
        {
            Canvas.SetZIndex(element, index);
            GraphCanvas.Children.Add(element);
        }

        private void ClearLines()
        {
            foreach (Line line in edges.Keys)
            {
                GraphCanvas.Children.Remove(line);
            }
            edges.Clear();
            //foreach (Line line in lines)
            //    GraphCanvas.Children.Remove(line);
            //lines.Clear();
        }

        private void ClearAll()
        {
            GraphCanvas.Children.Clear();
            vertices.Clear();
            edges.Clear();
            solutionEdges.Clear();
            graph.Clear();
        }

        private void ClearEllipses()
        {
            foreach (Ellipse el in vertices.Keys)
            {
                GraphCanvas.Children.Remove(el);
            }
            vertices.Clear();
            //foreach (Ellipse el in ellips)
            //    GraphCanvas.Children.Remove(el);
            //ellips.Clear();
        }

        #endregion

        #region File Handling

        private void WriteObjectToJson(Object obj, Stream stream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Graph));
            serializer.WriteObject(stream, graph);
            stream.Close();
        }

        private void WriteGdml(Object obj, Stream stream)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Graph));
            serializer.WriteObject(stream, graph);
            stream.Close();
        }

        private void OpenObjectFromJson(Object obj, Stream stream)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Graph));
            ClearAll();
            graph = (Graph)serializer.ReadObject(stream);
            graph.ClearSolution();
            stream.Close();
            DrawGraph();
        }

        private void OpenGdml(Object obj, Stream stream)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Graph));
            ClearAll();
            graph = (Graph)serializer.ReadObject(stream);
            graph.ClearSolution();
            stream.Close();
            DrawGraph();
        }

        private void WriteGdmlz(Object obj, Stream stream, string filename)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Graph));
            Stream memStream = new MemoryStream();
            serializer.WriteObject(memStream, graph);
            ZipOutputStream zipOutStream = new ZipOutputStream(stream);
            //StreamReader sr = new StreamReader(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            //Stream sr = memStream;
            ZipEntry zipEntry = new ZipEntry(filename);
            zipOutStream.PutNextEntry(zipEntry);
            byte[] buffer = new byte[4096];
            int size;
            do
            {
                
                size = memStream.Read(buffer, 0, buffer.Length);
                zipOutStream.Write(buffer, 0, size);
            } while (size > 0);

            memStream.Close();
            zipOutStream.Close();
            stream.Close();
        }

        private void OpenGdmlz(Object obj, Stream stream)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Graph));
            ZipInputStream zipInputStream = new ZipInputStream(stream);
            ZipEntry zipEntry = zipInputStream.GetNextEntry();
            MemoryStream memStream = new MemoryStream();

            byte[] buffer = new byte[4096];
            int size;
            do
            {
                size = zipInputStream.Read(buffer, 0, buffer.Length);
                memStream.Write(buffer, 0, size);
            } while (size > 0);
            
            ClearAll();
            memStream.Seek(0, SeekOrigin.Begin);
            graph = (Graph)serializer.ReadObject(memStream);           
            graph.ClearSolution();
            DrawGraph();

            memStream.Close();
            zipInputStream.Close();
            stream.Close();
        }

        #endregion

        #region Event Handlers

        void Line_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isStartPointSet = false;
            if (state == NeoKruskalState.EdgeDrawing)
            {
                Line line = (Line)sender;
                if(selectedLine!=null)
                    selectedLine.Stroke = new SolidColorBrush(Colors.Gray);
                selectedLine = line;
                selectedLine.Stroke = new SolidColorBrush(Colors.Yellow);
                selectedEdge = edges[line];
                txtLineLength.Text = selectedEdge.Length.ToString() ;
            }
        }

        void Ellipse_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (state == NeoKruskalState.EdgeDrawing)
            {
                Ellipse elps = (Ellipse)sender;
                elps.Fill = new SolidColorBrush(Colors.Orange);
                if (isStartPointSet && vertices[elps] == startVertex)
                {
                    isStartPointSet = false;
                    if(selectedEllipse!=null)
                        selectedEllipse.Fill = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    DoEdgeDrawing(vertices[elps]);
                    if (isStartPointSet)
                        selectedEllipse = elps;
                    else
                    {
                        if (selectedEllipse != null)
                            selectedEllipse.Fill = new SolidColorBrush(Colors.Red);
                        elps.Fill = new SolidColorBrush(Colors.Red);
                    }
                }
            }
        }

        private void tglButton_Click(object sender, RoutedEventArgs e)
        {
            if (tglButton.IsChecked == true)
            {
                tglButton.Content = "Vertex";
                state = NeoKruskalState.VertexDrawing;
                if (selectedEllipse != null)
                    selectedEllipse.Fill = new SolidColorBrush(Colors.Red);
                if (selectedLine != null)
                    selectedLine.Stroke = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                tglButton.Content = "Edge";
                state = NeoKruskalState.EdgeDrawing;
                
            }
            isStartPointSet = false;
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in GraphCanvas.Children)
            {
                Canvas.SetTop(element, Canvas.GetTop(element) + 20.0);
            }
            foreach (Vertex v in graph.VertexList)
                v.Y+=20;
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in GraphCanvas.Children)
            {
                Canvas.SetLeft(element, Canvas.GetLeft(element) - 20.0);
            }
            foreach (Vertex v in graph.VertexList)
                v.X -= 20;
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in GraphCanvas.Children)
            {
                Canvas.SetTop(element, Canvas.GetTop(element) - 20.0);
            }
            foreach (Vertex v in graph.VertexList)
                v.Y -= 20;
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in GraphCanvas.Children)
            {
                Canvas.SetLeft(element, Canvas.GetLeft(element) + 20.0);
            }
            foreach (Vertex v in graph.VertexList)
                v.X += 20;
        }

        private void btnKruskal_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                avr = 0.0;
                for (int i = 0; i < iter; i++)
                {
                    DateTime start = DateTime.Now;
                    graph.GenerateKruskalMST();
                    avr += (DateTime.Now - start).TotalMilliseconds;
                }

                //txtRunningTime.Text = (DateTime.Now - start).TotalMilliseconds + " ms";
                avr = avr / iter;
                txtRunningTime.Text = avr + " ms";
                ShowCost();
                DrawSolutionEdges();
            }));
        }

        private void btnOurKruskal_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                double max = Double.MinValue;
                double min = Double.MaxValue;
                foreach (Edge edge in graph.EdgeList)
                {
                    if (edge.Length > max)
                        max = edge.Length;
                    if (edge.Length < min)
                        min = edge.Length;
                }

                avr = 0.0;
                for (int i = 0; i < iter; i++)
                {
                    DateTime start = DateTime.Now;
                    graph.GenerateOurKruskalMST(Int32.Parse(txtBucket.Text), max, min);
                    avr += (DateTime.Now - start).TotalMilliseconds;
                }
                //txtRunningTime.Text = (DateTime.Now - start).TotalMilliseconds + " ms";
                avr = avr / iter;
                txtRunningTime.Text = avr + " ms";
                ShowCost();
                DrawSolutionEdges();
            }));
        }

        private void btnOurQmwc_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                double max = Double.MinValue;
                double min = Double.MaxValue;
                foreach (Edge edge in graph.EdgeList)
                {
                    if (edge.Length > max)
                        max = edge.Length;
                    if (edge.Length < min)
                        min = edge.Length;
                }

                avr = 0.0;
                for (int i = 0; i < iter; i++)
                {
                    DateTime start = DateTime.Now;
                    graph.GenerateOurNeoKruskalMST(Int32.Parse(txtBucket.Text), max, min);
                    avr += (DateTime.Now - start).TotalMilliseconds;
                }
                //txtRunningTime.Text = (DateTime.Now - start).TotalMilliseconds + " ms";
                avr = avr / iter;
                txtRunningTime.Text = avr + " ms";
                ShowCost();
                DrawSolutionEdges();
            }));
        }

        private void btnPrim_Click(object sender, RoutedEventArgs e)
        {
            
            Dispatcher.BeginInvoke(new Action(delegate
            {
                avr = 0.0;

                double[,] matrix = new double[graph.VertexList.Count, graph.VertexList.Count];
                foreach (Edge edge in graph.EdgeList)
                {
                    int a = graph.VertexList.IndexOf(edge.VertexFirst);
                    int b = graph.VertexList.IndexOf(edge.VertexSecond);
                    matrix[a, b] = matrix[b, a] = edge.Length;
                }

                for (int i = 0; i < iter; i++)
                {
                    DateTime start = DateTime.Now;   
                    graph.GeneratePrimMst(matrix);
                    avr += (DateTime.Now - start).TotalMilliseconds;
                    //txtRunningTime.Text = (DateTime.Now - start).TotalMilliseconds + " ms";
                }
                avr = avr / iter;
                txtRunningTime.Text = avr + " ms";
                ShowCost();
                DrawSolutionEdges();
            }));
        }

        private void GridRoot_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // TODO: Add event handler implementation here.
            switch (state)
            {
                case NeoKruskalState.VertexDrawing:
                    DoVertexDrawing(e.GetPosition(GraphCanvas));
                    break;
                case NeoKruskalState.EdgeDrawing:
                    //GraphCanvas.RenderTransform.Transform(new Point(100, 100));
                    break;
                default: break;
            }
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                ClearAll();
                int total = Int32.Parse(txtRandom.Text);

                graph.Clear();

                while (total > 0)
                {
                    total--;

                    //MessageBox.Show(GraphCanvas.ActualWidth + "   " + Height);
                    DoVertexDrawing(new Point(rand.Next((int)(GraphCanvas.ActualWidth - 8.0)), rand.Next((int)(GraphCanvas.ActualHeight - 8.0))));
                }
                for (int i = 0; i < graph.VertexList.Count - 1; i++)
                    for (int j = i + 1; j < graph.VertexList.Count; j++)
                    {
                        graph.AddEdge(graph.VertexList[i], graph.VertexList[j]);
                        //Edge e = new Edge();
                        //DoEdgeDrawing(graph.VertexList[i], graph.VertexList[j]);
                    }
            }
              ));


        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEdge != null)
                selectedEdge.Length = Double.Parse(txtLineLength.Text);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }

        private void btnOriginal_Click(object sender, RoutedEventArgs e)
        {
            ClearSolution();
            graph.ClearSolution();
            foreach (Line line in edges.Keys)
            {
                //AddUIElement(line, -10);
                line.Stroke = new SolidColorBrush(Colors.Gray);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = filterString;
            saveFileDialog.FilterIndex = 2;
            if (saveFileDialog.ShowDialog() == true)
            {
                switch (saveFileDialog.FilterIndex)
                {
                    case 1:
                        WriteGdml(graph, saveFileDialog.OpenFile());
                        break;
                    case 2:
                        string filename = saveFileDialog.SafeFileName;
                        WriteGdmlz(graph, saveFileDialog.OpenFile(), filename.Remove(filename.Length-1));
                        break;
                    default: break;
                }
                
                //WriteObjectToJson(graph, saveFileDialog.OpenFile());
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filterString;
            openFileDialog.FilterIndex = 2;
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true)
            {
                switch (openFileDialog.FilterIndex)
                {
                    case 1:
                        OpenGdml(graph, openFileDialog.File.OpenRead());
                        break;
                    case 2:
                        OpenGdmlz(graph, openFileDialog.File.OpenRead());
                        break;
                    default: break;

                }
                
                //OpenObjectFromJson(graph, openFileDialog.File.OpenRead());
            }
        }

        #endregion
    }

    public enum NeoKruskalState
    {
        VertexDrawing,
        EdgeDrawing
    }
}