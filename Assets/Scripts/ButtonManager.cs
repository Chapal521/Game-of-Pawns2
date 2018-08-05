using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour {
	public Animator anim;
	public Text TPM;
	public Text win;
	public Text lose;
	public Text draw;

	public void Play(){
		SceneManager.LoadScene (1);
	}
	public void Quit()
	{
		Application.Quit ();
	}
	void Start()
	{
		anim.Play ("GameName");

//		PlayerPrefs.SetInt ("MatchWin",0);
//		PlayerPrefs.SetInt ("TotalMatch",0);
//		PlayerPrefs.SetInt ("Lose", 0);
//		PlayerPrefs.SetInt ("Draw", 0);
		TPM.text ="Match Played : " + PlayerPrefs.GetInt("TotalMatch").ToString();
		win.text = "Win : " + PlayerPrefs.GetInt ("MatchWin").ToString();
		lose.text = "Lose : " + PlayerPrefs.GetInt ("Lose").ToString ();//(PlayerPrefs.GetInt("TotalMatch") - (PlayerPrefs.GetInt("MatchWin") +PlayerPrefs.GetInt ("Draw"))).ToString();
		draw.text = "Draw : " + (PlayerPrefs.GetInt ("Draw")).ToString();

	}
}
