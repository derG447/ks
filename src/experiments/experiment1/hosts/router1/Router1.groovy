package experiments.experiment1.hosts.router1

import common.utils.Utils

/**
 * Ein IPv4-Router.<br/>
 * Nur als Ausgangspunkt für eigene Implementierung zu verwenden!<br/>
 * Verwendet UDP zur Verteilung der Routinginformationen.
 *
 */
class Router1 {

    //========================================================================================================
    // Vereinbarungen ANFANG
    //========================================================================================================

    /** Der Netzwerk-Protokoll-Stack */
    experiments.experiment1.stack.Stack stack

    /** Konfigurations-Objekt */
    ConfigObject config

    /** Stoppen der Threads wenn false */
    Boolean run = true

    /** Tabelle der IP-Adressen und UDP-Ports der Nachbarrouter */
    /*  z.B. [["1.2.3.4", 11],["5,6,7.8", 20]]
     */
    List<List> neighborTable

    /** Eine Arbeitskopie der Routingtabelle der Netzwerkschicht */
    List<List> routingTable

    /** ob: Routereigne Distanzmatrix zum errechnen der besten Wege */
    List<List> DistanzMatrix = []
    // ob: meine eigene Matrix zum errechnen der Routingwege:
    //
    // | Subnetz      | Kosten (1=eigenes Netz,2=Über einen Router,3=über 2 Router, ....) |  IP Adresse nächster hop | passender Link
    // | 192.168.1.0  | 1                                                                 | my ip adrr.              | lp1


    //========================================================================================================
    // Methoden ANFANG
    //========================================================================================================

    String DistanzMatrixToString(List<List> myDistanzMatrix){
        String tmp = ""
        for (entry in myDistanzMatrix) {
            //tmp = tmp + entry[0].toString() + '|' + entry[1].toString() + '|' + entry[2] + '|' + entry[3] + '#'
            tmp = tmp + "${entry[0]}|${entry[1]}|${entry[2]}|${entry[3]}#"
        }
        return tmp
    }

    List<List> StringToDistanzMatrix(String rInfo){
        List<List> tmp = []
        for (i in rInfo.tokenize('#')){
            tmp.add(i.tokenize('|'))
        }
        return tmp
    }

    void initDistanzMatrix(List<List> routingTable){
        List tmp = []
        for (entry in routingTable){
            tmp.add(entry[0])
            tmp.add("1") //Mein eigenes Netz ist 1
            tmp.add(entry[2])
            tmp.add(entry[3])
            DistanzMatrix.add(tmp)
            tmp = []
        }

    }

    //------------------------------------------------------------------------------
    /**
     * Start der Anwendung
     */
    static void main(String[] args) {
        // Router-Klasse instanziieren
        Router1 application = new Router1()
        // und starten
        application.router()
    }
    //------------------------------------------------------------------------------

    //------------------------------------------------------------------------------
    /**
     * Einfacher IP-v4-Forwarder.<br/>
     * Ist so schon funktiionsfähig, da die Wegewahl im Netzwerkstack erfolgt<br/>
     * Hier wird im Laufe des Versuchs ein Routing-Protokoll implementiert.
     */
    void router() {

        // Konfiguration holen
        config = Utils.getConfig("experiment1", "router1")

        // ------------------------------------------------------------

        // Netzwerkstack initialisieren
        stack = new experiments.experiment1.stack.Stack()
        stack.start(config)

        // ------------------------------------------------------------

        // ob: hier nachbarliste aus config holen:
        neighborTable = config.neighborTable;

        // hier DistanzMatrix initialisieren
        initDistanzMatrix(stack.getRoutingTable())

        // Thread zum Empfang von Routinginformationen erzeugen
        Thread.start{receiveFromNeigbor()}

        // ------------------------------------------------------------

        Utils.writeLog("Router1", "router1", "startet", 1)

        while (run) {
            // Periodisches Versenden von Routinginformationen
            sendPeriodical()
            sleep(config.periodRInfo)
        }
    }

    // ------------------------------------------------------------

    /**
     * Wartet auf Empfang von Routinginformationen
     *
     */
    void receiveFromNeigbor() {
        /** IP-Adresse des Nachbarrouters */
        String iPAddr

        /** UDP-Portnummer des Nachbarrouters */
        int port

        /** Empfangene Routinginformationen */
        String rInfo

        // Auf UDP-Empfang warten
        (iPAddr, port, rInfo) = stack.udpReceive()

        // Jetzt aktuelle Routingtablle holen:
        // rt = stack.getRoutingtable()
        // neue Routinginformationen bestimmen
        //    zum Zerlegen einer Zeichenkette siehe "tokenize()"
        // extrahieren von Information, dann iInfo als !Zeichenkette! erzeugen ...
        // Routingtabelle an Vermittlungsschicht uebergeben:
        // stack.setRoutingtable(rt)
        // und neue Routinginformationen verteilen:
        // rInfo = ...
        // sendToNeigbors(rInfo)
        // oder periodisch verteilen lassen

        List<List> recv_Matrix = StringToDistanzMatrix(rInfo)

        for (entry in recv_Matrix){
            for (i in DistanzMatrix){
                if (entry[0] == i[0]) { // in empfangener Matrix ist ein Eintrag, den wir auch haben
                // überprüfen, ob dies ein anderer Weg zu einem Ziel ist oder ob es ein Weg ist, den wir schon haben

                } else { // ein neuer weg kommt dazu

                }
            }
        }

        // Auf Basis unserer neuen Distanzmatrix muss nun die Routing Tabelle neu berechnet werden



    }

    // ------------------------------------------------------------


    /** Periodisches Senden der Routinginformationen */
    void sendPeriodical() {
        // Paket mit Routinginformationen packen
        // ... z.B.
        // routingTable = stack.getRoutingTable()

        String rInfo = DistanzMatrixToString(DistanzMatrix)

        // extrahieren von Information, dann iInfo als !Zeichenkette! erzeugen ...
        // rInfo = "inf1a, inf1b, ..., inf2a, inf2b, ..."

        // Zum Senden uebergeben
        sendToNeigbors(rInfo)
    }

    // ------------------------------------------------------------

    /** Senden von Routinginformationen an alle Nachbarrouter
     *
     * @param rInfo - Routing-Informationen
     */

    void sendToNeigbors(String rInfo) {
        // rInfo an alle Nachbarrouter versenden
        for (List neigbor in neighborTable) {
            stack.udpSend(dstIpAddr: neigbor[0], dstPort: neigbor[1],
                    srcPort: config.ownPort, sdu: rInfo)
        }
    }
    //------------------------------------------------------------------------------
}

