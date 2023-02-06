using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public int UnitSpeed = 3;

	[SerializeField] private GameObject mUnitPrefab;
	public GameManagerUI mGMUI { get; private set; }
	public GridManager mGridManager { get; private set; }
	public GridManagerDebug mGridManagerDebug { get; private set; }
	public List<Unit> mUnitList { get; private set; }

	private float mTimer;
	private const float MAX_TIME = 10;

	private const int SCREEN_WIDTH = 50; // x
	private const int SCREEN_HEIGHT = 50; // z
	
	private const int UNIT_SPAWN_AMOUNT = 250;
	private const int MAX_UNIT_NUMBER = 2000;
	
    private void Awake()
    {
		mGMUI = GetComponent<GameManagerUI>();
		mGridManager = GetComponent<GridManager>();
		mGridManagerDebug = GetComponent<GridManagerDebug>();
        mUnitList = new List<Unit>();
    }

    private void Start()
    {
		// Spawning the units at the start of the game
		for (int i = 0; i < UNIT_SPAWN_AMOUNT + UNIT_SPAWN_AMOUNT; i++)
		{
			if (mUnitList.Count >= MAX_UNIT_NUMBER) return;
			GameObject unit = Instantiate(mUnitPrefab);
			unit.transform.position = GetRandomPositionWithinTheGrid();
		}
	}

    private void Update()
    {
		UpdateUnitGoalDestination();

		// UI updates
		mGMUI.UpdateUnitCount();
	}

    private void FixedUpdate()
    {
        GamePlayControls();
    }
	
	private void GamePlayControls()
	{
		// game modifications
		SpawnUnits();
		UpdateUnitPosition();
		
		// key inputs
		MoveCamera();
		
		// scene management
		if (Input.GetKey(KeyCode.Escape)) Application.Quit();
		else if (Input.GetKey(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	
	private void UpdateUnitPosition()
	{
		if (mGridManager.CurrentFlowField == null) return;

		for (int i = 0; i < mUnitList.Count; i++)
		{
			Cell belowCell = mGridManager.CurrentFlowField.GetCellFromWorldPosition(mUnitList[i].transform.position);
			if (mUnitList[i].GetPreviousCell() == null) // should only be called once
			{
				mUnitList[i].SetPreviousCell(belowCell);
				mUnitList[i].GetPreviousCell().mIsOccupied = true;
			}
			mUnitList[i].SetCurrentCell(belowCell);
			mUnitList[i].GetCurrentCell().mIsOccupied = true;

			Vector3 moveDirection = new Vector3(belowCell.mBestDirection.mVector.x, 0, belowCell.mBestDirection.mVector.y);
			float unitSpeed = Random.Range(1, UnitSpeed);

			// prevent the units from overlapping each other
			List<Cell> currentNeighbors = mGridManager.CurrentFlowField.GetNeighborCells(belowCell.mGridIndex, GridDirection.AllDirections);
			foreach (Cell currentNeighbor in currentNeighbors)
			{
				if (currentNeighbor.mIsOccupied && currentNeighbor.mBestDirection == belowCell.mBestDirection)
				{
					unitSpeed /= 1.6f; // Slow Down
				}
			}

			mUnitList[i].transform.position += moveDirection * Time.fixedDeltaTime * unitSpeed;
		}
	}

	private void UpdateUnitGoalDestination()
	{
		if (mUnitList.Count <= 0) return;

		if (mTimer <= 0)
		{
			mTimer = MAX_TIME;
			mGridManager.UpdateFlowField(GetRandomPositionWithinTheGrid());
		}
		else
			mTimer -= Time.deltaTime;
	}
	
	private void SpawnUnits()
	{
		if (mUnitList.Count >= MAX_UNIT_NUMBER) return;

		if (Input.GetKeyUp(KeyCode.P))
        {
			for (int i = 0; i < UNIT_SPAWN_AMOUNT; i++)
			{
				if (mUnitList.Count >= MAX_UNIT_NUMBER) return;
				GameObject unit = Instantiate(mUnitPrefab);
				
				unit.transform.position = GetRandomPositionWithinTheGrid();
			}
		}
	}

	private void MoveCamera()
	{
		Vector3 cameraPos = Camera.main.transform.position;
		if (Input.GetKey(KeyCode.W))	  cameraPos.z += 1.0f;
		else if (Input.GetKey(KeyCode.A)) cameraPos.x -= 1.0f;
		else if (Input.GetKey(KeyCode.S)) cameraPos.z -= 1.0f;
		else if (Input.GetKey(KeyCode.D)) cameraPos.x += 1.0f;
		cameraPos.x = Mathf.Clamp(cameraPos.x, -SCREEN_WIDTH, SCREEN_WIDTH);
		cameraPos.z = Mathf.Clamp(cameraPos.z, -SCREEN_HEIGHT, SCREEN_HEIGHT);
		Camera.main.transform.position = cameraPos;
	}
	
	// GETTERS
	public List<Unit> GetUnitList(){return mUnitList;} // change this
	public int GetUnitListSize(){return mUnitList.Count;}
	public int GetScreenWidth(){return SCREEN_WIDTH;}
	public int GetScreenHeight(){return SCREEN_HEIGHT;}
	public Vector3 GetRandomPositionWithinTheGrid()
	{
		Vector3 spawnLoc = Vector3.zero;
		spawnLoc.x = Random.Range(mGridManager.GridOffset.x, mGridManager.GridSize.x + mGridManager.GridOffset.x);
		spawnLoc.z = Random.Range(mGridManager.GridOffset.y, mGridManager.GridSize.y + mGridManager.GridOffset.y);
		return spawnLoc;
	}
	
	// SETTERS
	public void AddUnitToUnitList(Unit unit){mUnitList.Add(unit);}
	public void RemoveUnitFromUnitList(Unit unit){mUnitList.Remove(unit);}
}