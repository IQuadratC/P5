# Reflexion

## Simulation
Im Laufe des Projektes ist uns das Problem über den Weg gelaufen das nur Tim zugang zu unserem Roboter hatte. 
Also haben wir uns dazu entschieden eine Simulation zu schreiben um weitere änderungen an der App oder dem Pathfinding Algorithmus ausprobieren zu können ohne uns dafür jedes mal absprechen zu müssen.

### Movement
Ich hatte vorher als einzige der Gruppe noch nicht mit Unity gearbeitet. 
Also habe ich als erstes eine Flächer erstellt auf der ich einen Würfel bewegen kann. 
Daran habe ich das Prinzip der Unity Engine gelernt und das erste mal C# genutzt.  

### String Interpreter
Damit der "Roboter" also in der Simulation der Würfel sich auch tatsächlich bewegt musste ich noch dafür sorgen das der String den die App zu allen aktionen schickt auch verstanden wird. 
Da zu ich zu diesem Zeitpunkt allerdings noch keinen TCP Server in der Simulation hatte an den die App auch tatsächlich ihre Nachrichten schiken kann habe ich ein Textfeld erstellt in das ich die Nachrichten manuell eingeben kann.

### LIDAR
Um den LIDAR Sensor auf dem Auto zu Simulieren habe ich Raycasting genutz. 
Ich hatte zwar keine ahnung wie das eigentlich funktioniert habe aber Relativ schnell herausgefunden das Unity dafür eine eigene funktion hat und man sehr einfach auf der Konsole oder auch später in einer Nachricht die an die App geschickt wird ausgeben kann welche Rays etwas treffen welchen Winkel die haben und wie weit  der "Roboter" von der getroffenen Wand entfernt ist. 
Damit das Funktioniert musste ich nur dafür sorgen das der "Roboter" auf der Richtigen Layer ist damit die Rays nicht schon im Ursprung auf etwas treffen. 
So habe ich auch gelernt das Unity ein Layer System hat. 

Danach musste ich noch dafür sorgen das die Nachrichten richtig formatiert sind das sie auch von der App die zu dem Zeitpunkt noch die Karte erstellt hat auch versteht.
ausser dem musste der String Interpreter noch Nachfragen nach LIDAR Daten verstehen und die Richtige Funktion ausführen.

### TCP Server
Einen TCP server aufzubauen war für mich das schwierigste an der Simulation da ich vorher noch nie wirklich etwas mit servern gemacht habe.


## Design
### Shader
