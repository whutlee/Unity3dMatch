using UnityEngine;
using System.Collections;

public class jewel : MonoBehaviour {
	
	internal JewelType m_type;
	internal JewelState m_state=JewelState.enIdle;
	private int m_x;
	private int m_y;
	private int m_tx;
	private int m_ty;
	public GameObject m_obj;	
	internal bool m_bMoving=false;

	internal GameObject[] m_FaceObj = new GameObject[5];
	
	public  int ty
    {
		get
        {
            return m_ty;
        }
	}
	public  int tx
    {
		get
        {
            return m_tx;
        }
	}
	public  int X
    {
        get
        {
            return m_x;
        }
		set
        {
            m_x=value;
			m_tx=m_x;
        }
    }
	
	public  int Y
    {
        get
        {
            return m_y;
        }
		set
        {
            m_y=value;
			m_ty=m_y;
        }
    }
	
	// Use this for initialization
	void Start () {
		if(m_obj)
		{
			m_FaceObj[(int)FaceAnim.enmDrag]=m_obj.transform.FindChild("boxface_drag").gameObject;
			m_FaceObj[(int)FaceAnim.enmHit]=m_obj.transform.FindChild("boxface_hit").gameObject;
			m_FaceObj[(int)FaceAnim.enmNormal]=m_obj.transform.FindChild("boxface_normal").gameObject;
			m_FaceObj[(int)FaceAnim.enmTouch]=m_obj.transform.FindChild("boxface_touch").gameObject;
		}
		
	}

	// Update is called once per frame
	void Update () {
		if(m_bMoving)
			OnMoving();
	
	}
	void OnMoving()
	{
		if(m_ty!=m_y||m_x!=m_tx)
		{
			Vector3 pos=game.Instance.GetPosFromGrid(m_tx,m_ty);
			Vector3 NewPos = transform.position;
			//Debug.Log("OnMoving");
			//Debug.Log(transform.position);
			if(NewPos.y>pos.y)
			{
				NewPos.y-=0.1f;
			}
			else if(NewPos.y<pos.y)
			{
				NewPos.y+=0.1f;
			}
			
			if(NewPos.x>pos.x)
			{
				NewPos.x-=0.1f;
			}
			else if(NewPos.x<pos.x)
			{
				NewPos.x+=0.1f;
			}
			//Debug.Log("NewPos");
			//Debug.Log(NewPos);
			transform.position=NewPos;

			if((pos-NewPos).magnitude<0.01f)
			{
				m_x=m_tx;
				m_y=m_ty;
				transform.position=pos;
				//Debug.Log("equal");
				//Debug.Log(pos);
				OnMoveEnd();
			}
		}
	}
	void OnMoveEnd()
	{
		Debug.Log ("OnMoveEnd");
		m_bMoving=false;
		game.Instance.MarkNeedCheck(new GridPos(){x=X,y=Y});
	}
	public void MoveTo(int x,int y)
	{
		m_tx=x;
		m_ty=y;
		if(m_ty==m_y&&m_x==m_tx)
			OnMoveEnd();
		else
			m_bMoving=true;
		//print(this);
	}
	public void SetJewelType(JewelType type)
	{
		m_type=type;
		if(m_type==JewelType.enIce)
			SetColor(Color.blue);
		else if(m_type==JewelType.enFire)
			SetColor(Color.red);
		else if(m_type==JewelType.enRecovery)
			SetColor(Color.green);
		else if(m_type==JewelType.enStorm)
			SetColor(Color.cyan);
		else if(m_type==JewelType.enAttack)
			SetColor(Color.magenta);
		else if(m_type==JewelType.enDummy)
			SetColor(Color.gray);
		
	}
	private IEnumerator WaitToShowColor(Color col)
	{
		while (true) {
			if (m_FaceObj[0]) {
				for(int n=0;n<(int)FaceAnim.enmNum;n++)
				{
					//Debug.Log(m_FaceObj[n]);
					m_FaceObj[n].renderer.material.color=col;
				}
				UpdataAnim(0f,0f);
				ShowState();
				yield break;
						
			} else
				yield return new WaitForFixedUpdate();
		}
		
	}
	void SetColor(Color col)
	{
		StartCoroutine(WaitToShowColor(col));
	}
	private IEnumerator WaitToShowAnim(FaceAnim anim)
	{
		while (true) {
			if (m_FaceObj[0]) {
				for(int n=0;n<(int)FaceAnim.enmNum;n++)
				{
					if((int)anim==n)
						m_FaceObj[n].SetActive(true);
					else
						m_FaceObj[n].SetActive(false);		
				}
				yield break;
						
			} else
				yield return new WaitForFixedUpdate();
		}
		
	}
	void ShowAim(FaceAnim anim)
	{
		StartCoroutine(WaitToShowAnim(anim));
	}
	void ShowState()
	{	
		switch(m_state)
		{
	
		case JewelState.enIdle:
			ShowAim(FaceAnim.enmNormal);
			break;
		case JewelState.enTouched:
			ShowAim(FaceAnim.enmTouch);
			break;	
		case JewelState.enEffecting:
			ShowAim(FaceAnim.enmHit);
			break;
		case JewelState.enSelected:
			ShowAim(FaceAnim.enmDrag);
			break;
		}
		
	}
	public JewelType GetJewelType()
	{
		return m_type;
	}
	
	public bool IsCanFall()
	{
		return m_state==JewelState.enIdle||m_state==JewelState.enTouched;
	}
	public bool IsCanSwap()
	{
		return m_state==JewelState.enIdle||m_state==JewelState.enSelected||m_state==JewelState.enTouched;
	}
	public bool IsMoving()
	{
		return m_bMoving;
	}

	public void SetState(JewelState state)
	{
		m_state=state;
		Debug.Log(m_state);
		ShowState();
	}
	public void UpdataAnim(float x,float y)
	{
		Animator anim = GetComponent<Animator>();
		anim.SetFloat("X",x);
		anim.SetFloat("Y",y);
	}
	//public void SetState(JewelState state)
	//{
	//	m_state=state;
	//	print("s")
	//}
	override public string ToString ()
	{
		return "jewel"+m_x.ToString()+"-"+m_y.ToString()+m_state.ToString();
	}
	
}