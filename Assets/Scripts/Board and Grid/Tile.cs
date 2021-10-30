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
using System;

public class Tile : MonoBehaviour {

	//Tile, render and selection color
	private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
	private static Tile previousSelected = null;
    private SpriteRenderer render;

	//Choose a tile only for destruction animation
	[Header("Tile for animation")]
	public GameObject tile_destroy;

	//Selection and match booleans
	private bool isSelected = false;
	private bool matchFound = false;

	//Adjacent tiles
	private Vector2[] adjacentDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
	
	void Awake() {
		//Get the tile render 
		render = GetComponent<SpriteRenderer>();
    }

	//Selection of tile state
	//Change state, color the sprite selected, appoint a previous selected tile and play a sound
	private void Select() {
		isSelected = true;
		render.color = selectedColor;
		previousSelected = gameObject.GetComponent<Tile>();
		SFXManager.instance.PlaySFX(Clip.Select);
	}

	//No tile is selected
	//Change state, return to the standard color, empty previous selected
	private void Deselect() {
		isSelected = false;
		render.color = Color.white;
		previousSelected = null;
	}

	//Decides what to do with mouse interactions
	void OnMouseDown()
	{
		//if board is busy - don't do anything
		if (render.sprite == null || BoardManager.instance.IsShifting)
		{
			return;
		}

		// Is it already selected? - deselect
		if (isSelected)
		{ 
			Deselect();
		}
		else
		{   // Is it the first tile selected - select
			if (previousSelected == null)
			{ 
				Select();
			}
			//If it is not the first tile selected
			else
			{	//Get all adjacent tiles
				var list = GetAllAdjacentTiles();

				//if tile selected is adjacent of the previous one - swap sprite and see if there are matches, deselect the previous sprite and see again if there are any matches
				if (GetAllAdjacentTiles().Contains(previousSelected.gameObject))
				{
					SwapSprite(previousSelected.render);
					previousSelected.ClearAllMatches();
					previousSelected.Deselect();
					ClearAllMatches();

				}
				//If the tile is not adjacent, deselect the previous and select the new one
				else
				{
					previousSelected.GetComponent<Tile>().Deselect();
					Select();
				}
			}

		}
	}

	//Swap sprite positions
	public void SwapSprite(SpriteRenderer render2)
	{	//If it is equal, don't do anything
		if (render.sprite == render2.sprite)
		{
			return;
		}

		//Substitute each sprite by the other, play a sound and take 1 from the total of moves
		Sprite tempSprite = render2.sprite;
		render2.sprite = render.sprite;
		render.sprite = tempSprite;
		SFXManager.instance.PlaySFX(Clip.Swap);
		GUIManager.instance.MoveCounter--; 
	}

	//Get adjacent tile in the wanted direction
	private GameObject GetAdjacent(Vector2 castDir)
	{	//Detect what is next to the tile
		RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(castDir.x,castDir.y, 0), castDir);

		//If there is a tile, return it 
		if (hit.collider != null)
		{
			return hit.collider.gameObject;
		}
		return null;
	}

	//Get all adjacent tiles
	private List<GameObject> GetAllAdjacentTiles()
	{
		List<GameObject> adjacentTiles = new List<GameObject>();

		//Get a list of the tiles next to the actual tile according to the adjacent directions
		for (int i = 0; i < adjacentDirections.Length; i++)
		{
			adjacentTiles.Add(GetAdjacent(adjacentDirections[i]));
		}
		return adjacentTiles;
	}

	//Get matching tiles
	private List<GameObject> FindMatch(Vector2 castDir)
	{
		List<GameObject> matchingTiles = new List<GameObject>(); 
		RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(castDir.x, castDir.y, 0), castDir); 

		//get matching tiles (tiles are not empty and are equal to the tile) and return list
		while (hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == render.sprite)
		{ 
			matchingTiles.Add(hit.collider.gameObject);
			hit = Physics2D.Raycast(hit.collider.transform.position + new Vector3(castDir.x, castDir.y, 0), castDir);
		}
		return matchingTiles; 
	}

	//Get all atching tiles, evaluate if there's more than 2 and clear the sprites
	private void ClearMatch(Vector2[] paths)
	{
		//Get a list of matching tiles
		List<GameObject> matchingTiles = new List<GameObject>();
		for (int i = 0; i < paths.Length; i++) 
		{
			matchingTiles.AddRange(FindMatch(paths[i]));
		}

		//Clear the matching tiles if there are more than two in the list
		if (matchingTiles.Count >= 2)
		{
			for (int i = 0; i < matchingTiles.Count; i++)
			{
				//Clear the matching tiles
				matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;
				//Play animation by intantiating a second type of tile
				GameObject newTile = Instantiate(tile_destroy, matchingTiles[i].transform.position, matchingTiles[i].transform.rotation);
			}
			matchFound = true;
		}
	}

	//Clear all matches and connect to fill the empty tiles
	public void ClearAllMatches()
	{
		//If the sprite is empty, don't run
		if (render.sprite == null)
			return;

		//Run clear match in all direction
		ClearMatch(new Vector2[2] { Vector2.left, Vector2.right });
		ClearMatch(new Vector2[2] { Vector2.up, Vector2.down });

		//If a match is found, play an animation, clear the tile, start a Coroutine to fill the empty tiles and play sound
		if (matchFound)
		{
			//Play animation by intantiating a second type of tile
			GameObject newTile = Instantiate(tile_destroy, this.transform.position, this.transform.rotation);

			render.sprite = null;
			matchFound = false;

			//Fill the empty tiles
			StopCoroutine(BoardManager.instance.FindNullTiles());
			StartCoroutine(BoardManager.instance.FindNullTiles());

			SFXManager.instance.PlaySFX(Clip.Clear);
		}
	}
}