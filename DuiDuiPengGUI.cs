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
		GraphicsDeviceManager graphics;		//设备管理器
		SpriteBatch spriteBatch;            //精灵对象

		enum GameState { Start, InGame, GameOver };		//游戏状态枚举
		GameState currentGameState = GameState.Start;	//设定游戏开始

		const int species = 11;             //图片种类数（最大为11），与难度挂钩，不宜设置过低
											//测试发现，在7*10的图中少于4种图片会无法生成可行解，期待数学家给出证明
		const int row = 7;					//设置行数
		const int col = 10;					//设置列数
		const int dx = 8;					//在窗口内显示图片位置的偏移量x和y,视为池子显示的位置起始坐标
		const int dy = 8;
		const int downside = 8;             //下边界，从池子区到下边界的距离

		const int PicSize = 64;				//图片边长（图片是正方形，单位为像素）
		const float scale = 0.9f;           //图片缩放量，实际显示大小为PicSize*scale
											//建议：大图1.0倍，中图0.8倍，小图0.6倍，不要小于0.6倍
		const int rightside = (int)(350 * scale);   //右边界，从池子区到右边界的距离
		const int block = (int)(PicSize * scale); //块的宽度（int型可以省去不必要的强制转换）
		const int spacing = 6;              //图片与图片之间的间距

		KeyboardState keyboardstate;        //键盘状态对象
		MouseState mousestate;              //鼠标状态对象的临时对象
		bool before = false;

		int TimeRemain = 0;

		GamePool gamepool = new GamePool(row, col, species);         //实例化游戏池
		Texture2D[] SourceImage = new Texture2D[12];        //源图数组(下标+1，多出来的1个为空白图片)
		SpriteFont MyFont;						//定义字体文件

		//重置图标及其位置（坐标与鼠标识别有关）
		Texture2D refress;							
		Vector2 RefressPos = new Vector2((30 + dx + row * (block + spacing)), 50 * scale);

		Vector2 Interesting = new Vector2(species + 1, species + 1); //兴趣点的数组坐标
		Vector2 IsChosen = new Vector2(species + 1, species + 1);    //被选中的数组坐标
		
		Vector2 mousePosInMap = new Vector2(species + 1, species + 1);	//将鼠标像素位置转换为矩阵坐标
		Vector2 tempMousePosInMap = new Vector2(species + 1, species + 1);

		public DuiDuiPengGUI()					//构造函数
		{
			graphics = new GraphicsDeviceManager(this);

			//窗口宽的计算：池子偏移量dx+池子右边的空间宽度rightside+池子的宽度：图片宽度row*(块宽度为缩放的图片宽度block)
			graphics.PreferredBackBufferWidth  = dx + rightside + row * (block + spacing) - spacing;	//设置窗口宽高
			graphics.PreferredBackBufferHeight = dy + downside + col * (block + spacing) - spacing;

			Content.RootDirectory = "Content";	//设置目录
		}

		protected override void Initialize()		//初始化游戏函数
		{
			// TODO: Add your initialization logic here
			
			base.Initialize();
		}

		protected override void LoadContent()		//加载游戏内容函数
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);	//新建精灵管理器
			IsMouseVisible = true;                          //允许显示鼠标位置

			Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, 
				graphics.PreferredBackBufferHeight / 2);	//将鼠标位置设置在窗口中心（没有什么特殊含义）

			//加载图片，此处数组下标从1开始，0为空白图片
			SourceImage[0] = Content.Load<Texture2D>(@"Images\blank");
			SourceImage[1] = Content.Load<Texture2D>(@"Images\c-sharp");
			SourceImage[2] = Content.Load<Texture2D>(@"Images\cpp");
			SourceImage[3] = Content.Load<Texture2D>(@"Images\go");
			SourceImage[4] = Content.Load<Texture2D>(@"Images\java");
			SourceImage[5] = Content.Load<Texture2D>(@"Images\javascript");
			SourceImage[6] = Content.Load<Texture2D>(@"Images\matlab");
			SourceImage[7] = Content.Load<Texture2D>(@"Images\perl");
			SourceImage[8] = Content.Load<Texture2D>(@"Images\php");
			SourceImage[9] = Content.Load<Texture2D>(@"Images\python");
			SourceImage[10] = Content.Load<Texture2D>(@"Images\ruby");
			SourceImage[11] = Content.Load<Texture2D>(@"Images\swift");

			refress = Content.Load<Texture2D>(@"Images\refress");

			//加载精灵字体
			MyFont = Content.Load<SpriteFont>(@"Fonts\Score");
			

			gamepool.SetStartTime();
		}

		protected override void UnloadContent()		//卸载内容
		{
			// TODO: Unload any non ContentManager content here
		}

		protected override void Update(GameTime gameTime)	//更新状态函数
		{
			keyboardstate = Keyboard.GetState();			//获取键盘状态
			mousestate = Mouse.GetState();                  //获取鼠标状态
			
			//将鼠标坐标转换为数组坐标
			mousePosInMap = new Vector2(BitmapToArrayX(mousestate.X), BitmapToArrayY(mousestate.Y));

			switch (currentGameState)		//不同状态不同更新方式
			{
				case GameState.Start:
					gamepool.SetStartTime();
					if (mousestate.LeftButton == ButtonState.Pressed)
					{
						currentGameState = GameState.InGame;
						gamepool.InitGame();
					}
					break;


				case GameState.InGame:

					//debug（也可以作弊）方法：同时按住键盘W键和E键
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.E))
					{
						if (mousestate.LeftButton == ButtonState.Released && before)
						{
							gamepool.Exchange(mousePosInMap, Interesting);
							Interesting = mousePosInMap;
						}
						while (gamepool.FindExplicit(false))                    //一直消除直到无法消除为止
						{
							gamepool.FindExplicit(true);
							gamepool.SetStartTime();
						}
					}
					else
					{
						//点击第一次激活方块，点击第二次交换，若交换后能消除则交换并消除，若交换后不能消除则不交换
						if (mousestate.LeftButton == ButtonState.Released && before)
						{
							gamepool.Exchange(mousePosInMap, Interesting);
							if (!gamepool.FindExplicit(false))                      //如果交换了无法消除，则不交换
							{
								gamepool.Exchange(Interesting, mousePosInMap);

							}
							while (gamepool.FindExplicit(false))                    //一直消除直到无法消除为止
							{
								gamepool.FindExplicit(true);
								gamepool.SetStartTime();
							}

							Interesting = mousePosInMap;
						}
					}

					//debug（也可以作弊）方法：同时按住键盘W键和E键时间加满
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.T))
						gamepool.SetStartTime();

					//单击refress按钮刷新数组&得分
					if (mousestate.X >= RefressPos.X && mousestate.X <= RefressPos.X + 128 * scale &&
					mousestate.Y >= RefressPos.Y && mousestate.Y <= RefressPos.Y + 128 * scale &&
					mousestate.LeftButton == ButtonState.Released && before)
					{
						gamepool.InitGame();
						gamepool.SetStartTime();
					}

					//处理单击逻辑：上一次点下且本次抬起才是有效单击，否则刷新本次记录
					if (mousestate.LeftButton == ButtonState.Pressed)
						before = true;
					else
						before = false;

					//剩余时间到0则gameover
					if (TimeRemain == 0)
					{
						currentGameState = GameState.GameOver;
						gamepool.SetHistoryScore();
					}

					break;


				case GameState.GameOver:
					gamepool.SetStartTime();
					if (mousestate.LeftButton == ButtonState.Pressed)
						currentGameState = GameState.Start;
					
					break;
			}
		
			//按Q键退出
			if (keyboardstate.IsKeyDown(Keys.Q))            //键盘输入Q退出
				this.Exit();

			//更新时间
			TimeRemain = gamepool.GetTimeRemain();
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)     //绘制函数
		{ 
			switch (currentGameState)
			{
				case GameState.Start:

					GraphicsDevice.Clear(Color.Orange);

					//绘制开始提示字符
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"click window to start game :)\n        good luck !",//放置会话
						new Vector2(                                //把字体放在窗口中间
							block,
							graphics.PreferredBackBufferHeight / 2 - block),								
						Color.DarkBlue,                             //颜色
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制作者信息
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"author: wuxiaohan\n   from UESTC",//放置会话
						new Vector2(                                //把字体放在窗口中间
							0,
							graphics.PreferredBackBufferHeight - block),
						Color.DarkBlue,                             //颜色
						0, Vector2.Zero, scale * 0.5f, SpriteEffects.None, 1);
					spriteBatch.End();

					break;


				case GameState.InGame:

					GraphicsDevice.Clear(Color.LightSkyBlue);               //白色刷新窗口

					//遍历显示池子中的图片
					spriteBatch.Begin();
					for (int i = 0; i < gamepool.GetRow(); i++)
						for (int j = 0; j < gamepool.GetCol(); j++)
							spriteBatch.Draw(SourceImage[gamepool.GetBrick(i, j)], //从源图中选取显示的图片作为池子里的brick数显示
								ArrayToBitmap(i, j),    //设置坐标，通过scale调整大小，通过dx和dy调整偏移量，PicSize为源图尺寸
								null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);  //设置旋转、对称等显示方法
					spriteBatch.End();

					//显示一个叠加层，高亮指示兴趣点
					spriteBatch.Begin();
					spriteBatch.Draw(
						SourceImage[gamepool.GetBrick(Interesting)],
						ArrayToBitmap(Interesting), null, Color.Green,            //此处修改叠加层的颜色
						0, Vector2.Zero, scale, SpriteEffects.None, 0.99f);       //叠加层为最表层（最后一个形参）
					spriteBatch.End();

					//绘制得分
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Score:" + gamepool.GetScore(),             //放置得分格式
						new Vector2((30 + dx + row * (block + spacing)), dy), //放置的位置向量
						Color.DarkBlue,                             //颜色
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制refress按钮
					spriteBatch.Begin();
					spriteBatch.Draw(
						refress,
						RefressPos, null, Color.White,
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制时间
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Time:" + TimeRemain,                       //放置时间
						new Vector2((30 + dx + row * (block + spacing)), dy + 200), //放置的位置向量
						Color.DarkBlue,                             //颜色
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					break;


				case GameState.GameOver:

					GraphicsDevice.Clear(Color.OrangeRed);

					//绘制Game Over字符
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Game Over :(",								//放置会话
						new Vector2(                                //把字体放在窗口中间
							4 * block,
							graphics.PreferredBackBufferHeight / 2 -  2 * block),
						Color.DarkBlue,                             //颜色
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制最终得分
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Score:  "+gamepool.GetScore(),               //放置会话
						new Vector2(                                //把字体放在窗口中间
							4 * block,
							graphics.PreferredBackBufferHeight / 2),
						Color.DarkBlue,                             //颜色
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制历史最高分
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Highest:" + gamepool.GetHistoryScore(),               //放置会话
						new Vector2(                                //把字体放在窗口中间
							4 * block,
							graphics.PreferredBackBufferHeight / 2 + block),
						Color.DarkBlue,                             //颜色
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//绘制点击界面继续字符
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"click window to restart",                  //放置会话
						new Vector2(                                //把字体放在窗口中间
							3 * block,
							graphics.PreferredBackBufferHeight / 2 + 2 * block),
						Color.DarkBlue,                             //颜色
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					break;
			}

			base.Draw(gameTime);
		}

		
		public int BitmapToArrayX(int x)					//将像素的x坐标转换为数组坐标
		{
			if (x < 0 || x> Window.ClientBounds.Width)		//x应当在图中
				return -1;									//若不在数组中，返回-1

			int n = (x - dx) / (block + spacing);			//粗略的数组坐标（向下取整导致精度不高）

			if (x > dx + n * (block + spacing) &&			//鼠标在左边界右边
				x < dx + n * (block + spacing) + block)		//鼠标在右边界左边
				return n;
			else
				return -1;									//若不在数组中，返回-1
		}

		public int BitmapToArrayY(int y)					//将像素的y坐标转换为数组坐标
		{													//注释同上
			if (y < 0 || y > Window.ClientBounds.Height)	
				return -1;

			int n = (y - dy) / (block + spacing);			

			if (y > dy + n * (block + spacing) &&
				y< dy + n * (block + spacing) + block)
				return n;
			else
				return -1;
		}
	
		public Vector2 ArrayToBitmap(Vector2 vetcor2)		//将数组坐标转换为标准的像素绘制位置
		{
			int i = (int)vetcor2.X;
			int j = (int)vetcor2.Y;
			return new Vector2((block + spacing) * i + dx, (block + spacing) * j + dy);
		}

		public Vector2 ArrayToBitmap(int i,int j)           //将数组坐标转换为标准的像素绘制位置
		{
			return new Vector2((block + spacing) * i + dx, (block + spacing) * j + dy);
		}
	}
}

