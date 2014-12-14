package experiments.experiment1.hosts.nameserver

import common.utils.Utils

import java.util.regex.Matcher

/**
 * Ein Server der Gerätenamen in IPv4-Adressen auflöst. Als Transport-Protokoll wird UDP verwendet.
 */
class NameServer {

    //========================================================================================================
    // Vereinbarungen ANFANG
    //========================================================================================================

    /** Der Netzwerk-Protokoll-Stack */
    experiments.experiment1.stack.Stack stack

    /** Konfigurations-Objekt */
    ConfigObject config

    /** Stoppen der Threads wenn false */
    Boolean run = true

    //Flow
    /** Der im HTTP-Request gelieferte Name des angeforderten Objekts*/
    String name = ""

    /** Ein Matcher-Objekt zur Verwendung regulärer Ausdruecke */
    Matcher matcher

    /** IP-Adresse und Portnummer des client */
    String srcIpAddr
    int srcPort

    /** Anwendungsprotokolldaten als String */
    String data

    /** Eigene Portnummer */
    int ownPort

    /** Länge der gesendeten Daten */
    int dataLength = 0

    GString reply1 =
            """\
HTTP/1.1 200 OK
Content-Length: ${->dataLength}
Content-Type: text/plain

"""
    //Flow ende

    /** Tabelle zur Umsetzung von Namen in IP-Adressen */
    Map<String, String> nameTable = [
            "meinhttpserver": "192.168.2.10",//TBC
            "alice": "0.0.0.0",
            "bob": "0.0.0.0",
    ]

    //========================================================================================================
    // Methoden ANFANG
    //========================================================================================================

    //------------------------------------------------------------------------------
    /**
     * Start der Anwendung
     */
    static void main(String[] args) {
        // Client-Klasse instanziieren
        NameServer application = new NameServer()
        // und starten
        application.nameserver()
    }
    //------------------------------------------------------------------------------

    /**
     * Der Namens-Dienst
     */
    void nameserver() {

        //------------------------------------------------

        // Konfiguration holen
        config = Utils.getConfig("experiment1", "nameserver")

        // ------------------------------------------------------------

        // Netzwerkstack initialisieren
        stack = new experiments.experiment1.stack.Stack()
        stack.start(config)
        //flow
        ownPort = config.ownPort
        //flow ende
        Utils.writeLog("NameServer", "nameserver", "startet", 1)

        while (run) {
            // Hier Protokoll implementieren:
            //Flow
            // auf Empfang ueber UDP warten
            (srcIpAddr, srcPort, data) = stack.udpReceive()

            Utils.writeLog("Nameserver", "Nameserver", "empfängt: $data", 1)

            // Abbruch wenn Länge der empfangenen Daten == 0
            if (!data)
                break

            // Parsen des HTTP-Kommandos
            matcher = (data =~ /GET\s*\/(.*?)\s*HTTP\/1\.1/)

            name = ""

            // Wurde das Header-Feld gefunden?
            if (matcher) {
                // Ja
                // Name des zu liefernden Objekts
                name = (matcher[0] as List<String>)[1]


                String reply = ""

                // Namen über nameTable in IP-Adresse aufloesen

                String temp = nameTable.getAt(name)


                //Länge bestimmen
                //FORMATIERUNG BEIBEHALTEN!!!!!!
                if(!temp){
                    temp = "ERROR"
                    reply1 =
                            """\
HTTP/1.1 404 Not Found
Content-Length: ${->dataLength}
Content-Type: text/plain

"""
                }
                //Ab hier FORMATIERUNG wieder frei
                dataLength = temp.size()
                reply = reply1 + temp

                Utils.writeLog("Nameserver", "Nameserver", "sendet: $reply", 11)

                // IP-Adresse ueber UDP zuruecksenden
                stack.udpSend(dstIpAddr: srcIpAddr, dstPort: srcPort,
                        srcPort: ownPort, sdu: reply)

            }

        }
    }
    //------------------------------------------------------------------------------
}
