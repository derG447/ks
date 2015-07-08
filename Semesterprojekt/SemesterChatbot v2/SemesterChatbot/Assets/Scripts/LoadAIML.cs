using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIMLbot;





public class LoadAIML : MonoBehaviour {
	

	public Button MyEnterButton;
	public Text MyChatText;
	public InputField MyInputField;


	AIMLbot.Bot Testbot;
  AIMLbot.Bot Querybot;
	AIMLbot.User Testuser;
  AIMLbot.User Queryuser;

  //string queryPrefix = "http://de.dbpedia.org/sparql?query=";
  string queryPrefix = "http://dbpedia.org/sparql?query=";
  string querySuffix = "&format=csv";


  void activateFailure() {
      Request r = new Request("activate failure", this.Testuser, this.Testbot);
      Result res = this.Testbot.Chat(r);
  }

  void deactivateFailure()
  {
      Request r = new Request("deactivate failure", this.Testuser, this.Testbot);
      Result res = this.Testbot.Chat(r);
  }


	void Start () {

    MyChatText.text = "Load Chatbot: ";

		this.Testbot = new Bot ();
		this.Testuser = new User ("1", Testbot);

    this.Querybot = new Bot();
    this.Queryuser = new User("2", Querybot);

		this.Testbot.loadSettings();
    this.Querybot.loadSettings();

		this.Testbot.isAcceptingUserInput = false;
		this.Testbot.loadAIMLFromFiles();
		this.Testbot.isAcceptingUserInput = true;
    this.Querybot.isAcceptingUserInput = false;
    this.Querybot.loadAIMLFromFiles();
    this.Querybot.isAcceptingUserInput = true;

    MyChatText.text = MyChatText.text + "AIML geladen, ";

		MyEnterButton.onClick.AddListener (() => {EnterClick ();});

    MyChatText.text = MyChatText.text + "Event Handler geladen.";

    // den Querybot auf das Thema <query> setzen:
    Request Request = new Request("activate query", this.Queryuser, this.Querybot);
    Result query = this.Querybot.Chat(Request); 

		MyChatText.supportRichText = true;
		MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + "Hallo. Ich bin Henry, der Essayhelfer. Wie kann ich dir helfen?" + "</color>";
	}

	void EnterClick (){
		string input = MyInputField.text;
		MyChatText.text = MyChatText.text + "\n" + "<color=#0000a0ff>" + input + "</color>";

    Request queryRequest = new Request(MyInputField.text, this.Queryuser, this.Querybot);
    Result query = this.Querybot.Chat(queryRequest);
    string[] words = query.Output.Split('#');//in words[0] steht die Fragekategorie, in words[1] der rest
    
    if(words[0] != "query"){
        // normales Gespräch mit dem Chatbot
        Request r = new Request(input, this.Testuser, this.Testbot);
        Result res = this.Testbot.Chat(r);
        MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + res.Output + "</color>";
    } else {
        string queryInfix = words[2].Substring(0, words[2].Length - 1); // Den Punkt am Ende entfernen
        string finalQuery = queryPrefix + queryInfix + querySuffix;
        //MyChatText.text = MyChatText.text + "\n" + "<color=#0000a0ff>" + "Die Anfrage ist:\n" + finalQuery + "</color>";
        WWW SPARQLrequest = new WWW(finalQuery);
        StartCoroutine(WaitForRequest(SPARQLrequest, input, words[1]));
    }
	}

	IEnumerator WaitForRequest(WWW request, string input, string fragekategorie)
	{
		yield return request;

    Request req_for_henry;
    Result res_from_henry;

    string[] answerLines = request.text.ToString().Split('\n');
    string infixList = "";
    string infixString = "";

    string prefix = "<color=#a52a2aff>";
    string suffix = "</color>";
    string keyword;

    if (request.error == null && answerLines.Length > 1 && answerLines[1] != "")
    {
        infixString = answerLines[1].Substring(1, answerLines[1].Length - 2); // hat bei abstract noch einen Punkt am Ende
        foreach (string line in answerLines.Skip(1).TakeWhile(x => x.Length > 0))
        {
            infixList = infixList + "\n" + line.Substring(1, line.Length - 2);
        }

        req_for_henry = new Request(input, this.Testuser, this.Testbot);
        res_from_henry = this.Testbot.Chat(req_for_henry);

        if (fragekategorie == "1")
        {
            keyword = "#abstract#";
            infixString = infixString.Substring(0, infixString.Length-1);
            infixString = res_from_henry.Output.Replace(keyword, "\n" + infixString);
            MyChatText.text = MyChatText.text + "\n" + prefix + infixString + suffix;
        } 
        else if (fragekategorie == "2")
        {
            keyword = "#unterthema#";
            infixString = res_from_henry.Output.Replace(keyword, "\n" + infixList);
            MyChatText.text = MyChatText.text + "\n" + prefix + infixString + suffix;
        }
        else if (fragekategorie == "3")
        {
            keyword = "#quellen#";
            infixString = res_from_henry.Output.Replace(keyword, "\n" + infixList);
            MyChatText.text = MyChatText.text + "\n" + prefix + infixString + suffix;
        }

    }
    else
    {
        activateFailure();

        req_for_henry = new Request(input, this.Testuser, this.Testbot);
        res_from_henry = this.Testbot.Chat(req_for_henry);

        infixString = res_from_henry.Output;
        MyChatText.text = MyChatText.text + "\n" + prefix + infixString + suffix;
        //MyChatText.text = MyChatText.text + "\n" + "HTTP Error gefunden. Meine Analyse ergab:\n" + request.error;

        deactivateFailure();
    }
	}
}
