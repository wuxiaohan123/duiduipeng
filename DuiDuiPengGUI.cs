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
		GraphicsDeviceManager graphics;		//�豸������
		SpriteBatch spriteBatch;            //�������

		enum GameState { Start, InGame, GameOver };		//��Ϸ״̬ö��
		GameState currentGameState = GameState.Start;	//�趨��Ϸ��ʼ

		const int species = 11;             //ͼƬ�����������Ϊ11�������Ѷȹҹ����������ù���
											//���Է��֣���7*10��ͼ������4��ͼƬ���޷����ɿ��н⣬�ڴ���ѧ�Ҹ���֤��
		const int row = 7;					//��������
		const int col = 10;					//��������
		const int dx = 8;					//�ڴ�������ʾͼƬλ�õ�ƫ����x��y,��Ϊ������ʾ��λ����ʼ����
		const int dy = 8;
		const int downside = 8;             //�±߽磬�ӳ��������±߽�ľ���

		const int PicSize = 64;				//ͼƬ�߳���ͼƬ�������Σ���λΪ���أ�
		const float scale = 0.9f;           //ͼƬ��������ʵ����ʾ��СΪPicSize*scale
											//���飺��ͼ1.0������ͼ0.8����Сͼ0.6������ҪС��0.6��
		const int rightside = (int)(350 * scale);   //�ұ߽磬�ӳ��������ұ߽�ľ���
		const int block = (int)(PicSize * scale); //��Ŀ�ȣ�int�Ϳ���ʡȥ����Ҫ��ǿ��ת����
		const int spacing = 6;              //ͼƬ��ͼƬ֮��ļ��

		KeyboardState keyboardstate;        //����״̬����
		MouseState mousestate;              //���״̬�������ʱ����
		bool before = false;

		int TimeRemain = 0;

		GamePool gamepool = new GamePool(row, col, species);         //ʵ������Ϸ��
		Texture2D[] SourceImage = new Texture2D[12];        //Դͼ����(�±�+1���������1��Ϊ�հ�ͼƬ)
		SpriteFont MyFont;						//���������ļ�

		//����ͼ�꼰��λ�ã����������ʶ���йأ�
		Texture2D refress;							
		Vector2 RefressPos = new Vector2((30 + dx + row * (block + spacing)), 50 * scale);

		Vector2 Interesting = new Vector2(species + 1, species + 1); //��Ȥ�����������
		Vector2 IsChosen = new Vector2(species + 1, species + 1);    //��ѡ�е���������
		
		Vector2 mousePosInMap = new Vector2(species + 1, species + 1);	//���������λ��ת��Ϊ��������
		Vector2 tempMousePosInMap = new Vector2(species + 1, species + 1);

		public DuiDuiPengGUI()					//���캯��
		{
			graphics = new GraphicsDeviceManager(this);

			//���ڿ�ļ��㣺����ƫ����dx+�����ұߵĿռ���rightside+���ӵĿ�ȣ�ͼƬ���row*(����Ϊ���ŵ�ͼƬ���block)
			graphics.PreferredBackBufferWidth  = dx + rightside + row * (block + spacing) - spacing;	//���ô��ڿ��
			graphics.PreferredBackBufferHeight = dy + downside + col * (block + spacing) - spacing;

			Content.RootDirectory = "Content";	//����Ŀ¼
		}

		protected override void Initialize()		//��ʼ����Ϸ����
		{
			// TODO: Add your initialization logic here
			
			base.Initialize();
		}

		protected override void LoadContent()		//������Ϸ���ݺ���
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);	//�½����������
			IsMouseVisible = true;                          //������ʾ���λ��

			Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, 
				graphics.PreferredBackBufferHeight / 2);	//�����λ�������ڴ������ģ�û��ʲô���⺬�壩

			//����ͼƬ���˴������±��1��ʼ��0Ϊ�հ�ͼƬ
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

			//���ؾ�������
			MyFont = Content.Load<SpriteFont>(@"Fonts\Score");
			

			gamepool.SetStartTime();
		}

		protected override void UnloadContent()		//ж������
		{
			// TODO: Unload any non ContentManager content here
		}

		protected override void Update(GameTime gameTime)	//����״̬����
		{
			keyboardstate = Keyboard.GetState();			//��ȡ����״̬
			mousestate = Mouse.GetState();                  //��ȡ���״̬
			
			//���������ת��Ϊ��������
			mousePosInMap = new Vector2(BitmapToArrayX(mousestate.X), BitmapToArrayY(mousestate.Y));

			switch (currentGameState)		//��ͬ״̬��ͬ���·�ʽ
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

					//debug��Ҳ�������ף�������ͬʱ��ס����W����E��
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.E))
					{
						if (mousestate.LeftButton == ButtonState.Released && before)
						{
							gamepool.Exchange(mousePosInMap, Interesting);
							Interesting = mousePosInMap;
						}
						while (gamepool.FindExplicit(false))                    //һֱ����ֱ���޷�����Ϊֹ
						{
							gamepool.FindExplicit(true);
							gamepool.SetStartTime();
						}
					}
					else
					{
						//�����һ�μ���飬����ڶ��ν��������������������򽻻������������������������򲻽���
						if (mousestate.LeftButton == ButtonState.Released && before)
						{
							gamepool.Exchange(mousePosInMap, Interesting);
							if (!gamepool.FindExplicit(false))                      //����������޷��������򲻽���
							{
								gamepool.Exchange(Interesting, mousePosInMap);

							}
							while (gamepool.FindExplicit(false))                    //һֱ����ֱ���޷�����Ϊֹ
							{
								gamepool.FindExplicit(true);
								gamepool.SetStartTime();
							}

							Interesting = mousePosInMap;
						}
					}

					//debug��Ҳ�������ף�������ͬʱ��ס����W����E��ʱ�����
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.T))
						gamepool.SetStartTime();

					//����refress��ťˢ������&�÷�
					if (mousestate.X >= RefressPos.X && mousestate.X <= RefressPos.X + 128 * scale &&
					mousestate.Y >= RefressPos.Y && mousestate.Y <= RefressPos.Y + 128 * scale &&
					mousestate.LeftButton == ButtonState.Released && before)
					{
						gamepool.InitGame();
						gamepool.SetStartTime();
					}

					//�������߼�����һ�ε����ұ���̧�������Ч����������ˢ�±��μ�¼
					if (mousestate.LeftButton == ButtonState.Pressed)
						before = true;
					else
						before = false;

					//ʣ��ʱ�䵽0��gameover
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
		
			//��Q���˳�
			if (keyboardstate.IsKeyDown(Keys.Q))            //��������Q�˳�
				this.Exit();

			//����ʱ��
			TimeRemain = gamepool.GetTimeRemain();
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)     //���ƺ���
		{ 
			switch (currentGameState)
			{
				case GameState.Start:

					GraphicsDevice.Clear(Color.Orange);

					//���ƿ�ʼ��ʾ�ַ�
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"click window to start game :)\n        good luck !",//���ûỰ
						new Vector2(                                //��������ڴ����м�
							block,
							graphics.PreferredBackBufferHeight / 2 - block),								
						Color.DarkBlue,                             //��ɫ
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//����������Ϣ
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"author: wuxiaohan\n   from UESTC",//���ûỰ
						new Vector2(                                //��������ڴ����м�
							0,
							graphics.PreferredBackBufferHeight - block),
						Color.DarkBlue,                             //��ɫ
						0, Vector2.Zero, scale * 0.5f, SpriteEffects.None, 1);
					spriteBatch.End();

					break;


				case GameState.InGame:

					GraphicsDevice.Clear(Color.LightSkyBlue);               //��ɫˢ�´���

					//������ʾ�����е�ͼƬ
					spriteBatch.Begin();
					for (int i = 0; i < gamepool.GetRow(); i++)
						for (int j = 0; j < gamepool.GetCol(); j++)
							spriteBatch.Draw(SourceImage[gamepool.GetBrick(i, j)], //��Դͼ��ѡȡ��ʾ��ͼƬ��Ϊ�������brick����ʾ
								ArrayToBitmap(i, j),    //�������꣬ͨ��scale������С��ͨ��dx��dy����ƫ������PicSizeΪԴͼ�ߴ�
								null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);  //������ת���ԳƵ���ʾ����
					spriteBatch.End();

					//��ʾһ�����Ӳ㣬����ָʾ��Ȥ��
					spriteBatch.Begin();
					spriteBatch.Draw(
						SourceImage[gamepool.GetBrick(Interesting)],
						ArrayToBitmap(Interesting), null, Color.Green,            //�˴��޸ĵ��Ӳ����ɫ
						0, Vector2.Zero, scale, SpriteEffects.None, 0.99f);       //���Ӳ�Ϊ���㣨���һ���βΣ�
					spriteBatch.End();

					//���Ƶ÷�
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Score:" + gamepool.GetScore(),             //���õ÷ָ�ʽ
						new Vector2((30 + dx + row * (block + spacing)), dy), //���õ�λ������
						Color.DarkBlue,                             //��ɫ
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//����refress��ť
					spriteBatch.Begin();
					spriteBatch.Draw(
						refress,
						RefressPos, null, Color.White,
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//����ʱ��
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Time:" + TimeRemain,                       //����ʱ��
						new Vector2((30 + dx + row * (block + spacing)), dy + 200), //���õ�λ������
						Color.DarkBlue,                             //��ɫ
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					break;


				case GameState.GameOver:

					GraphicsDevice.Clear(Color.OrangeRed);

					//����Game Over�ַ�
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Game Over :(",								//���ûỰ
						new Vector2(                                //��������ڴ����м�
							4 * block,
							graphics.PreferredBackBufferHeight / 2 -  2 * block),
						Color.DarkBlue,                             //��ɫ
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//�������յ÷�
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Score:  "+gamepool.GetScore(),               //���ûỰ
						new Vector2(                                //��������ڴ����м�
							4 * block,
							graphics.PreferredBackBufferHeight / 2),
						Color.DarkBlue,                             //��ɫ
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//������ʷ��߷�
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Highest:" + gamepool.GetHistoryScore(),               //���ûỰ
						new Vector2(                                //��������ڴ����м�
							4 * block,
							graphics.PreferredBackBufferHeight / 2 + block),
						Color.DarkBlue,                             //��ɫ
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//���Ƶ����������ַ�
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"click window to restart",                  //���ûỰ
						new Vector2(                                //��������ڴ����м�
							3 * block,
							graphics.PreferredBackBufferHeight / 2 + 2 * block),
						Color.DarkBlue,                             //��ɫ
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					break;
			}

			base.Draw(gameTime);
		}

		
		public int BitmapToArrayX(int x)					//�����ص�x����ת��Ϊ��������
		{
			if (x < 0 || x> Window.ClientBounds.Width)		//xӦ����ͼ��
				return -1;									//�����������У�����-1

			int n = (x - dx) / (block + spacing);			//���Ե��������꣨����ȡ�����¾��Ȳ��ߣ�

			if (x > dx + n * (block + spacing) &&			//�������߽��ұ�
				x < dx + n * (block + spacing) + block)		//������ұ߽����
				return n;
			else
				return -1;									//�����������У�����-1
		}

		public int BitmapToArrayY(int y)					//�����ص�y����ת��Ϊ��������
		{													//ע��ͬ��
			if (y < 0 || y > Window.ClientBounds.Height)	
				return -1;

			int n = (y - dy) / (block + spacing);			

			if (y > dy + n * (block + spacing) &&
				y< dy + n * (block + spacing) + block)
				return n;
			else
				return -1;
		}
	
		public Vector2 ArrayToBitmap(Vector2 vetcor2)		//����������ת��Ϊ��׼�����ػ���λ��
		{
			int i = (int)vetcor2.X;
			int j = (int)vetcor2.Y;
			return new Vector2((block + spacing) * i + dx, (block + spacing) * j + dy);
		}

		public Vector2 ArrayToBitmap(int i,int j)           //����������ת��Ϊ��׼�����ػ���λ��
		{
			return new Vector2((block + spacing) * i + dx, (block + spacing) * j + dy);
		}
	}
}

