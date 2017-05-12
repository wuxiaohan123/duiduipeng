using System;
using System.Collections;
using Microsoft.Xna.Framework;

namespace DuiDuiPeng
{
	class DuiDuiPeng									//控制台版本用到，在图形版中这个类没有用处。
	{
		static void main(string[] args)					//注意Main是M而不是m（后者是故意用错的）
		{
			GamePool Pool1 = new GamePool(5, 5, 11);    //设置游戏区的大小
			EngineeringPool debug = Pool1;				//debug可以调用工程类中隐藏的Exchange方法
			debug.ConsolePrintPool();

			while (true)								//游戏循环
			{
				Console.WriteLine("请输入两个坐标,每个坐标的x和y用空格分开，坐标之间用回车分开：");
				debug.ConsoleExchange();				//在debug静态方法里可任意交换两个块
				debug.ConsolePrintPool();				//控制台显示函数
				//Console.ReadLine();
			}
		}
	}


	abstract class Pool					//池子类,抽象类
	{
		private int[,] pool;            //定义一个池子数组
		private int row, col;           //池子的行、列
		private int score;				//游戏分数
		private static int species;     //图片的种类
		
		public int Row					//Row属性
		{
			get{return row;}
			set{row = value;}
		}

		public int Col					//Col属性
		{
			get { return col; }
			set { col = value; }
		}

		public int Score				//查询返回分数
		{
			get { return score;}
			set
			{
				if (value == 0)
					score = 0;
			}
		}

		public int Species				//返回图片种类数
		{
			get { return species; }
		}

		public Pool(int row, int col, int Species)  //构造函数
		{
			if (row < 0)                //避免错误的行、列数初始化游戏
				row = 0;
			if (col < 0)
				col = 0;
			if (Species < 0)
				Species = 0;

			pool = new int[row, col];   //实例化池子

			this.row = row;
			this.col = col;
			Pool.species = Species;
			score = 0;
		}

		public int GetBrick(int x, int y)//获取砖块的值
		{
			if (x < 0 || y < 0 || x > row - 1 || y > col - 1)
				return 0;               //避免输入错误的地址
			else
				return pool[x, y];
		}

		public void AddScore(int add)   //add可正可负，只能累加，不能直接更改成绩
		{
			if (score < 1000000)            //限定最高分（再高会超出显示范围）
				score += add;
		}

		public bool SetBrick(int x,int y ,int value)	//设置单个砖块的值		
		{
			if (value < 0 || x < 0 || y < 0 || x > row - 1 || y > col - 1) 
				return false;			

			pool[x, y] = value;         //砖块的值大于0小于等于图片数
			return true;
		}

		public void InitPool()			//初始化所有砖块（值设为0）
		{
			int i = 0, j = 0;
			for(i=0;i<row;i++)
				for(j=0;j<col;j++)
					pool[i, j] = 0;
			//score = 0;					//得分归零
			return;
		}
		
	}


	class EngineeringPool : Pool							//工程池类，包含控制台运行的代码
	{
		public Vector2[,] move ;							//显示坐标偏移数组，与动画有关

		public EngineeringPool(int row, int col, int Species) : base(row, col,Species)      //继承并初始化基类：Pool
		{
			move = new Vector2[Row, Col];
		}	

		public bool Exchange(int x1,int y1,int x2,int y2)	//无条件交换(x1,y1)和(x2,y2)的值，无论两块是否相邻
		{
			if (x1 < 0 || x1 > Row + 1 || y1 < 0 || y1 > Col + 1 ||
				x2 < 0 || x2 > Row + 1 || y2 < 0 || y2 > Col + 1)
				return false;
			int temp = GetBrick(x1, y1);
			SetBrick(x1, y1, GetBrick(x2, y2));
			SetBrick(x2, y2, temp);
			return true;
		}

		public void FixZero()								//遍历池子，发现有0元素则生成随机数填充之
		{
			int i, j;
			int row = Row;
			int col = Col;
			Random ran = new Random();						//设置时间为种子生成随机数

			for (i = 0; i < row; i++)
				for (j = 0; j < col; j++)
					if (GetBrick(i, j) == 0)
						SetBrick(i, j, ran.Next() % Species + 1);   //生成1到species的随机数填充进值为0的格子里

		}

		public void Downward()								//向下补充被消掉的块（如果将来需要消除动画，需要覆盖本函数的功能）
		{
			int row = Row;
			int col = Col;
			int i, j, temp;
			Queue q = new Queue();							//通过队列来实现元素的下落

			for (j = row - 1; j >= 0; j--)					//从最底层向上遍历
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
						temp = i;							//回到刚刚的第i行，
						while (temp >= 0)					//出队列，队列空了之后剩余位置填充0
						{
							if (q.Count > 0)
								SetBrick(j, temp, (int)q.Dequeue());
							else
								SetBrick(j, temp, 0);
							temp--;
						}
						q.Clear();                          //清空队列

						for (int k = i; k >= 0; k--)		//将消去的块上移100px，使之产生掉落效果
							move[j, k].Y = -100;
					}
			return;
		}

		public void _Downward()								//向下补充被消掉的块（原控制台函数，含有bug，元素从左向右下落）
		{													//故而在此被弃用。
			int row = Row;
			int col = Col;
			int i, j, temp;
			Queue q = new Queue();                          //通过队列来实现元素的下落

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
						while (temp >= 0)					//出队列，队列空了之后填充0
						{
							if (q.Count > 0)
								SetBrick(temp, j, (int)q.Dequeue());
							else
								SetBrick(temp, j, 0);
							temp--;
						}
						q.Clear();                          //清空队列
					}
			return;
		}


		public void RandomPool()        //随机填充池子
		{
			int row = Row;
			int col = Col;

			InitPool();                 //先清空池子
			FixZero();					//再随机填充
		}

		public bool FindExplicit()		//寻找显性解,发现至少有一个就返回true
		{
			int i, j;
			int temp=0, num = 0;		//temp保存临时值，num用于计数
			int row = Row;
			int col = Col;

			//逐行扫描，寻找显性解
			for (i = 0; i < row; i++)
				for (j = 0, temp = GetBrick(i,0), num = 0; j < col; j++)	//遍历每一行之前将temp初始化为行首的值，
				{															//即从行首开始，
					if (GetBrick(i, j) == temp && GetBrick(i, j) > 0)       //每发现一个元素就与前一个元素比较，
						num++;												//如果相同，则计数+1
					else
						num = 1;											//如果不相同，则计数回归到1，
					temp = GetBrick(i, j);									//并将temp向前推进一格

					if (num >= 3)											//如果发现三个及三个以上的值，
						return true;										//则判定存在显性解，可以消除（后边可以调用消除函数了）
				}

			//逐列扫描，寻找显性解。原理同逐行扫描，因此不再注释。
			for (j = 0; j < col; j++)
				for (i = 0, temp = GetBrick(0, j), num = 0; i < row; i++)   //遍历每一列之前将temp初始化为列首的值
				{
					if (GetBrick(i, j) == temp && GetBrick(i, j) > 0)
						num++;
					else
						num = 1;

					temp = GetBrick(i, j);									//将temp向前推进一格
					if (num >= 3)
						return true;
				}
			return false;
		}

		public bool FindExplicit(bool choice)                               //对寻找显性解函数的重载，可以解显性解，
		{                                                                   //同时加入一个缺省的选项，
			if (!choice)													//传入形参为false或缺省，
				return(FindExplicit());                                     //则默认只寻找而不消除

			int i, j;
			bool flag = false;
			int temp = 0, num = 0;											//temp保存临时值，num用于计数
			int row = Row;
			int col = Col;

			//逐行扫描
			for (i = 0; i < row; i++)
				for (j = 0, temp = GetBrick(i, 0), num = 0; j < col; j++)   //遍历每一行之前将temp初始化为行首的值
				{
					if (GetBrick(i, j) == temp && GetBrick(i, j) > 0)
						num++;
					else
					{ 
						num = 1;
						temp = GetBrick(i, j);			//将temp向前推进一格
					}

					if (num == 3)						//发现三连消
					{
						SetBrick(i, j, 0);
						SetBrick(i, j - 1, 0);
						SetBrick(i, j - 2, 0);
						AddScore(100);					//加100分
						flag = true;
					}
					else if(num==4)						//发现四连消
					{
						SetBrick(i, j, 0);
						SetBrick(i, j - 1, 0);
						SetBrick(i, j - 2, 0);
						SetBrick(i, j - 3, 0);
						AddScore(200);					//加200分
						flag = true;
					}
					else if(num==5)						//发现五连消（直线不会超过五连消）
					{
						SetBrick(i, j, 0);
						SetBrick(i, j - 1, 0);
						SetBrick(i, j - 2, 0);
						SetBrick(i, j - 3, 0);
						SetBrick(i, j - 4, 0);
						AddScore(200);					//加300分
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
						temp = GetBrick(i, j);			//将temp向前推进一格
					}

					if (num == 3)
					{
						SetBrick(i, j, 0);
						SetBrick(i - 1, j, 0);
						SetBrick(i - 2, j, 0);
						AddScore(100);
						flag = true;
					}
					else if (num == 4)
					{
						SetBrick(i, j, 0);
						SetBrick(i - 1, j, 0);
						SetBrick(i - 2, j, 0);
						SetBrick(i - 3, j, 0);
						AddScore(200);
						flag = true;
					}
					else if (num == 5)
					{
						SetBrick(i, j, 0);
						SetBrick(i - 1, j, 0);
						SetBrick(i - 2, j, 0);
						SetBrick(i - 3, j, 0);
						AddScore(300);
						flag = true;
					}

				}

			Downward();					//消除了的块上方的块下落填补				
			FixZero();					//上方空出来的块随机生成
			return flag;
		}

		public Vector3 FindRecessive()		//寻找隐性解，返回三维向量前两维为数组坐标，第三维为方向，0为横，1为竖，-1为无
		{
			//隐性解的定义：在图中存在潜在的、可以通过一次相邻块的交换而转变为显性解的情况。
			//游戏的进行必须要保证始终存在隐性解，如果无隐性解，玩家将被迫结束游戏
			//因此当发现无隐性解时需要打乱池子直到产生隐性解

			int row = Row;
			int col = Col;
			int i, j;
#if true
			//纵向1101解：
			for (j = 0; j < col - 3; j++)
				for (i = 0; i < row; i++)
					if (GetBrick(i, j) == GetBrick(i, j + 1) &&
						(GetBrick(i, j) == GetBrick(i, j + 3) || 
						GetBrick(i, j) == GetBrick(i + 1, j + 2) || 
						GetBrick(i, j) == GetBrick(i - 1, j + 2))) 
						return new Vector3(i, j, 1);
#endif
#if true

			//纵向1011解：
			for (j = 0; j < col - 3; j++)
				for (i = 0; i < row; i++)
					if (GetBrick(i, j + 2) == GetBrick(i, j + 3) &&
						(GetBrick(i, j + 2) == GetBrick(i + 1, j + 1) || 
						GetBrick(i, j + 2) == GetBrick(i - 1, j + 1) || 
						GetBrick(i, j + 2) == GetBrick(i, j))) 
						return new Vector3(i, j + 1, 1);

#endif
#if true
			//纵向101解：
			for (j = 0; j < col - 2; j++)
				for (i = 0; i < row; i++)
					if (GetBrick(i, j) == GetBrick(i, j + 2) &&
						(GetBrick(i, j) == GetBrick(i + 1, j + 1) ||
						GetBrick(i, j) == GetBrick(i - 1, j + 1)))
						return new Vector3(i, j, 1);
#endif
#if true
			//横向1101解：
			for (i = 0; i < row - 3; i++)
				for (j = 0; j < col; j++)
					if (GetBrick(i, j) == GetBrick(i + 1, j) &&
						(GetBrick(i, j) == GetBrick(i + 3, j) ||
						GetBrick(i, j) == GetBrick(i + 2, j + 1) || 
						GetBrick(i, j) == GetBrick(i + 2, j - 1))) 
						return new Vector3(i, j, 0);
#endif
#if true
			//横向1011解：
			for (i = 0; i < row - 3; i++)
				for (j = 0; j < col; j++)
					if (GetBrick(i + 2, j) == GetBrick(i + 3, j) &&
						(GetBrick(i + 2, j) == GetBrick(i, j) ||
						GetBrick(i + 2, j) == GetBrick(i + 1, j + 1) ||
						GetBrick(i + 2, j) == GetBrick(i + 1, j - 1)))  
						return new Vector3(i + 1, j, 0);
#endif
#if true
			//横向101解：
			for (i = 0; i < row - 2; i++)
				for (j = 0; j < col; j++)
					if (GetBrick(i, j) == GetBrick(i + 2, j) &&
						(GetBrick(i, j) == GetBrick(i + 1, j + 1) ||
						GetBrick(i, j) == GetBrick(i + 1, j - 1)))
						return new Vector3(i, j, 0);
#endif
			return new Vector3(-1, -1, -1);		

		}

		public void InitGame()			//初始化游戏
		{
			RandomPool();				//打乱池子
			while (FindExplicit())		//如果有显性解，则打乱池子直到不存在显性解
				RandomPool();
			while (((int)FindRecessive().Z) == -1)	//如果无隐性解，则打乱池子直到存在隐性解
				RandomPool();
			Score = 0;
		}

		public void ConsolePrintPool()		//控制台显示函数，仅用于debug
		{
			int row = Row;
			int col = Col;
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
			//输入格式：先输入第一个格子的的x和y坐标，中间用空格隔开，然后回车。然后同理输入第二个格子的坐标
			x1 = Convert.ToInt32(str1.Remove(str1.IndexOf(" ")));
			y1 = Convert.ToInt32(str1.Remove(0, str1.IndexOf(" ") + 1));
			x2 = Convert.ToInt32(str2.Remove(str2.IndexOf(" ")));
			y2 = Convert.ToInt32(str2.Remove(0, str2.IndexOf(" ") + 1));

			//无条件交换两个块
			Exchange(x1, y1, x2, y2);

			while (FindExplicit())			//一直消除直到无法消除为止
				FindExplicit(true);
		}

	}

	class GamePool : EngineeringPool		//游戏池类，加入了与XNA框架交互的函数
	{	
		private int StartTime;				//开始时间，数值为从当天0:00:00起到此刻的时间
		private int highScore = 0;          //保存历次游戏最高分（随对象回收而消失）
		private int life = 30;				//生命值，典型值为15秒，15秒内无操作则游戏结束

		public int Life						//生命值属性
		{
			get { return life; }
			set { life = value; }			//更改生命长度可以改变游戏难度
		}

		public int HighScore				//最高分属性
		{
			get { return highScore; }
			set
			{
				if (highScore < Score) 		//当前分高于历史最高分才予以更新最高分
					highScore = value;
			}
		}

		public GamePool(int row, int col, int Species) : base(row, col,Species)		//构造函数
		{
			InitGame();						//初始化游戏
		}
		
		new public bool Exchange(int x1, int y1, int x2, int y2)	//交换相邻两个块的值（隐藏工程类的同名方法）
		{
			if (x1 < 0 || x1 > Row + 1 || y1 < 0 || y1 > Col + 1 ||
				x2 < 0 || x2 > Row + 1 || y2 < 0 || y2 > Col + 1)
				return false;

			if (Math.Abs(x1 - x2) == 1 && y1==y2 || Math.Abs(y1 - y2) == 1 && x1==x2)	//交换仅限上下左右相邻的块
			{
				int temp = GetBrick(x1, y1);	
				SetBrick(x1, y1, GetBrick(x2, y2));
				SetBrick(x2, y2, temp);
				return true;
			}
			else return false;
		}

		public bool Exchange(Vector2 PosA,Vector2 PosB)		//重载交换函数，但使用XNA框架中的向量结构
		{
			int x1 = (int)PosA.X;
			int y1 = (int)PosA.Y;
			int x2 = (int)PosB.X;
			int y2 = (int)PosB.Y;
			return Exchange(x1, y1, x2, y2);				//将向量转换为标量后调用函数
		}

		public Vector2 XY2Vector(int x,int y)				//将二维坐标转换为二维向量
		{
			return new Vector2(x, y);
		}

		public int GetBrick(Vector2 vector2)				//重载GetBrick函数，形参为向量
		{
			if ((int)vector2.X < 0 || (int)vector2.X > Row - 1 ||
				(int)vector2.Y < 0 || (int)vector2.Y > Col - 1)
				return 0;
			else
				return GetBrick((int)vector2.X, (int)vector2.Y); 
		}

		public int GetNowTime()								//时间查询函数，查询从0:00:00到此刻的秒数
		{
			int hour = DateTime.Now.Hour;
			int min = DateTime.Now.Minute;
			int sec = DateTime.Now.Second;
			return hour * 3600 + min * 60 + sec;
		}

		public void SetStartTime()							//将此刻设置为游戏开始时刻
		{
			int hour = DateTime.Now.Hour;
			int min = DateTime.Now.Minute;
			int sec = DateTime.Now.Second;
			StartTime = hour * 3600 + min * 60 + sec;
		}

		public int GetTimeRemain()							//返回所剩的时间，从生命值倒计时至0，负数也显示为0
		{
			int life = Life;								//生命值
			int hour = DateTime.Now.Hour;
			int min = DateTime.Now.Minute;
			int sec = DateTime.Now.Second;
			int TimeRemain = life - ((hour * 3600 + min * 60 + sec) - StartTime);	//计算此刻的生命长度并返回
			if (TimeRemain >= 0)
				return TimeRemain;							//生命为负也显示为0
			else
				return 0;
		}

	}
}

