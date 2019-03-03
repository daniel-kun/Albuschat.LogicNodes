# Web Request

Der Baustein ermöglicht es, HTTP-Anfragen an Webseiten, REST-APIs oder sonstige 
HTTP Gegenstellen zu richten und die Ergebnisse weiterzuverarbeiten.

Im einfachsten Falle kann ein textueller oder numerischer Wert von einer 
HTTP-Schnittstelle eines Smart Home-Gerätes abgerufen und in der Logik 
weiterverarbeitet werden. Über das Nutzen der Methode "POST" oder anderer 
Methoden ist es aber auch möglich, Geräte zu steuern, Werte in den Geräten
oder Webschnittstellen zu verändern, zu Löschen, etc. Voraussetzung ist,
dass die entsprechende Gegenstelle eine HTTP-Schnittstelle bietet.

Populäre Beispiele von Geräten oder Services mit HTTP-Schnittstelle sind
unter Anderem:

- IFTTT (https://ifttt.com/services/maker_webhooks)
- Nuki (https://nuki.io/de/api/)
- Phillips Hue (https://www.developers.meethue.com/documentation/getting-started, benötigt vorher eine Registrierung)
- Diverse netzwerkfähige Fernseher, Beamer und HiFi-Verstärker

Auch "Smart Home Gateways" können damit angesteurt werden, wodurch
sich Möglichkeiten zu allen von den Gateways unterstützten Geräten und Services
öffnet, was in Summe eine nahezu allumfassende Basis bietet:

- Home Assistant - Open Source (https://developers.home-assistant.io/docs/en/external_api_rest.html)
- iobroker - Open Source (https://github.com/ioBroker/ioBroker.simple-api)
- FHEM - Open Source (https://fhem.de/)
- Homee (https://hom.ee/)

Am Ende finden sich praktikable Beispiele zu einigen dieser Geräte oder Dienste,
die als Basis verwendet werden können.

## Einschränkungen

Es gibt ein paar Dinge, die mit diesem Baustein nicht möglich sind:
- HTTPS-Anfragen sind nicht möglich
- Ein paar spezielle Header können nicht gesetzt werden (Range, Proxy-Connection, Expect, Transfer-Encoding)

## Eingänge

| Bezeichnung | Porttyp | Beschreibung |
|-------------|---------|--------------|
| Trigger | Binary | Trigger muss auf '1' gesetzt werden, um die Webanfrage gemäß der folgenden Inputs abzusenden. *Wichtig:*  Die Anfrage wird *nur* durch Setzen des Triggers auf '1' ausgelöst und nicht durch die Veränderungen einer oder mehrerer der anderen Inputs. |
| URL | String| Die URL, die abgerufen werden soll. Muss mit http:// beginnen. Die URL kann beliebig viele Variablen beinhalten, siehe Beschreibung der Inputs <Variable>. |
| Methode | HTTP Methode | Die Abfrage-Methode, die verwendet werden soll. Bei der Methode 'GET' wird die URL genau so abgerufen, wie es ein Browser machen würde, wenn man die URL in der Adresszeile eingibt. Benutzen Sie 'GET' beispielsweise, um normale Webseiteninhalte abzurufen. Andere häufig verwendete Methoden sind 'POST', 'PUT' und 'DELETE'. Welche Methode zu verwenden ist, entnehmen Sie der Spezifikation der API, die Sie ansprechen. |
| Authorization | HTTP Authorization-Methode | Gibt an, ob eine Authorisierungs-Methode bei der Abfrage genutzt werden soll, und falls ja, welche. Es stehen Basic Authorization und Bearer Token zur Auswahl. Spezieller Authorization-Methoden können stattdessen als benutzerdefinierter Header verwendet werden. |
| Benutzername | String | Der Benutzername, mit dem sich an der Gegenstelle per HTTP Basic Authorization angemeldet werden soll. Nur sichtbar, wenn Authorization auf "Basic Auth" gestellt wurde. |
| Passwort | String | Das Passwort passend zum Benutzernamen, mit dem sich an der Gegenstelle per HTTP Basic Authorization angemeldet werden soll. Nur sichtbar, wenn Authorization auf "Basic Auth" gestellt wurde. |
| Token | String | Der Authorization Bearer Token, mit dem sich an der Gegenstelle angemeldet werden soll. Nur sichtbar, wenn Authorization auf "Bearer Token" gestellt wurde. |
| Content-Type | Content Type | Nur sichtbar, wenn die Methode nicht 'GET' ist. Gibt an, in welchem Format der Inhalt der Abfrage im Body angegeben wird. Welcher Content-Type zu verwenden ist, und wie der Body formatiert werden muss, entnehmen Sie bitte der Spezifikation der API, die Sie ansprechen. |
| Body| String | Nur sichtbar, wenn die Methode nicht 'GET' ist. Enthält den Inhalt der Anfrage, die an den Server gesendet werden soll. Der Inhalt muss entsprechend dem gewählten Content-Type formatiert sein. Siehe Liste der Content-Types weiter unten. Wie der Inhalt des Body gestaltet werden muss, entnehmen Sie bitte der Spezifikation der API, die Sie ansprechen. Der Body kann beliebig viele Variablen beinhalten, siehe Beschreibung der Inputs <Variable>. |
| \<Variablen\> | String |In der URL und dem Body können Variablen verwendet werden, die mit geschweiften Klammern gekennzeichnet werden. z.B. {Variable1}. Variablen dürfen nur Buchstaben und Ziffern beinhalten und müssen mit einem Buchstaben beginnen. Für jede Variable wird ein neuer Input erzeugt. Beim Ausführen des Web Request per Trigger werden in der URL und dem Body alle Vorkomnisse einer Variablen durch den Inhalt des jeweiligen Inputs ersetzt. Variablen können in URL und Body mehrfach verwendet werden. Es entsteht dann ein Input, dessen Wert bei allen Vorkomnissen der Variable eingesetzt wird. |
| Eigene Header setzen | Headeranzahl |Gibt die Anzahl der eigenen Header an, die bei der Anfrage gesetzt werden sollen. Je nach angesprochener API müssen spezielle, teils API-spezifische Header gesetzt werden, um Inhalte abzurufen oder Aktionen auszuführen. Welche Header zus setzen sind, entnehmen Sie bitte der Spezifikation der API, die Sie ansprechen. 
| Header #n | String |Enthält einen Header, der bei der Anfrage gesetzt werden soll. Header müssen das Format "<Name>: <Inhalt>" haben, wobei "<Name>" nur aus Buchstaben und Bindestrichen bestehen darf. |

## Content-Type

Der Content-Type wird vorwiegend bei Anfragen der Methode 'POST', 'PUT und 'DELETE' verwendet. Bei der Methode 'GET' ist das Setzen des Content-Type und das Senden eines Body nicht möglich.

### text/plain

Der Inhalt von Body ist in keinem speziellen Format, sondern regulärer Fließtext. Er wird in UTF-8 übertragen.

### application/json

Der Inhalt von Body ist im JSON-Format. Siehe https://de.wikipedia.org/wiki/JavaScript_Object_Notation.
Beispiel:
```json
{ "value": "1", "brightess": "100" }
```

### application/xml

Der Inhalt von Body ist im XML-Format. Siehe https://de.wikipedia.org/wiki/Extensible_Markup_Language.
Beispiel:
```xml
<body value="1"><brightness>100</brightness></body>
```

### application/x-www-form-urlencoded

Der Inhalt von Body ist in dem Format, in dem das Übertragen von HTML-Forms üblicherweise geschieht. Siehe https://en.wikipedia.org/wiki/POST_(HTTP)#Use_for_submitting_web_forms.
Beispiel:
```
Value=1&Brightness=100
```

## Ausgänge

| Bezeichnung | Porttyp | Beschreibung |
|-------------|---------|--------------|
| Antwort | String | Ist der Web Request erfolgreich, wird der Inhalt, den der Server gesendet hat, in diesen Ausgang geschrieben. Wird beispielsweise ein GET auf eine Webseite gemacht, enthält dieser Ausgang den HTML-Code. Werden REST APIs angesprochen, enthält der Ausgang üblicherweise die in JSON oder XML gestaltete Antwort des Servers. |
| Fehlercode (HTTP Status) | Integer | Ist der Web Request fehlgeschlagen, enthält dieser Ausgang einen entsprecheden Fehlercode. Konnte die Anfrage gar nicht erst gesendet werden, werden die Fehlercodes 997-999 gesetzt. Wurde die Anfrage zwar erfolgreich abgesetzt, aber vom Server mit einem Fehler quittiert, wird der entsprechende HTTP-kompatible Statuscode vom Server gesetzt. Siehe Beschreibung der Fehlercodes unten. _Hinweis:_ Da Weiterleitungen immer gefolgt wird, werden keine Fehlercodes, die zwischen '300' und '399' liegen, ausgegeben. |
| Fehlermeldung | String | Ist der Web Request fehlgeschlagen, enthält dieser Ausgang eine Fehlermeldung. Im Falle der Fehlercodes 997-999 ist die Fehlermeldung auf Englisch und beschreibt, weshalb der Web Request nicht abgesetzt werden konnte. Bei anderen Fehlercodes stammt die Fehlermeldung vom Server. |

### Fehlercodes

| Fehlercode | Beschreibung |
|------------|--------------|
| 400-499 | HTTP Statuscode. Die URL, Methode oder der Content-Type oder Body sind nicht korrekt. Z.B. existiert die Webseite nicht oder der Body ist nicht richtig formatiert. |
| 500-599 | HTTP Statuscode. Der Server konnte die Anfrage nicht bearbeiten, ggf. wegen eines internen Serverfehlers. | 
| 997 | Interner Fehlercode. Die URL beginnt nicht mit http://. Bitte beachten Sie, dass HTTPS nicht unterstützt wird. |
| 998 | Interner Fehlercode. Ein unbekannter Fehler ist aufgetreten. In "Fehlermeldung" steht die originale Fehlermeldung der C#-Runtime. |
| 999 | Interner Fehlercode. Die URL keine gültige URL. Der Web Request wurde nicht abgesetzt. |

Mehr Infos zu den HTTP Statuscodes finden sich in der Spezifikation der API, die Sie ansprechen.
Eine allgemeine Übersicht findet sich hier: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes.

# Beispiele

## IFTTT WebHook auslösen

Es ist möglich, einen IFTTT "WebHook" als "IF"-Bestandteil eines Applets zu verwenden, der sich
per Web Request-Baustein auslösen lässt. Dafür muss zuerst auf IFTTT ein Applet
zusammengebaut werden, das einen WebHook als Auslöser verwendet. Außerdem muss
die URL herausgefunden werden, mit der der WebHook ausgelöst wird und der den individuellen "Key"
enthält.

Diese URL hat den Aufbau "https://maker.ifttt.com/trigger/<individueller-event-name>/with/key/<dein-key>".
Diese URL kann einfach in einem Web Request-Baustein als URL mit der Methode "GET" eingetragen werden, mit entsprechendem
Event-Namen (dieser wird in dem verwendeten Applet festgelegt) und Key (dieser findet sich auf IFTTT in
der Doku des WebHook services). Es muss allerdings das https:// durch ein http:// ersetzt werden.

Sollen jedoch Werte übertragen werden, muss die Methode zu "POST", der Content-Type zu "application/json"
und im Body gültiges JSON eingetragen werden. Hier ein Beispiel:

| Input | Wert | Beschreibung |
|-------|------|--------------|
| URL | http://maker.ifttt.com/trigger/<event-name>/with/key/<dein-key> | Der \<event-name\> kann selbst gewählt werden und wird im Applet, das durch diesen WebHook getriggert wird, eingetragen. Der Key findet sich i der Dokumentatio des WebHook-Services in Ihrem IFTTT-Account. |
| Methode | POST | Es wird POST verwendet, damit auch Werte gesendet werden können |
| Content-Type | application/json | IFTTT verwendet das JSON-Format, um Werte an einen WebHook zu übergeben |
| Body | ```{ "value1": "x", "value2": "y", "value3": "z" } | Sendet die drei Werte x, y und z an die Variablen "value1", "value2" und "value3", die im Applet in der Action verwendet werden kann. (per "Add ingredient") |

## Nuki Bridge API

Nuki bietet verschiedene APIs an. Am sinnvollsten ist in Verbindung mit dem Web Request-Baustein wohl die Bridge API zu verwenden,
da diese ausschließlich lokal kommuniziert und keine Cloud-Anbindung oder Internetverbindung benötigt.

Am 14.10.2018 war die aktuelle Version der Bridge API hier zu finden: https://nuki.io/wp-content/uploads/2018/04/20180330-Bridge-API-v1.7.pdf

Zuerst muss die Bridge discovered und ein Auth-Token erzeugt und die ID des Nuki-Schlosses ermittelt werden, 
wie in der API-Dokumentation beschrieben.
Die Nuki Bridge-API verwendet ausschließlich die Methode GET und alle Parameter werden in der URL übergeben.

So könnte beispielhaft ein Web Request Baustein parametriert sein, um die Nuki Bridge API anzusteuern:

| Input | Wert | Beschreibung |
|-------|------|--------------|
| URL | http://\<ip-der-nuki-bridge\>:8080/lockAction?nukiId={NukiID}&token={NukiToken}&action={NukiLockAction} | I der URL werden Variablen verwendet, für die jeweils eigene Inputs erzeugt werden. Die NukiId entspricht der ID des Schlosses, das angesteuert werden soll und das NukiToken ist das Authentifizierungs-Token. Diese beiden Variablen sind eher selten veränderlich. Die NukiLockAction hingegen kann genutzt werden, um zwischen Auf- und Zuschließen zu unterscheiden. Diese kann als Input variabel gesetzt werden. Näheres dazu unten. |
| Methode | GET | Die Nuki Bridge API benutzt ausschließlich die Methode 'GET', weshalb man sie auch einfach im Browser testen kann. |
| NukiID | z.B. 1 | Enthält die ID des Nuki-Schlosses, das angesteuert werden soll. Dies ist ein dynamischer Input, der sich aus dem Platzhalter in der URL ergeben hat. |
| NukiToken | Über die Nuki Bridge API zu ermitteln | Enthält den Authentifizierungs-Token der Nuki-Bridge. Dies ist ein dynamischer Input, der sich aus dem Platzhalter in der URL ergeben hat. |
| NukiLockAction | z.B. 1 = aufschließen, 2 = abschließen | Dies ist ein dynamischer Input, der sich aus dem Platzhalter in der URL ergeben hat. Siehe den Abschnitt "Lock Actions" in der Nuki Bridge API-Dokumentation. |

In diesem Beispiel werden drei Variablen in der URL verwendet. Zwei der Variablen sind eher statisch; die NukiID und das NukiToken. Diese können trotzdem aus einem Variablen-Datenpunkt gefüllt werden 
(an dem Datenpunkt sollte unbedingt ein Vorgabewert und der Haken "Wert speichern" gesetzt werden!), damit sie bei Veränderungen in der Anlage auch ohne neue Inbetriebnahme verändert werden können.
Mit Hilfe des Gira S1 geht dies sogar sehr einfach Remote, da dieser auch einen Zugriff auf die Nuki Bridge erlaubt, um die neue Nuki ID oder das Token zu ermitteln.

Die dritte Variable, die NukiLockAction, ist hingegen dynamisch. Über sie wird angegeben, ob das Schloss ab- oder aufgeschlossen
werden soll.

Damit nach dem Ändern der NukiLockAction der Web Request ausgeführt wird, ist es notwendig, den Trigger neu auszulösen.
