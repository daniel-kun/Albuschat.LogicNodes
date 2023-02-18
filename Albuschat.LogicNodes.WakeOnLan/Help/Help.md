# Dokumentation

Der Baustein Wake on LAN ermöglicht es, Geräte mit einer LAN-Schnittstelle (mit RJ45 oder "Ethernet"-Buchse) einzuschalten,
sofern diese Wake on LAN unterstützen. 

Das sind die meisten Geräte mit LAN-Schnittstelle, die seit 2010 auf den Markt gekommen sind. Auch ältere Geräte können
Wake on LAN unterstützen. Netzwerkkarten von PCs und Laptops unterstützen dies bereits üblicherweise seit 2000. Meistens wird die Wake On LAN-Fähigkeit
von Geräten nicht dokumentiert, deshalb ist man häufig darauf angewiesen, diese zu testen.

Ob ein Gerät Wake On LAN unterstütz, kann mit Hilfe von mobilen Apps getestet werden, beispielsweise 
für [Android](https://play.google.com/store/apps/details?id=co.uk.mrwebb.wakeonlan&hl=en) und [iOS](https://apps.apple.com/de/app/mocha-wol/id422625778).

Manche Geräte, wie die Sony Playstation 4, unterstützen auf Grund der Politik der Hersteller leider kein Wake On LAN, obwohl sie sehr neu sind und alle Voraussetzungen erfüllen.

Um ein Wake On LAN durchzuführen, wird die MAC-Adresse des aufzuweckenden Gerätes benötigt. Diese wird üblicherweise im Format "AA-BB-CC-DD-EE-FF" oder "AA:BB:CC:DD:EE:FF" ausgegeben und kann entweder
über die oben genannten Apps, den Heimrouter (z.B. eine Fritz!Box), den Bereich "Netzwerk" im Windows Explorer oder den Befehl `arp` der Kommandozeile herausgefunden werden.

| Bezeichnung | Porttyp | Beschreibung |
|-------------|---------|--------------|
| `Trigger` | Binary | Trigger muss auf '1' gesetzt werden, um die Webanfrage gemäß der folgenden Inputs abzusenden. |
| `MAC-Adresse` | String| Die MAC-Adresse des Gerätes, das eingeschaltet werden soll. Kann im Format AA-BB-CC-DD-EE-FF oder AA:BB:CC:DD:EE:FF oder AABBCCDDEEFF sein. |

Ob das Wake On LAN erfolgreich war, kann leider nicht ermittelt werden. Deshalb hat der Baustein keine Outputs.

# Hilfestellung

Hilfe bei Fragen zu diesem Baustein erhalten Sie am besten über das KNX-User-Forum im Bereich [Gira Logik SDK](https://knx-user-forum.de/forum/supportforen/gira-logik-sdk). Ein Account im KNX-User-Forum kann kostenfrei erstellt werden.

Erwähnen Sie @dalbuschat in Ihrem Posting, damit der Autor per E-Mail über Ihr Anliegen benachrichtigt wird. Da es sich um einen kostenfreien Baustein handelt, kann leider keine Gewährleistung gegeben werden, noch kann Support zugesagt werden.

# Nutzungslizenz

Dieser Baustein unterliegt der MIT License:

```
MIT License

Copyright (c) 2018-2019 Albuschat, Daniel

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

