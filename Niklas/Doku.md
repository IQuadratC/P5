# Reflexion

## Algorithmus
Nachdem wir uns in der Projektmanagement Woche das Ziel unseres Projektes ausformuliert hatten, begonnen wir uns zu überlegen wie wir aus zwei Punktewolken die Verschiebung zu errechnen.
Wir hatten bereits die Idee uns auf gerade Wende zu konzentrieren und wussten auch schon wie wir diese Erkennen konnten.
Um den Algorithmus weiter zu vereinfachen hatte ich die Idee anstatt mit geraden mit den Schnittpunkten dieser Geraden zu rechnen. Der vorteil dieser Methode liegt darin das ein Punkt nur aus einem Vektor besteht, im gegensätz zu einer Geraden welche aus Zwei Vektoren besteht, außerdem gibt es unendlich viele möglichkeiten eine gerade zu speichern wohingegen punkte eindeutig sind.

## TCP
Um unsere App mit dem Roboter zu verbinden brauchten wir ein einfaches protokoll um daten zu übertragen. Da wir Wlan als Übertragungsweg nutzen war das erste Protokoll welches mir eingefallen ist HTTP, dieses wird auch für Internetseiten verwendet und ist weit Verbreitet.
Nach dem ich einen einfachen HTTP server in Python aufgesetzt hatte wollte ich einen Client für diesen in Unity schreiben. Allerdings wäre es in Unity wesentlich aufwändiger gewesen einen HTTP Client zu schreiben daher habe ich mich für ein einfacheres Protokoll entschieden TCP welches das HTTP zugrunde liegende Protokoll ist.

## Bewegung der Kamera
In unserer App befindet sich eine Repräsentation der daten welche der Roboter mit dem LIDAR sensor misst zusammen mit der Position des Roboters und der Früher gemessenen Daten.
Da man allerdings nicht immer nur das nahe umfeld des roboters sehen möchte haben wir die möglichkeit eingebaut durch finger bewegungen die Position der Kamera welche das Bild rendert zu bewegen.

## Joystick Skript

## GameVariables / GameEvents

## Pathfinding

## Simulation

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
