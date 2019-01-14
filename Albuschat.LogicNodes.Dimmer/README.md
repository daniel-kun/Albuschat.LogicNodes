# Umschalten und Dimmen per Binäreingang

Mit Hilfe dieses Bausteines ist es möglich, per Binäreingänge oder ähnliche "zustandslose" Sender, einen Aktor umzuschalten oder zu dimmen.
Beispiele hierfür ist das An-/Ausschalten und Dimmen eines Lichts per Taste an einem RF Tastsensor. Der RF Tastsensor kennt den Zustand des Lichts nicht und kann deshalb
nicht entscheiden, ob er bei einer Betätigung an- oder ausschalten soll, weshalb klassischer Weise in diesem Falle jeweils eine Taste für Ein- und Ausschalten verwendet wird.

Der Baustein "Umschalten und Dimmen per Binäreingang" kennt den Zustand des Aktors und hat deshalb alle Möglichkeiten, die ein drahtgebundener Tastsensor auch hat.

Es gibt drei Modi, in denen dieser Baustein genutzt werden kann, die weiter unten näher beschrieben werden:

- "Umschalten, 1 Taste": In diesem Modus kann mit Hilfe eines Eingangssignals, z.B. eines RF Tastsensors oder simplen Binäreingangs, ein Aktor "umgeschaltet" werden. D.h. es wird bei jeder Betätigung der Aktor in den jeweils anderen Zustand geschaltet.
- "Umschalten und Dimmen, 1 Taste": In diesem Modus kann nicht nur umgeschaltet, sondern auch relativ gedimmt werden. Dabei verhält sich der Baustein wie ein Gira Tastsensor, indem er bei kurzem Tastendruck umschaltet, und bei langem Tastendruck dimmt. Jedes Mal, wenn die Taste lange gedrückt gehalten wird, wird gewechselt, ob hoch oder runter gedimmt werden soll.
- "Umschalten und Dimmen, 2 Tasten": In diesem Modus kann mit Hilfe zweier Tasten oder Binäreingänge ein Aktor umgeschaltet und gedimmt werden, wobei für heller und dunkler jeweils eine dedizierte Taste gewählt wird. Das erlaubt eine leichtere Bedienung, benötigt aber zwei Tasten je angesteuertem Licht.

## Modus "Umschalten, 1 Taste"

Dieser Modus kann mit allen Aktoren verwendet werden, die ein 1-bit Schaltobjekt und 1-bit Rückmeldeobjekt anbieten. Diese müssen im GPA als ein Datenpunkt zusammengefasst werden.

### Eingänge

| Bezeichnung | Porttyp | Beschreibung |
|-------------|---------|--------------|
| Umschalten | BOOL | Empfängt das Umschaltsignal vom Taster oder Binäreingang. Es wird lediglich auf gesendete "1"-Telegramme reagiert, etwaige "0"-Telegramme werden ignoriert. |
| Aktueller Schaltzustand | BOOL | Empfängt den aktuellen Schaltzustand des Aktors. Dieser Eingang sollte unbedingt mit demselben Datenpunkt wie der Ausgang "Schalten" verbunden werden. |

### Ausgänge

| Bezeichnung | Porttyp | Beschreibung |
|-------------|---------|--------------|
| Schalten | BOOL | Bei Senden einer "1" auf den Eingang "Umschalten" wird auf den Ausgang "Schalten" der invertierte Wert des Eingangs "Aktueller Schaltzustand" gesendet - d.h. ist der aktuelle Schaltzustand "1", wird auf "Schalten" eine "0" gesendet und ist der Schaltzustand "0", wird auf "Schalten" eine "1" gesendet. Dieser Ausgang sollte unbedingt mit demselben Datenpunkt wie der Eingang "Aktueller Schaltzustand" verbunden werden. |


## Modus "Umschalten und Dimmen, 1 Taste"

### Parameter


| Bezeichnung | Porttyp | Einheit | Beschreibung |
|-------------|---------|---------|--------------|
| Zeit zwischen Schalten und Dimmen | INTEGER | Millisekunden | Gibt an, wie lange eine Taste gedrückt gehalten werden muss, damit das Dimmen beginnt |
| Dimmen um... | INTEGER | Prozent | Gibt an, um wie viel Prozent gedimmt werden soll. Dieser Parameter muss passend zur Konfiguration des KNX Kommunikationsobjekts des relativen Dimmens des Aktors eingestellt werden. |

### Eingänge


| Bezeichnung | Porttyp | Beschreibung |
|-------------|---------|--------------|
| Umschalten oder Dimmen | BOOL | Empfängt das Signal des Tasters oder Binäreingangs. Ein "1"-Telegramm bedeutet, dass die Taste gedrückt wurde und ein "0"-Telegramm bedeutet, dass die Taste losgelassen wurde. Wird die Taste länger als "Zeit zwischen Schalten und Dimmen" gedrückt gehalten, wird um "Dimmen um..." Prozent gedimmt. Dabei wird immer abwechselt heller bzw. dunkler gedimmt, so dass sich auch mit einer Taste eine einfache Justierung der Helligkeit vornehmen lässt. |
| Aktueller Schaltzustand | BOOL | Empfängt den aktuellen Schaltzustand des Aktors. Dieser Eingang sollte unbedingt mit demselben Datenpunkt wie der Ausgang "Schalten" verbunden werden. |


### Ausgänge

| Bezeichnung | Porttyp | Beschreibung |
|-------------|---------|--------------|
| Schalten | BOOL | Bei Senden einer "1" auf den Eingang "Umschalten" wird auf den Ausgang "Schalten" der invertierte Wert des Eingangs "Aktueller Schaltzustand" gesendet - d.h. ist der aktuelle Schaltzustand "1", wird auf "Schalten" eine "0" gesendet und ist der Schaltzustand "0", wird auf "Schalten" eine "1" gesendet. Dieser Ausgang sollte unbedingt mit demselben Datenpunkt wie der Eingang "Aktueller Schaltzustand" verbunden werden. |
| Dimmen relativ | NUMBER | Sendet das Dimmtelegramm gemäß dem Parameter "Dimmen um...", wenn auf "Umschalten oder Dimmen" länger als die "Zeit zwischen Schalten und Dimmen" eine "1" anliegt, ohne dass über das Empfangen einer "0" das Loslassen der Taste signalisiert wurde. Sendet ein Stopptelegramm, wenn die Taste dann anschließend losgelassen wurde. |


## Modus "Umschalten und Dimmen, 2 Tasten"

### Parameter


| Bezeichnung | Porttyp | Einheit | Beschreibung |
|-------------|---------|---------|--------------|
| Zeit zwischen Schalten und Dimmen | INTEGER | Millisekunden | Gibt an, wie lange eine Taste gedrückt gehalten werden muss, damit das Dimmen beginnt |
| Dimmen um... | INTEGER | Prozent | Gibt an, um wie viel Prozent gedimmt werden soll. Dieser Parameter muss passend zur Konfiguration des KNX Kommunikationsobjekts des relativen Dimmens des Aktors eingestellt werden. |

### Eingänge


| Bezeichnung | Porttyp | Beschreibung |
|-------------|---------|--------------|
| Umschalten oder Heller | BOOL | Empfängt das Signal des Tasters oder Binäreingangs. Ein "1"-Telegramm bedeutet, dass die Taste gedrückt wurde und ein "0"-Telegramm bedeutet, dass die Taste losgelassen wurde. Wird die Taste länger als "Zeit zwischen Schalten und Dimmen" gedrückt gehalten, wird um "Dimmen um..." Prozent __heller__ gedimmt, bis die Taste losgelassen wird. |
| Umschalten oder Dunkler | BOOL | Empfängt das Signal des Tasters oder Binäreingangs. Ein "1"-Telegramm bedeutet, dass die Taste gedrückt wurde und ein "0"-Telegramm bedeutet, dass die Taste losgelassen wurde. Wird die Taste länger als "Zeit zwischen Schalten und Dimmen" gedrückt gehalten, wird um "Dimmen um..." Prozent __dunkler__ gedimmt, bis die Taste losgelassen wird. |
| Aktueller Schaltzustand | BOOL | Empfängt den aktuellen Schaltzustand des Aktors. Dieser Eingang sollte unbedingt mit demselben Datenpunkt wie der Ausgang "Schalten" verbunden werden. |


### Ausgänge

| Bezeichnung | Porttyp | Beschreibung |
|-------------|---------|--------------|
| Schalten | BOOL | Bei Senden einer "1" auf den Eingang "Umschalten" wird auf den Ausgang "Schalten" der invertierte Wert des Eingangs "Aktueller Schaltzustand" gesendet - d.h. ist der aktuelle Schaltzustand "1", wird auf "Schalten" eine "0" gesendet und ist der Schaltzustand "0", wird auf "Schalten" eine "1" gesendet. Dieser Ausgang sollte unbedingt mit demselben Datenpunkt wie der Eingang "Aktueller Schaltzustand" verbunden werden. |
| Dimmen relativ | NUMBER | Sendet das Dimmtelegramm gemäß dem Parameter "Dimmen um...", wenn auf "Umschalten oder Heller" bzw. "Umschalten oder Dunkler" länger als die "Zeit zwischen Schalten und Dimmen" eine "1" anliegt, ohne dass über das Empfangen einer "0" das Loslassen der Taste signalisiert wurde. Sendet ein Stopptelegramm, wenn die Taste dann anschließend losgelassen wurde. |


