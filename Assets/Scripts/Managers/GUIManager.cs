/*
 * Copyright (c) 2017 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class GUIManager : MonoBehaviour {
	public static GUIManager instance;

	//Score and Moves (public)
	[Header("Game logic")]
	public int MaxMoves;
	public int GoalScoreValue;
	public int MovesGained;
	//Game Objects
	public GameObject boardManager;
	//Score and Moves (private)
	private int score;
	private int moveCounter;
	private int intermediateScore;

	//Music
	[Header("Music")]
	public GameObject Music;
	private static bool playFirstTime = true;

	//Game over UI Text
	[Header("Game Over UI")]
	public GameObject gameOverLayer;
	public Text yourScoreTxt;
	public Text highScoreTxt;
	public Text moveCounterTxt;


	//Help UI Text
	[Header("Help UI ")]
	public GameObject HelpLayer;
	public Text HelpScoreToMovesTxt;


	// slider
	[Header("Slider")]
	public ScoreProgressBar sliderScore;
	public MovesProgressBar sliderMoves;

	// Cursor
	[Header("Cursor")]
	public Texture2D cursorImage;

	public int Score
	{
		get
		{
			return score;
		}

		set
		{
			score = value;
			//Send full score to score slider
			sliderScore.ScoreOGValue = score;
		}
	}


	public int IntermediateScore
	{
		get
		{
			return intermediateScore;
		}

		set
		{
			intermediateScore = value;

			//Send value of score between goal scores to score slider
			sliderScore.Score = (float)intermediateScore / (float)GoalScoreValue;

			//Empty slider if reaches the intermediate goal, add moves to the counter and play sound
			if (intermediateScore >= GoalScoreValue)
			{
				intermediateScore = 0;
				moveCounter += MovesGained;
				moveCounterTxt.text = moveCounter.ToString();
				sliderMoves.Moves = (float)moveCounter / MaxMoves;
				SFXManager.instance.PlaySFX(Clip.MoreMoves);
			}


		}
	}

	public int MoveCounter
	{
		get
		{
			return moveCounter;
		}

		set
		{
			moveCounter = value;

			//Ends the level if there's no more moves left
			if (moveCounter <= 0)
			{
				moveCounter = 0;

				//Lock and hide cursor
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;

				//Transition to the Game Over layer
				StartCoroutine(WaitForShifting());

			}

			//Update moves slider
			moveCounterTxt.text = moveCounter.ToString();
			sliderMoves.Moves = (float)moveCounter / MaxMoves;
		}
	}


	void Awake() {

		//Get full level of moves
		moveCounter = MaxMoves;
		sliderMoves.Moves = MaxMoves;
		moveCounterTxt.text = moveCounter.ToString();

		//Set up own instance
		instance = GetComponent<GUIManager>();

		//Set up part of the help text
		HelpScoreToMovesTxt.text = GoalScoreValue.ToString() + " points = " + MovesGained.ToString() + " moves";

		//Get music to play constantly
		if (playFirstTime)
        {
			playFirstTime = false;
			DontDestroyOnLoad(Music);
		}
        else
        {
			Destroy(Music);
        }
	}

	void Update()
	{	
		//Escape key closes the game
		if (Input.GetKey("escape"))
		{
			ExitGame();
		}
	}

	//Waiting to deploy the Game Over layer
	private IEnumerator WaitForShifting()
	{
		//Waits until board is no longer changing 
		yield return new WaitUntil(() => !BoardManager.instance.IsShifting);
		//Waits some seconds
		yield return new WaitForSeconds(1.5f);
		GameOver();
	}

	// Show the game over panel
	public void GameOver() {

		//Get Game Over layer and deactivate the board manager
		gameOverLayer.SetActive(true);
		boardManager.SetActive(false);

		//Unlock cursor
		Cursor.SetCursor(cursorImage, Vector2.zero, CursorMode.Auto);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		

		//Register new high score or get the previous one
		if (score > PlayerPrefs.GetInt("HighScore")) {
			PlayerPrefs.SetInt("HighScore", score);
			highScoreTxt.text = "New Best: " + PlayerPrefs.GetInt("HighScore").ToString();
		} else {
			highScoreTxt.text = "Best: " + PlayerPrefs.GetInt("HighScore").ToString();
		}
		//Show new score
		yourScoreTxt.text = "Your Score: " + score.ToString();
	}


	//Get Help Layer
	public void LoadHelp()
	{
		HelpLayer.SetActive(true);
		boardManager.SetActive(false);
	}

	//Closes the Help Layer
	public void QuitHelp()
	{
		HelpLayer.SetActive(false);
		boardManager.SetActive(true);
	}

	//Reloads scene after pressing the play again button in Game Over layer
	public void ReloadScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		StartCoroutine(WaitForShiftingReloadScene());
	}

	//Waiting to unveil the new level
	private IEnumerator WaitForShiftingReloadScene()
	{
		//Waits until board is no longer changing 
		yield return new WaitUntil(() => !BoardManager.instance.IsShifting);
		//Waits some seconds
		yield return new WaitForSeconds(1.50f);
		ReloadSceneAfterWait();
	}

	//Sets Game Over layer as deactivated after preparing the transition from game over to new level
	public void ReloadSceneAfterWait()
	{
		gameOverLayer.SetActive(false);
	}

	public void ExitGame()
	{
		// If we are running in a standalone build of the game
		#if UNITY_STANDALONE
			// Quit the application
			Application.Quit();
		#endif

		// If we are running in the editor
		#if UNITY_EDITOR
		// Stop playing the scene
		UnityEditor.EditorApplication.isPlaying = false;
		#endif
	}
}
