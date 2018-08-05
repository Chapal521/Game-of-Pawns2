using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Game_Control : MonoBehaviour {

	public static GameObject[,] boardSpaces = new GameObject[8, 8];

	//0 - no chip, 1 - player chip, 2 - AI chip
	public static int[,] spaceOwner = new int[8, 8]; 

    
	public static int[] flipCounts = new int[8];
	//Array for storing keep leaderboard data
	int Tmp;
	int lose;
	int win;
	int draw;

    public GameObject chip;
    static bool playerTurn = true;
    private int placesLeft = 60;

    public Text alert;

    public Text playerScoreText;
    public Text AIScoreText;

    private int stall = 0;

    private bool gameOver;
	private bool waitTime = true;

	public Text whoPlay;

	bool check = false; 
	bool check1 = false;

    void Start() {

		leaderBoard ();

		whoPlay.text = "Your turn.Plz play...";
		for (int i = 0; i < 8; i++) {
			flipCounts [i] = 0;
			for (int j = 0; j < 8; j++) {
				spaceOwner [i, j] = 0;
				boardSpaces [i, j] = null;
			}
		}
			
        GameObject black1 = Instantiate(chip, new Vector3((float)(3.5), (float)(-3.5), (float)8.0), transform.rotation);
        GameObject black2 = Instantiate(chip, new Vector3((float)(4.5), (float)(-4.5), (float)8.0), transform.rotation);
        GameObject white1 = Instantiate(chip, new Vector3((float)(3.5), (float)(-4.5), (float)8.0), transform.rotation);
        GameObject white2 = Instantiate(chip, new Vector3((float)(4.5), (float)(-3.5), (float)8.0), transform.rotation);

        black1.transform.Rotate(new Vector3(180, 0, 0));
        black2.transform.Rotate(new Vector3(180, 0, 0));

        boardSpaces[3, 3] = black1;
        boardSpaces[4, 4] = black2;
        boardSpaces[3, 4] = white1;
        boardSpaces[4, 3] = white2;

        spaceOwner[3, 3] = 1;
        spaceOwner[4, 4] = 1;
        spaceOwner[3, 4] = 2;
        spaceOwner[4, 3] = 2;



        //Initialize animation asset
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);

		win = PlayerPrefs.GetInt ("MatchWin");
		draw = PlayerPrefs.GetInt ("TotalMatch");
		lose = PlayerPrefs.GetInt ("Lose");


    }
	public void leaderBoard()
	{
		Tmp = PlayerPrefs.GetInt ("TotalMatch");
		Tmp++; // total match played;
		PlayerPrefs.SetInt("TotalMatch",Tmp);
	}

    void LateUpdate () {

		if (waitTime) {
			if (gameOver) {
				int[] scores = scoreBoard (spaceOwner, false);
				if (scores [0] > scores [1]) {
					if (!check) {
						win++; // numberb of wins
						PlayerPrefs.SetInt ("MatchWin", win);
						check = true;
					}
					alert.text = "You won !!!";
				} else if (scores [0] == scores [1]) {
					alert.text = "It's a draw!";
					draw++;
					PlayerPrefs.SetInt ("Draw", draw);
				} 
				else {
					alert.text = "You lose !!!";
					if (!check1) {
						lose++;
						PlayerPrefs.SetInt ("Lose", lose);
						check1 = true;
					}
				}
				return;
			}
			else {
				
				if (placesLeft == 0) {
					gameOver = true;
					return;
				}
				if (!hasMoves ()) {
					int[] scores = scoreBoard (spaceOwner, false);

					if (scores [0] * scores [1] == 0) {
						gameOver = true;
					}
					if (++stall >= 2) {
						gameOver = true;
					}
                else {
						String player = playerTurn ? "YOU" : "AI";
						alert.text = player + " HAD NO MOVES!";
						Invoke ("resetAlertText", 2);
						playerTurn = !playerTurn;
					}
					return;
				}
				stall = 0;
            
				if (playerTurn) {
					if (Input.GetMouseButtonDown (0)) {
						Debug.Log ("Player Turn");
						Ray mouse = Camera.main.ScreenPointToRay (Input.mousePosition);
						RaycastHit info;
						bool hit = Physics.Raycast (mouse, out info, 500.0f);

						if (hit) {
							int x = (int)Math.Floor (info.point.x);
							int yPos = (int)Math.Ceiling (info.point.y);
							int y = Math.Abs (yPos);

							if (x >= 0 && x < 8 &&
							                     yPos <= 0 && y < 8) {
							
								if (isMove (x, y, spaceOwner)) {
									GameObject newPiece = Instantiate (chip, new Vector3 ((float)(x + .5), (float)(yPos - .5), 0), transform.rotation);
									newPiece.transform.Rotate (180, 0, 0); 
									newPiece.transform.DOMoveZ (8, (float).5, true);

									boardSpaces [x, y] = newPiece;
									spaceOwner [x, y] = 1;
									placesLeft--;

									findFlipDirections (x, y, spaceOwner, true);
									playerTurn = !playerTurn;
									alert.text = "";

								} else {
									alert.text = "Invalid Move."; //AAA
									Debug.Log ("That was not a valid move.");
								}

							}
						}
						updateScore ();
					}
				} else {
					Debug.Log ("AI Turn");
					whoPlay.text = "AI Turn.Plz wait.....";
					StartCoroutine (wait ());
					waitTime = false;
					updateScore ();
				}
			}
		}
    }
	IEnumerator wait(){
		yield return new WaitForSeconds (0.8f);
		AI ();
		waitTime = true;
	}

    private bool hasMoves()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (isMove(i, j, spaceOwner))
                {
                    return true;
                }
            }
        }
        return false;
    }
		
    private void updateScore()
    {
        int[] currentScores = scoreBoard(spaceOwner, false);
        playerScoreText.text = "Player Score : " + currentScores[0];
        AIScoreText.text = "AI Score : " + currentScores[1];
    }




    void AI()
    {
        int[] nextMove = new int[2] { -1, -1 };
		negaMax(spaceOwner, 1, ref nextMove);

        playerTurn = false;
        if (nextMove[0] >= 0 && nextMove[1] >= 0)
        {
            int x = nextMove[0];
            int y = nextMove[1];
            checkFlips(x, y, spaceOwner);
            GameObject newPiece = Instantiate(chip, new Vector3((float)(x + .5), (float)(-y - .5), 0), transform.rotation);
            newPiece.transform.DOMoveZ(8, (float)0.5, true);
            boardSpaces[x, y] = newPiece;
            spaceOwner[x, y] = 2;
            placesLeft--;
            findFlipDirections(x, y, spaceOwner, true);
        }
        else
        {
            Debug.Log("Error: A best move was not found.");
        }
		updateScore (); 
		whoPlay.text = "Your turn.Plz play...";
		playerTurn = true;
    }
//    //Used for debugging
//    private static void DebugBoard()
//    {
//        Debug.Log("[" + spaceOwner[0, 0] + "," + spaceOwner[1, 0] + "," + spaceOwner[2, 0] + "," + spaceOwner[3, 0] + "," + spaceOwner[4, 0] + "," + spaceOwner[5, 0] + "," + spaceOwner[6, 0] + "," + spaceOwner[7, 0] + "]\n" +
//                    "[" + spaceOwner[0, 1] + "," + spaceOwner[1, 1] + "," + spaceOwner[2, 1] + "," + spaceOwner[3, 1] + "," + spaceOwner[4, 1] + "," + spaceOwner[5, 1] + "," + spaceOwner[6, 1] + "," + spaceOwner[7, 1] + "]\n" +
//                    "[" + spaceOwner[0, 2] + "," + spaceOwner[1, 2] + "," + spaceOwner[2, 2] + "," + spaceOwner[3, 2] + "," + spaceOwner[4, 2] + "," + spaceOwner[5, 2] + "," + spaceOwner[6, 2] + "," + spaceOwner[7, 2] + "]\n" +
//                    "[" + spaceOwner[0, 3] + "," + spaceOwner[1, 3] + "," + spaceOwner[2, 3] + "," + spaceOwner[3, 3] + "," + spaceOwner[4, 3] + "," + spaceOwner[5, 3] + "," + spaceOwner[6, 3] + "," + spaceOwner[7, 3] + "]\n" +
//                    "[" + spaceOwner[0, 4] + "," + spaceOwner[1, 4] + "," + spaceOwner[2, 4] + "," + spaceOwner[3, 4] + "," + spaceOwner[4, 4] + "," + spaceOwner[5, 4] + "," + spaceOwner[6, 4] + "," + spaceOwner[7, 4] + "]\n" +
//                    "[" + spaceOwner[0, 5] + "," + spaceOwner[1, 5] + "," + spaceOwner[2, 5] + "," + spaceOwner[3, 5] + "," + spaceOwner[4, 5] + "," + spaceOwner[5, 5] + "," + spaceOwner[6, 5] + "," + spaceOwner[7, 5] + "]\n" +
//                    "[" + spaceOwner[0, 6] + "," + spaceOwner[1, 6] + "," + spaceOwner[2, 6] + "," + spaceOwner[3, 6] + "," + spaceOwner[4, 6] + "," + spaceOwner[5, 6] + "," + spaceOwner[6, 6] + "," + spaceOwner[7, 6] + "]\n" +
//                    "[" + spaceOwner[0, 7] + "," + spaceOwner[1, 7] + "," + spaceOwner[2, 7] + "," + spaceOwner[3, 7] + "," + spaceOwner[4, 7] + "," + spaceOwner[5, 7] + "," + spaceOwner[6, 7] + "," + spaceOwner[7, 7] + "]\n" );
//    }
    private int[] scoreBoard(int[,] board, bool hueristic)
    {
        int newPlayerScore = 0;
        int newAIScore = 0;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (board[i, j] == 1)
                {
                    int scoreBias = hueristic ? (isCorner(i, j) ? 7 : isSide(i, j) ? 3 : 1) : 1;
                    newPlayerScore += scoreBias;
                }
                else if (board[i, j] == 2)
                {
                    int scoreBias = hueristic ? (isCorner(i, j) ? 7 : isSide(i, j) ? 3 : 1) : 1;
                    newAIScore += scoreBias;
                }
            }
        }
        return new int[2] { newPlayerScore, newAIScore };
    }

    
    private bool isCorner(int i, int j)
    {
        return (i == 0 && j == 0) || (i == 0 && j == 7) || (i == 7 && j == 0) || (i == 7 && j == 7);
    }

    private bool isSide(int i, int j)
    {
        return i == 7 || i == 0 || j == 0 || j == 7;
    }
		
    private int negaMax(int [,] board, int depth, ref int[] myBestMove)
    {
        double bestScore = Double.NegativeInfinity;

        if (depth == 0)
        {
            int[] scores = scoreBoard(board, true);
            int pAdvantage = scores[0] - scores[1];

            return pAdvantage * (playerTurn ? -1 : 1);
        }
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (isMove(i, j, board))
                {
                    
                    int[,] newBoard = (int[,])board.Clone();
                    newBoard[i, j] = playerTurn ? 1 : 2;

                    findFlipDirections(i, j, newBoard, false);


                    int[] childBestMove = new int[2];
                    playerTurn = !playerTurn;
                    int score = -negaMax(newBoard, depth-1, ref childBestMove);
				
                    if(score > bestScore)
                    {
                        bestScore = score;
                        myBestMove = new int[2] { i, j };
                    }
                }
            }
        }

        return (int)bestScore;
    }
		
    bool isMove(int x, int y, int[,] board)
    {
        if (board[x, y] != 0)
        {
            return false;
        }

        checkFlips(x, y, board);
        return findValidMove();
    }

    private bool findValidMove()
    {
        bool result = false;

        for(int i = 0; i < flipCounts.Length; i++)
        {
            result |= flipCounts[i] > 0;
        }
        
        return result;
    }

    void checkFlips(int x, int y, int[,] board)
    {
        flipCounts = new int[8];
        int count = 0;

        if(countFlips(x, y, -1, -1, ref count, board))
        {
            flipCounts[0] = count;
        }
        count = 0;

        if(countFlips(x, y, 0, -1, ref count, board))
        {
            flipCounts[1] = count;
        }
        count = 0;

        if(countFlips(x, y, 1, -1, ref count, board))
        {
            flipCounts[2] = count;
        }
        count = 0;

        if(countFlips(x, y, 1, 0, ref count, board))
        {
            flipCounts[3] = count;
        }
        count = 0;

        if(countFlips(x, y, 1, 1, ref count, board))
        {
            flipCounts[4] = count;
        }
        count = 0;

        if(countFlips(x, y, 0, 1, ref count, board))
        {
            flipCounts[5] = count;
        }
        count = 0;

        if(countFlips(x, y, -1, 1, ref count, board))
        {
            flipCounts[6] = count;
        }
        count = 0;

        if(countFlips(x, y, -1, 0, ref count, board))
        {
            flipCounts[7] = count;
        }
    }

    bool countFlips(int startX, int startY, int xModify, int yModify, ref int count, int [,] board)
    {
        int currentX = startX + xModify;
        int currentY = startY + yModify;

        if (currentX > 7 || currentX < 0 ||
            currentY > 7 || currentY < 0)
        {
            return false;
        }

        if (board[currentX, currentY] != 0)
        {
            if (isMyPiece(currentX, currentY, board))
            {
                return count > 0;
            }
            else
            {
                count++;
                return countFlips(currentX, currentY, xModify, yModify, ref count, board);
            }
        }
        else
            return false;
    }

    private bool isMyPiece(int x, int y, int[,] board)
    {
        return playerTurn ? board[x, y] == 1 : board[x, y] == 2;
    }

    void findFlipDirections(int x, int y, int[,] board, bool realMove)
    {                
        if(flipCounts[0] > 0)
        {
            flipPieces(x, y, -1, -1, board, realMove);
        }
        if (flipCounts[1] > 0)
        {
            flipPieces(x, y, 0, -1, board, realMove);
        }
        if (flipCounts[2] > 0)
        {
            flipPieces(x, y, 1, -1, board, realMove);
        }
        if (flipCounts[3] > 0)
        {
            flipPieces(x, y, 1, 0, board, realMove);
        }
        if (flipCounts[4] > 0)
        {
            flipPieces(x, y, 1, 1, board, realMove);
        }
        if (flipCounts[5] > 0)
        {
            flipPieces(x, y, 0, 1, board, realMove);
        }
        if (flipCounts[6] > 0)
        {
            flipPieces(x, y, -1, 1, board, realMove);
        }
        if (flipCounts[7] > 0)
        {
            flipPieces(x, y, -1, 0, board, realMove);
        }
    }

    void flipPieces(int startX, int startY, int xModify, int yModify, int[,] board, bool realMove)
    {

        
        int currentX = startX + xModify;
        int currentY = startY + yModify;

        if (isMyPiece(currentX, currentY, board))
        {
            //Done
            return;
        }
        else
        {
            if (realMove)
            {
                int targetRotation = playerTurn ? 90 : -90;
                boardSpaces[currentX, currentY].transform.DORotate(new Vector3(targetRotation, 0, 0), 1);
            }
	
            board[currentX, currentY] = playerTurn ? 1 : 2;
            
            flipPieces(currentX, currentY, xModify, yModify, board, realMove);
        }
    }

    void resetAlertText()
    {
        alert.text = "";
    }
		

	public void reStart(){
		SceneManager.LoadScene (1);
	}
	public void Home(){
		SceneManager.LoadScene (0);
	}
}