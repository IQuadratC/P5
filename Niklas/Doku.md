# Reflexion

## Algorithmus
Nachdem wir uns in der Projektmanagement Woche das Ziel unseres Projektes ausformuliert hatten, begonnen wir uns zu überlegen wie wir aus zwei Punktewolken die Verschiebung zu errechnen.
Wir hatten bereits die Idee uns auf gerade Wende zu konzentrieren und wussten auch schon wie wir diese Erkennen konnten.
Um den Algorithmus weiter zu vereinfachen hatte ich die Idee anstatt mit geraden mit den Schnittpunkten dieser Geraden zu rechnen. Der vorteil dieser Methode liegt darin das ein Punkt nur aus einem Vektor besteht, im gegensätz zu einer Geraden welche aus Zwei Vektoren besteht, außerdem gibt es unendlich viele möglichkeiten eine gerade zu speichern wohingegen punkte eindeutig sind.

## Netzwerk
### TCP
Um unsere App mit dem Roboter zu verbinden brauchten wir ein einfaches protokoll um daten zu übertragen. Da wir W-lan als Übertragungsweg nutzen war das erste Protokoll welches mir eingefallen ist HTTP, dieses wird auch für Internetseiten verwendet und ist weit Verbreitet.
Nach dem ich einen einfachen HTTP server in Python aufgesetzt hatte wollte ich einen Client für diesen in Unity schreiben. Allerdings waren alle informationen zu "networking" in Unity mit TCP daher wäre es in Unity wesentlich aufwändiger gewesen einen HTTP Client zu schreiben und so habe ich mich für ein einfacheres Protokoll entschieden, TCP welches das HTTP zugrunde liegende Protokoll ist.

### Protokoll
TCP gibt uns die möglichkeit daten zu übertragen, um das Protokoll leicht verständlich zu machen, was auch bei der Fehlersuche hilft, haben wir uns entschieden Text zu senden welcher von menschen gelesen werden kann.

Eine Beispielnachricht wäre
```
roboter summlidardata 20
```
Das erste Wort der Nachricht ist der ort an welchen es geleitet wird, die Idee hierbei ist das dieses Wort dem TCP Server/Client sagt zu welchem weiterem program es die empfangenen daten senden soll.
Zum beispiel war ursprünglich die idee das es ein Python Program gibt welches daten empfängt um den Roboter zu Bewegen und ein C++ Programm welches die LIDAR daten des Sensor auswertet.
Das Zweite Wort ist der Befehl, dieser sagt dem Program was nun zu tun ist. In diesem Fall Daten messen und zurück senden.
Das letzte Wort ist das Argument dieses gibt extra Informationen zu dem Befehl in diesem Fall das sich der Sensor 20 umdrehungen machen soll.

Um die daten im Argument besser zu organisieren haben wir uns ein System ausgedacht um dies zu tun.
Zu erst können einzelne informationen durch Kommata getrennt werden.
Dies ist nötig da zum Beispiel die Lidar daten aus vielen Messpunkten bestehen.
Allerdings besteht jeder einzelne Messpunkt aus zwei Zahlen. Daher werden diese mit dem nächsten trenn zeichen getrennt.
Wir hätten für solche Daten auch Klammern nutzen können allerdings sind diese wesentlich Schwieriger auszuwerten als diese Zeichen.
Denn die meisten Programmirsprachen bringen bereits die möglichkeit mit Zeichenketten an bestimmten zeichen aufzuteilen.
Dies macht es sehr einfach ein Argument Rekursiv zu verarbeiten, man teilt die erste Ebene nach den Leerzeichen auf, ersetzt kommata durch lerzeichen und die anderen trenn zeichen mit dem trenn zeichen der Ebene darüber und gibt diese liste an Argumenten einzeln an die entsprechende Funktion.

Ein beispiel ist die Multi funktion welche mehrere Bewegungs Befehle an den Roboter sendet.
```
multi move,1;1;1,rotate,90;1
```
Da multi an der von dem selben Code interpretiert wird wie move und Rotate könnte man sogar mehrere multi Funktionen ineinander Schreiben auch wen dies keinen sinn erfüllt.
### Python zu C++
Ursprünglich hatte ich den gesamten code welcher auf dem Roboter lief in Python geschrieben.
Allerdings war es nicht möglich den code für den LIDAR sensor in Python zu schreiben da Python zu langsam für diesen Sensor war daher musste der Code um den Sensor auszulesen in C++ geschrieben werden.
Da wir allerdings schon den TCP server und die Motorsteuerung in Python implementiert hatten wollten wir einfach ein Program schreiben welches den Lidar ausliest und diese Daten zurück an Python überträgt.
Da Tim welcher den C++ Code geschrieben hatte dies aber nicht zum Laufen bringen konnte haben wir uns entschieden den code in C++ umzuschreiben. Zum glück war das Python Program nur 122 Zeilen lang und so recht einfach umzuschreiben.
## Bewegung der Kamera
In unserer App befindet sich eine Repräsentation der daten welche der Roboter mit dem LIDAR sensor misst zusammen mit der Position des Roboters und der Früher gemessenen Daten.
Da man allerdings nicht immer nur das nahe umfeld des roboters sehen möchte haben wir die möglichkeit eingebaut durch finger bewegungen die Position der Kamera welche das Bild rendert zu bewegen.


## AI
Da unser Projekt eine Karte und eine Position Erzeugt ist es eine Gute Demonstration wen der Roboter in der Lage ist selbstständig zu angegebenen Zielen zu navigieren.
### A*
Ich habe mich für A* entschieden da es ein Weit verbreiteter Patfindig Algorithmus ist und es gute Erklärungen gibt wie dieser Funktioniert.
Um A* nutzen zu können und weil es algebein das Nutzen unserer Daten einfacher macht habben wir unsere messdaten Rastarisiert, heißt wir speichern mit einer auflösung von einem Zentimeter ob ein Punkt eine wand ist oder nicht.
Man gibt diesem Algorithmus nun einen Statt und end Punkt zusammen mit einer liste der Wände und er findet einen Weg.
Ein Problem welches ich zu begin hatte war das A* seine Daten in einer Liste gespeichert hatte was sehr ineffizient ist.
Daher habe ich einen Sogenannten Heap implementiert.

# Quellen
## 26.01.2012
installed software, created Unity project.
## 28.01.2021
HTTPs://gist.github.com/danielbierwirth/0636650b005834204cb19ef5ae6ccedb
HTTPs://forum.unity.com/threads/how-do-i-detect-when-a-button-is-being-pressed-held-on-eventtype.352368/
## 09.02.2021
HTTPs://www.youtube.com/watch?v=0G4vcH9N0gc
HTTPs://pressstart.vip/tutorials/2018/07/12/44/pan--zoom.html
## 10.02.2021
HTTPs://arongranberg.com/astar/docs/

### A*
HTTPs://www.youtube.com/watch?v=-L-WgKMFuhE
HTTPs://github.com/SebLague/Pathfinding/tree/master/Episode%2004%20-%20heap
## 12.02.2021
HTTPs://pressstart.vip/tutorials/2018/06/22/39/mobile-joystick-in-unity.html
## 27.03.2021
Yesenia geholfen die Roboter Simulation zu schreiben
