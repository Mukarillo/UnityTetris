using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TetrisEngine.TetriminosPiece;
using TetrisEngine;
using System.IO;
using System.Linq;

public class GameSettingsWindow : EditorWindow {
	private GUISkin mGuiSkin;

	private GUIStyle mSelectedStyle;
	private GUIStyle mNormalStyle;

	private GameSettings mGameSettings;

	private int[][][] mTetriminoLayout;
	private Vector2Int[] mInitialPosition = new Vector2Int[Tetrimino.BLOCK_ROTATIONS];
	private Color mColor;
	private string mName;

	private float mTimeToStep;
	private int mPointsByBreakingLine;
	private bool mControledRandomMode;
	private bool mDebugMode;
 
	private KeyCode mMoveRightKey;
	private KeyCode mMoveLeftKey;
	private KeyCode mMoveDownKey;
    private KeyCode mRotateRightKey;
	private KeyCode mRotateLeftKey;

	private Vector2 mScrollImportedPosition;
	private Vector2 mScrollPosition;

	private int mCurrentEditing = -1;

    [MenuItem("Window/GameSettings Creator")]
    static void Init()
	{         
		GameSettingsWindow window = (GameSettingsWindow)EditorWindow.GetWindow(typeof(GameSettingsWindow));
        window.Show();
    }

	private void OnEnable()
	{
		mCurrentEditing = -1;
		mTetriminoLayout = GetEmptyLayout();

		mGuiSkin = Resources.Load<GUISkin>("GameSettingsCreatorSkin");
		mSelectedStyle = mGuiSkin.GetStyle("Selected");
		mNormalStyle = mGuiSkin.GetStyle("Normal");
	}
       
	void OnGUI()
    {
		GUI.skin = mGuiSkin;

		mScrollPosition = GUILayout.BeginScrollView(mScrollPosition, true, true, GUILayout.Width(position.width), GUILayout.Height(position.height));

		GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

		if (GUILayout.Button("IMPORT JSON", GUILayout.Width(150), GUILayout.Height(50)))
        {
			ImportJson();
        }

		if (GUILayout.Button("CREATE NEW", GUILayout.Width(150), GUILayout.Height(50)))
        {
			mCurrentEditing = -1;
			mGameSettings = new GameSettings();
			mGameSettings.pieces = new List<TetriminoSpecs>();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

		if (mGameSettings == null)
		{
			GUILayout.EndScrollView();
			return;
		}

		mTimeToStep = EditorGUILayout.FloatField("Time between update step", mTimeToStep);
		if (mTimeToStep < 0.01f) mTimeToStep = 0.01f;
		mPointsByBreakingLine = EditorGUILayout.IntField("Points by breaking lines", mPointsByBreakingLine);
		if (mPointsByBreakingLine < 0) mPointsByBreakingLine = 0;
		mControledRandomMode = EditorGUILayout.ToggleLeft("Controled random mode", mControledRandomMode);
		mDebugMode = EditorGUILayout.ToggleLeft("Debug mode", mDebugMode);

		mMoveRightKey = (KeyCode)EditorGUILayout.EnumPopup("Key to move the right", mMoveRightKey);
		mMoveLeftKey = (KeyCode)EditorGUILayout.EnumPopup("Key to move the left", mMoveLeftKey);
		mMoveDownKey = (KeyCode)EditorGUILayout.EnumPopup("Key to make the piece fall faster", mMoveDownKey);
		mRotateRightKey = (KeyCode)EditorGUILayout.EnumPopup("Key to rotate right", mRotateRightKey);
		mRotateLeftKey = (KeyCode)EditorGUILayout.EnumPopup("Key to rotate left", mRotateLeftKey);
		
		GUILayout.Space(20);
		var pieceHeight = 30f;
		var scrollHeight = Mathf.Clamp(pieceHeight * (2 + mGameSettings.pieces.Count), 0, pieceHeight * 4);
		mScrollImportedPosition = GUILayout.BeginScrollView(
			mScrollImportedPosition, true, true, 
            GUILayout.Width(position.width - 30),
			GUILayout.Height(scrollHeight));

		if (GUILayout.Button("ADD NEW", GUILayout.Width(position.width - 60), GUILayout.Height(pieceHeight)))
        {
			mScrollImportedPosition = new Vector2(0, scrollHeight);
            BeginEdit(-1, new TetriminoSpecs());
        }

		bool? mFinishedLayout = null;
		int counter = 0;
		for (int i = 0; i < mGameSettings.pieces.Count; i++)
		{
			if(counter++ == 0)
			{
				GUILayout.BeginHorizontal();
				mFinishedLayout = false;
			}
			
			var index = i;
			GUI.color = mGameSettings.pieces[i].color;
			if(GUILayout.Button(mGameSettings.pieces[i].name, GUILayout.Width(150), GUILayout.Height(pieceHeight)))
			{
				BeginEdit(index, mGameSettings.pieces[i]);
			}

			if (counter == 4)
			{
				GUILayout.EndHorizontal();
				mFinishedLayout = true;
				counter = 0;
			}
		}

		GUI.color = Color.white;
              
		if(mFinishedLayout.HasValue && !mFinishedLayout.Value)
			GUILayout.EndHorizontal();
        
        GUILayout.EndScrollView();
        
		if (mCurrentEditing != -1)
		{         
			GUILayout.Space(30);
			mName = EditorGUILayout.TextField("Tetrimino name", mName);
			GUILayout.Space(10);
			mColor = EditorGUILayout.ColorField("Tetrimino Color", mColor);
			mColor.a = 1f;
			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal();
			for (int t = 0; t < Tetrimino.BLOCK_ROTATIONS; t++)
				GUILayout.TextArea("Rotation " + (t + 1));
			EditorGUILayout.EndHorizontal();

			for (int i = 0; i < Tetrimino.BLOCK_AREA; i++)
			{
				EditorGUILayout.BeginHorizontal();
				for (int j = 0; j < Tetrimino.BLOCK_ROTATIONS; j++)
				{
					EditorGUILayout.BeginHorizontal();
					for (int l = 0; l < Tetrimino.BLOCK_AREA; l++)
					{
						bool active = mTetriminoLayout[j][i][l] == 1;
						GUI.color = active ? mColor : Color.white;
						if (GUILayout.Button(string.Format("{0},{1}", i, l), active ? mSelectedStyle : mNormalStyle))
							mTetriminoLayout[j][i][l] = active ? 0 : 1;
						GUI.color = Color.white;
					}
					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal();
			for (int i = 0; i < mInitialPosition.Length; i++)
			{
				mInitialPosition[i] = EditorGUILayout.Vector2IntField("Init Pos", mInitialPosition[i], GUILayout.Width(position.width / Tetrimino.BLOCK_ROTATIONS - 10));
			}

			EditorGUILayout.EndHorizontal();

			EndEdit();

			GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

			GUI.color = Color.red;
            if (GUILayout.Button("REMOVE", GUILayout.Width(150), GUILayout.Height(50)))
                RemoveSelected();
            GUI.color = Color.white;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();         
		}

		GUILayout.Space(50);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
         
		if(GUILayout.Button("SAVE", GUILayout.Width(150), GUILayout.Height(50)))         
			ExportSettings();
		
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
    }

	private void RemoveSelected()
	{
		mGameSettings.pieces.RemoveAt(mCurrentEditing);
		mCurrentEditing = -1;
	}

    private void ImportJson()
	{
		var path = EditorUtility.OpenFilePanel("Choose GameSettings json.", "Assets", "json");
        if (path.Length != 0)
        {
            var json = File.ReadAllText(path);
			mGameSettings = JsonUtility.FromJson<GameSettings>(json);

			mTimeToStep = mGameSettings.timeToStep;
			mPointsByBreakingLine = mGameSettings.pointsByBreakingLine;
			mControledRandomMode = mGameSettings.controledRandomMode;
			mDebugMode = mGameSettings.debugMode;

			mMoveRightKey = mGameSettings.moveRightKey;
			mMoveLeftKey = mGameSettings.moveLeftKey;
			mMoveDownKey = mGameSettings.moveDownKey;
			mRotateLeftKey = mGameSettings.rotateLeftKey;
			mRotateRightKey = mGameSettings.rotateRightKey;

			mCurrentEditing = -1;
        }
	}

	private void BeginEdit(int index, TetriminoSpecs specs)
	{
		if(index == -1)
		{
			specs.name = "New Tetrimino";
			specs.color = Color.white;
			specs.serializedBlockPositions = GetSerializableLayout(GetEmptyLayout());
			specs.initialPosition = GetInitialPositions();
			mGameSettings.pieces.Add(specs);
			index = mGameSettings.pieces.Count - 1;
		}

		GUIUtility.keyboardControl = 0;
        GUIUtility.hotControl = 0;

		mCurrentEditing = index;

		var pos = 0;
		var blockPositions = new int[Tetrimino.BLOCK_ROTATIONS][][];
        for (int i = 0; i < blockPositions.Length; i++)
        {
			blockPositions[i] = new int[Tetrimino.BLOCK_AREA][];
            for (int j = 0; j < blockPositions[i].Length; j++)
            {
				blockPositions[i][j] = new int[Tetrimino.BLOCK_AREA];
                for (int k = 0; k < blockPositions[i][j].Length; k++)
                {
					blockPositions[i][j][k] = specs.serializedBlockPositions[pos++];
                }
            }
        }

		mTetriminoLayout = blockPositions;
		mInitialPosition = specs.initialPosition;
		mColor = specs.color;
		mName = specs.name;
	}

    private void EndEdit()
	{
		if (mGameSettings != null)
		{
			var specs = new TetriminoSpecs();
			specs.name = mName;
			specs.color = mColor;
			specs.initialPosition = mInitialPosition;
			specs.serializedBlockPositions = GetSerializableLayout(mTetriminoLayout);
			mGameSettings.pieces[mCurrentEditing] = specs;
		}
	}

	private void ExportSettings()
	{
		var elements = 0;
		var squaredArea = Tetrimino.BLOCK_AREA * Tetrimino.BLOCK_AREA;
		foreach(var piece in mGameSettings.pieces)
		{
			elements = 0;
			while (elements * squaredArea < piece.serializedBlockPositions.Count)
			{
				if(piece.serializedBlockPositions.Skip(elements * squaredArea).Take(squaredArea).Sum() == 0)
				{
					throw new System.Exception(string.Format("Exportation failed. Rotation number {0} of piece {1} was left empty. A tetrimino must have at least one block.", elements + 1, piece.name));
				}

				elements++;
			}         
		}
		mGameSettings.timeToStep = mTimeToStep;
		mGameSettings.pointsByBreakingLine = mPointsByBreakingLine;
		mGameSettings.controledRandomMode = mControledRandomMode;
		mGameSettings.debugMode = mDebugMode;

		CheckValidKey(mMoveRightKey, "Move Right");
		mGameSettings.moveRightKey = mMoveRightKey;
		CheckValidKey(mMoveLeftKey, "Move Left");
		mGameSettings.moveLeftKey = mMoveLeftKey;
		CheckValidKey(mMoveDownKey, "Move Down");
		mGameSettings.moveDownKey = mMoveDownKey;
		CheckValidKey(mRotateRightKey, "Rotate Right");
		mGameSettings.rotateRightKey = mRotateRightKey;
		CheckValidKey(mRotateLeftKey, "Rotate Left");
		mGameSettings.rotateLeftKey = mRotateLeftKey;

		var json = JsonUtility.ToJson(mGameSettings, true);      
		string path = EditorUtility.SaveFilePanel("Save GameSettings Json", "Assets", "GameSettings", "json");      
		if(path.Length == 0)
		{
			Debug.LogError("Invalid path.");
			return;
		}

		File.WriteAllText(path, json);      
	}

	private void CheckValidKey(KeyCode code, string keyTitle)
	{
		if(code == KeyCode.None)
			throw new System.Exception(string.Format("Exportation failed. {0} key cannot be None", keyTitle));
	}

	private Vector2Int[] GetInitialPositions()
	{
		var initialPositions = new Vector2Int[Tetrimino.BLOCK_ROTATIONS];
		for (int i = 0; i < Tetrimino.BLOCK_ROTATIONS; i++)
			initialPositions[i] = new Vector2Int(-Tetrimino.BLOCK_AREA / 2, -Tetrimino.BLOCK_AREA / 2);
		return initialPositions;
	}

    private int[][][] GetEmptyLayout()
	{
		var layout = new int[Tetrimino.BLOCK_ROTATIONS][][];
		for (int i = 0; i < layout.Length; i++)
        {
			layout[i] = new int[Tetrimino.BLOCK_AREA][];
			for (int j = 0; j < layout[i].Length; j++)
            {
				layout[i][j] = new int[Tetrimino.BLOCK_AREA];
            }
        }

		return layout;
	}

    private List<int> GetSerializableLayout(int[][][] layout)
	{
		var serializableLayout = new List<int>();
        for (int i = 0; i < Tetrimino.BLOCK_ROTATIONS; i++)
        {
            for (int j = 0; j < Tetrimino.BLOCK_AREA; j++)
            {
                for (int l = 0; l < Tetrimino.BLOCK_AREA; l++)
                {
					serializableLayout.Add(layout[i][j][l]);
                }
            }
        }

		return serializableLayout;
	}
}
