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
	internal bool m_Selected=false;
	
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
	
	}

	// Update is called once per frame
	void Update () {
		if(m_state==JewelState.enMoving)
			OnMoving();
	}
	void OnMoving()
	{
		if(m_ty!=m_y||m_x!=m_tx)
		{
			Vector3 pos=game.Instance.GetPosFromGrid(m_tx,m_ty);
			Vector3 NewPos = transform.position;
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
			
			transform.position=NewPos;
			if((pos-NewPos).magnitude<0.01f)
			{
				m_x=m_tx;
				m_y=m_ty;
				transform.position=pos;
				OnMoveEnd();
				//print(this);
			}
		}
	}
	void OnMoveEnd()
	{
		m_state=JewelState.enIdle;
		game.Instance.MarkNeedCheck(new GridPos(){x=X,y=Y});
	}
	public void MoveTo(int x,int y)
	{
		m_tx=x;
		m_ty=y;
		if(m_ty==m_y&&m_x==m_tx)
			OnMoveEnd();
		else
			m_state=JewelState.enMoving;
		//print(this);
	}
	public void SetJewelType(JewelType type)
	{
		m_type=type;
		ShowState();
		
	}
	void ShowState()
	{
	
		if(m_type==JewelType.enIce)
			renderer.material.color=Color.blue;
		else if(m_type==JewelType.enFire)
			renderer.material.color=Color.red;
		else if(m_type==JewelType.enRecovery)
			renderer.material.color=Color.green;
		else if(m_type==JewelType.enStorm)
			renderer.material.color=Color.cyan;
		else if(m_type==JewelType.enAttack)
			renderer.material.color=Color.magenta;
		else if(m_type==JewelType.enDummy)
			renderer.material.color=Color.gray;
		if(m_Selected)
		{
			renderer.material.color/=3;

		}
	}
	public JewelType GetJewelType()
	{
		return m_type;
	}
	
	public void SetSelected(bool selected)
	{
		m_Selected=selected;
		ShowState();
	}
	
	public bool GetSelected()
	{
		return m_Selected;
	}
	public bool IsCanFall()
	{
		return m_state==JewelState.enIdle||m_state==JewelState.enMoving;
	}
	public bool IsCanSwap()
	{
		return m_state==JewelState.enIdle;
	}
	public void SetState(JewelState state)
	{
		m_state=state;
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