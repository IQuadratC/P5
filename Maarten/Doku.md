# 6.2.2021
Erster Versuch die Position zu errechnen.

Ansatz:
- input csv datei
- enthält Winkel r und Entferung d
- in Lidarpoint Objekt speichern
- Durchschnitt errechen d = d0 + d1 + d2 ... dn / n
- XY - Kordinate p aus d und r

Linen erkennen:
- loop all p
- Gerade mit i und i-1 erstellen.
- Checken wie viele punkte größer i und  kleiner i auf der Geraden liegen.
- Begrenzt bei Abstand zur Geraden
- Als Linie abspeichern.
- Linien nach Länge sortiern
- Loop:
    - die Lägste abspeichern. 
    - alle Punke aus anderen Linien löschen die in der Lägsten sind.
    - Linien nach Länge sortiern
    - Alle Linien löschen die zu kurz sind.
    - Fertig wenn linien liste leer ist
- Geraden aus Linien finden indem FindLinearLeastSquaresFit auf Punkte angwendet wird.

Schnittpunkte finden:
- Loop durch Geraden:
    - Loop durch Geraden:
        - Schnittpunkt zwischen Geraden finden.
        - Schnittpunkt abspeichern.

Lidarpoint objekte über einader legen:
- Loop durch Schnittpunkt von Lidarpoint 1:
    - Loop durch Schnittpunkt von Lidarpoint 1:
        - Loop durch Schnittpunkt von Lidarpoint 2:
            - Loop durch Schnittpunkt von Lidarpoint 2:
                - Schnittpunkte Paare finden die gleich weit von einader entfernt sind.
                - Die Schnittpunkt Paare übereinader legen:
                    - dir1 = p1 - p2
                    - dir2 = p3 - p4
                    - winkel = Winkel zwischen dir1 und dir2 * -1
                    - position ist p1 - p3 um den winkel gredeht.
                    - Daten in Overlap speichern
                
                - Loop durch Schnittpunkt von Lidarpoint 1:
                    - Loop durch Schnittpunkt von Lidarpoint 2:
                        - Nächsten Schnittpunkt in Lidarpoit 2 finden wenn overlap angwendet ist.
                    - Alle entferunegn auf addieren und in accuracy speichern
                - Overlap in Liste speichern
- Liste nach accuracy sortiern.
- Den ersten nehmen.
                
Geschwinfikeits verbesserungen:

Input:
- csv laden / daten satz hizufügen

- Loop winkel:
    - durchschnitt errechnen
    - position errechnen
    - position speichern

- Loop Positionen:
    - Linie pro Position errechnen
    - Linie speichern
    - Zu kurze löschen
    - Geraden errechnen
    - Gerade speichern

- Linien sortiren
- Zu kurze löschen

- Loop Linien:
    - Loop Linien:
        - Schnitpunkt finden
        - Schnitpunkt speichern

- Schnittpunkte zusammen fassen

# 09.02.2021

Adunio:
- Motorsteuerung
- Über Serial mit Respi verbunden.
- Tim

Respi:
- AutoComputer

- Py:
    - Schnitstelle zwischen C++ und Aduino
    - Umrechnen der Befehle von string to bits für Aduino
    - Niklas

- C++:
    - Main Auto logic
    - TCP Server
    - TCP strings für Ardunio an Py weiterleiten.
    - Lidar sensor lesen
    - Lidar daten über TCP senden
    - Tim

App:
- Fersteureung
- TCP Client.
- Beweguns Befehle an Arduino senden. Niklas
- Lidar Daten entfangen. Maarten
- Lidar Daten umrechnen. Maarten
- Lidar Karte erstellen. Maarten
- Algoritmus um in Karte zu navigiren. 
- Auf Position in der Karte fahren. 
- Route abfahren.
- Karte erkunden.
- Karte darstellen. 
- Shader für Karte schreiben.
- StartScerren für App.

TCP Syntax: Entfänger Befehl Argumente ...

# 12.02.2021 

App Logik:
- TCP:
    - Liste Send Event (Send String)
    - Raises Recive Event (Recive String)

- LidarController:
    - State
    - Data (List Points)
    - Position

    - Listen GetData Event
    - Raises Send Event
    - Raises NewPoint Event
    - Listen Recive Event
    
- LidarMap: 
    - Listen NewPoint

- Controller UI
    - Toggle AI

    - Raises Send Event
    - Raises GetData Event

- Controller AI
    - Listen NewPoint Event
    - Raises Send Event
