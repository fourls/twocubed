using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour {

	[Serializable]
	public class RarityGroup {
		public GameObject[] common;
		public GameObject[] uncommon;
		public GameObject[] rare;
		public GameObject[] legendary;

		GameObject GetRandom () {
			int whichOne = Random.Range (0, 15);
			if (whichOne == 0) // 0
				return legendary [Random.Range (0, legendary.Length)];
			else if (whichOne > 0 && whichOne < 3) // 1, 2
				return rare [Random.Range (0, rare.Length)];
			else if (whichOne > 2 && whichOne < 7) // 3, 4, 5, 6
				return uncommon [Random.Range (0, uncommon.Length)];
			else if (whichOne > 6) // 7, 8, 9, 10, 11, 12, 13, 14
				return common [Random.Range (0, common.Length)];
			else
				return null;
		}

		public void LayoutAtRandom (int min, int max,BoardManager thisManager) {
			int objectCount = Random.Range (min, max + 1);

			for(int i = 0; i < objectCount; i++) {
				Vector3 pos = thisManager.GetRandomGridPos();
				GameObject chosen = GetRandom ();
				GameObject instance = Instantiate (chosen,pos,Quaternion.identity) as GameObject;
				instance.transform.SetParent(thisManager.itemContainer);
			}
		}
	}

	[Serializable]
	public class Count {
		public int min;
		public int max;

		public Count (int mn,int mx) {
			min = mn;
			max = mx;
		}
	}

	public int rows = 20;
	public int columns = 20;
	public Count wallCount = new Count (5, 15);
	public Count itemCount = new Count (5, 15);
	public Count signCount = new Count (0, 1);
	public Count NPCCount = new Count (0, 2);
	public Count resourceCount = new Count (3, 7);
	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject[] enemyTiles;
	public GameObject[] items;
	public GameObject[] entryTiles;
	public GameObject[] exitTiles;
	public GameObject[] pathTiles;
	public RarityGroup resourceTiles;
	public GameObject[] signs;
	public GameObject[] NPCs;

	private Transform boardContainer;
	private Transform itemContainer;
	private List <Vector3> gridPositions = new List<Vector3>();

	void InitList () {
		gridPositions.Clear ();

		for (int x = 1; x < columns - 1; x++) {
			for (int y = 1; y < rows - 1; y++) {
				gridPositions.Add (new Vector3 (x, y, 0f));
			}
		}
	}

	void BoardSetup () {
		boardContainer = new GameObject ("Board Container").transform;
		itemContainer = new GameObject ("Item Container").transform;

		for (int x = -1; x < columns + 1; x++) {
			for (int y = -1; y < rows + 1; y++) {
				GameObject toInstantiate = floorTiles[Random.Range(0,floorTiles.Length)];
				// Wall Tiles
				if(x == -1 || x == columns || y == -1 || y == rows)
					toInstantiate = wallTiles[Random.Range(0,wallTiles.Length)];
				// Path Tiles
				if((x == columns - 1 && y == rows) || (x == 0 && y == -1))
					toInstantiate = pathTiles[Random.Range(0,pathTiles.Length)];
				// Entry Tile
				if((x == 0 && y == 0))
					toInstantiate = entryTiles[Random.Range(0,entryTiles.Length)];
				GameObject instance = Instantiate (toInstantiate,new Vector3(x,y,0f),Quaternion.identity) as GameObject;

				instance.transform.SetParent(boardContainer);
			}
		}
	}

	Vector3 GetRandomGridPos () {
		int randIndex = Random.Range (0, gridPositions.Count);
		Vector3 randomPos = gridPositions [randIndex];
		gridPositions.RemoveAt (randIndex);
		return randomPos;
	}

	void LayoutAtRandom (GameObject[] objects, int min, int max) {
		int objectCount = Random.Range (min, max + 1);

		for(int i = 0; i < objectCount; i++) {
			Vector3 pos = GetRandomGridPos();
			GameObject chosen = objects[Random.Range (0,objects.Length)];
			GameObject instance = Instantiate (chosen,pos,Quaternion.identity) as GameObject;
			instance.transform.SetParent(itemContainer);
		}
	}

	public void SetupScene (int level) {
		BoardSetup ();
		InitList ();
		LayoutAtRandom (wallTiles, wallCount.min, wallCount.max);
		resourceTiles.LayoutAtRandom (resourceCount.min, resourceCount.max,this);
		LayoutAtRandom (items, itemCount.min, itemCount.max);
		LayoutAtRandom (signs, signCount.min, signCount.max);
		int enemyCount = (int)Mathf.Log (level, 2f);
		LayoutAtRandom (enemyTiles, enemyCount, enemyCount);
		LayoutAtRandom (NPCs, NPCCount.min, NPCCount.max);
		GameObject exitInstance = exitTiles[Random.Range(0,exitTiles.Length)];
		Instantiate (exitInstance, new Vector3 (columns - 1, rows - 1, 0f), Quaternion.identity);
	}
}


