using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace DuiDuiPeng
{
	public class DuiDuiPengGUI : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;				//定义设备管理器（用于设定显示窗口）
		SpriteBatch spriteBatch;					//实例化精灵对象（绘制精灵）

		enum GameState { Start, InGame, GameOver };		//游戏状态枚举
		GameState currentGameState = GameState.Start;	//设定游戏开始

		//关于游戏的设置
		const int species = 11;						//游戏使用的图片种类数（最大为11），与难度挂钩，不宜设置过低
													//测试发现，在7*10的图中少于4种图片会无法生成可行解，期待数学家给出证明
		const int row = 16;							//设置行数
		const int col = 12;							//设置列数
		const int dx = 8;							//在窗口内显示池子位置的偏移量x和y,视为池子显示的位置起始坐标
		const int dy = 8;
		const int downside = 8;						//下边界，从池子区底部到下边界的距离
		const int rightside = (int)(350 * scale);   //右边界，从池子区到右边界的距离
		const int PicSize = 64;						//图片边长（图片是正方形，单位为像素）
		const float scale = 0.75f;					//显示缩放量，与窗口内各对象大小有关
													//建议：大图1.0倍，中图0.8倍，小图0.6倍，不要大于1.0倍也不要小于0.6倍
		const int block = (int)(PicSize * scale);	//图片的显示宽度（scale为浮点型）
		const int spacing = (int)(8 * scale);		//图片与图片之间的间距
		KeyboardState keyboardstate;				//键盘状态对象
		MouseState mousestate;						//鼠标状态对象
		bool before = false;                        //保存上一次鼠标点击的结果，用于单击判定
		bool FirstClick = false;					//记录上一次点击是否成功消除方块
		int TimeRemain = 0;                         //剩余时间，在update方法中从GamePool类中不断更新
		int GameOverTime = 0;						//记录GameOver的时间（防止沉迷）
		int Wait3Second = 0;                        //等待三秒（用于GameOver状态和提示状态）
		int IdeaLeftBehind;							//提示的次数
		bool ShowRecessive;							//显示隐性解的开关

		GamePool gamepool = new GamePool(row, col, species);//实例化游戏池

		//资源文件，图片、字体等
		Texture2D[] SourceImage = new Texture2D[12];//源图数组(11+1，多出来的1个为空白图片)
		Texture2D refress;                          //重置(reset)图标	
		Texture2D idea;								//提示(idea)图标
		Texture2D box;                              //提示隐性解的方框	
		Texture2D box1;
		SpriteFont MyFont;							//定义字体文件
		
		//显示器二维坐标
		Vector2 ScorePos = new Vector2((30 + dx + row * (block + spacing)), dy);					//得分的显示坐标
		Vector2 RefressPos = new Vector2((30 + dx + row * (block + spacing)), 1.5f * block * scale);//重置(refress)图标的显示坐标
		Vector2 IdeaPos = new Vector2((30 + dx + row * (block + spacing)), 5 * block * scale);		//隐性解提示(idea)图标的显示坐标
		Vector2 TimeRemainPos = new Vector2((30 + dx + row * (block + spacing)), 9 * block * scale); //剩余时间的显示坐标

		//显示隐性解的位置以及方向
		Vector2 BoxPos;
		Vector2 BoxPos1;
		int BoxPosAxis;
		

		//数组二维坐标
		Vector2 Interesting = new Vector2(species + 1, species + 1);						//兴趣点的数组坐标
		Vector2 mousePosInMap = new Vector2(species + 1, species + 1);						//将鼠标像素位置转换为数组坐标
		Vector2 tempMousePosInMap = new Vector2(species + 1, species + 1);



		public DuiDuiPengGUI()								//构造函数
		{
			graphics = new GraphicsDeviceManager(this);		//新建显示窗口

			//窗口宽的计算：池子偏移量dx+池子右边的空间宽度rightside+池子的宽度
			//窗口高的计算：池子偏移量dy+池子下边的空间宽度downside+池子的高度
			graphics.PreferredBackBufferWidth  = dx + rightside + row * (block + spacing) - spacing;	//设置显示区域宽度
			graphics.PreferredBackBufferHeight = dy + downside + col * (block + spacing) - spacing;		//设置显示区域高度

			Content.RootDirectory = "Content";				//设置目录
		}

		protected override void Initialize()				//初始化游戏函数
		{
			base.Initialize();
		}

		protected override void LoadContent()				//加载游戏内容函数
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);	//新建精灵管理器
			IsMouseVisible = true;                          //允许窗口内显示鼠标

			Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, 
				graphics.PreferredBackBufferHeight / 2);	//将鼠标位置设置在窗口中心（没有什么特殊含义）

			//加载图片，此处数组下标从1开始，0为空白图片
			SourceImage[0] = Content.Load<Texture2D>(@"Images\blank");
			SourceImage[1] = Content.Load<Texture2D>(@"Images\c-sharp");
			SourceImage[2] = Content.Load<Texture2D>(@"Images\java");
			SourceImage[3] = Content.Load<Texture2D>(@"Images\cpp");
			SourceImage[4] = Content.Load<Texture2D>(@"Images\php");
			SourceImage[5] = Content.Load<Texture2D>(@"Images\go");
			SourceImage[6] = Content.Load<Texture2D>(@"Images\python");
			SourceImage[7] = Content.Load<Texture2D>(@"Images\matlab");
			SourceImage[8] = Content.Load<Texture2D>(@"Images\javascript");
			SourceImage[9] = Content.Load<Texture2D>(@"Images\perl");
			SourceImage[10] = Content.Load<Texture2D>(@"Images\ruby");
			SourceImage[11] = Content.Load<Texture2D>(@"Images\swift");
			refress = Content.Load<Texture2D>(@"Images\refress");
			box = Content.Load<Texture2D>(@"Images\box");
			box1 = Content.Load<Texture2D>(@"Images\box1");
			idea = Content.Load<Texture2D>(@"Images\idea");

			//加载精灵字体
			MyFont = Content.Load<SpriteFont>(@"Fonts\Score");
			
			gamepool.SetStartTime();						//设定开始时间
		}

		protected override void UnloadContent() { }         //卸载内容函数

		protected override void Update(GameTime gameTime)	//更新状态函数
		{
			keyboardstate = Keyboard.GetState();			//获取键盘状态
			mousestate = Mouse.GetState();                  //获取鼠标状态
			
			
			mousePosInMap = new Vector2(                    //将鼠标显示坐标转换为数组坐标
				BitmapToArrayX(mousestate.X), 
				BitmapToArrayY(mousestate.Y));

			switch (currentGameState)						//不同状态不同更新方式
			{
				case GameState.Start:						//游戏开始前状态

					gamepool.SetStartTime();                //保持进入游戏前时间始终为满

					if (mousestate.LeftButton == ButtonState.Released && before &&
						mousestate.X >= 0 && mousestate.Y >= 0 &&
						mousestate.X < graphics.PreferredBackBufferWidth &&
						mousestate.Y < graphics.PreferredBackBufferHeight)
					{										//判断鼠标单击一次
						gamepool.InitGame();				//初始化游戏
						currentGameState = GameState.InGame;//切换到游戏中模式
						IdeaLeftBehind = 5;
					}

					break;


				case GameState.InGame:                      //游戏中状态

					//交换方块：
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.E))
					{
						//debug方法：同时按住键盘W键和E键，可以无条件交换两个相邻块
						//（此功能可以用于作弊，一般人我不告诉他哈哈）
						if (mousestate.LeftButton == ButtonState.Released && before)//检测单击事件
						{
							if (mousePosInMap != new Vector2(-1, -1))
							{
								gamepool.Exchange(mousePosInMap, Interesting);          //无条件交换当前点击的块跟兴趣块（必须是相邻才能交换）
								Interesting = mousePosInMap;                            //将当前点击的块设置为兴趣块
							}
						}
						while (gamepool.FindExplicit(false))                        //一直消除直到无法消除为止
						{
							gamepool.FindExplicit(true);
							gamepool.SetStartTime();
						}
					}
					else
					{
						//点击第一次激活方块，点击第二次交换，若交换后能消除则交换并消除，若交换后不能消除则不交换
						if (mousestate.LeftButton == ButtonState.Released && before)//检测单击事件
						{
							gamepool.Exchange(mousePosInMap, Interesting);          //先交换两个块，

							if (!gamepool.FindExplicit(false))                      //如果发现交换了无法消除，则撤销交换
							{
								gamepool.Exchange(Interesting, mousePosInMap);
								FirstClick = true;
							}
							while (gamepool.FindExplicit(false))                    //一直消除直到无法消除为止
							{
								gamepool.FindExplicit(true);
								gamepool.SetStartTime();
							}

							//此功能的目的是：在一次成功消除之后，取消当前的兴趣点，再点击一次则设置兴趣点，避免连击
							if (FirstClick)                                         //FirstClick表示上一次点击是否成功消除									
								Interesting = mousePosInMap;                        //设置兴趣块为当前鼠标所点的块
							else
								Interesting = new Vector2(row, col);                //取消图中的兴趣块

							FirstClick = false;                                     //成功消除之后将FirstClick记录改为false
							Wait3Second = 0;                                        //消除后立即关闭提示框
						}
					}

					if ((gamepool.FindRecessive().Z) == -1)
					{
						gamepool.RandomPool();
						while (gamepool.FindExplicit(false))						//一直消除直到无法消除为止
						{
							gamepool.FindExplicit(true);
							gamepool.SetStartTime();
						}
					}

					//获得隐性解提示的坐标
					Vector3 temp = gamepool.FindRecessive();
					BoxPos = ArrayToBitmap((int)temp.X, (int)temp.Y) + new Vector2(-4, -4);
					BoxPos1 = ArrayToBitmap((int)temp.X, (int)temp.Y) + new Vector2(-4, -4);
					BoxPosAxis =(int)(gamepool.FindRecessive()).Z;

					//BoxPos =new Vector2((int)mousestate.X, (int)mousestate.Y);   //debug：显示鼠标位置
					//BoxPosAxis = 0;

					
					//debug方法：同时按住键盘W键和T键时间加满
					//（此功能可以用于作弊，一般人我不告诉他哈哈）
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.T))
						gamepool.SetStartTime();

					//debug方法：同时按住键盘W键和K键自杀
					//（这功能怕是没有人会用吧……）
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.K))
					{
						currentGameState = GameState.GameOver;      //转换到GameOver状态
						GameOverTime = gamepool.GetNowTime();       //获得GameOver的时间
						gamepool.HighScore = gamepool.Score;        //设定最高分（当得分大于历史最高分时才可以设定成功）
					}

					//debug方法：同时按住键盘W键和S键快速加得分
					//（此功能可以用于作弊，一般人我不告诉他哈哈）
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.S))
						gamepool.AddScore(1000);

					//单击refress按钮刷新数组&得分清零，if中语句判断点击是否在refress图片区域内且单击有效
					if (mousestate.X >= RefressPos.X && mousestate.X <= RefressPos.X + 128 * scale &&
					mousestate.Y >= RefressPos.Y && mousestate.Y <= RefressPos.Y + 128 * scale &&
					mousestate.LeftButton == ButtonState.Released && before)
					{
						gamepool.InitGame();			//初始化游戏
						gamepool.SetStartTime();		//得分清零
					}


					//单击idea按钮显示提示（显示三秒）
					if (mousestate.X >= IdeaPos.X && mousestate.X <= RefressPos.X + 128 * scale &&
						mousestate.Y >= IdeaPos.Y && mousestate.Y <= IdeaPos.Y + 128 * scale &&
						mousestate.LeftButton == ButtonState.Released && before && IdeaLeftBehind > 0)
					{
						Wait3Second = 3 + gamepool.GetNowTime();
						IdeaLeftBehind--;
					}

					//3秒内显示
					//debug方法：同时按住键盘W键和R键显示隐性解提示框
					if ((Wait3Second - gamepool.GetNowTime() > 0) || 
						keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.R))
						ShowRecessive = true;
					else
						ShowRecessive = false;

					//控制难度：时间阶梯减少
					if (gamepool.Score > 100000)
						gamepool.Life = 30 - gamepool.Score / 33333;//每十万分减少3秒
					

					//剩余时间到0则game over
					if (TimeRemain == 0)
					{
						currentGameState = GameState.GameOver;      //转换到GameOver状态
						GameOverTime = gamepool.GetNowTime();		//获得GameOver的时间
						gamepool.HighScore = gamepool.Score;		//设定最高分（当得分大于历史最高分时才可以设定成功）
					}

					break;


				case GameState.GameOver:                            //GameOver状态

					//等待3秒：防止沉迷，防止GameOver之后马上开始下一轮游戏
					Wait3Second = 3 - (gamepool.GetNowTime() - GameOverTime);   //获取从GameOver到现在的时间
					gamepool.Life = 30;                             //生命值恢复30秒

					if (mousestate.LeftButton == ButtonState.Released &&
						mousestate.X >= 0 && mousestate.Y >= 0 &&
						mousestate.X < graphics.PreferredBackBufferWidth &&
						mousestate.Y < graphics.PreferredBackBufferHeight &&
						before && Wait3Second <= 0) 
					{
						gamepool.InitGame();
						gamepool.SetStartTime();                    //时间清零
						IdeaLeftBehind = 5;							//提示次数恢复
						currentGameState = GameState.InGame;		//跳转至游戏状态
					}

					break;
			}

			//处理单击逻辑：上一次点下且本次抬起才是有效单击，否则刷新本次记录
			if (mousestate.LeftButton == ButtonState.Pressed)
				before = true;
			else
				before = false;

			//按Q键退出
			if (keyboardstate.IsKeyDown(Keys.Q))            //键盘输入Q退出
				this.Exit();

			//从GamePool类示例更新剩余时间
			TimeRemain = gamepool.GetTimeRemain();

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)					//绘制函数
		{ 
			switch (currentGameState)									//不同状态绘制不同界面
			{
				case GameState.Start:									//开始界面

					GraphicsDevice.Clear(Color.Orange);					//橙色背景

					//绘制开始提示字符
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"click window to start game :)\n        good luck !",	//放置会话
						new Vector2(                                    //显示坐标（9*block宽）
							graphics.PreferredBackBufferWidth / 2 - 5 * block,
							graphics.PreferredBackBufferHeight / 2 - block),								
						Color.DarkBlue,									//颜色
						0, Vector2.Zero, 
						scale,											//按比例缩放
						SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制作者信息
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"author: wuxiaohan\n   from UESTC",				//放置作者信息
						new Vector2(                                    //显示坐标
							0, 
							graphics.PreferredBackBufferHeight - block),
						Color.DarkBlue, 0, Vector2.Zero, 
						scale * 0.5f,									//缩小至0.5倍的小字
						SpriteEffects.None, 1);
					spriteBatch.End();

					break;


				case GameState.InGame:									//游戏中状态

					GraphicsDevice.Clear(Color.WhiteSmoke);             //用白烟色刷新窗口

					//遍历显示池子中的图片
					spriteBatch.Begin();
					for (int i = 0; i < gamepool.Row; i++)
						for (int j = 0; j < gamepool.Col; j++)
							spriteBatch.Draw(
								SourceImage[gamepool.GetBrick(i, j)],	//从源图中选取显示的图片作为池子里的brick数显示
								ArrayToBitmap(i, j),					//设置坐标
								null,									//关于24格动画数组的设定，此处留空
								Color.White, 
								0, Vector2.Zero, 
								scale,									//设置缩放
								SpriteEffects.None, 0);					//设置旋转、对称、层深等显示方法
					spriteBatch.End();

					//显示一个叠加层，高亮指示兴趣点
					spriteBatch.Begin();
					spriteBatch.Draw(
						SourceImage[gamepool.GetBrick(Interesting)],
						ArrayToBitmap(Interesting), 
						null, 
						Color.Green,									//此处修改叠加层的颜色
						0, Vector2.Zero, 
						scale, 
						SpriteEffects.None, 0.99f);						//叠加层为最表层（最后一个形参）
					spriteBatch.End();

					//绘制得分
					spriteBatch.Begin();
					spriteBatch.DrawString(
						MyFont,											//设置字体
						"Score:" + gamepool.Score,						//设置得分格式
						ScorePos,										//放置的位置向量
						Color.DarkBlue,									//字体颜色
						0, Vector2.Zero, 
						scale,											//字体缩放
						SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制refress按钮
					spriteBatch.Begin();
					spriteBatch.Draw(									//绘制方式同上，此处不再注释
						refress,
						RefressPos, 
						null, 
						Color.White,
						0, Vector2.Zero, 
						scale, 
						SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制idea按钮
					spriteBatch.Begin();
					spriteBatch.Draw(                                   //绘制方式同上，此处不再注释
						idea,
						IdeaPos,
						null,
						Color.White,
						0, Vector2.Zero,
						scale,
						SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制idea剩余提示次数
					spriteBatch.Begin();
					spriteBatch.DrawString(
						MyFont,
						" = " + IdeaLeftBehind,							//放置次数
						IdeaPos + new Vector2(128 * scale, 40 * scale), //放置的位置向量
						Color.DarkBlue,                                 //颜色
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制剩余时间
					spriteBatch.Begin();
					spriteBatch.DrawString(
						MyFont,
						"Time:" + TimeRemain,							//放置时间
						TimeRemainPos,									//放置的位置向量
						Color.DarkBlue,									//颜色
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制隐性解的提示框(按W和R键显示),rect表示矩形是横的还是竖的（0是横，1是竖）
					if (ShowRecessive && BoxPosAxis==0)
					{
						spriteBatch.Begin();
						spriteBatch.Draw(
							box,
							BoxPos,
							null,
							Color.White,
							0, Vector2.Zero,
							scale,
							SpriteEffects.None, 1
							);
						spriteBatch.End();
					}
					else if (ShowRecessive && BoxPosAxis == 1)
					{
						spriteBatch.Begin();
						spriteBatch.Draw(
							box1,
							BoxPos1,
							null,
							Color.White,
							0, Vector2.Zero,
							scale,
							SpriteEffects.None, 1
							);
						spriteBatch.End();
					}

					break;


				case GameState.GameOver:

					GraphicsDevice.Clear(Color.OrangeRed);

					//绘制Game Over字符
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Game Over :(",									//放置会话
						new Vector2(                                    //字体显示坐标（宽度为7*block）
							graphics.PreferredBackBufferWidth / 2 - 5 * block,
							graphics.PreferredBackBufferHeight / 2 -  2 * block),
						Color.DarkBlue,							
						0, Vector2.Zero, 
						scale * 2,										//两倍缩放
						SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制最终得分
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Score:  "+gamepool.Score,						//放置会话
						new Vector2(                                    //字体显示坐标（比GameOver向右2*block）
							graphics.PreferredBackBufferWidth / 2 - 5 * block,
							graphics.PreferredBackBufferHeight / 2),
						Color.DarkBlue,								
						0, Vector2.Zero, scale, 
						SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制历史最高分
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Highest:" + gamepool.HighScore,				//放置会话
						new Vector2(                                    //字体显示坐标
							graphics.PreferredBackBufferWidth / 2 - 5 * block,
							graphics.PreferredBackBufferHeight / 2 + block),
						Color.DarkBlue,                            
						0, Vector2.Zero, scale, 
						SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制点击界面继续字符
					if (Wait3Second <= 0)
					{
						spriteBatch.Begin();
						spriteBatch.DrawString(MyFont,
							"click window to restart...",                   //放置会话
							new Vector2(                                    //字体显示坐标
								graphics.PreferredBackBufferWidth / 2 - 5 * block,
								graphics.PreferredBackBufferHeight / 2 + 3 * block),
							Color.DarkBlue,
							0, Vector2.Zero, scale,
							SpriteEffects.None, 1);
						spriteBatch.End();
					}

					break;
			}

			base.Draw(gameTime);
		}

		
		public int BitmapToArrayX(int x)					//将显示坐标的x坐标转换为数组坐标
		{
			if (x < dx || x> dx + row * (block + spacing) - spacing)//x应当在图中
				return -1;									//若不在数组中，返回-1

			int n = (x - dx) / (block + spacing);			//粗略的数组坐标（向下取整导致精度不高）

			if (x > dx + n * (block + spacing) &&			//鼠标在左边界右边
				x < dx + n * (block + spacing) + block)		//鼠标在右边界左边
				return n;
			else
				return -1;									//若不在数组中，返回-1
		}

		public int BitmapToArrayY(int y)					//将显示坐标的y坐标转换为数组坐标
		{													//注释同上
			if (y < 0 || y > dy + col * (block + spacing) - spacing)	
				return -1;

			int n = (y - dy) / (block + spacing);			

			if (y > dy + n * (block + spacing) &&
				y< dy + n * (block + spacing) + block)
				return n;
			else
				return -1;
		}
	
		public Vector2 ArrayToBitmap(Vector2 vetcor2)		//将数组二维坐标转换为显示二维坐标
		{
			int i = (int)vetcor2.X;
			int j = (int)vetcor2.Y;
			return new Vector2((block + spacing) * i + dx, (block + spacing) * j + dy);
		}

		public Vector2 ArrayToBitmap(int i,int j)           //将数组坐标转换为显示二维坐标
		{
			return new Vector2((block + spacing) * i + dx, (block + spacing) * j + dy);
		}
	}
}

