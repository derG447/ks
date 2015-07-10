using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AIMLbot;





public class LoadAIML : MonoBehaviour {
	

	public Button MyEnterButton;
	public Text MyChatText;
	public InputField MyInputField;

  public GameObject ChatWindow;
  private ScrollRect ChatWindowScrollRect;

  private AIMLbot.Bot henry;
  private AIMLbot.Bot query_builder;
  private AIMLbot.User essayschreiber;
  private AIMLbot.User query_user;

  //string queryPrefix = "http://de.dbpedia.org/sparql?query=";
  private string queryPrefix = "http://dbpedia.org/sparql?query=";
  private string querySuffix = "&format=csv";

  private StreamWriter log;
  private string logpath;

  void addToLog(string line)
  {
      log = File.AppendText(logpath);
      log.WriteLine(line);
      log.Close();
  }

	void Start () {

    MyChatText.text = "Load Chatbot: ";

    this.henry = new Bot();
    this.essayschreiber = new User("henry", henry);

    this.query_builder = new Bot();
    this.query_user = new User("query", query_builder);

    this.henry.loadSettings();
    this.query_builder.loadSettings();

    this.henry.isAcceptingUserInput = false;
    this.henry.loadAIMLFromFiles();
    this.henry.isAcceptingUserInput = true;
    this.query_builder.isAcceptingUserInput = false;
    this.query_builder.loadAIMLFromFiles();
    this.query_builder.isAcceptingUserInput = true;

    MyChatText.text = MyChatText.text + "Config geladen, ";

    logpath = Directory.GetCurrentDirectory() + "\\log.txt";
    addToLog("Start session at " + DateTime.Now.ToString());

    MyChatText.text = MyChatText.text + "Log angelegt, ";

		MyEnterButton.onClick.AddListener (() => {EnterClick ();});

    this.ChatWindowScrollRect = this.ChatWindow.GetComponent<ScrollRect>();
    this.ChatWindowScrollRect.verticalNormalizedPosition = 0;

    MyChatText.text = MyChatText.text + "Events geladen, ";

    // Zusätzliche aiml files für Henry laden:
    Request req_for_henry = new Request("load henry", this.essayschreiber, this.henry);
    Result res_from_henry = this.henry.Chat(req_for_henry);

    req_for_henry = new Request("load fragekategorie 1", this.essayschreiber, this.henry);
    res_from_henry = this.henry.Chat(req_for_henry);

    req_for_henry = new Request("load fragekategorie 2", this.essayschreiber, this.henry);
    res_from_henry = this.henry.Chat(req_for_henry);

    req_for_henry = new Request("load fragekategorie 3", this.essayschreiber, this.henry);
    res_from_henry = this.henry.Chat(req_for_henry);

    req_for_henry = new Request("load allgemein", this.essayschreiber, this.henry);
    res_from_henry = this.henry.Chat(req_for_henry);

    // Zusätzliche aiml files für Querys laden:
    Request req_for_query = new Request("load query", this.query_user, this.query_builder);
    Result res_from_query = this.query_builder.Chat(req_for_query);

    req_for_query = new Request("load fragekategorie 1", this.query_user, this.query_builder);
    res_from_query = this.query_builder.Chat(req_for_query);

    req_for_query = new Request("load fragekategorie 2", this.query_user, this.query_builder);
    res_from_query = this.query_builder.Chat(req_for_query);

    req_for_query = new Request("load fragekategorie 3", this.query_user, this.query_builder);
    res_from_query = this.query_builder.Chat(req_for_query);

    MyChatText.text = MyChatText.text + "AIML geladen.\n";

		MyChatText.supportRichText = true;
		MyChatText.text = MyChatText.text + "<color=#a52a2aff>" + "Hallo. Ich bin Henry, der Essayhelfer. Wie kann ich dir helfen?" + "</color>" + "\n";
    addToLog("Hallo. Ich bin Henry, der Essayhelfer. Wie kann ich dir helfen?");
	}

	void EnterClick (){
		string input = MyInputField.text;
    MyChatText.text = MyChatText.text + "<color=#0000a0ff>" + input + "</color>" + "\n";
    addToLog("user: " + input);
    this.ChatWindowScrollRect.verticalNormalizedPosition = 0;

    Request queryRequest = new Request(input, this.query_user, this.query_builder);
    Result query = this.query_builder.Chat(queryRequest);
    string[] words = query.Output.Split('#');//in words[0] steht die Fragekategorie, in words[1] der rest
    
    if(words[0] != "query"){
        // normales Gespräch mit dem Chatbot
        Request r = new Request(input, this.essayschreiber, this.henry);
        Result res = this.henry.Chat(r);
        MyChatText.text = MyChatText.text + "<color=#a52a2aff>" + res.Output + "</color>" + "\n";
        addToLog("henry: " + res.Output);
        this.ChatWindowScrollRect.verticalNormalizedPosition = 0;
    } else {
        string queryInfix = words[2].Substring(0, words[2].Length - 1); // Den Punkt am Ende entfernen
        string finalQuery = this.queryPrefix + queryInfix + this.querySuffix;
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

        req_for_henry = new Request(input, this.essayschreiber, this.henry);
        res_from_henry = this.henry.Chat(req_for_henry);

        if (fragekategorie == "1")
        {
            keyword = "#abstract#";
            infixString = infixString.Substring(0, infixString.Length-1);
            infixString = res_from_henry.Output.Replace(keyword, "\n" + infixString);
            MyChatText.text = MyChatText.text + prefix + infixString + suffix + "\n";
            addToLog("henry: " + infixString);
        } 
        else if (fragekategorie == "2")
        {
            keyword = "#unterthema#";
            infixString = res_from_henry.Output.Replace(keyword, infixList);
            MyChatText.text = MyChatText.text + prefix + infixString + suffix + "\n";
            addToLog("henry: " + infixString);
        }
        else if (fragekategorie == "3")
        {
            keyword = "#quellen#";
            infixString = res_from_henry.Output.Replace(keyword, infixList);
            MyChatText.text = MyChatText.text + prefix + infixString + suffix + "\n";
            addToLog("henry: " + infixString);
        } 
    }
    else
    {
        string fehlerprefix = "_fehler ";

        req_for_henry = new Request(fehlerprefix + input, this.essayschreiber, this.henry);
        res_from_henry = this.henry.Chat(req_for_henry);

        infixString = res_from_henry.Output;
        MyChatText.text = MyChatText.text + prefix + infixString + suffix + "\n";
        addToLog("henry / fehler: " + infixString);
        //MyChatText.text = MyChatText.text + "\n" + "HTTP Error gefunden. Meine Analyse ergab:\n" + request.error;
    }
    this.ChatWindowScrollRect.verticalNormalizedPosition = 0;
	}

  void Update()
  {
      if (Input.GetButtonDown("Submit"))
      {
          this.EnterClick();
          this.ChatWindowScrollRect.verticalNormalizedPosition = 0;
      }
  }
}
