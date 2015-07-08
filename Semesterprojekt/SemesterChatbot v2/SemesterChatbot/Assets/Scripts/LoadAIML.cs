using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
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
  string querySuffix = "&format=json";


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
		
		// check for errors
		if (request.error == null)
		{
			JSONObject j = new JSONObject(request.text.ToString());
      
      if (fragekategorie == "1"){
          printAbstract(j, input);

      }else if (fragekategorie == "2"){
          List<string> subjectList = new List<string>();
          System.Random rnd = new System.Random();

          printRandomSubject(j, input, subjectList);
          int randomIndex = rnd.Next(0, subjectList.Count);

          //MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + subjectList[randomIndex] + "</color>";
          Request r = new Request(input, this.Testuser, this.Testbot);
          Result res = this.Testbot.Chat(r);
          MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + res.Output.Replace("#unterthema#", subjectList[randomIndex]) + "</color>";
      } else if (fragekategorie == "3"){
          List<string> QuellenListe = new List<string>();
          string prefix;
          string infix = "\n";
          string suffix;

          Request r = new Request(input, this.Testuser, this.Testbot);
          Result res = this.Testbot.Chat(r);

          printQuellen(j, input, QuellenListe);
          if (QuellenListe.Count != 0)
          {
              prefix = "\n" + "<color=#a52a2aff>";
              suffix = "</color>";
              foreach (string quelle in QuellenListe)
              {
                  infix = infix + quelle + "\n";
              }
              MyChatText.text = MyChatText.text + prefix + res.Output.Replace("#quellen#", infix) + suffix;
              
          }
      }
		} else {
        MyChatText.text = MyChatText.text + "\n" + "Tut mir leid, ich habe deine Frage nicht verstanden. Meine Analyse ergab:\n" + request.error;
		}

	}


  void printAbstract(JSONObject obj, string input)
  {
      switch (obj.type)
      {
          case JSONObject.Type.OBJECT:
              for (int i = 0; i < obj.list.Count; i++)
              {
                  JSONObject j = (JSONObject)obj.list[i];

                  if (j.HasField("bindings"))
                  {
                      if (j.GetField("bindings").IsArray)
                      {
                          if (j.GetField("bindings").Count == 0)
                          {
                              activateFailure();
                             
                              Request r = new Request(input, this.Testuser, this.Testbot);
                              Result res = this.Testbot.Chat(r);
                              MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + res.Output + "</color>";

                              deactivateFailure();
                          }
                      }
                  }
                 
                  if (j.HasField("value"))
                  {
                      Request r = new Request(input, this.Testuser, this.Testbot);
                      Result res = this.Testbot.Chat(r);
                      byte[] bytes = Encoding.Default.GetBytes(j.GetField("value").str.Substring(0, j.GetField("value").str.Length - 1));
                      string tmp = Encoding.UTF8.GetString(bytes);
                      //MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + res.Output.Replace("#abstract#", "\n" + j.GetField("value").str.Substring(0, j.GetField("value").str.Length - 1)) + "</color>";
                      MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + res.Output.Replace("#abstract#", "\n" + tmp) + "</color>";                  
                  }

                  printAbstract(j, input);
              }
              break;
          case JSONObject.Type.ARRAY:
              foreach (JSONObject j in obj.list)
              {
                  printAbstract(j, input);
              }
              break;
          default:
              //MyChatText.text = "\n" + MyChatText.text + "NULL";
              break;
      }
  }

  void printRandomSubject(JSONObject obj, string input, List<string> subjectList)
  {
      switch (obj.type)
      {
          case JSONObject.Type.OBJECT:
              for (int i = 0; i < obj.list.Count; i++)
              {
                  JSONObject j = (JSONObject)obj.list[i];

                  if (j.HasField("bindings"))
                  {
                      if (j.GetField("bindings").IsArray)
                      {
                          if (j.GetField("bindings").Count == 0)
                          {
                              activateFailure();

                              Request r = new Request(input, this.Testuser, this.Testbot);
                              Result res = this.Testbot.Chat(r);
                              MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + res.Output + "</color>";

                              deactivateFailure();
                          }
                      }
                  }

                  if (j.HasField("value"))
                  {
                      //Request r = new Request(input, this.Testuser, this.Testbot);
                      //Result res = this.Testbot.Chat(r);
                      //MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + res.Output.Replace("#abstract#", "\n" + j.GetField("value").str.Substring(0, j.GetField("value").str.Length - 1)) + "</color>";
                      subjectList.Add(j.GetField("value").str);
                  }

                  printRandomSubject(j, input, subjectList);
              }
              break;
          case JSONObject.Type.ARRAY:
              foreach (JSONObject j in obj.list)
              {
                  printRandomSubject(j, input, subjectList);
              }
              break;
          default:
              //MyChatText.text = "\n" + MyChatText.text + "NULL";
              break;
      }
  }

  void printQuellen(JSONObject obj, string input, List<string> QuellenListe)
  {
      switch (obj.type)
      {
          case JSONObject.Type.OBJECT:
              for (int i = 0; i < obj.list.Count; i++)
              {
                  JSONObject j = (JSONObject)obj.list[i];

                  if (j.HasField("bindings"))
                  {
                      if (j.GetField("bindings").IsArray)
                      {
                          if (j.GetField("bindings").Count == 0)
                          {
                              activateFailure();

                              Request r = new Request(input, this.Testuser, this.Testbot);
                              Result res = this.Testbot.Chat(r);
                              MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + res.Output + "</color>";

                              deactivateFailure();
                          }
                      }
                  }

                  if (j.HasField("value"))
                  {
                      //Request r = new Request(input, this.Testuser, this.Testbot);
                      //Result res = this.Testbot.Chat(r);
                      //MyChatText.text = MyChatText.text + "\n" + "<color=#a52a2aff>" + res.Output.Replace("#abstract#", "\n" + j.GetField("value").str.Substring(0, j.GetField("value").str.Length - 1)) + "</color>";
                      QuellenListe.Add(j.GetField("value").str);
                  }

                  printQuellen(j, input, QuellenListe);
              }
              break;
          case JSONObject.Type.ARRAY:
              foreach (JSONObject j in obj.list)
              {
                  printQuellen(j, input, QuellenListe);
              }
              break;
          default:
              //MyChatText.text = "\n" + MyChatText.text + "NULL";
              break;
      }
  }



    




    // Gibt ein JSON Objekt aus (vollständig)
	void accessData(JSONObject obj){
		switch (obj.type) {
		case JSONObject.Type.OBJECT:
			for (int i = 0; i < obj.list.Count; i++) {
				string key = (string)obj.keys [i];
				JSONObject j = (JSONObject)obj.list [i];
        MyChatText.text = "\n" + MyChatText.text + key;
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
      MyChatText.text = "\n" + MyChatText.text + obj.str;
			break;
		case JSONObject.Type.NUMBER:
			Debug.Log (obj.n);
      MyChatText.text = "\n" + MyChatText.text + obj.n;
			break;
		case JSONObject.Type.BOOL:
			Debug.Log (obj.b);
      MyChatText.text = "\n" + MyChatText.text + obj.b;
			break;
		case JSONObject.Type.NULL:
			Debug.Log ("NULL");
      MyChatText.text = "\n" + MyChatText.text + "NULL";
			break;
		}
	}

}
