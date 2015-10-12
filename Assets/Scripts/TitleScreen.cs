using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour
{
	public int RightMargin;
	public int BottomMargin;
	public int StartButtonWidth;
	public int StartButtonHeight;
	private Rect startButtonRect;
	public string StartButtonSceneToLoad;

	public Texture StartButtonNormal;
	public Texture StartButtonHover;
	public Texture StartButtonActive;

	void Start ()
	{
		this.startButtonRect = new Rect(Screen.width - this.RightMargin - this.StartButtonWidth,
			                            Screen.height - this.BottomMargin - this.StartButtonHeight,
			                            this.StartButtonWidth,
			                            this.StartButtonHeight);
	}

	void OnGUI ()
	{
		GUIStyle buttonStyle = new GUIStyle("button");
		buttonStyle.normal.background = (Texture2D)this.StartButtonNormal;
		buttonStyle.hover.background = (Texture2D)this.StartButtonHover;
		buttonStyle.active.background = (Texture2D)this.StartButtonActive;
		if (GUI.Button (this.startButtonRect, "Start Game", buttonStyle))
		{
			Application.LoadLevel (this.StartButtonSceneToLoad);
		}
	}
}
