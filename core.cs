using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DuiDuiPeng
{
	class DuiDuiPeng		//函数的主入口
	{
		static void main(string[] args)				//注意Main是M而不是m（后者是故意用错的）
		{
			GamePool Pool1 = new GamePool(5, 5, 11);    //设置游戏区的大小
			EngineeringPool debug = Pool1;			//debug可以调用工程类中隐藏的Exchange方法
			debug.ConsolePrintPool();

			while (true)
			{
				Console.WriteLine("请输入两个坐标,每个坐标的x和y用空格分开，坐标之间用回车分开：");
				debug.ConsoleExchange();	//在debug静态方法里可任意交换两个块
				debug.ConsolePrintPool();
//				Console.ReadLine();
			}
		}
	}

	abstract class Pool					//池子类,抽象类
	{
		int[,] pool;					//池子
		int row, col;
		public static int Species;		//图片的种类
		int score;                      //游戏分数

		public Pool(int row,int col, int Species)	//构造函数
		{
			if (row < 0)				//避免错误值出现
				row = 0;
			if (col < 0)
				col = 0;
			pool = new int[row, col];
			this.row = row;
			this.col = col;
			Pool.Species = Species;
			score = 0;
		}

		public int GetBrick(int x,int y)	//获取砖块的值
		{
			if (x < 0 || y < 0 || x > row - 1 || y > col - 1)
				return 0;
			else
				return pool[x,y];
		}

		public int GetRow()		//获取行数
		{
			return row;
		}

		public int GetCol()		//获取列数
		{
			return col;
		}

		public void AddScore(int add)	//add可正可负，只能累加不能改变
		{
			if(score<1000000)			//限定最高分（再高就有作弊嫌疑了）
				score += add;
		}

		public int GetScore()
		{
			return score;
		}

		public int GetSpecies()                         //返回图片种类数
		{
			return Species;
		}

		public bool SetBrick(int x,int y ,int value)	//设置砖块的值		
		{
			if (value < 0 || x < 0 || y < 0 || x > row - 1 || y > col - 1) 
				return false;

			pool[x, y] = value;
			return true;
		}

		public void InitPool()			//初始化所有砖块（值设为0）
		{
			int i = 0, j = 0;
			for(i=0;i<row;i++)
				for(j=0;j<col;j++)
					pool[i, j] = 0;
			score = 0;
			return;
		}
		
	}

	class EngineeringPool : Pool	//工程池类，包含控制代码
	{
		public EngineeringPool(int row, int col, int Species) : base(row, col,Species) {; }	//继承并初始化基类：Pool

		public bool Exchange(int x1,int y1,int x2,int y2)	//无条件交换(x1,y1)和(x2,y2)的值
		{
			int temp = GetBrick(x1, y1);
			SetBrick(x1, y1, GetBrick(x2, y2));
			SetBrick(x2, y2, temp);
			return true;
		}

		public void FixZero()       //遍历池子，发现有0元素则生成随机数填充之
		{
			int i, j;
			int row = GetRow();
			int col = GetCol();
			Random ran = new Random();  //设置时间为种子生成随机数

			for (i = 0; i < row; i++)
				for (j = 0; j < col; j++)
					if (GetBrick(i, j) == 0)
						SetBrick(i, j, ran.Next() % Species + 1);   //生成1到species的随机数填充进去

		}

		public void Downward()      //向下补充被消掉的块（bug）
		{
			int row = GetRow();
			int col = GetCol();
			int i, j, temp;
			Queue q = new Queue();

			for (j = row - 1; j >= 0; j--)                  //从最底层向上遍历
				for (i = 0; i < col; i++)
					if (GetBrick(j, i) == 0)                //搜索到为0的值时，
					{
						temp = i - 1;                       //从i-1行起，
						while (temp >= 0)                   //将j列的所有非0数从下往上入列
						{
							if (GetBrick(j, temp) != 0)
								q.Enqueue(GetBrick(j, temp));
							temp--;
						}
						temp = i;
						while (temp >= 0)                       //出队列，队列空了之后填充0
						{
							if (q.Count > 0)
								SetBrick(j, temp, (int)q.Dequeue());
							else
								SetBrick(j, temp, 0);
							temp--;
						}
						q.Clear();
					}
			return;
		}

		public void _Downward()      //向下补充被消掉的块（bug）
		{
			int row = GetRow();
			int col = GetCol();
			int i, j, temp;
			Queue q = new Queue();

			for (i = row - 1; i >= 0; i--)                  //从最底层向上遍历
				for (j = 0; j < col - 1; j++)
					if (GetBrick(i, j) == 0)                //搜索到为0的值时，
					{
						temp = i - 1;                       //从i-1行起，
						while (temp >= 0)                   //将j列的所有非0数从下往上入列
						{
							if (GetBrick(temp, j) != 0)
								q.Enqueue(GetBrick(temp, j));
							temp--;
						}
						temp = i;
						while (temp >= 0)                       //出队列，队列空了之后填充0
						{
							if (q.Count > 0)
								SetBrick(temp, j, (int)q.Dequeue());
							else
								SetBrick(temp, j, 0);
							temp--;
						}
						q.Clear();
					}
			return;
		}


		public void RandomPool()        //随机填充池子
		{
			int row = GetRow();
			int col = GetCol();

			InitPool();                 //先清空池子
			FixZero();			//再随机填充
		}

		public bool FindExplicit()		//寻找显性解,发现至少有一个就返回true
		{
			int i, j;
			int temp=0, num = 0;		//temp保存临时值，num用于计数
			int row = GetRow();
			int col = GetCol();

			//逐行扫描
			for (i = 0; i < row; i++)
				for (j = 0, temp = GetBrick(i,0), num = 0; j < col; j++)	//遍历每一行之前将temp初始化为行首的值
				{
					if (GetBrick(i, j) == temp && GetBrick(i, j) > 0) 
						num++;
					else
						num = 1;
					temp = GetBrick(i, j);	//将temp向前推进一格

					if (num >= 3)
						return true;
				}

			//逐列扫描
			for (j = 0; j < col; j++)
				for (i = 0, temp = GetBrick(0, j), num = 0; i < row; i++)   //遍历每一列之前将temp初始化为列首的值
				{
					if (GetBrick(i, j) == temp && GetBrick(i, j) > 0)
						num++;
					else
						num = 1;

					temp = GetBrick(i, j);  //将temp向前推进一格
					if (num >= 3)
						return true;
				}
			return false;
		}

		public bool FindExplicit(bool choice)	//对寻找显性解函数的重载，可以解显性解，同时加入一个缺省的选项
		{
			if (!choice)							//传入形参为false或缺省，则默认只寻找而不消除
				return(FindExplicit());

			int i, j;
			bool flag = false;
			int temp = 0, num = 0;				//temp保存临时值，num用于计数
			int row = GetRow();
			int col = GetCol();

			//逐行扫描
			for (i = 0; i < row; i++)
				for (j = 0, temp = GetBrick(i, 0), num = 0; j < col; j++)   //遍历每一行之前将temp初始化为行首的值
				{
					if (GetBrick(i, j) == temp && GetBrick(i, j) > 0)
						num++;
					else
					{ 
						num = 1;
						temp = GetBrick(i, j);  //将temp向前推进一格
					}

					if (num >= 3)
					{
						SetBrick(i, j, 0);
						SetBrick(i, j - 1, 0);
						SetBrick(i, j - 2, 0);
						AddScore(100);
						flag = true;
					}
				}

			//逐列扫描
			for (j = 0; j < col; j++)
				for (i = 0, temp = GetBrick(0, j), num = 0; i < row; i++)   //遍历每一列之前将temp初始化为列首的值
				{
					if (GetBrick(i, j) == temp && GetBrick(i, j) > 0)
						num++;
					else
					{
						num = 1;
						temp = GetBrick(i, j);  //将temp向前推进一格
					}

					if (num >= 3)
					{
						SetBrick(i, j, 0);
						SetBrick(i - 1, j, 0);
						SetBrick(i - 2, j, 0);
						AddScore(100);
						flag = true;
					}
				}
			Downward();
			FixZero();
			return flag;
		}

		public bool FindRecessive()		//寻找隐性解
		{
			//等待补全这部分的功能

			return false;
		}

		public void InitGame()			//初始化游戏
		{
			RandomPool();
			while (FindExplicit()) 
				RandomPool();
			while (FindRecessive())
				RandomPool();
			
		}

		public void ConsolePrintPool()		//控制台显示函数，仅用于debug
		{
			int row = GetRow();
			int col = GetCol();
			int i, j;
			int[] temp = new int[col];

			for (i = 0; i < row; i++)
			{
				for (j = 0; j < col; j++)
					Console.Write("{0,2:D}", GetBrick(i, j)); //三位数显示
				Console.WriteLine();
			}
			Console.WriteLine();
			return;
		}

		public void ConsoleExchange()		//控制台交互函数，无条件交换两个块（即使并不相邻）
		{
			int x1 = -1, y1 = -1, x2 = -1, y2 = -1;             //交换用到的两个块的坐标
			string str1 = Console.ReadLine();                   //小心操作，如果输入的不是两个数字则引发错误
			string str2 = Console.ReadLine();                   //同上

			//分割字符串里的两个字符
			x1 = Convert.ToInt32(str1.Remove(str1.IndexOf(" ")));
			y1 = Convert.ToInt32(str1.Remove(0, str1.IndexOf(" ") + 1));
			x2 = Convert.ToInt32(str2.Remove(str2.IndexOf(" ")));
			y2 = Convert.ToInt32(str2.Remove(0, str2.IndexOf(" ") + 1));

			//无条件交换两个块
			Exchange(x1, y1, x2, y2);

			while (FindExplicit())                        //一直消除直到无法消除为止
				FindExplicit(true);
		}

	}

	class GamePool : EngineeringPool
	{
		int StartTime;
		int HistoryScore = 0;

		public GamePool(int row, int col, int Species) : base(row, col,Species)          //构造函数，无内容
		{
			InitGame();
			StartTime = 0;
		}		
		
		new public bool Exchange(int x1, int y1, int x2, int y2)    //交换相邻两个块的值（隐藏工程类的同名方法）
		{
			if (Math.Abs(x1 - x2) == 1 && y1==y2 || Math.Abs(y1 - y2) == 1 && x1==x2)    //确保交换的两个块是直线相邻而不是对角线相邻
			{
				int temp = GetBrick(x1, y1);
				SetBrick(x1, y1, GetBrick(x2, y2));
				SetBrick(x2, y2, temp);
				return true;
			}
			else return false;
		}

		public bool Exchange(Vector2 PosA,Vector2 PosB)				//重载，使用向量
		{
			int x1 = (int)PosA.X;
			int y1 = (int)PosA.Y;
			int x2 = (int)PosB.X;
			int y2 = (int)PosB.Y;
			return Exchange(x1, y1, x2, y2);
		}

		public Vector2 XY2Vector(int x,int y)		//将坐标转换为二维向量
		{
			return new Vector2(x, y);
		}

		public int GetBrick(Vector2 vector2)       //重载GetBrick函数，形参为向量，返回值为图片数
		{
			if ((int)vector2.X < 0 || (int)vector2.X > GetRow() - 1 ||
				(int)vector2.Y < 0 || (int)vector2.Y > GetCol() - 1)
				return 0;
			else
				return GetBrick((int)vector2.X, (int)vector2.Y); 
		}

		public int GetNowTime()
		{
			int hour = DateTime.Now.Hour;
			int min = DateTime.Now.Minute;
			int sec = DateTime.Now.Second;
			return hour * 3600 + min * 60 + sec;
		}

		public void SetStartTime()
		{
			int hour = DateTime.Now.Hour;
			int min = DateTime.Now.Minute;
			int sec = DateTime.Now.Second;
			StartTime = hour * 3600 + min * 60 + sec;
		}

		public int GetTimeRemain()
		{
			int hour = DateTime.Now.Hour;
			int min = DateTime.Now.Minute;
			int sec = DateTime.Now.Second;
			int TimeRemain = 15 - ((hour * 3600 + min * 60 + sec) - StartTime);
			if (TimeRemain >= 0)
				return TimeRemain;
			else
				return 0;
		}

		public void SetHistoryScore()
		{
			if(HistoryScore<GetScore())
				HistoryScore = GetScore();
		}

		public int GetHistoryScore()
		{
			return HistoryScore;
		}
	}
}

