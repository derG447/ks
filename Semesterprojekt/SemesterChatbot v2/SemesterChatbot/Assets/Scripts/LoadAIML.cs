using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using AIMLbot;





public class LoadAIML : MonoBehaviour {
	

	public Button MyEnterButton;
	public Button MyDebugButton;
	public Text MyChatText;
	public Text MyDebugText;
	public InputField MyInputField;
	public InputField MyDebugInputField;


	AIMLbot.Bot Testbot;
	AIMLbot.User Testuser;

	// Use this for initialization
	void Start () {

		MyDebugText.text = "Load Chatbot";

		MyDebugText.text = MyDebugText.text + "\n" + "...create bot and user";
		this.Testbot = new Bot ();
		this.Testuser = new User ("1", Testbot);

		MyDebugText.text = MyDebugText.text + "\n" + "...load setting files";
		this.Testbot.loadSettings();

		MyDebugText.text = MyDebugText.text + "\n" + "...load AIML files";
		this.Testbot.isAcceptingUserInput = false;
		this.Testbot.loadAIMLFromFiles();
		this.Testbot.isAcceptingUserInput = true;


		MyDebugText.text = MyDebugText.text + "\n" + "...add button listener";
		MyDebugButton.onClick.AddListener (() => {DebugClick ();});
		MyEnterButton.onClick.AddListener (() => {EnterClick ();});

		MyDebugText.text = MyDebugText.text + "\n" + "Load Chatbot Successfully";

		MyChatText.supportRichText = true;
		MyChatText.text = "<color=#a52a2aff>" + "Hello. My name is chatbot. How are you?" + "</color>";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void EnterClick (){
		string input = MyInputField.text;
		MyChatText.text = MyChatText.text + "\n" + "<color=#0000a0ff>" + input + "</color>";

		Request r = new Request(MyInputField.text, this.Testuser, this.Testbot);
		Result res = this.Testbot.Chat(r);
		MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + res.Output + "</color>";
	}

	void DebugClick (){
		string input = MyDebugInputField.text;
     	//string url = "http://de.dbpedia.org/data/" + input + ".json";
		string url = "http://dbpedia.org/sparql?query=PREFIX+dbp%3A+<http%3A%2F%2Fdbpedia.org%2Fresource%2F>%0D%0APREFIX+dbp2%3A+<http%3A%2F%2Fdbpedia.org%2Fontology%2F>%0D%0A+%0D%0ASELECT+%3Fabstract%0D%0AWHERE+{%0D%0A+++++dbp%3ALondon+dbp2%3Aabstract+%3Fabstract+.+%0D%0A+++++FILTER+langMatches(lang(%3Fabstract)%2C+'en')%0D%0A}%0D%0A&format=json";
		//string url = "http://dbpedia.org/sparql?default-graph-uri=http%3A%2F%2Fdbpedia.org&query=select*%7Bdbpedia%3A" + input + "+rdfs%3Aabstract+%3Fabstract%7D&format=json";
		//string url = "http://www.wikidata.org/w/api.php?action=wbgetentities&sites=itwiki&titles=Pizza&format=json";
		// Start a download of the given URL
		WWW request = new WWW (url);
		// Wait for download to complete
		StartCoroutine(WaitForRequest(request));
	}

	IEnumerator WaitForRequest(WWW request)
	{
		yield return request;
		
		// check for errors
		if (request.error == null)
		{
			//MyDebugText.text = "WWW Ok!: " + request.text;
			MyDebugText.text = "";
			JSONObject j = new JSONObject(request.text);
			accessData(j);
			//access data (and print it)
		} else {
			MyDebugText.text = "WWW Error: "+ request.error;
		}    
	}
	
	void accessData(JSONObject obj){
		switch (obj.type) {
		case JSONObject.Type.OBJECT:
			for (int i = 0; i < obj.list.Count; i++) {
				string key = (string)obj.keys [i];
				JSONObject j = (JSONObject)obj.list [i];
				//Debug.Log(key);
				MyDebugText.text = MyDebugText.text + key;
				accessData (j);
			}
			break;
		case JSONObject.Type.ARRAY:
			foreach (JSONObject j in obj.list) {
				accessData (j);
			}
			break;
		case JSONObject.Type.STRING:
			Debug.Log (obj.str);
			MyDebugText.text = MyDebugText.text + obj.str;
			break;
		case JSONObject.Type.NUMBER:
			Debug.Log (obj.n);
			MyDebugText.text = MyDebugText.text + obj.n;
			break;
		case JSONObject.Type.BOOL:
			Debug.Log (obj.b);
			MyDebugText.text = MyDebugText.text + obj.b;
			break;
		case JSONObject.Type.NULL:
			Debug.Log ("NULL");
			MyDebugText.text = MyDebugText.text + "NULL";
			break;
			
		}
	}
}
