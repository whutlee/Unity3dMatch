using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//
public class game : MonoBehaviour
{

	// Use this for initialization
	public GameObject JewelPre ;
	public GameObject DestroyPre;
	public GameObject pc;
	public GameObject npc;
	public float DestoryDelay;
	public float ChangeTurnDelay;
	private jewel[,] Jewels = new jewel[10, 10];
	private int LineNum = 6;
	private jewel selected;
	private float clickDelay = 0f;
	private static game instance = null;
	
	
	internal List<GridPos> markChanged = new List<GridPos> ();
	internal List<ClearInfo>  ClearInfos = new List<ClearInfo> ();
	internal GameTurn turn;
	internal GameTurn lastTurn;
	private int CurJewelNum=0;
	private GameResult Result = GameResult.enNone;
	public Texture texWin;
	public Texture texLose;
	public Texture texRestart;
	private int CurCombo=0;
	public static game Instance {
		get {
			return instance;
		}
	}

	void Start ()
	{
		if (instance != null && instance != this) {
			Destroy (this);
			return;
		}
		instance = this;
		
		StartGame ();
		
	}
	public void OnPlayerDie(player p)
	{
		player pl = pc.GetComponent<player>();
		if(pl == p)
			Result = GameResult.enLose;
		else
			Result = GameResult.enWin;
		EndGame();
	}

	JewelType GetRandomTypeExcept (JewelType except1, JewelType except2)
	{
		JewelType type = except1;
		while (type==except1||type==except2) {
			type = (JewelType)Random.Range (1, (int)JewelType.enNum);
		}
		
		return type;
	}
	
	List<GridPos>  MergeList (List<GridPos> l, List<GridPos> r)
	{
		List<GridPos> NewList = new List<GridPos> ();
		NewList.AddRange (l);
		for (int n=0; n<r.Count; n++) {
			if (!NewList.Contains (r [n]))
				NewList.Add (r [n]);
		}
		return NewList;
	}
	
	List<GridPos> CheckFullMatch (GridPos pos, List<GridPos> IgnorePos)
	{
		List<GridPos> ret = CheckMatch (pos, false);
		for (int n=0; n<ret.Count; n++) {
			if (ret [n] != pos && !IgnorePos.Contains (ret [n])) {
				IgnorePos.Add (ret [n]);
				ret = MergeList (CheckFullMatch (ret [n], IgnorePos), ret);
			}
		}
		return ret;
	}

	List<GridPos> CheckMatch (GridPos pos, bool bFullCheck)
	{
		// no full check just check current oos in cross;
		List<GridPos> l = new List<GridPos> ();
		if (bFullCheck) {
			return CheckFullMatch (pos, l);
		}
		List<GridPos> ret = new List<GridPos> ();
		ret.Add (pos);
		GridPos check = pos;
		JewelType curType = Jewels [pos.x, pos.y].GetJewelType ();
		int num = 0;
		while (--check.x>=0) {
			jewel obj = Jewels [check.x, check.y];
			if (obj && obj.GetJewelType () == curType && obj.IsCanSwap ()) {
				l.Add (check);
				num++;
			} else
				break;
		}
		check = pos;
		while (++check.x<LineNum) {
			jewel obj = Jewels [check.x, check.y];
			if (obj && obj.GetJewelType () == curType && obj.IsCanSwap ()) {
				l.Add (check);
				num++;
			} else
				break;
		}
		
		if (num >= 2) { // find match
			
			ret.AddRange (l);
		}
		
		l.Clear ();
		num = 0;
		check = pos;
		while (--check.y>=0) {
			jewel obj = Jewels [check.x, check.y];
			if (obj && obj.GetJewelType () == curType && obj.IsCanSwap ()) {
				l.Add (check);
				num++;
			} else
				break;
		}
		check = pos;
		while (++check.y<LineNum) {
			jewel obj = Jewels [check.x, check.y];
			if (obj && obj.GetJewelType () == curType && obj.IsCanSwap ()) {
				l.Add (check);
				num++;
			} else
				break;
		}
		
		if (num >= 2) { // find match
			
			ret.AddRange (l);
		}
		if (ret.Count == 1)
			ret.RemoveAt (0);
		
		return ret;	
	}

	bool IsSwapUseful (jewel l, jewel r)
	{
		if (l == null || r == null) {
			return false;
		}
		Jewels [l.X, l.Y] = r;
		Jewels [r.X, r.Y] = l;
		bool ret = (CheckMatch (new GridPos (){x=l.X,y=l.Y}, false).Count > 0 ||
		CheckMatch (new GridPos (){x=r.X,y=r.Y}, false).Count > 0);
		
		Jewels [l.X, l.Y] = l;
		Jewels [r.X, r.Y] = r;
		return ret;	
	}

	bool FindSolution (out GridPos posFrom, out GridPos posTo)
	{
		posFrom = new GridPos ();
		posTo = new GridPos ();
		for (int i=0; i<LineNum; i++) {	
			for (int j=1; j<LineNum; j++) {
				posFrom.x = i;
				posFrom.y = j;
				if (i > 0) {
					if (IsSwapUseful (Jewels [i, j], Jewels [i - 1, j])) {
						posTo.x = i - 1;
						posTo.y = j;
						return true;
					}
				}
				
				if (i < LineNum - 1) {
					if (IsSwapUseful (Jewels [i, j], Jewels [i + 1, j])) {
						posTo.x = i + 1;
						posTo.y = j;
						return true;
					}
				}
				
				if (j > 0) {
					if (IsSwapUseful (Jewels [i, j], Jewels [i, j - 1])) {
						posTo.x = i;
						posTo.y = j - 1;
						return true;
					}
				}
				
				if (j < LineNum - 1) {
					if (IsSwapUseful (Jewels [i, j], Jewels [i, j + 1])) {
						posTo.x = i;
						posTo.y = j + 1;
						return true;
					}
				}
			}
		}
		
		return false;
	}
	
	jewel CreateNewJewel ()
	{
		jewel obj;
		GameObject gameObj;
		gameObj = Instantiate (JewelPre, new Vector3 (0f, 0f, 0f), Quaternion.identity) as GameObject;		
		obj = gameObj.GetComponent ("jewel") as jewel;
		obj.m_obj = gameObj;
		if(++CurJewelNum>LineNum*LineNum)
			Debug.Log("Fatal error ,jewel num is more than max");
		return obj;
	}
	void DestoryJewel(jewel obj)
	{
		if(--CurJewelNum<0)
			Debug.Log("Fatal error ,jewel num is less than 0");
		Destroy (obj.m_obj);
	}
	void ClearMap()
	{
		Debug.Log("clear old map");
		for (int i=0; i<LineNum; i++) {
			for (int j=0; j<LineNum; j++) {
				
				jewel obj = Jewels[i,j];
				if(obj)
				{
					DestoryJewel(obj);
					Jewels[i,j]=null;
				}
			}
		}
	}
	void GenerateMap ()
	{
		Debug.Log("generate new map");
		for (int i=0; i<LineNum; i++) {
			for (int j=0; j<LineNum; j++) {
				
				jewel obj = CreateNewJewel ();
				Jewels [i, j] = obj;
				obj.X=i;
				obj.Y=j-LineNum;
				ResetPos (obj);
				obj.MoveTo(i,j);
				
				JewelType excType1 = JewelType.enNone;
				JewelType excType2 = JewelType.enNone;
				if (i >= 2) {
					if (Jewels [(i - 1), j].GetType () == Jewels [(i - 2), j].GetType ())
						excType1 = Jewels [(i - 1), j].GetJewelType ();
				}
				if (j >= 2) {
					if (Jewels [(i), j - 1].GetType () == Jewels [(i), j - 2].GetType ())
						excType2 = Jewels [(i), j - 1].GetJewelType ();
				}
				JewelType newType = GetRandomTypeExcept (excType1, excType2);
				obj.SetJewelType (newType);
				
			}
		}
	}

	void StartGame ()
	{		
		Result = GameResult.enNone;
		OnGameStart();
		pc.GetComponent<player>().OnGameStart();
		npc.GetComponent<player>().OnGameStart();
		ClearMap();
		ChangeTurn (GameTurn.enYourTurn);
		GenerateMap ();
			
	}
	
	void EndGame()
	{
		OnGameEnd();
		ChangeTurn(GameTurn.enNone);
		ClearInfos.Clear();
		markChanged.Clear();
		CurCombo=0;
		turn=GameTurn.enNone;
		lastTurn=turn;
	}
	
	public void MarkNeedCheck (List<GridPos> points)
	{
		for (int n=0; n<points.Count; n++)
			MarkNeedCheck (points [n]);
	}

	public void MarkNeedCheck (GridPos pos)
	{
		if (!markChanged.Contains (pos))
			markChanged.Add (pos);
	}

	public Vector3 GetPosFromGrid (int x, int y)
	{
		return new Vector3 (-2.5f + x * 1f, 2.5f - y * 1f, -3.0f);
	}
	
	void ResetPos (jewel obj)
	{
		obj.transform.position = GetPosFromGrid (obj.X, obj.Y);
	}
	
	void ClearJewel (List<GridPos> clearPos)
	{
		//print ("clear jewels");
		for (int n=0; n<clearPos.Count; n++) {
			jewel obj = Jewels [clearPos [n].x, clearPos [n].y];
			Jewels [clearPos [n].x, clearPos [n].y] = null;
			DestoryJewel (obj);
		}
		//FillJewels ();
	}

	void FillJewels ()
	{
		for (int i=0; i<LineNum; i++) {
			int NewJewelPosY = 0;
			for (int j=LineNum-1; j>=0; j--) {
				if (Jewels [i, j])
					continue;
				
				int bottom = j;
				
				bool bFindSwapOne = false;

				while (--bottom>=0) {
					if (Jewels [i, bottom]&&Jewels [i, bottom].IsCanFall()) {
						jewel swapObj = Jewels [i, bottom];
						
						Jewels [i, bottom] = null;
						bFindSwapOne = true;
						Jewels [i, j] = swapObj;
						swapObj.MoveTo (i, j);
						
						break;
					}
				}
				
				if (!bFindSwapOne) {
					jewel obj = CreateNewJewel ();
					obj.SetJewelType (GetRandomTypeExcept (JewelType.enNone, JewelType.enNone));
					jewel swapObj = obj;
					swapObj.X = i;
					swapObj.Y = --NewJewelPosY;
					ResetPos (swapObj);
					
					Jewels [i, j] = swapObj;
					swapObj.MoveTo (i, j);
				}
				
				
			}
			
		}
	}
	
	void CheckHelpInput ()
	{
		if (Input.GetKey (KeyCode.C)) {
			if (IsOperateAvailable()) {
				
				DestoryJewel(Jewels[3,3]);
				DestoryJewel(Jewels[3,4]);
				Debug.Log ("check test");
				Jewels[3,3]=null;
				Jewels[3,4]=null;
				FillJewels();
				ClearOperate();
			}
		}
		
		if(Input.GetKey (KeyCode.E))
		{
			EndGame();
		}
		else if(Input.GetKey (KeyCode.G))
		{
			StartGame();
		}
		else if (Input.GetKey (KeyCode.A)) {
			if (selected)
				selected.SetJewelType (JewelType.enIce);
		} else if (Input.GetKey (KeyCode.S)) {
			if (selected)
				selected.SetJewelType (JewelType.enFire);
		} else if (Input.GetKey (KeyCode.D)) {
			if (selected)
				selected.SetJewelType (JewelType.enRecovery);
		}
	}
	
	void CheckJewels ()
	{
		int count = 0;
		while (markChanged.Count>0&&count<markChanged.Count) {
			List<GridPos> ret = CheckMatch (markChanged [count], true);
			
			if (ret.Count > 0) {
				OnClear (ret);
				for (int m=0; m<ret.Count; m++) {
					if (markChanged.Contains (ret [m]))
						markChanged.Remove (ret [m]);
				}
			} else
				count++;
		}
	
		markChanged.Clear ();
	}
	
	void MakeSureHasSoulution ()
	{
		GridPos fromPos, toPos;
		bool bFind = FindSolution (out fromPos, out toPos);
		if(!bFind)
		{
			ClearMap();
			GenerateMap();
		}
	}

	private IEnumerator ToggleTurn (GameTurn t)
	{
		yield return new WaitForSeconds(ChangeTurnDelay);
		if (t == GameTurn.enAITurn) {
			ChangeTurn (GameTurn.enYourTurn);
		} else if (t == GameTurn.enYourTurn) {
			ChangeTurn (GameTurn.enAITurn);
			CheckAI ();
		}
	}

	private IEnumerator DelayToggleTurn (GameTurn t)
	{
		while (true) {
			if (ClearInfos.Count == 0 && IsAllJewelIdle () && markChanged.Count == 0) {
				MakeSureHasSoulution();
				Debug.Log ("wait for changing turn");
				CurCombo=0;
				StartCoroutine ("ToggleTurn", t);
				yield break;
						
			} else
				yield return new WaitForFixedUpdate();
		}
	}
		
	void PreClearJewels (float delta)
	{
		int n = 0;
		
		while (ClearInfos.Count>0&&n<ClearInfos.Count) {
			ClearInfo info = ClearInfos [n];
			info.deltaTime += delta;
			
			if (info.deltaTime > info.ClearTime) {
				ClearJewel (info.position);
				OnAttack(info.type,info.position.Count,info.owner,CurCombo++);
				ClearInfos.RemoveAt (n);
			} else {
				ClearInfos [n] = info;
				n++;
			}
		}
		FillJewels();
	}
	
	void OnGameEnd()
	{
		Debug.Log("game end");
	}
	void OnGameStart()
	{
		Debug.Log("game start");
	}
	void OnAttack(JewelType type,int num,GameTurn turn,int combo)
	{
		string text;
		text = string.Format ("clear type:{0} clear num:{1}{2} combo:{3}", type, num,turn,combo);
		Debug.Log (text);
		GameObject attacker = (turn == GameTurn.enYourTurn)? pc : npc;
		player p = attacker.GetComponent<player>();
		p.OnAttack(type,num,combo);
	}
	// Update is called once per frame
	bool IsOperateAvailable ()
	{
		if (clickDelay > 0.3f) {	
			return turn == GameTurn.enYourTurn;
		} else {
			clickDelay += Time.deltaTime;
			return false;
		}
	}

	void ClearOperate ()
	{
		clickDelay = 0f;
	}

	void ChangeTurn (GameTurn v)
	{
		lastTurn=turn;
		turn = v;
		string text;
		text = string.Format ("It is {0}", turn);
		Debug.Log (text);
	}

	bool IsAllJewelIdle ()
	{
		for (int i=0; i<LineNum-1; i++) {
			for (int j=0; j<LineNum-1; j++) {
				if (!Jewels [i, j].IsCanSwap ())
					return false;
				
			}
		}
		return true;
	}

	void CheckAI ()
	{
		if (turn == GameTurn.enAITurn) {
			GridPos fromPos, toPos;
			bool bFind = FindSolution (out fromPos, out toPos);
			SwapJewel (Jewels [fromPos.x, fromPos.y], Jewels [toPos.x, toPos.y]);
			if (!bFind)
				Debug.Log ("Fatal error ,can find solution!");
			else {
				string text = string.Format ("ai change [{0},{1}] to [{2},{3}]", fromPos.x, fromPos.y, toPos.x, toPos.y);
				Debug.Log (text);
			}
			ChangeTurn (GameTurn.enNone);
			StartCoroutine ("DelayToggleTurn", GameTurn.enAITurn);
		}
	}

	void OnPickObj (jewel obj)
	{
		if (selected) {
			selected.SetSelected (false);
			if (selected == obj) {
				Debug.Log ("cancel swap");
			}
			if (!selected.IsCanSwap () || !obj.IsCanSwap ()) {
				Debug.Log ("can not swap");
							
				selected = null;
			} else if (IsNeighbour (selected, obj)) {
							
				if (IsSwapUseful (selected, obj)) {
							
					Debug.Log ("swap");
					SwapJewel (selected, obj);
					ChangeTurn (GameTurn.enNone);
					StartCoroutine ("DelayToggleTurn", GameTurn.enYourTurn);
				} else {
					OnSwapFailed (selected, obj);
				}
			}
			selected = null;
		} else {
			Debug.Log ("select");
			selected = obj;
			selected.SetSelected (true);
		}
	}
	
	void CheckGameInput()
	{
		if(!IsOperateAvailable ())
			return;
		bool hasInput = false;
		Vector3 pos = new Vector3(0,0,0);
		if (Input.GetKey (KeyCode.Mouse0)) {
			hasInput = true;
			pos = Input.mousePosition;
		} 
		
        foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Began) {
				hasInput = true;
				pos = touch.position;
			}
		}
		
		if(hasInput)
		{			
			ClearOperate ();
			Ray ray = Camera.main.ScreenPointToRay (pos);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)) {
				jewel obj = hit.collider.gameObject.GetComponent ("jewel") as jewel;
				if (obj) {
					OnPickObj (obj);
				}
					
			}
		}
	}

	void Update ()
	{
		CheckJewels ();
		PreClearJewels (Time.deltaTime);
		CheckHelpInput ();
		CheckGameInput();
		
	}

	void SwapJewel (jewel l, jewel r)
	{
		if (!l.IsCanSwap () || !r.IsCanSwap ()) {
			Debug.Log ("can't swap these");
		}
		int tx = l.X;
		int ty = l.Y;
		
		Jewels [l.X, l.Y] = r;
		Jewels [r.X, r.Y] = l;
		
		l.MoveTo (r.X, r.Y);
		r.MoveTo (tx, ty);

	}

	bool IsNeighbour (jewel l, jewel r)
	{
		if (l.X == r.X && Mathf.Abs (l.Y - r.Y) == 1)
			return true;
		else if (l.Y == r.Y && Mathf.Abs (l.X - r.X) == 1)
			return true;
		else 
			return false;
		
	}
	private IEnumerator FaildAnimation (PairJewel obj)
	{
		while(true)
		{
			if(obj.r.IsCanSwap()&&obj.l.IsCanSwap())
			{
				yield return new WaitForSeconds(0.1f);
				SwapJewel(obj.l,obj.r);
				yield break;
			}
			else
				yield return new WaitForFixedUpdate();
		}
		
	}
	void OnSwapFailed (jewel l, jewel r)
	{
		Debug.Log ("swap failed");
		SwapJewel(l,r);
		PairJewel obj =new PairJewel();
		obj.l=l;
		obj.r=r;
		StartCoroutine("FaildAnimation", obj);
	}

	void OnClear (List<GridPos> clearPos)
	{
		GridPos pos = clearPos [0];
	
		for (int n=0; n<clearPos.Count; n++) {
			jewel obj = Jewels [clearPos [n].x, clearPos [n].y];
			obj.SetState (JewelState.enEffecting);
			
			GameObject effect;
			Vector3 positon = obj.transform.position;
			positon.z -= 0.5f;
			effect = Instantiate (DestroyPre, positon, Quaternion.identity) as GameObject;	
			effect.transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);
		}
		ClearInfo info = new ClearInfo ();
		info.type = Jewels [pos.x, pos.y].GetJewelType ();
		info.position = new List<GridPos> ();
		info.position.AddRange (clearPos);
		info.ClearTime = DestoryDelay;
		info.deltaTime = 0f;
		info.owner = lastTurn;
		ClearInfos.Add (info);	
		
	}
	
	void OnGUI()
	{
		if(Result != GameResult.enNone)
		{
			if(Result == GameResult.enWin)
				GUI.DrawTexture(new Rect(200,100 ,256,128),texWin);	
			else if(Result == GameResult.enLose)
				GUI.DrawTexture(new Rect(200,100 ,256,128),texLose);
			if(GUI.Button(new Rect(500,330 ,128,64),texRestart))
			{
				StartGame();
			}
		}
	}
}
