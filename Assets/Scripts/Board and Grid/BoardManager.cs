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
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour {

	public static BoardManager instance;

	//List of different sprites
	public List<Sprite> characters = new List<Sprite>();

	//Board Size
	[Header("Board Size")]
	public GameObject tile;
	public int xSize, ySize;

	//Tile score
	[Header("Score")]
	public int tileScore;

	//create tiles
	private GameObject[,] tiles;

	//Bool if tiles are moving or not
	public bool IsShifting { get; set; }

	//Instantiate the board according to the number and position of tiles
	void Start () {
		instance = GetComponent<BoardManager>();
		Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);
    }

	//Render the board
	private void CreateBoard (float xOffset, float yOffset) {
		
		//Set start points and variables to help fill the rest
		tiles = new GameObject[xSize, ySize];

        float startX = transform.position.x;
		float startY = transform.position.y;
		Sprite[] previousLeft = new Sprite[ySize];
		Sprite previousBelow = null;

		//Create the board
		for (int x = 0; x < xSize; x++) {
			for (int y = 0; y < ySize; y++) {

				GameObject newTile = Instantiate(tile, new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
				tiles[x, y] = newTile;
				newTile.transform.parent = transform;

				//Get tiles that don't create automatic matches
				List<Sprite> possibleCharacters = new List<Sprite>();
				possibleCharacters.AddRange(characters);
				possibleCharacters.Remove(previousLeft[y]);
				possibleCharacters.Remove(previousBelow);
				Sprite newSprite = possibleCharacters[Random.Range(0, possibleCharacters.Count)];
				newTile.GetComponent<SpriteRenderer>().sprite = newSprite;
				previousLeft[y] = newSprite;
				previousBelow = newSprite;

			}
		}
    }

	//Get the empty tiles and link to ShiftTilesDown and ClearAllMatches
	public IEnumerator FindNullTiles()
	{
		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				if (tiles[x, y].GetComponent<SpriteRenderer>().sprite == null)
				{
					yield return StartCoroutine(ShiftTilesDown(x, y));
					break;
				}
			}
		}

		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				tiles[x, y].GetComponent<Tile>().ClearAllMatches();
			}
		}

	}

	//Get the tiles above the empty ones and moves them down, and then create new ones to fill the rest of the empties
	private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .05f)
	{
		IsShifting = true;
		List<SpriteRenderer> renders = new List<SpriteRenderer>();
		int nullCount = 0;

		//Only get new tiles if the empty tiles are on the top of the board
		if (yStart == this.ySize-1)
        {
			tiles[x,yStart].GetComponent<SpriteRenderer>().sprite = GetNewSprite(x, ySize - 1);
			GUIManager.instance.Score += tileScore;
			yield return null;
		}

		//Get the number of empty tiles
		for (int y = yStart; y < ySize; y++)
		{ 
			SpriteRenderer render = tiles[x, y].GetComponent<SpriteRenderer>();
			if (render.sprite == null)
			{
				nullCount++;
			}
			renders.Add(render);
		}

		//Fill the empty tiles
		for (int i = 0; i < nullCount; i++)
		{ 
			GUIManager.instance.Score += tileScore;
			GUIManager.instance.IntermediateScore += tileScore;

			yield return new WaitForSeconds(shiftDelay);
			for (int k = 0; k < renders.Count - 1; k++)
			{
					//Move the tiles down
					renders[k].sprite = renders[k + 1].sprite;
					//Fill the rest
					renders[k + 1].sprite = GetNewSprite(x, ySize - 1);
			}
		}
		IsShifting = false;
	}

	//Get new sprites that don't create automatic matches.
	private Sprite GetNewSprite(int x, int y)
	{
		List<Sprite> possibleCharacters = new List<Sprite>();
		possibleCharacters.AddRange(characters);

		if (x > 0)
		{
			possibleCharacters.Remove(tiles[x - 1, y].GetComponent<SpriteRenderer>().sprite);
		}
		if (x < xSize - 1)
		{
			possibleCharacters.Remove(tiles[x + 1, y].GetComponent<SpriteRenderer>().sprite);
		}
		if (y > 0)
		{
			possibleCharacters.Remove(tiles[x, y - 1].GetComponent<SpriteRenderer>().sprite);
		}

		return possibleCharacters[Random.Range(0, possibleCharacters.Count)];
	}

}
