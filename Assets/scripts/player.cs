using UnityEngine;
using System.Collections;

public class player : MonoBehaviour {
	public int max_hp = 100;
	public int hp = 100;
	public int hp_speed = 1;
	public int attack = 5;
	public GameObject target;
	public Vector2 bloodSize = new Vector2(100,20);
	public Texture blood_red;
	public Texture blood_black;
	int curhp = 100;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(curhp > hp)
		{
			curhp -= hp_speed;
		}
		
	}
	
	IEnumerator PlayAnim(int AnimID)
	{
		Animator anim = GetComponent<Animator>();
		anim.SetInteger("Action",AnimID);
		//if(AnimID != 2)
		{
			yield return new WaitForSeconds(0.2f);
			anim.SetInteger("Action",0);
		}
	}
	
	public void OnGameStart()
	{
		curhp = hp = max_hp;
		StartCoroutine(PlayAnim(5));		
	}
	
	public void OnAttack(JewelType type,int num)
	{
		StartCoroutine(PlayAnim(1));
		player p = target.GetComponent<player>();
		p.hp -= num * attack;
		if(p.hp <= 0)
		{
			p.hp = 0;
			p.OnDie();//game over
		}
		else
			p.OnBeAttacked();//game over
			
	}
	public void OnBeAttacked()
	{
		StartCoroutine(PlayAnim(3));
	}
	
	public void OnDie()
	{
		StartCoroutine(PlayAnim(2));
		game g = GameObject.Find("cube").GetComponent<game>();
		g.OnPlayerDie(this);
	}
	
	void OnGUI()
	{
		Vector3 worldPosition = new Vector3 (transform.position.x , transform.position.y,transform.position.z);

		Vector2 position = Camera.main.WorldToScreenPoint (worldPosition);
		position = new Vector2 (position.x, position.y);
				
		float blood_width = bloodSize.x * curhp/100;
		GUI.DrawTexture(new Rect(position.x - (bloodSize.x/2),position.y - bloodSize.y ,bloodSize.x,bloodSize.y),blood_black);
		GUI.DrawTexture(new Rect(position.x - (bloodSize.x/2),position.y - bloodSize.y ,blood_width,bloodSize.y),blood_red);	
	}
}
