package experiments.experiment1.hosts.router3

import common.utils.Utils

/**
 * Ein IPv4-Router.<br/>
 * Nur als Ausgangspunkt für eigene Implementierung zu verwenden!<br/>
 * Verwendet UDP zur Verteilung der Routinginformationen.
 *
 */
class Router3 {

    //========================================================================================================
    // Vereinbarungen ANFANG
    //========================================================================================================

    /** Der Name des Routers für die Consolenausgabe */
    final String routername_display = "Router3"

    /** Der Name des Routers */
    final String routername = "router3"

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

    /** Gruppe: Enthält zu allen Nachbarn aus der neighborTable, wie oft ich ihnen Infosgesendet habe,
     *  ohne etwas von ihnen bekommen zu haben */
    List<List> neighborAvaiableTable = []

    /** Die Anzahl der Ticks, bis ein Nachbar als Ausfall gilt */
    final int routerTimeout = 6

    /** Gruppe: Enthält alle eigenen IP Adressen, wird aus der config ausgelesen */
    List myIPtable

    /** Eine Arbeitskopie der Routingtabelle der Netzwerkschicht */
    List<List> routingTable

    /** Gruppe: Routereigene Distanzmatrix zum errechnen der besten Wege */
    List<List> DistanzMatrix = []
    // Aufbau:
    // | Subnetz      | Subnetzmaske  | Kosten  |  IP Adresse nächster hop | passender Link (kann leer sein)
    // | 192.168.1.0  | 255.255.255.0 | 1       | my ip adrr.              | lp1
    // wobei Kosten 1=eigenes Netz,2=Über einen Router,3=über 2 Router, ....


    //========================================================================================================
    // Methoden ANFANG
    //========================================================================================================

    //------------------------------------------------------------------------------
    /**
     * Gruppe: die folgenden 4 Methoden sind Hilfsfunktionen für das Routing
     */
    String DistanzMatrixToString(List<List> myDistanzMatrix){
        String tmp = ""
        for (entry in myDistanzMatrix) {
            tmp = tmp + "${entry[0]}|${entry[1]}|${entry[2]}|${entry[3]}|${entry[4]}#"
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
            tmp.add(entry[1])
            tmp.add("1") //Mein eigenes Netz ist 1
            tmp.add(entry[2])
            tmp.add(entry[3])
            DistanzMatrix.add(tmp)
            tmp = []
        }

    }

    boolean isPathtoMe(String recv_ip){
        myIPtable = myIPtable
        for (i in myIPtable){
            String myip = i.ipAddr
            if (recv_ip == myip){
                return true
            }
        }
        return false
    }
    //------------------------------------------------------------------------------

    //------------------------------------------------------------------------------
    /**
     * Start der Anwendung
     */
    static void main(String[] args) {
        // Router-Klasse instanziieren
        Router3 application = new Router3()
        // und starten
        application.router()
    }
    //------------------------------------------------------------------------------

    //------------------------------------------------------------------------------
    /**
     * Einfacher IP-v4-Forwarder.<br/>
     * Ist so schon funktionsfähig, da die Wegewahl im Netzwerkstack erfolgt<br/>
     * Hier wird im Laufe des Versuchs ein Routing-Protokoll implementiert.
     */
    void router() {

        // Konfiguration holen
        config = Utils.getConfig("experiment1", routername)
        //config.networkConnectors[0].
        // ------------------------------------------------------------

        // Netzwerkstack initialisieren
        stack = new experiments.experiment1.stack.Stack()
        stack.start(config)

        // ------------------------------------------------------------

        // Gruppe: hier neighborTable aus config bekommen, hatte in der Vorlage gefehlt
        neighborTable = config.neighborTable

        // Hier wird die Avaiable Tabelle initialisiert
        neighborAvaiableTable = neighborTable
        for (entry in neighborAvaiableTable){
            entry.add(routerTimeout)
        }

        // Gruppe: hier die Liste der network connectors bekommen, eigentlich werden nur die IP Adressen benötigt
        myIPtable = config.networkConnectors

        // Gruppe: hier DistanzMatrix initialisieren
        initDistanzMatrix(stack.getRoutingTable())

        // Thread zum Empfang von Routinginformationen erzeugen
        Thread.start{receiveFromNeigbor()}

        // ------------------------------------------------------------

        Utils.writeLog(routername_display, routername, "startet", 1)
        sleep(15000)
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

        while(true) {
            // Auf UDP-Empfang warten
            (iPAddr, port, rInfo) = stack.udpReceive()

            // den Avaiable Counter um eins runtersetzen
            for (entry in neighborAvaiableTable){
                if (entry[0] == iPAddr){
                    if (entry[3] == 0){
                        Utils.writeLog(routername_display, "send", "${entry[0]} wieder da", 11)
                    }
                    entry[3] = routerTimeout
                }
            }

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
            List<List> tmp_DistanzMatrix = DistanzMatrix.clone()

            Utils.writeLog(routername_display, "receive", "Informationen von Nachbar ${iPAddr} angekommen. Größe seiner Matrix: ${recv_Matrix.size()}", 1)

            boolean unknownSubnetz = true;
            for (List recv_entry in recv_Matrix) {
                if (isPathtoMe(recv_entry[3])) {
                    // Gruppe: this is a subnet already reachable over myself
                } else {
                    for (List my_entry in tmp_DistanzMatrix) {
                        if ((recv_entry[0] == my_entry[0]) && (recv_entry[1] == my_entry[1])) {
                            unknownSubnetz = false

                            // Gruppe: Habe schon einen Eintrag zu diesem Subnetz, ist es ein anderer pfad?
                            if (my_entry[3] == iPAddr) {
                                // Gruppe: Nein, ist derselbe Pfad, aktualisiere nur die Kosten
                                Utils.writeLog(routername_display, "receive", "Kosten aktualisiert für schon bekanntes Subnetz ${recv_entry[0]}, Neu: ${((recv_entry[2] as int) + 1)} Alt: ${my_entry[2]}", 1)
                                my_entry[2] = ((recv_entry[2] as int) + 1) as String
                            } else {
                                // Gruppe: Ja, es ist ein anderer pfad, füge ihn hinzu ..
                                if (((my_entry[2] as int) <= ((recv_entry[2] as int) + 1)) && ((my_entry[2] as int) != 0)){
                                    // .. ausser er läuft in mein direkt angeschlossenes Netz, dann nicht
                                    Utils.writeLog(routername_display, "receive", "Zweiter Weg zu einem Subnetz verworfen, ich habe einen besseren", 1)
                                } else {
                                    // da er besser ist, übernehme ich ihn und ersetze den anderen damit
                                    my_entry[2] = ((recv_entry[2] as int) + 1) as String
                                    my_entry[3] = iPAddr
                                    my_entry[4] = (neighborTable.find { entry -> iPAddr == entry[0]})[2]



                                    /*
                                    Utils.writeLog(routername_display, "receive", "Neuer Eintrag für schon bekanntes Subnetz ${recv_entry[0]}", 1)
                                    List tmp_entry = recv_entry
                                    tmp_entry[2] = ((tmp_entry[2] as int) + 1) as String
                                    tmp_entry[3] = iPAddr
                                    tmp_entry[4] = (neighborTable.find { entry -> iPAddr == entry[0]})[2]
                                    DistanzMatrix.add(tmp_entry)*/
                                }
                            }
                        } else {
                        }
                    }
                    // Gruppe: Subnetz noch ganz unbekannt?
                    if (unknownSubnetz) {
                        Utils.writeLog(routername_display, "receive", "Neuer Eintrag für unbekanntes Subnetz ${recv_entry[0]}", 1)
                        List tmp_entry = recv_entry
                        tmp_entry[2] = ((recv_entry[2] as int) + 1) as String
                        tmp_entry[3] = iPAddr
                        tmp_entry[4] = (neighborTable.find { entry -> iPAddr == entry[0]})[2]
                        DistanzMatrix.add(tmp_entry)
                    }
                    unknownSubnetz = true
                }
            }

            // Gruppe: Auf Basis unserer neuen Distanzmatrix muss nun die Routing Tabelle neu berechnet werden

            List<List> tmp_routingTable = []

            for (List my_entry in DistanzMatrix) {
                // Gruppe: Gehe nun die ganze Matrix durch und suche für jedes Subnetz den kürzesten Weg
                List best_candidate = my_entry
                for (List my_entry_2 in DistanzMatrix) {
                    if((best_candidate[0] == my_entry_2[0]) && (best_candidate[1] == my_entry_2[1])){
                        if((best_candidate[2] as int) <= (my_entry_2[2] as int)){
                            // Der kürzere Weg ist schon ausgewählt
                        } else {
                            best_candidate = my_entry_2
                        }
                    }
                }
                // Gruppe: hier in neue Routingtabelle eintragen
                List tmp_routingentry = []
                tmp_routingentry.add(best_candidate[0])
                tmp_routingentry.add(best_candidate[1])
                tmp_routingentry.add(best_candidate[3])
                tmp_routingentry.add(best_candidate[4])
                tmp_routingTable.add(tmp_routingentry)
            }
            routingTable = tmp_routingTable
            stack.setRoutingTable(tmp_routingTable)

            Utils.writeLog(routername_display, "receive", "Informationen von Nachbar ${iPAddr} verarbeitet. Größe DistanzMatrix: ${DistanzMatrix.size()}, RoutingTable: ${routingTable.size()}", 1)
        }
    }

    // ------------------------------------------------------------


    /** Periodisches Senden der Routinginformationen */
    void sendPeriodical() {
        // Paket mit Routinginformationen packen
        // extrahieren von Information, dann rInfo als !Zeichenkette! erzeugen ...

        String rInfo = DistanzMatrixToString(DistanzMatrix)

        // Zum Senden uebergeben
        Utils.writeLog(routername_display, "send", "Sende meine Matrix an alle Nachbarn. Größe meiner Matrix: ${DistanzMatrix.size()}", 1)
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
            // den Avaiable Counter um eins runtersetzen
            for (entry in neighborAvaiableTable){
                if (entry[0] == neigbor[0]){
                    entry[3] = entry[3] -1

                    // wenn der nachbar weg/tot/unererichbar ist, dann wird er beim nächsten recv gestrichen
                    if (entry[3] <= 0){
                        for (route in DistanzMatrix){
                            if (route[3] == neigbor[0]){
                                route[2] = 0
                                Utils.writeLog(routername_display, "send", "Ausfall von ${neigbor[0]} entdeckt", 11)
                            }
                        }
                    }
                }
            }
            stack.udpSend(dstIpAddr: neigbor[0], dstPort: neigbor[1],
                    srcPort: config.ownPort, sdu: rInfo)
        }
    }
    //------------------------------------------------------------------------------
}

