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
		GraphicsDeviceManager graphics;				//�����豸�������������趨��ʾ���ڣ�
		SpriteBatch spriteBatch;					//ʵ����������󣨻��ƾ��飩

		enum GameState { Start, InGame, GameOver };		//��Ϸ״̬ö��
		GameState currentGameState = GameState.Start;	//�趨��Ϸ��ʼ

		//������Ϸ������
		const int species = 11;						//��Ϸʹ�õ�ͼƬ�����������Ϊ11�������Ѷȹҹ����������ù���
													//���Է��֣���7*10��ͼ������4��ͼƬ���޷����ɿ��н⣬�ڴ���ѧ�Ҹ���֤��
		const int row = 16;							//��������
		const int col = 12;							//��������
		const int dx = 8;							//�ڴ�������ʾ����λ�õ�ƫ����x��y,��Ϊ������ʾ��λ����ʼ����
		const int dy = 8;
		const int downside = 8;						//�±߽磬�ӳ������ײ����±߽�ľ���
		const int rightside = (int)(350 * scale);   //�ұ߽磬�ӳ��������ұ߽�ľ���
		const int PicSize = 64;						//ͼƬ�߳���ͼƬ�������Σ���λΪ���أ�
		const float scale = 0.75f;					//��ʾ���������봰���ڸ������С�й�
													//���飺��ͼ1.0������ͼ0.8����Сͼ0.6������Ҫ����1.0��Ҳ��ҪС��0.6��
		const int block = (int)(PicSize * scale);	//ͼƬ����ʾ��ȣ�scaleΪ�����ͣ�
		const int spacing = (int)(8 * scale);		//ͼƬ��ͼƬ֮��ļ��
		KeyboardState keyboardstate;				//����״̬����
		MouseState mousestate;						//���״̬����
		bool before = false;                        //������һ��������Ľ�������ڵ����ж�
		bool FirstClick = false;					//��¼��һ�ε���Ƿ�ɹ���������
		int TimeRemain = 0;                         //ʣ��ʱ�䣬��update�����д�GamePool���в��ϸ���
		int GameOverTime = 0;						//��¼GameOver��ʱ�䣨��ֹ���ԣ�
		int Wait3Second = 0;                        //�ȴ����루����GameOver״̬����ʾ״̬��
		int IdeaLeftBehind;							//��ʾ�Ĵ���
		bool ShowRecessive;							//��ʾ���Խ�Ŀ���

		GamePool gamepool = new GamePool(row, col, species);//ʵ������Ϸ��

		//��Դ�ļ���ͼƬ�������
		Texture2D[] SourceImage = new Texture2D[12];//Դͼ����(11+1���������1��Ϊ�հ�ͼƬ)
		Texture2D refress;                          //����(reset)ͼ��	
		Texture2D idea;								//��ʾ(idea)ͼ��
		Texture2D box;                              //��ʾ���Խ�ķ���	
		Texture2D box1;
		SpriteFont MyFont;							//���������ļ�
		
		//��ʾ����ά����
		Vector2 ScorePos = new Vector2((30 + dx + row * (block + spacing)), dy);					//�÷ֵ���ʾ����
		Vector2 RefressPos = new Vector2((30 + dx + row * (block + spacing)), 1.5f * block * scale);//����(refress)ͼ�����ʾ����
		Vector2 IdeaPos = new Vector2((30 + dx + row * (block + spacing)), 5 * block * scale);		//���Խ���ʾ(idea)ͼ�����ʾ����
		Vector2 TimeRemainPos = new Vector2((30 + dx + row * (block + spacing)), 9 * block * scale); //ʣ��ʱ�����ʾ����

		//��ʾ���Խ��λ���Լ�����
		Vector2 BoxPos;
		Vector2 BoxPos1;
		int BoxPosAxis;
		

		//�����ά����
		Vector2 Interesting = new Vector2(species + 1, species + 1);						//��Ȥ�����������
		Vector2 mousePosInMap = new Vector2(species + 1, species + 1);						//���������λ��ת��Ϊ��������
		Vector2 tempMousePosInMap = new Vector2(species + 1, species + 1);



		public DuiDuiPengGUI()								//���캯��
		{
			graphics = new GraphicsDeviceManager(this);		//�½���ʾ����

			//���ڿ�ļ��㣺����ƫ����dx+�����ұߵĿռ���rightside+���ӵĿ��
			//���ڸߵļ��㣺����ƫ����dy+�����±ߵĿռ���downside+���ӵĸ߶�
			graphics.PreferredBackBufferWidth  = dx + rightside + row * (block + spacing) - spacing;	//������ʾ������
			graphics.PreferredBackBufferHeight = dy + downside + col * (block + spacing) - spacing;		//������ʾ����߶�

			Content.RootDirectory = "Content";				//����Ŀ¼
		}

		protected override void Initialize()				//��ʼ����Ϸ����
		{
			base.Initialize();
		}

		protected override void LoadContent()				//������Ϸ���ݺ���
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);	//�½����������
			IsMouseVisible = true;                          //����������ʾ���

			Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, 
				graphics.PreferredBackBufferHeight / 2);	//�����λ�������ڴ������ģ�û��ʲô���⺬�壩

			//����ͼƬ���˴������±��1��ʼ��0Ϊ�հ�ͼƬ
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

			//���ؾ�������
			MyFont = Content.Load<SpriteFont>(@"Fonts\Score");
			
			gamepool.SetStartTime();						//�趨��ʼʱ��
		}

		protected override void UnloadContent() { }         //ж�����ݺ���

		protected override void Update(GameTime gameTime)	//����״̬����
		{
			keyboardstate = Keyboard.GetState();			//��ȡ����״̬
			mousestate = Mouse.GetState();                  //��ȡ���״̬
			
			
			mousePosInMap = new Vector2(                    //�������ʾ����ת��Ϊ��������
				BitmapToArrayX(mousestate.X), 
				BitmapToArrayY(mousestate.Y));

			switch (currentGameState)						//��ͬ״̬��ͬ���·�ʽ
			{
				case GameState.Start:						//��Ϸ��ʼǰ״̬

					gamepool.SetStartTime();                //���ֽ�����Ϸǰʱ��ʼ��Ϊ��

					if (mousestate.LeftButton == ButtonState.Released && before &&
						mousestate.X >= 0 && mousestate.Y >= 0 &&
						mousestate.X < graphics.PreferredBackBufferWidth &&
						mousestate.Y < graphics.PreferredBackBufferHeight)
					{										//�ж���굥��һ��
						gamepool.InitGame();				//��ʼ����Ϸ
						currentGameState = GameState.InGame;//�л�����Ϸ��ģʽ
						IdeaLeftBehind = 5;
					}

					break;


				case GameState.InGame:                      //��Ϸ��״̬

					//�������飺
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.E))
					{
						//debug������ͬʱ��ס����W����E�������������������������ڿ�
						//���˹��ܿ����������ף�һ�����Ҳ�������������
						if (mousestate.LeftButton == ButtonState.Released && before)//��ⵥ���¼�
						{
							if (mousePosInMap != new Vector2(-1, -1))
							{
								gamepool.Exchange(mousePosInMap, Interesting);          //������������ǰ����Ŀ����Ȥ�飨���������ڲ��ܽ�����
								Interesting = mousePosInMap;                            //����ǰ����Ŀ�����Ϊ��Ȥ��
							}
						}
						while (gamepool.FindExplicit(false))                        //һֱ����ֱ���޷�����Ϊֹ
						{
							gamepool.FindExplicit(true);
							gamepool.SetStartTime();
						}
					}
					else
					{
						//�����һ�μ���飬����ڶ��ν��������������������򽻻������������������������򲻽���
						if (mousestate.LeftButton == ButtonState.Released && before)//��ⵥ���¼�
						{
							gamepool.Exchange(mousePosInMap, Interesting);          //�Ƚ��������飬

							if (!gamepool.FindExplicit(false))                      //������ֽ������޷���������������
							{
								gamepool.Exchange(Interesting, mousePosInMap);
								FirstClick = true;
							}
							while (gamepool.FindExplicit(false))                    //һֱ����ֱ���޷�����Ϊֹ
							{
								gamepool.FindExplicit(true);
								gamepool.SetStartTime();
							}

							//�˹��ܵ�Ŀ���ǣ���һ�γɹ�����֮��ȡ����ǰ����Ȥ�㣬�ٵ��һ����������Ȥ�㣬��������
							if (FirstClick)                                         //FirstClick��ʾ��һ�ε���Ƿ�ɹ�����									
								Interesting = mousePosInMap;                        //������Ȥ��Ϊ��ǰ�������Ŀ�
							else
								Interesting = new Vector2(row, col);                //ȡ��ͼ�е���Ȥ��

							FirstClick = false;                                     //�ɹ�����֮��FirstClick��¼��Ϊfalse
							Wait3Second = 0;                                        //�����������ر���ʾ��
						}
					}

					if ((gamepool.FindRecessive().Z) == -1)
					{
						gamepool.RandomPool();
						while (gamepool.FindExplicit(false))						//һֱ����ֱ���޷�����Ϊֹ
						{
							gamepool.FindExplicit(true);
							gamepool.SetStartTime();
						}
					}

					//������Խ���ʾ������
					Vector3 temp = gamepool.FindRecessive();
					BoxPos = ArrayToBitmap((int)temp.X, (int)temp.Y) + new Vector2(-4, -4);
					BoxPos1 = ArrayToBitmap((int)temp.X, (int)temp.Y) + new Vector2(-4, -4);
					BoxPosAxis =(int)(gamepool.FindRecessive()).Z;

					//BoxPos =new Vector2((int)mousestate.X, (int)mousestate.Y);   //debug����ʾ���λ��
					//BoxPosAxis = 0;

					
					//debug������ͬʱ��ס����W����T��ʱ�����
					//���˹��ܿ����������ף�һ�����Ҳ�������������
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.T))
						gamepool.SetStartTime();

					//debug������ͬʱ��ס����W����K����ɱ
					//���⹦������û���˻��ðɡ�����
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.K))
					{
						currentGameState = GameState.GameOver;      //ת����GameOver״̬
						GameOverTime = gamepool.GetNowTime();       //���GameOver��ʱ��
						gamepool.HighScore = gamepool.Score;        //�趨��߷֣����÷ִ�����ʷ��߷�ʱ�ſ����趨�ɹ���
					}

					//debug������ͬʱ��ס����W����S�����ټӵ÷�
					//���˹��ܿ����������ף�һ�����Ҳ�������������
					if (keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.S))
						gamepool.AddScore(1000);

					//����refress��ťˢ������&�÷����㣬if������жϵ���Ƿ���refressͼƬ�������ҵ�����Ч
					if (mousestate.X >= RefressPos.X && mousestate.X <= RefressPos.X + 128 * scale &&
					mousestate.Y >= RefressPos.Y && mousestate.Y <= RefressPos.Y + 128 * scale &&
					mousestate.LeftButton == ButtonState.Released && before)
					{
						gamepool.InitGame();			//��ʼ����Ϸ
						gamepool.SetStartTime();		//�÷�����
					}


					//����idea��ť��ʾ��ʾ����ʾ���룩
					if (mousestate.X >= IdeaPos.X && mousestate.X <= RefressPos.X + 128 * scale &&
						mousestate.Y >= IdeaPos.Y && mousestate.Y <= IdeaPos.Y + 128 * scale &&
						mousestate.LeftButton == ButtonState.Released && before && IdeaLeftBehind > 0)
					{
						Wait3Second = 3 + gamepool.GetNowTime();
						IdeaLeftBehind--;
					}

					//3������ʾ
					//debug������ͬʱ��ס����W����R����ʾ���Խ���ʾ��
					if ((Wait3Second - gamepool.GetNowTime() > 0) || 
						keyboardstate.IsKeyDown(Keys.W) && keyboardstate.IsKeyDown(Keys.R))
						ShowRecessive = true;
					else
						ShowRecessive = false;

					//�����Ѷȣ�ʱ����ݼ���
					if (gamepool.Score > 100000)
						gamepool.Life = 30 - gamepool.Score / 33333;//ÿʮ��ּ���3��
					

					//ʣ��ʱ�䵽0��game over
					if (TimeRemain == 0)
					{
						currentGameState = GameState.GameOver;      //ת����GameOver״̬
						GameOverTime = gamepool.GetNowTime();		//���GameOver��ʱ��
						gamepool.HighScore = gamepool.Score;		//�趨��߷֣����÷ִ�����ʷ��߷�ʱ�ſ����趨�ɹ���
					}

					break;


				case GameState.GameOver:                            //GameOver״̬

					//�ȴ�3�룺��ֹ���ԣ���ֹGameOver֮�����Ͽ�ʼ��һ����Ϸ
					Wait3Second = 3 - (gamepool.GetNowTime() - GameOverTime);   //��ȡ��GameOver�����ڵ�ʱ��
					gamepool.Life = 30;                             //����ֵ�ָ�30��

					if (mousestate.LeftButton == ButtonState.Released &&
						mousestate.X >= 0 && mousestate.Y >= 0 &&
						mousestate.X < graphics.PreferredBackBufferWidth &&
						mousestate.Y < graphics.PreferredBackBufferHeight &&
						before && Wait3Second <= 0) 
					{
						gamepool.InitGame();
						gamepool.SetStartTime();                    //ʱ������
						IdeaLeftBehind = 5;							//��ʾ�����ָ�
						currentGameState = GameState.InGame;		//��ת����Ϸ״̬
					}

					break;
			}

			//�������߼�����һ�ε����ұ���̧�������Ч����������ˢ�±��μ�¼
			if (mousestate.LeftButton == ButtonState.Pressed)
				before = true;
			else
				before = false;

			//��Q���˳�
			if (keyboardstate.IsKeyDown(Keys.Q))            //��������Q�˳�
				this.Exit();

			//��GamePool��ʾ������ʣ��ʱ��
			TimeRemain = gamepool.GetTimeRemain();

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)					//���ƺ���
		{ 
			switch (currentGameState)									//��ͬ״̬���Ʋ�ͬ����
			{
				case GameState.Start:									//��ʼ����

					GraphicsDevice.Clear(Color.Orange);					//��ɫ����

					//���ƿ�ʼ��ʾ�ַ�
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"click window to start game :)\n        good luck !",	//���ûỰ
						new Vector2(                                    //��ʾ���꣨9*block��
							graphics.PreferredBackBufferWidth / 2 - 5 * block,
							graphics.PreferredBackBufferHeight / 2 - block),								
						Color.DarkBlue,									//��ɫ
						0, Vector2.Zero, 
						scale,											//����������
						SpriteEffects.None, 1);
					spriteBatch.End();

					//����������Ϣ
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"author: wuxiaohan\n   from UESTC",				//����������Ϣ
						new Vector2(                                    //��ʾ����
							0, 
							graphics.PreferredBackBufferHeight - block),
						Color.DarkBlue, 0, Vector2.Zero, 
						scale * 0.5f,									//��С��0.5����С��
						SpriteEffects.None, 1);
					spriteBatch.End();

					break;


				case GameState.InGame:									//��Ϸ��״̬

					GraphicsDevice.Clear(Color.WhiteSmoke);             //�ð���ɫˢ�´���

					//������ʾ�����е�ͼƬ
					spriteBatch.Begin();
					for (int i = 0; i < gamepool.Row; i++)
						for (int j = 0; j < gamepool.Col; j++)
							spriteBatch.Draw(
								SourceImage[gamepool.GetBrick(i, j)],	//��Դͼ��ѡȡ��ʾ��ͼƬ��Ϊ�������brick����ʾ
								ArrayToBitmap(i, j),					//��������
								null,									//����24�񶯻�������趨���˴�����
								Color.White, 
								0, Vector2.Zero, 
								scale,									//��������
								SpriteEffects.None, 0);					//������ת���Գơ��������ʾ����
					spriteBatch.End();

					//��ʾһ�����Ӳ㣬����ָʾ��Ȥ��
					spriteBatch.Begin();
					spriteBatch.Draw(
						SourceImage[gamepool.GetBrick(Interesting)],
						ArrayToBitmap(Interesting), 
						null, 
						Color.Green,									//�˴��޸ĵ��Ӳ����ɫ
						0, Vector2.Zero, 
						scale, 
						SpriteEffects.None, 0.99f);						//���Ӳ�Ϊ���㣨���һ���βΣ�
					spriteBatch.End();

					//���Ƶ÷�
					spriteBatch.Begin();
					spriteBatch.DrawString(
						MyFont,											//��������
						"Score:" + gamepool.Score,						//���õ÷ָ�ʽ
						ScorePos,										//���õ�λ������
						Color.DarkBlue,									//������ɫ
						0, Vector2.Zero, 
						scale,											//��������
						SpriteEffects.None, 1);
					spriteBatch.End();

					//����refress��ť
					spriteBatch.Begin();
					spriteBatch.Draw(									//���Ʒ�ʽͬ�ϣ��˴�����ע��
						refress,
						RefressPos, 
						null, 
						Color.White,
						0, Vector2.Zero, 
						scale, 
						SpriteEffects.None, 1);
					spriteBatch.End();

					//����idea��ť
					spriteBatch.Begin();
					spriteBatch.Draw(                                   //���Ʒ�ʽͬ�ϣ��˴�����ע��
						idea,
						IdeaPos,
						null,
						Color.White,
						0, Vector2.Zero,
						scale,
						SpriteEffects.None, 1);
					spriteBatch.End();

					//����ideaʣ����ʾ����
					spriteBatch.Begin();
					spriteBatch.DrawString(
						MyFont,
						" = " + IdeaLeftBehind,							//���ô���
						IdeaPos + new Vector2(128 * scale, 40 * scale), //���õ�λ������
						Color.DarkBlue,                                 //��ɫ
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//����ʣ��ʱ��
					spriteBatch.Begin();
					spriteBatch.DrawString(
						MyFont,
						"Time:" + TimeRemain,							//����ʱ��
						TimeRemainPos,									//���õ�λ������
						Color.DarkBlue,									//��ɫ
						0, Vector2.Zero, scale, SpriteEffects.None, 1);
					spriteBatch.End();

					//�������Խ����ʾ��(��W��R����ʾ),rect��ʾ�����Ǻ�Ļ������ģ�0�Ǻᣬ1������
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

					//����Game Over�ַ�
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Game Over :(",									//���ûỰ
						new Vector2(                                    //������ʾ���꣨���Ϊ7*block��
							graphics.PreferredBackBufferWidth / 2 - 5 * block,
							graphics.PreferredBackBufferHeight / 2 -  2 * block),
						Color.DarkBlue,							
						0, Vector2.Zero, 
						scale * 2,										//��������
						SpriteEffects.None, 1);
					spriteBatch.End();

					//�������յ÷�
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Score:  "+gamepool.Score,						//���ûỰ
						new Vector2(                                    //������ʾ���꣨��GameOver����2*block��
							graphics.PreferredBackBufferWidth / 2 - 5 * block,
							graphics.PreferredBackBufferHeight / 2),
						Color.DarkBlue,								
						0, Vector2.Zero, scale, 
						SpriteEffects.None, 1);
					spriteBatch.End();

					//������ʷ��߷�
					spriteBatch.Begin();
					spriteBatch.DrawString(MyFont,
						"Highest:" + gamepool.HighScore,				//���ûỰ
						new Vector2(                                    //������ʾ����
							graphics.PreferredBackBufferWidth / 2 - 5 * block,
							graphics.PreferredBackBufferHeight / 2 + block),
						Color.DarkBlue,                            
						0, Vector2.Zero, scale, 
						SpriteEffects.None, 1);
					spriteBatch.End();

					//���Ƶ����������ַ�
					if (Wait3Second <= 0)
					{
						spriteBatch.Begin();
						spriteBatch.DrawString(MyFont,
							"click window to restart...",                   //���ûỰ
							new Vector2(                                    //������ʾ����
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

		
		public int BitmapToArrayX(int x)					//����ʾ�����x����ת��Ϊ��������
		{
			if (x < dx || x> dx + row * (block + spacing) - spacing)//xӦ����ͼ��
				return -1;									//�����������У�����-1

			int n = (x - dx) / (block + spacing);			//���Ե��������꣨����ȡ�����¾��Ȳ��ߣ�

			if (x > dx + n * (block + spacing) &&			//�������߽��ұ�
				x < dx + n * (block + spacing) + block)		//������ұ߽����
				return n;
			else
				return -1;									//�����������У�����-1
		}

		public int BitmapToArrayY(int y)					//����ʾ�����y����ת��Ϊ��������
		{													//ע��ͬ��
			if (y < 0 || y > dy + col * (block + spacing) - spacing)	
				return -1;

			int n = (y - dy) / (block + spacing);			

			if (y > dy + n * (block + spacing) &&
				y< dy + n * (block + spacing) + block)
				return n;
			else
				return -1;
		}
	
		public Vector2 ArrayToBitmap(Vector2 vetcor2)		//�������ά����ת��Ϊ��ʾ��ά����
		{
			int i = (int)vetcor2.X;
			int j = (int)vetcor2.Y;
			return new Vector2((block + spacing) * i + dx, (block + spacing) * j + dy);
		}

		public Vector2 ArrayToBitmap(int i,int j)           //����������ת��Ϊ��ʾ��ά����
		{
			return new Vector2((block + spacing) * i + dx, (block + spacing) * j + dy);
		}
	}
}

