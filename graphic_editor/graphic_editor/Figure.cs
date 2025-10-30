using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace graphic_editor
{
    class Figure
    {
        public List<PointF> PointList;//список точек фигуры
        public Color FigureColor;//цвет фигуры
        public bool LineOrCurve = false;//является ли фигура кривой или отрезком
        public int TMOfigure = -1;//индекс той фигуры, с которой произошла ТМО
        public int TMOtype;//тип ТМО
        public bool TMOFirst = false;//была ли выбрана эта фигура первой для ТМО(нужно для ТМО разность)
        public Figure(Color FigureColor)
        {            
            PointList = new List<PointF>();
            this.FigureColor = FigureColor;
        }
        public Figure(Color FigureColor, bool LineOrCurve)
        {
            PointList = new List<PointF>();
            this.FigureColor = FigureColor;
            this.LineOrCurve = LineOrCurve;            
        }
        public void Fill(Graphics g)//метод закрашивания фигуры
        {
            int Ymin = (int)PointList[0].Y;
            int Ymax = (int)PointList[0].Y;
            List<int> Xb = new List<int>();
            int k;
            float x;
            for (int i = 0; i < PointList.Count(); i++)
            {                
                if (Ymin > PointList[i].Y)
                {
                    Ymin = (int)PointList[i].Y;
                }
                if (Ymax < (int)PointList[i].Y)
                {
                    Ymax = (int)PointList[i].Y;
                }
            }
            for (int Y = Ymin; Y <= Ymax; Y++)//для всех строк от Ymin до Ymax
            {
                Xb.Clear();
                for (int i = 0; i < PointList.Count(); i++)//проход по всем точкам
                {
                    if (i < PointList.Count() - 1)//если i не последняя точка
                    {
                        k = i + 1;//к - следующая точка после i
                    }
                    else k = 0;//иначе к - первая точка
                    if (((PointList[i].Y < Y) && (PointList[k].Y >= Y)) || ((PointList[i].Y >= Y) && (PointList[k].Y < Y)))
                    {
                        x = (Y - (float)PointList[i].Y) / ((float)PointList[k].Y - (float)PointList[i].Y) * ((float)PointList[k].X - (float)PointList[i].X) + (float)PointList[i].X; //нахождение точки пересечения с помощью уравнения
                        Xb.Add((int)Math.Round(x));
                    }
                }
                Xb.Sort();
                for (int i = 0; i < Xb.Count(); i += 2)
                {
                    g.DrawLine(new Pen(FigureColor), Xb[i], Y, Xb[i + 1], Y);
                }
            }
        }
        public bool ThisFigure(int mouseX, int mouseY)//метод, определяющий принадлежит ли заданная точка фигуре
        {
            int n = PointList.Count() - 1, k;
            PointF Pi, Pk;
            double x;
            List<int> Xb = new List<int>(); // буфер сегментов
            bool check = false;
            Xb.Clear();
            for (int i = 0; i <= n; i++)
            {
                if (i < n) k = i + 1; else k = 0;
                Pi = PointList[i];
                Pk = PointList[k];
                if ((Pi.Y < mouseY) & (Pk.Y >= mouseY) | (Pi.Y >= mouseY) & (Pk.Y < mouseY))
                {
                    x = (mouseY - Pi.Y) * (Pk.X - Pi.X) / (Pk.Y - Pi.Y) + Pi.X;
                    Xb.Add((int)Math.Round(x));
                }
            }
            if (Xb.Count() > 0)
            {
                Xb.Sort(); // сортировка по возрастанию
                for (int i = 0; i < Xb.Count; i += 2)
                    if (mouseX >= Xb[i] & mouseX <= Xb[i + 1]) { check = true; break; }
            }
            return check;
        }
        private double[,] Multiplication(double[,] matrix1, double[,] matrix2)// умножение матриц
        {
            double[,] result = new double[matrix1.GetLength(0), matrix2.GetLength(1)];
            for (int i = 0; i < matrix1.GetLength(0); i++)
            {
                for (int j = 0; j < matrix2.GetLength(1); j++)
                {
                    for (int k = 0; k < matrix2.GetLength(0); k++)
                    {
                        result[i, j] += matrix1[i, k] * matrix2[k, j];
                    }
                }
            }
            return result;
        }
        private void SameAction(double[,] matrix1, double[,] matrix2)//данные действия выполнялись в каждом геометрическом преобразовании, поэтому были вынесены в отдельную функцию
        {
            int n = PointList.Count();
            PointF fP = new Point();
            double[,] result;
            for (int i = 0; i < n; i++)
            {
                matrix1[0, 0] = PointList[i].X; matrix1[0, 1] = PointList[i].Y; matrix1[0, 2] = 1;
                result = Multiplication(matrix1, matrix2);
                fP.X = (float)result[0, 0]; fP.Y = (float)result[0, 1];
                PointList[i] = fP;
            }
        }        
        public void Move(int dx, int dy)// метод Плоскопараллельное перемещение
        {
            double[,] Matrix1 = new double[1, 3];
            double[,] Matrix2 = new double[3, 3];
            Matrix2[0, 0] = Matrix2[1, 1] = Matrix2[2, 2] = 1;
            Matrix2[2, 0] = dx;
            Matrix2[2, 1] = dy;
            SameAction(Matrix1, Matrix2);
        }        
        public void Turn(Point P1, Point P2, Point Center)//метод поворот
        {
            double alpha, al; //угол поворота
            double a, b; //a, b - радиус к первой и второй точке;
            double a2, b2, c2; //квадраты сторон
            a2 = Math.Pow(P1.X - Center.X, 2) + Math.Pow(P1.Y - Center.Y, 2);
            b2 = Math.Pow(P2.X - Center.X, 2) + Math.Pow(P2.Y - Center.Y, 2);
            c2 = Math.Pow(P2.X - P1.X, 2) + Math.Pow(P2.Y - P1.Y, 2);
            a = (Math.Sqrt(a2));
            b = (Math.Sqrt(b2));
            al = (a2 + b2 - c2) / (2 * a * b);
            if (al > 1) al = 1;
            //поиск угла поворота            
            if (P1.X >= Center.X & P1.Y <= Center.Y) //1 четверть
            {
                if ((P2.X - P1.X + P2.Y - P1.Y) > 0) alpha = Math.Acos(al); //по часовой 
                else alpha = -Math.Acos(al); //против часовой
            }
            else if (P1.X < Center.X & P1.Y < Center.Y) //2 четверть
            {
                if ((P2.X - P1.X) > (P2.Y - P1.Y))alpha = Math.Acos(al); //по часовой
                else alpha = -Math.Acos(al); //против часовой
            }
            else if (P1.X <= Center.X & P1.Y >= Center.Y) //3 четверть
            {
                if ((P2.X - P1.X + P2.Y - P1.Y) < 0) alpha = Math.Acos(al); //по часовой
                else alpha = -Math.Acos(al); //против часовой
            }
            else // 4 четверть
            {
                if ((P2.X - P1.X) < (P2.Y - P1.Y)) alpha = Math.Acos(al); //по часовой
                else alpha = -Math.Acos(al); //против часовой
            }
            double[,] Matrix1 = new double[1, 3];
            double[,] Matrix2 = new double[3, 3];
            Matrix2[2, 2] = 1;
            Matrix2[0, 0] = Matrix2[1, 1] = Math.Cos(alpha);
            Matrix2[0, 1] = Math.Sin(alpha);
            Matrix2[1, 0] = -Math.Sin(alpha);
            Move(-Center.X, -Center.Y);//передвижение фигуры в координаты [0;0]
            SameAction(Matrix1, Matrix2);
            Move(Center.X, Center.Y);//передвижение фигуры обратно на то место, где она была
        }
        public void Scale(Point P1, Point P2, Point Center)//метод масштабирование
        {
            double[,] Matrix1 = new double[1, 3];
            double[,] Matrix2 = new double[3, 3];
            double eVector = Math.Sqrt(Math.Pow(P2.X - Center.X, 2) + Math.Pow(P2.Y - Center.Y, 2));//длина вектора от центра фигуры к текущей позиции мышки
            double PosVector = Math.Sqrt(Math.Pow(P1.X - Center.X, 2) + Math.Pow(P1.Y - Center.Y, 2));//длина вектора от центра фигуры к позиции, которая была до текущей позиции мышки
            double kf = eVector / PosVector;//коэффициент масштабирования 
            Matrix2[0, 0] = kf;
            Matrix2[1, 1] = 1;
            Matrix2[2, 2] = 1;
            Move(-Center.X, -Center.Y);//передвижение фигуры в координаты [0;0]
            SameAction(Matrix1, Matrix2);
            Move(Center.X, Center.Y);//передвижение фигуры обратно на то место, где она была
        }
        public void Mirror(PointF P, PointF F)//метод отражение
        {
            float y = (P.Y * F.X - P.X * F.Y) / (F.X - P.X);//передвигаем фигуру так, чтобы прямая проходила через начало координат
            float x = 0;
            if (y < 0)
            {
                x = (P.X * F.Y - P.Y * F.X) / (F.Y - P.Y);
                Move(-(int)x, 0);
                P.X -= x;
            }
            else
            {
                Move(0, -(int)y);
                P.Y -= y;
            }
            double rotate = -Math.Atan(P.Y / P.X);//поворачиваем фигуру так, чтобы прямая совпала с осью икс
            double[,] Matrix1 = new double[1, 3];
            double[,] Matrix2 = new double[3, 3];
            Matrix2[2, 2] = 1;
            Matrix2[0, 0] = Matrix2[1, 1] = Math.Cos(rotate);
            Matrix2[0, 1] = Math.Sin(rotate);
            Matrix2[1, 0] = -Math.Sin(rotate);            
            SameAction(Matrix1, Matrix2);
            for (int i = 0; i < PointList.Count; ++i)//отражаем фигуру по оси икс
            {                
                PointF Pp = new PointF(PointList[i].X, -PointList[i].Y);
                PointList[i] = Pp;                
            }            
            rotate = -rotate;//поворачиваем фигуру обратно
            Matrix2[2, 2] = 1;
            Matrix2[0, 0] = Matrix2[1, 1] = Math.Cos(rotate);
            Matrix2[0, 1] = Math.Sin(rotate);
            Matrix2[1, 0] = -Math.Sin(rotate);
            SameAction(Matrix1, Matrix2);            
            if (y < 0)//возвращаем фигуру обратно
            {
                Move((int)x, 0);                
            }
            else
            {
                Move(0, (int)y);                
            }
        }
        public Point FindCenter()//метод поиск центральной точки
        {
            Point C = new Point();
            int xMax = 0, xMin = (int)Math.Round(PointList[0].X), yMax = 0, yMin = (int)Math.Round(PointList[0].Y);            
            for (int i = 0; i < PointList.Count; i++)//находим максимальные и минимальные вершины
            {
                if (PointList[i].X > xMax) xMax = (int)Math.Round(PointList[i].X);
                else if (PointList[i].X < xMin) xMin = (int)Math.Round(PointList[i].X);
                if (PointList[i].Y > yMax) yMax = (int)Math.Round(PointList[i].Y);
                else if (PointList[i].Y < yMin) yMin = (int)Math.Round(PointList[i].Y);
            }            
            C.X = xMin + ((xMax - xMin) / 2);//находим центральную точку
            C.Y = yMin + ((yMax - yMin) / 2);
            return C;
        }
    }
}