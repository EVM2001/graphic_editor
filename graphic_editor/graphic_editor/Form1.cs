using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace graphic_editor
{
    public partial class Form1 : Form
    {
        Graphics g;
        Bitmap myBitmap;
        List<PointF> VertexList = new List<PointF>();//список точек одной фигуры
        List<Figure> ListOfFigures = new List<Figure>();//список фигур
        int PointCounter, FigureCounter;
        Point pictureBox1MousePosition = new Point();
        Pen DrawPen = new Pen(Color.Black);
        int PrimitiveType, GeometricOperationType, TMOtype;
        int mode = 0;//1-рисование, 2-геометрические операции, 3-ТМО
        bool rightclick = false;//нужно для отрисовки кривой Безье
        bool checkPrimitive = false; //показывает, выделен ли примитив
        int Current;//индекс выделенного примитива
        Point Center;//координаты центра выделенного примитива
        Pen Xpen = new Pen(Color.Red);
        bool Figure1captured = false;//переменная для реализации ТМО
        int Figure1index, Figure2index;//индекс первой и второй фигур, учавствующих в ТМО
        int MirrorPointCount = 0;
        Point MirrorPoint1, MirrorPoint2 = new Point();//координаты двух точек прямой общего положения        
        public Form1()
        {
            InitializeComponent();
            myBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(myBitmap);

        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)//обработчик изменения "Примитивы"
        {
            comboBox2.Text = "Преобразования";
            comboBox3.Text = "ТМО";
            PrimitiveType = comboBox1.SelectedIndex;
            mode = 1;
            Figure1captured = false;//отмена выделения фигуры для ТМО
            MirrorPointCount = 0;//отмена операции отражения
            VertexList.Clear();
            PointCounter = 0;
            g.Clear(pictureBox1.BackColor);
            DrawAllFigures(ListOfFigures);
            pictureBox1.Image = myBitmap;
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)//обработчик изменения "Преобразования"
        {
            if (FigureCounter == 0)
            {
                comboBox2.Text = "Преобразования";
                MessageBox.Show($"Сначала нарисуйте хотябы одну фигуру");
                mode = 0;
            }
            else
            {                
                GeometricOperationType = comboBox2.SelectedIndex;
                mode = 2;
            }
            comboBox1.Text = "Примитивы";
            comboBox3.Text = "ТМО";
            Figure1captured = false;
            MirrorPointCount = 0;
            VertexList.Clear();
            PointCounter = 0;
            g.Clear(pictureBox1.BackColor);
            DrawAllFigures(ListOfFigures);
            pictureBox1.Image = myBitmap;
        }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)//обработчик изменения "ТМО"
        {
            if (FigureCounter < 2)
            {
                comboBox3.Text = "ТМО";               
                MessageBox.Show($"Сначала нарисуйте хотябы две фигуры");
                mode = 0;
            }
            else
            {                
                TMOtype = comboBox3.SelectedIndex;
                mode = 3;
            }
            comboBox1.Text = "Примитивы";
            comboBox2.Text = "Преобразования";
            Figure1captured = false;
            MirrorPointCount = 0;
            VertexList.Clear();
            PointCounter = 0;
            g.Clear(pictureBox1.BackColor);
            DrawAllFigures(ListOfFigures);
            pictureBox1.Image = myBitmap;
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)//обработчик нажатия кнопок мыши в области pictureBox1
        {
            pictureBox1MousePosition = e.Location;
            if (checkBox1.Checked)//если включен режим удаления
            {
                if (e.Button == MouseButtons.Right)//фигуры удалются при нажатии ПКМ по ним
                {
                    int n = SelectPrimitive(pictureBox1MousePosition);//индекс фигуры
                    if (checkPrimitive)//если фигура выбрана
                    {
                        if (ListOfFigures[n].TMOfigure != -1)//если фигура участвует в ТМО
                        {
                            int f = ListOfFigures[n].TMOfigure;
                            if (n > ListOfFigures[n].TMOfigure)//если индекс выбранной фигуры больше индекса связанной с ней фигурой
                            {
                                ListOfFigures.RemoveAt(ListOfFigures[n].TMOfigure);//удаление связанной фигуры
                                ListOfFigures.RemoveAt(n - 1);//удаление выбранной фигуры
                                for (int i = 0; i < ListOfFigures.Count; i++)//изменение поля TMOfigure у всех других фигур, участвующих в ТМО и изменивших свой индекс в результате удаления
                                {
                                    if (ListOfFigures[i].TMOfigure != -1)
                                    {
                                        if (ListOfFigures[i].TMOfigure > f & ListOfFigures[i].TMOfigure < n) ListOfFigures[i].TMOfigure -= 1;
                                        else if (ListOfFigures[i].TMOfigure > f & ListOfFigures[i].TMOfigure > n) ListOfFigures[i].TMOfigure -= 2;
                                    }
                                }
                            }
                            else//если индекс выбранной фигуры меньше индекса связанной с ней фигурой
                            {
                                ListOfFigures.RemoveAt(ListOfFigures[n].TMOfigure);
                                ListOfFigures.RemoveAt(n);
                                for (int i = 0; i < ListOfFigures.Count; i++)
                                {
                                    if (ListOfFigures[i].TMOfigure != -1)
                                    {
                                        if (ListOfFigures[i].TMOfigure > n & ListOfFigures[i].TMOfigure < f) ListOfFigures[i].TMOfigure -= 1;
                                        else if (ListOfFigures[i].TMOfigure > n & ListOfFigures[i].TMOfigure > f) ListOfFigures[i].TMOfigure -= 2;
                                    }
                                }
                            }
                            FigureCounter -= 2;
                        }
                        else//если удаление фигуры, не участвующей в ТМО
                        {
                            ListOfFigures.RemoveAt(n);
                            for (int i = 0; i < ListOfFigures.Count; i++)//изменение поля TMOfigure у всех других фигур, участвующих в ТМО и изменивших свой индекс в результате удаления
                            {
                                if (ListOfFigures[i].TMOfigure != -1)
                                {
                                    if (ListOfFigures[i].TMOfigure > n) ListOfFigures[i].TMOfigure -= 1;
                                }
                            }
                            FigureCounter--;
                        }
                        g.Clear(pictureBox1.BackColor);
                        DrawAllFigures(ListOfFigures);
                    }
                }
            }
            else
            {
                if (mode == 1)//если включен режим рисования
                {
                    if (e.Button == MouseButtons.Right)
                        rightclick = true;
                    else
                        g.DrawEllipse(DrawPen, e.X - 2, e.Y - 2, 5, 5);
                    DrawPrimitive();
                }
                else if (mode == 2)// если включен режим преобразования 
                {                    
                    if (GeometricOperationType == 3)//если выбрано отражение
                    {
                        if (e.Button == MouseButtons.Right)
                        {                            
                            switch (MirrorPointCount)
                            {                                
                                case 0:
                                    {
                                        MirrorPoint1 = new Point(e.X, e.Y);//первая точка прямой общего положения
                                        g.DrawEllipse(DrawPen, e.X - 2, e.Y - 2, 5, 5);
                                        MirrorPointCount++;
                                        break;
                                    }
                                case 1:
                                    {
                                        MirrorPoint2 = new Point(e.X, e.Y);//вторая точка общего положения
                                        if (ListOfFigures[Current].TMOfigure != -1)//если фигура участвует в ТМО
                                            ListOfFigures[ListOfFigures[Current].TMOfigure].Mirror(MirrorPoint1, MirrorPoint2);//отражается связанная с ней фигура                                        
                                        ListOfFigures[Current].Mirror(MirrorPoint1, MirrorPoint2);//отражается сама фигура
                                        g.Clear(pictureBox1.BackColor);
                                        g.DrawEllipse(DrawPen, MirrorPoint1.X, MirrorPoint1.Y, 5, 5);//зарисовывается прямая общего положения
                                        g.DrawEllipse(DrawPen, MirrorPoint2.X, MirrorPoint2.Y, 5, 5);
                                        g.DrawLine(DrawPen, MirrorPoint1, MirrorPoint2);
                                        DrawAllFigures(ListOfFigures);
                                        MirrorPointCount = 0;                                       
                                        break;
                                    }
                            }
                        }
                    }
                    if (e.Button == MouseButtons.Left)// находим индекс и центр фигуры, выбранной пользователем(выбор осуществляется нажатием лкм в зоне фигуры)
                    {
                        int n = SelectPrimitive(pictureBox1MousePosition);
                        if (checkPrimitive)
                        {
                            Current = n;//запоминается индекс выбранной фигуры
                            Center = ListOfFigures[Current].FindCenter();//находится центр выбранной фигуры
                            MirrorPointCount = 0;//отмена операции отражения
                            g.Clear(pictureBox1.BackColor);
                            DrawAllFigures(ListOfFigures);
                            DrawX(pictureBox1MousePosition);//зарисовка перекрестия                         
                        }
                    }
                }
                else if (mode == 3)// если включен режим ТМО
                {
                    if (e.Button == MouseButtons.Left)
                    {                       
                        if (!Figure1captured)//если первая фигура не выбрана
                        {
                            Figure1index = SelectPrimitive(pictureBox1MousePosition);
                            if (checkPrimitive)
                            {
                                if (ListOfFigures[Figure1index].TMOfigure != -1)//если пользователь пытается выбрать результат ТМО в качестве операнда ТМО
                                {
                                    MessageBox.Show($"С результатом ТМО нельзя проводить ТМО");
                                }
                                else if (ListOfFigures[Figure1index].LineOrCurve)//если пользователь пытается выбрать кривую или отрезок в качестве операнда ТМО
                                {
                                    MessageBox.Show($"С данным примитивом нельзя проводить ТМО");
                                }
                                else
                                {                                    
                                    Figure1captured = true;
                                    DrawX(pictureBox1MousePosition);//зарисовка перекрестия                                  
                                }
                            }
                        }
                        else//если первая фигура выбрана
                        {
                            Figure2index = SelectPrimitive(pictureBox1MousePosition);
                            if (checkPrimitive)
                            {
                                if (ListOfFigures[Figure2index].TMOfigure != -1)
                                {
                                    MessageBox.Show($"С результатом ТМО нельзя проводить ТМО");
                                }
                                else if (ListOfFigures[Figure2index].LineOrCurve)
                                {
                                    MessageBox.Show($"С данным примитивом нельзя проводить ТМО");
                                }
                                else if (Figure1index == Figure2index)//если пользователь пытается выбрать в качестве второй фигуры первую фигуру
                                {
                                    MessageBox.Show($"Выберите два разных примитива");
                                }
                                else
                                {
                                    ListOfFigures[Figure1index].FigureColor = DrawPen.Color;//задание цвета результату ТМО
                                    ListOfFigures[Figure1index].TMOfigure = Figure2index;//у первой фигуры запоминается индекс второй
                                    ListOfFigures[Figure1index].TMOtype = TMOtype;//запоминание типа ТМО
                                    ListOfFigures[Figure1index].TMOFirst = true;//обозначение того, какая фигура была выбрана первой
                                    ListOfFigures[Figure2index].FigureColor = DrawPen.Color;//задание цвета результату ТМО
                                    ListOfFigures[Figure2index].TMOfigure = Figure1index;//у второй фигуры запоминается индекс первой
                                    Figure1captured = false;
                                    g.Clear(pictureBox1.BackColor);
                                    DrawAllFigures(ListOfFigures);
                                }
                            }
                        }
                    }
                }
            }
            pictureBox1.Image = myBitmap;
        }       
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)//обработчик движения мыши в области pictureBox1
        {
            if (mode == 2)// если включен режим преобразования
            {
                switch (GeometricOperationType)
                {
                    case 0://Перемещение
                        {
                            if (e.Button == MouseButtons.Left)
                            {
                                if (ListOfFigures[Current].TMOfigure != -1)//если выбранная фигура участвует в ТМО
                                    ListOfFigures[ListOfFigures[Current].TMOfigure].Move(e.X - pictureBox1MousePosition.X, e.Y - pictureBox1MousePosition.Y);//двигается связаная с ней фигура
                                ListOfFigures[Current].Move(e.X - pictureBox1MousePosition.X, e.Y - pictureBox1MousePosition.Y);//двигается выбранная фигура
                                g.Clear(pictureBox1.BackColor);
                                DrawAllFigures(ListOfFigures);
                                pictureBox1.Image = myBitmap;
                                pictureBox1MousePosition = e.Location;
                            }
                            break;
                        }
                    case 1://Поворот
                        {
                            if (e.Button == MouseButtons.Left)
                            {
                                if (ListOfFigures[Current].TMOfigure != -1)//если выбранная фигура участвует в ТМО
                                    ListOfFigures[ListOfFigures[Current].TMOfigure].Turn(pictureBox1MousePosition, new Point(e.X, e.Y), Center);//поворачивается связаная с ней фигура
                                ListOfFigures[Current].Turn(pictureBox1MousePosition, new Point(e.X, e.Y), Center);//поворачивается выбранная фигура
                                g.Clear(pictureBox1.BackColor);
                                DrawAllFigures(ListOfFigures);
                                pictureBox1.Image = myBitmap;
                                pictureBox1MousePosition = e.Location;
                            }
                            break;
                        }
                    case 2://Масштабирование
                        {
                            if (e.Button == MouseButtons.Left)
                            {
                                if (ListOfFigures[Current].TMOfigure != -1)//если выбранная фигура участвует в ТМО
                                    ListOfFigures[ListOfFigures[Current].TMOfigure].Scale(pictureBox1MousePosition, new Point(e.X, e.Y), Center);//масштабируется связаная с ней фигура
                                ListOfFigures[Current].Scale(pictureBox1MousePosition, new Point(e.X, e.Y), Center);//масштабируется выбранная фигура
                                g.Clear(pictureBox1.BackColor);
                                DrawAllFigures(ListOfFigures);
                                pictureBox1.Image = myBitmap;
                                pictureBox1MousePosition = e.Location;
                            }
                            break;
                        }
                }
            }
        }
        public void DrawPrimitive()//функция создания примитивов
        {
            switch (PrimitiveType)
            {
                case 0://Кривая Безье
                    {
                        if (!rightclick)//пока не нажата ПКМ
                        {
                            VertexList.Add(pictureBox1MousePosition);//запоминаются точки                          
                        }
                        else
                        {
                            VertexList.Add(pictureBox1MousePosition);//запоминается точка ПКМ
                            ListOfFigures.Add(new Figure(DrawPen.Color, true));//в список фигур добавляется новая фигура
                            ListOfFigures[FigureCounter].PointList.AddRange(VertexList);//у только что добавленной фигуры заполняется список точек                       
                            FigureCounter++;//счетчик фигур
                            rightclick = false;
                            g.Clear(pictureBox1.BackColor);
                            DrawAllFigures(ListOfFigures);                            
                            VertexList.Clear();                            
                        }
                        break;
                    }
                case 1://Равнобедренный треугольник
                    {
                        switch (PointCounter)
                        {
                            case 0:
                                {
                                    VertexList.Add(pictureBox1MousePosition);//добавляется первая точка
                                    PointCounter++;
                                    break;
                                }
                            case 1:
                                {
                                    VertexList.Add(pictureBox1MousePosition);//добавляется вторая точка
                                    int dx = (int)(VertexList[1].X - VertexList[0].X);//по имеющимся координатам первых двух точек
                                    VertexList.Add(new PointF(VertexList[1].X + dx, VertexList[0].Y));//создается третья точка
                                    VertexList.Add(VertexList[0]);//в конец списка фигур добавляется первая точка(для реализации ТМО)
                                    ListOfFigures.Add(new Figure(DrawPen.Color));
                                    ListOfFigures[FigureCounter].PointList.AddRange(VertexList);
                                    FigureCounter++;
                                    g.Clear(pictureBox1.BackColor);
                                    DrawAllFigures(ListOfFigures);
                                    VertexList.Clear();
                                    PointCounter = 0;
                                    break;
                                }
                        }
                        break;
                    }
                case 2://Флаг
                    {
                        switch (PointCounter)
                        {
                            case 0:
                                {
                                    VertexList.Add(pictureBox1MousePosition);//добавление первой точки
                                    PointCounter++;
                                    break;
                                }
                            case 1:
                                {
                                    int dx = (int)(pictureBox1MousePosition.X - ((pictureBox1MousePosition.X - VertexList[0].X) / 3));//координаты для выреза на флажке
                                    int dy = (int)(VertexList[0].Y + ((pictureBox1MousePosition.Y - VertexList[0].Y) / 2));                                     
                                    VertexList.Add(new PointF(VertexList[0].X, pictureBox1MousePosition.Y));//по имеющимся координатам первых двух точек
                                    VertexList.Add(pictureBox1MousePosition);//создаются остальные точки
                                    VertexList.Add(new Point(dx, dy));
                                    VertexList.Add(new PointF(pictureBox1MousePosition.X, VertexList[0].Y));
                                    VertexList.Add(VertexList[0]);
                                    ListOfFigures.Add(new Figure(DrawPen.Color));
                                    ListOfFigures[FigureCounter].PointList.AddRange(VertexList);                                    
                                    FigureCounter++;
                                    g.Clear(pictureBox1.BackColor);
                                    DrawAllFigures(ListOfFigures);
                                    VertexList.Clear();
                                    PointCounter = 0;
                                    break;
                                }
                        }
                        break;
                    }
                case 3://Отрезок прямой
                    {
                        switch (PointCounter)
                        {
                            case 0:
                                {
                                    VertexList.Add(pictureBox1MousePosition);
                                    PointCounter++;
                                    break;
                                }
                            case 1:
                                {
                                    VertexList.Add(pictureBox1MousePosition);
                                    ListOfFigures.Add(new Figure(DrawPen.Color, true));
                                    ListOfFigures[FigureCounter].PointList.AddRange(VertexList);                                                                        
                                    FigureCounter++;
                                    g.Clear(pictureBox1.BackColor);
                                    DrawAllFigures(ListOfFigures);
                                    VertexList.Clear();
                                    PointCounter = 0;
                                    break;
                                }
                        }
                        break;
                    }
            }
        }
        public void DrawBezie(Pen DrPen, List<PointF> P, int n)//визуализация кривой Безье
        {
            const double dt = 0.004;
            double term = 1 + dt / 2;
            double nFact = Factorial(n);
            double t = dt;
            int xPred = (int)P[0].X, yPred = (int)P[0].Y;
            while (t < term)
            {
                double xt = 0, yt = 0;
                int i = 0;
                double fi = 1;
                double fni = nFact;
                while (i <= n)
                {                    
                    double J = Math.Pow(t, i) * Math.Pow(1 - t, n - i) * nFact / (fi * fni); //интерполяционный полином Бернштейна
                    xt = xt + P[i].X * J; //вычисление х координаты следующей точки
                    yt = yt + P[i].Y * J; //вычисление у координаты следующей точки
                    fni /= n - i; //вычисление факториала 
                    i += 1; //шаг
                    fi *= i; //вычисление факториала 
                }
                g.DrawLine(DrPen, xPred, yPred, (int)xt, (int)yt);
                t += dt;
                xPred = (int)xt; //задание х координаты предыдущей точки
                yPred = (int)yt; //задание у координаты предыдущей точки
            }
        }
        static double Factorial(int n)
        {
            double x = 1;
            for (int i = 1; i <= n; i++)
                x *= i;
            return x;
        }
        private void DrawAllFigures(List<Figure> Figures)//метод зарисовки всех фигур
        {
            for (int i = 0; i < Figures.Count(); i++)//проход по списку фигур
            {
                if (ListOfFigures[i].TMOfigure != -1)//если фигура участвует в ТМО
                {
                    if (Figures[i].TMOFirst)//если фигура первая 
                        TMO(Figures[i].TMOtype, Figures[i], Figures[Figures[i].TMOfigure]);//происходит ТМО
                }
                else DrawFigure(Figures[i]);//зарисовка фигуры
            }
        }
        private void DrawFigure(Figure Figure)//метод зарисовки фигуры
        {
            if (Figure.LineOrCurve)//если фигура это отрезок прямой или кривая
            {
                if (Figure.PointList.Count() == 2)//Отрезок прямой
                {
                    g.DrawLine(new Pen(Figure.FigureColor), Figure.PointList[0], Figure.PointList[1]);
                }
                else//Кривая Безье
                {
                    DrawBezie(new Pen(Figure.FigureColor), Figure.PointList, Figure.PointList.Count() - 1);
                }
            }
            else//Треугольник или флаг
            {                
                Figure.Fill(g);
            }
        }
        public int SelectPrimitive(Point pictureBox1MousePos)//выбор фигуры (возвращает ее индекс в списке фигур и задает checkPrimitive == true)
        {
            int n = ListOfFigures.Count + 1;
            for (int i = ListOfFigures.Count - 1; i >= 0; i--)//проход по списку фигур, начиная с конца
            {
                if (ListOfFigures[i].ThisFigure(pictureBox1MousePos.X, pictureBox1MousePos.Y))//если заданная точка принадлежит фигуре
                {
                    checkPrimitive = true;
                    n = i;//запоминается индекс этой фигуры
                    break;
                }
            }
            if (n == ListOfFigures.Count + 1)//если заданная точка не принадлежит ни одной фигуре
            {
                checkPrimitive = false;
                MessageBox.Show($"Укажите точку, лежащую на одном из примитивов");
                return n;
            }
            else return n;
        }
        private void TMO(int type, Figure Figure1, Figure Figure2)
        {
            List<int> Xal = new List<int>(), Xar = new List<int>();//список левых и список правых границ фигуры А
            List<int> Xbl = new List<int>(), Xbr = new List<int>();//список левых и список правых границ фигуры В
            int YminA = (int)Figure1.PointList[0].Y;//нижняя граница фигуры А
            int YmaxA = (int)Figure1.PointList[0].Y;//верхняя граница фигуры А
            int YminB = (int)Figure2.PointList[0].Y;//нижняя граница фигуры В
            int YmaxB = (int)Figure2.PointList[0].Y;//верхняя граница фигуры В
            int ko;
            float xF;//вычисленный икс
            int X;//округленный вычисленный икс            
            int AindexMin = 1;//индекс точки, являющейся нижней границей фигуры А
            bool aCW;//обход фигуры А
            int BindexMin = 1;//индекс точки, являющейся нижней границей фигуры В
            bool bCW;//обход фигуры В

            for (int i = 1; i < Figure1.PointList.Count()-1; i++)
            {
                if (YminA > Figure1.PointList[i].Y)
                {
                    YminA = (int)Figure1.PointList[i].Y;
                    AindexMin = i;
                }
                if (YmaxA < Figure1.PointList[i].Y)
                {
                    YmaxA = (int)Figure1.PointList[i].Y;
                }
            }
            for (int i = 1; i < Figure2.PointList.Count()-1; i++)
            {
                if (YminB > Figure2.PointList[i].Y)
                {
                    YminB = (int)Figure2.PointList[i].Y;
                    BindexMin = i;
                }
                if (YmaxB < Figure2.PointList[i].Y)
                {
                    YmaxB = (int)Figure2.PointList[i].Y;
                }
            }
            //вычисление обхода фигуры А
            if ((Figure1.PointList[AindexMin - 1].X * (Figure1.PointList[AindexMin].Y - Figure1.PointList[AindexMin + 1].Y) + Figure1.PointList[AindexMin].X * (Figure1.PointList[AindexMin + 1].Y - Figure1.PointList[AindexMin - 1].Y) + Figure1.PointList[AindexMin + 1].X * (Figure1.PointList[AindexMin - 1].Y - Figure1.PointList[AindexMin].Y)) > 0)//формула площади треугольника 
                aCW = true;//если площадь треугольника > 0 обход фигуры по часовой стрелке
            else aCW = false;//иначе против часовой

            //вычисление обхода фигуры В
            if ((Figure2.PointList[BindexMin - 1].X * (Figure2.PointList[BindexMin].Y - Figure2.PointList[BindexMin + 1].Y) + Figure2.PointList[BindexMin].X * (Figure2.PointList[BindexMin + 1].Y - Figure2.PointList[BindexMin - 1].Y) + Figure2.PointList[BindexMin + 1].X * (Figure2.PointList[BindexMin - 1].Y - Figure2.PointList[BindexMin].Y)) > 0)
                bCW = true;
            else bCW = false;

            for (int Y = Math.Min(YminA, YminB); Y <= Math.Max(YmaxA, YmaxB); Y++)//для всех строк от Ymin до Ymax
            {
                Xal.Clear();
                Xar.Clear();
                for (int i = 0; i < Figure1.PointList.Count(); i++)//проход по всем точкам фигуры А
                {
                    if (i < Figure1.PointList.Count() - 1)//если i не последняя точка
                    {
                        ko = i + 1;//ко - следующая точка после i
                    }
                    else ko = 0;//иначе ко - первая точка
                    if (((Figure1.PointList[i].Y < Y) && (Figure1.PointList[ko].Y >= Y)) || ((Figure1.PointList[i].Y >= Y) && (Figure1.PointList[ko].Y < Y)))
                    {
                        xF = ((float)Y - (float)Figure1.PointList[i].Y) / ((float)Figure1.PointList[ko].Y - (float)Figure1.PointList[i].Y) * ((float)Figure1.PointList[ko].X - (float)Figure1.PointList[i].X) + (float)Figure1.PointList[i].X; //нахождение икса точки пересечения строки Y со стороной фигуры А с помощью уравнения
                        X = (int)Math.Round(xF);
                        if (aCW)//если по часовой
                        {
                        if (Figure1.PointList[ko].Y - Figure1.PointList[i].Y > 0) Xar.Add(X); //заполнение списка "правых" иксов фигуры А
                        else Xal.Add(X);//заполнение списка "левых" иксов фигуры А
                        }
                        else if (!aCW)
                        {
                            if (Figure1.PointList[ko].Y - Figure1.PointList[i].Y < 0) Xar.Add(X);
                            else Xal.Add(X);
                        }
                    }
                }
                Xal.Sort();//сортировка по возрастанию
                Xar.Sort();

                //все тоже самое для фигуры В
                Xbl.Clear();
                Xbr.Clear();
                for (int i = 0; i < Figure2.PointList.Count(); i++)
                {
                    if (i < Figure2.PointList.Count() - 1)
                    {
                        ko = i + 1;
                    }
                    else ko = 0;
                    if (((Figure2.PointList[i].Y < Y) && (Figure2.PointList[ko].Y >= Y)) || ((Figure2.PointList[i].Y >= Y) && (Figure2.PointList[ko].Y < Y)))
                    {
                        xF = ((float)Y - (float)Figure2.PointList[i].Y) / ((float)Figure2.PointList[ko].Y - (float)Figure2.PointList[i].Y) * ((float)Figure2.PointList[ko].X - (float)Figure2.PointList[i].X) + (float)Figure2.PointList[i].X;
                        X = (int)Math.Round(xF);
                        if (bCW)
                        {
                        if (Figure2.PointList[ko].Y - Figure2.PointList[i].Y > 0) Xbr.Add(X);
                        else Xbl.Add(X);
                        }
                        else if (!bCW)
                        {
                            if (Figure2.PointList[ko].Y - Figure2.PointList[i].Y < 0) Xbr.Add(X); 
                            else Xbl.Add(X);
                        }
                    }
                }
                Xbl.Sort();
                Xbr.Sort();

                List<int> Mx = new List<int>();//список для записи координаты x границы сегмента
                int[] MdQ = new int[Xal.Count() + Xar.Count() + Xbl.Count() + Xbr.Count()];//массив для записи соответствующего приращения пороговой функции с учетом веса операнда
                int[] SetQ = new int[2];//множество значений суммы Q пороговых функций операндов, соответствующее заданной ТМО;
                int nM, n;
                List<int> Xrl = new List<int>(), Xrr = new List<int>();//Результатом работы алгоритма будут массивы Xrl и Xrr левых и правых границ сегментов сечения результирующей области строкой Y.                                                                       
                if (type == 0) { SetQ[0] = 3; SetQ[1] = 3; }//пересечение                
                else if (type == 1) { SetQ[0] = 2; SetQ[1] = 2; }//разность                
                n = Xal.Count();
                for (int i = 1; i <= n; i++)
                {
                    Mx.Add(Xal[i - 1]);
                    MdQ[i - 1] = 2;
                }
                nM = n;
                n = Xar.Count();
                for (int i = 1; i <= n; i++)
                {
                    Mx.Add(Xar[i - 1]);
                    MdQ[nM + i - 1] = -2;
                }
                nM += n;
                n = Xbl.Count();
                for (int i = 1; i <= n; i++)
                {
                    Mx.Add(Xbl[i - 1]);
                    MdQ[nM + i - 1] = 1;
                }
                nM += n;
                n = Xbr.Count();
                for (int i = 1; i <= n; i++)
                {
                    Mx.Add(Xbr[i - 1]);
                    MdQ[nM + i - 1] = -1;
                }
                nM += n; // общее число элементов в массиве Mх

                for (int i = 0; i < nM; i++)//сортировка списка Мх по возрастанию и массива МdQ относительно массива Мх
                {
                    for (int j = 0; j < nM - 1 - i; j++)
                    {
                        if (Mx[j] > Mx[j + 1])
                        {
                            int buff1 = Mx[j];
                            int buff2 = MdQ[j];

                            Mx[j] = Mx[j + 1];
                            MdQ[j] = MdQ[j + 1];

                            Mx[j + 1] = buff1;
                            MdQ[j + 1] = buff2;
                        }
                    }
                }

                int Q = 0;
                for (int i = 0; i < nM; i++)//проход по всему списку Мх
                {
                    int x = Mx[i];
                    int Qnew = Q + MdQ[i];
                    if (!(Q >= SetQ[0] && Q <= SetQ[1]) && (Qnew >= SetQ[0] && Qnew <= SetQ[1]))
                    {
                        Xrl.Add(x);
                    }
                    if ((Q >= SetQ[0] && Q <= SetQ[1]) && !(Qnew >= SetQ[0] && Qnew <= SetQ[1]))
                    {
                        Xrr.Add(x);
                    }
                    Q = Qnew;
                }

                for (int i = 0; i < Xrl.Count(); i++)//закраска результирующей области
                {
                    g.DrawLine(new Pen(Figure1.FigureColor), Xrl[i], Y, Xrr[i], Y);                    
                }
            }
        }
        private void Clear_Button_Click(object sender, EventArgs e)//обработчик нажатия кнопки "Очистить"
        {
            PointCounter = 0;
            FigureCounter = 0;
            VertexList.Clear();
            ListOfFigures.Clear();
            DrawPen.Color = Color.Black;
            mode = 0;
            g.Clear(pictureBox1.BackColor);
            checkPrimitive = false;
            pictureBox1.Image = myBitmap;
            PrimitiveType = 0;
            GeometricOperationType = 0;
            TMOtype = 0;
            pictureBox1MousePosition = new Point();
            MirrorPoint1 = new Point();
            MirrorPoint2 = new Point();
            Current = 0;
            Center = new Point();
            comboBox2.Text = "Преобразования";
            comboBox3.Text = "ТМО";
            comboBox1.Text = "Примитивы";
        }
        private void ColorChoice(object sender, EventArgs e)//обработчик кнопки "Цвет"
        {
            colorDialog1.ShowDialog();
            DrawPen.Color = colorDialog1.Color;
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)//обработчик включения/выключения "Режим удаления"
        {
            if (checkBox1.Checked)//если режим включен
            {
                comboBox1.Text = "Примитивы";
                comboBox2.Text = "Преобразования";
                comboBox3.Text = "ТМО";
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
                mode = 0;
            }
            else
            {
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                Current = 0;
                Center = new Point();
            }
        }
        public void DrawX(Point P)//зарисовка перекрестия
        {
            g.DrawLine(Xpen, P.X - 9, P.Y, P.X + 9, P.Y);
            g.DrawLine(Xpen, P.X, P.Y - 9, P.X, P.Y + 9);
        }
        private void button3_Click(object sender, EventArgs e)//обработчик кнопки "Справка"
        {
            MessageBox.Show($"  После выбора примитива:\n" +
                $"  - Для визуализации кривой Безье необходимо последовательно задавать точки нажатием ЛКМ, нажатием ПКМ вы зададите последнюю точку и выполнится зарисовка кривой\n" +
                $"  - Для визуализации равнобедренного треугольника нужно ввести две точки нажатием ЛКМ, задающие одну из равнобедренных сторон, затем треугольник дорисуется и закрасится\n" +
                $"  - Визуализация флага аналогична визуализации треугольника, только в данном случае две точки задают диагональ фигуры\n" +
                $"  - Для визуализации отрезка прямой нужно ввести две точки нажатием ЛКМ\n\n" +
                $"  Когда будет нарисован хотя бы один примитив, станут доступны геометрические преобразования, выбрав преобразование:\n" +
                $"  - Для перемещения примитива, выделите его нажатием ЛКМ, затем, не отпуская кнопки, перемещайте мышь\n" +
                $"  - Для поворота примитива, выделите его нажатием ЛКМ, затем, не отпуская кнопки, перемещайте мышь\n" +
                $"  - Для масштабирования примитива по оси Х, выделите его нажатием ЛКМ, затем, не отпуская кнопки, перемещайте мышь\n" +
                $"  - Для отражения примитива, выделите его нажатием ЛКМ, затем двумя последовательными нажатиями ПКМ задайте прямую общего положения, относительно которой произойдет отражение\n\n" +
                $"  Когда будет нарисовано от двух примитивов, станут доступны теоретико множественные операции (далее ТМО), выбрав ТМО:\n" +
                $"  - Для выполнения ТМО Пересечение и Разность, выделите два примитива нажатием ЛКМ\n" +
                $"  - Обратите внимание, что для операции Разность имеет значение порядок выделения фигур(из первой выделенной фигуры вычитается вторая)\n" +
                $"  - Операции над фигурами являются необратимыми\n\n" +
                $"  - Для удаления объектов (по одному), включите Режим удаления, затем нажмите на интересующий вас объект ПКМ\n" +
                $"  - Для выбора цвета (по умолчанию цвет черный), нажмите на кнопку Цвет\n" +
                $"  - Для удаления объектов (всех сразу), нажмите на кнопку Очистить",
                $"Справка");
        }
        private void Form1_Shown(object sender, EventArgs e)//при запуске программы появляется инструкция
        {
            MessageBox.Show($"  Вашему вниманию представляется инструкция пользования данным программным продуктом (для того чтобы еще раз увидеть инструкцию, нажмите на кнопку Справка) \n\n" +
                $"  После выбора примитива:\n" +
                $"  - Для визуализации кривой Безье необходимо последовательно задавать точки нажатием ЛКМ, нажатием ПКМ вы зададите последнюю точку и выполнится зарисовка кривой\n" +
                $"  - Для визуализации равнобедренного треугольника нужно ввести две точки нажатием ЛКМ, задающие одну из равнобедренных сторон, затем треугольник дорисуется и закрасится\n" +
                $"  - Визуализация флага аналогична визуализации треугольника, только в данном случае две точки задают диагональ фигуры\n" +
                $"  - Для визуализации отрезка прямой нужно ввести две точки нажатием ЛКМ\n\n" +
                $"  Когда будет нарисован хотя бы один примитив, станут доступны геометрические преобразования, выбрав преобразование:\n" +
                $"  - Для перемещения примитива, выделите его нажатием ЛКМ, затем, не отпуская кнопки, перемещайте мышь\n" +
                $"  - Для поворота примитива, выделите его нажатием ЛКМ, затем, не отпуская кнопки, перемещайте мышь\n" +
                $"  - Для масштабирования примитива по оси Х, выделите его нажатием ЛКМ, затем, не отпуская кнопки, перемещайте мышь\n" +
                $"  - Для отражения примитива, выделите его нажатием ЛКМ, затем двумя последовательными нажатиями ПКМ задайте прямую общего положения, относительно которой произойдет отражение\n\n" +
                $"  Когда будет нарисовано от двух примитивов, станут доступны теоретико множественные операции (далее ТМО), выбрав ТМО:\n" +
                $"  - Для выполнения ТМО Пересечение и Разность, выделите два примитива нажатием ЛКМ\n" +
                $"  - Обратите внимание, что для операции Разность имеет значение порядок выделения фигур(из первой выделенной фигуры вычитается вторая)\n" +
                $"  - Операции над фигурами являются необратимыми\n\n" +
                $"  - Для удаления объектов (по одному), включите Режим удаления, затем нажмите на интересующий вас объект ПКМ\n" +
                $"  - Для выбора цвета (по умолчанию цвет черный), нажмите на кнопку Цвет\n" +                
                $"  - Для удаления объектов (всех сразу), нажмите на кнопку Очистить",
                $"Инструкция пользования");
        }
    }
}