# 6.2.2021
Erster Versuch die Position zu errechnen.

Ansatz:
- input csv datei
- enthält Winkel r und Entferung d
- in Lidarpoint Objekt speichern
- Durchschnitt errechen d = d0 + d1 + d2 ... dn / n
- XY - Kordinate p aus d und 

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
    - Toggle AI / HI

- Controller HI
    - Raises Send Event
    - Raises GetData Event

- Controller AI
    - Listen NewPoint Event
    - Raises Send Event

# Convention 

TCP einheiten:
- Länge cm
- Geschwindikeit Prozent
- Zeit ms
- Datentypen int

1. Space (Ort Befehl Argument)
2. , (Argumente ,..,..)
3. ; ...
4. :
5. |

roboter move 1,1,1
roboter multi move,1;1;1,move,1;1;1,move,1;1;1


# UI Instpirationen:

https://thumbs.dreamstime.com/z/hud-interface-elements-set-circles-loading-frames-web-applications-futuristic-ui-illustration-vector-hud-interface-elements-113938768.jpg

Der Hintergrund von dem:
https://www.google.com/search?q=ark+ui+barground&tbm=isch&ved=2ahUKEwjxl6jaiu_uAhVCP-wKHSCeB2wQ2-cCegQIABAA&oq=ark+ui+barground&gs_lcp=CgNpbWcQAzoCCAA6BAgAEB46BggAEAUQHjoGCAAQCBAeOgQIABAYUOIVWPMwYNIzaABwAHgAgAGKAYgB0gaSAQM3LjOYAQCgAQGqAQtnd3Mtd2l6LWltZ8ABAQ&sclient=img&ei=gxUsYLHKL8L-sAegvJ7gBg&bih=938&biw=1920&client=firefox-b-d#imgrc=EQJ38rEdd16hsM

https://images.squarespace-cdn.com/content/v1/51be3e56e4b09edc5f81e74c/1551795556726-AYCIRFK3Z8SAZQ11RRF5/ke17ZwdGBToddI8pDm48kFTEgwhRQcX9r3XtU0e50sUUqsxRUqqbr1mOJYKfIPR7LoDQ9mXPOjoJoqy81S2I8N_N4V1vUb5AoIIIbLZhVYxCRW4BPu10St3TBAUQYVKc2PPwcWkLXq-DNe4zpT7he0M_zRUr6912vvtObYWjAW-pUdJURR5nHbHpk7AZw8X9/SciFi_LoaderBlue.gif?format=1500w

Als Untergrund für Hologramm
https://i.pinimg.com/originals/22/c8/87/22c887ab8cd375d078f0f2178e400374.gif

Für Wechsel zwisch Pages
https://i.pinimg.com/originals/a8/9a/e3/a89ae31a165ddd140618e2d2709a8c9b.gif

https://i.pinimg.com/originals/f0/2f/5c/f02f5cf8169af4e54da28ed8ce69c96b.jpg

Und wir brauchen paar Consolen Logs

Virleich sollten wir auch eine Version mit weniger Effeckten mach um die Leherer nicht zu überfordern.

# Colors

Blau
#0F4655
#156075
#229DBF
#A0DDEE

Rot
#C81829
#DB5461


Final ToDos:

Tim - HI fertig machen
Maarten - IP Bug, Bild vom Roboter, 
Yesenia - Close Button, Play Button , HiAi Schalter
Niklas - HI ticken
Niklas Maarten - aufräumen und builden, AI planene

Projekt namen:
Ferngesteuerter Roboter mit Lidar Sensor
Auf LIDAR basierender SLAM Roboter
Ferngesteuerter Roboter mit LIDAR Mapping

Inhalt:
•	Einleitung (Problemstellung, Anwendungsbereiche)
•	Grund Idee (Roboterauto und Steuerung über Unity App auf Handy)
•	Einleitung Roboterauto und Lidar (Grundsätzlicher Aufbau)
•	Einleitung Unity App (Warum wir Unity genutzt haben)

Arbeits-Segmente:
•	Roboter: Tim
o	    Motorsteuerung
o	    3D Model
o	    Drucken
o	    Lidarsteuerung
o	    Testen
•	App: Yesenia
o	    Grafiken Yesenia
	        Welches Programm 
o	    Shader warum?
o	    UI
•	Simulation: Yesenia
o	    Logic
o	    UI
•	Network: Niklas
o	    App
o	    Roboter
o	    Simulation
•	SLAM: 
o	    App Version 1 Maarten
o	    App Version 2 Maarten
o	    Roboter Version 3 Tim
•	AI: Niklas
o	    Pathfinding
o	Goal Logic
Fertig bis 23 Mai.


# 26.5.2021

Was noch fählt:
- [ ] Fazit von jedem  -> Jeder
- [ ] Reflexion -> Jeder
    - bis So
- [ ] Einleitung -> Maarten
- [ ] Schluss / Ausblick -> Maarten ( Ziele erreicht? )
- [ ] Alles zusammen fügen -> Maarten
- [ ] GitHub Aufraümen -> Niklas
- [ ] Formalien -> Niklas
- [ ] Roboter zum laufen kriegen -> Tim / Maarten
- [ ] Rechtschreibung -> Yesenia
- [ ] Protokoll Nr.2 -> Yesenia
- [ ] Anhang -> Niklas, Yesenia
    - [ ]  Protokoll zur Konsultationssitzung Nr. 1
    - [ ]  Protokoll zur Konsultationssitzung Nr. 2
    - [ ]  Eigenständigkeitserklärung
    - [ ]  Projektanmeldung
    - bis 7.


# Präsentation

- Roboter Tim
- Ipad mit App Tim
- 1 Laptop mit Simulation Niklas
- 1 Laptop mit Viedeo vom Roboter Maarten
- 1 Laptop mit Unity und Code Yesenia
- Diagramm Ipad Maarten
- UI Yesenia

- Themen
    - Roboter an

    - Vorführen
    - Erklären

    - Roboter aus

    - Hadware Tim
    - SLAM Maarten

    - Netzwerk Niklas
    - AI Niklas

    - Desing / Shader Yesenia
    - Simulation Yesenia

    - Jugend Forscht

https://drive.google.com/file/d/1j7rcBBZoyYNirTSLLCFqCwP3waX_deLG/view?usp=sharing
