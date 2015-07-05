using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using System ;
using System.IO;

public class Evaluation : MonoBehaviour {
	
	public Button MyEnterButton;
	public Text MyChatText;
	public InputField MyInputField;
	int i;
	string frage;
	string path,path2;
	//string dire;
	
	// Use this for initialization
	void Start () {
		i = 0;
		MyEnterButton.onClick.AddListener (() => {EnterClick ();});
		
		path = Directory.GetCurrentDirectory();
		path2 = path + "\\Evaluation.txt";
		System.IO.File.WriteAllText(path2, "");
		
	}
	
	void EnterClick (){
		
		
		switch (i)
		{
		case (0):
			MyChatText.supportRichText = true;
			MyChatText.text = "Evaluation:";
			//MyChatText.text = path2;
			MyChatText.text = MyChatText.text + "\n" + "Bitte geben sie ihre Antwort in das Chatfeld ein und drücken sie erneut auf 'Evaluieren' um Ihre Antwort abzuschicken." ;
			frage = "Wie hat der Chatbot ihnen gefallen?";
			MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + frage + "</color>";
			break;
		case (1):
			MyChatText.text = MyChatText.text + "\n" + "<color=#0000a0ff>" + MyInputField.text + "</color>";
			string text = frage + " : " + MyInputField.text + "\n";
			//Frage + Antwort in Datei schreiben
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path2 , true))
			{
				file.WriteLine(text);
			}
			frage = "Wurde ein Thema gefunden?";
			MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + frage + "</color>";
			break;
		case (2):
			MyChatText.text = MyChatText.text + "\n" + "<color=#0000a0ff>" + MyInputField.text + "</color>";
			text = frage + " : " + MyInputField.text + "\n";
			//Frage + Antwort in Datei schreiben
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path2, true))
			{
				file.WriteLine(text);
			}
			frage = "Wurden Informationen zum Thema gefunden?";
			MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + frage + "</color>";
			break;
		case (3):
			MyChatText.text = MyChatText.text + "\n" + "<color=#0000a0ff>" + MyInputField.text + "</color>";
			text = frage + " : " + MyInputField.text + "\n";
			//Frage + Antwort in Datei schreiben
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path2, true))
			{
				file.WriteLine(text);
			}
			frage = "Waren die Antworten des Bot sinnvoll?";
			MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + frage + "</color>";
			break;
		case (4):
			MyChatText.text = MyChatText.text + "\n" + "<color=#0000a0ff>" + MyInputField.text + "</color>";
			text = frage + " : " + MyInputField.text + "\n";
			//Frage + Antwort in Datei schreiben
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path2, true))
			{
				file.WriteLine(text);
			}
			frage = "Hat dich das Gespräch mit dem Bot weitergebracht?";
			MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + frage + "</color>";
			break;
		case (5):
			MyChatText.text = MyChatText.text + "\n" + "<color=#0000a0ff>" + MyInputField.text + "</color>";
			text = frage + " : " + MyInputField.text + "\n";
			//Frage + Antwort in Datei schreiben
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path2, true))
			{
				file.WriteLine(text);
			}
			frage = "Nach was für Themengebieten wurde gesucht?";
			MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + frage + "</color>";
			break;
		case (6):
			MyChatText.text = MyChatText.text + "\n" + "<color=#0000a0ff>" + MyInputField.text + "</color>";
			text = frage + " : " + MyInputField.text + "\n";
			//Frage + Antwort in Datei schreiben
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path2, true))
			{
				file.WriteLine(text);
			}
			frage = "Was haben sie für Verbesserungsvorschläge?";
			MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + frage + "</color>";
			break;
		case (7):
			MyChatText.text = MyChatText.text + "\n" + "<color=#0000a0ff>" + MyInputField.text + "</color>";
			text = frage + " : " + MyInputField.text + "\n";
			//Frage + Antwort in Datei schreiben
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path2, true))
			{
				file.WriteLine(text);
			}
			frage = "Alles in allem Bewerte ich den Bot mit der Schulnote:";
			MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + frage + "</color>";
			break;
		case (8):
			MyChatText.text = MyChatText.text + "\n" + "<color=#0000a0ff>" + MyInputField.text + "</color>";
			//Frage + Antwort in Datei schreiben
			text = frage + " : " + MyInputField.text + "\n";
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(@path2, true))
			{
				file.WriteLine(text+"\n");
			}
			MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + "Danke für das Feedback" + "</color>";
			i = -1;
			break;
		default:
			//
			break;
		}
		i = i + 1;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
