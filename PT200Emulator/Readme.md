# PT200 Emulator

En emulator för PT200-terminalen med stöd för klassiska ESC-sekvenser, loggning och konfigurerbar UI.  
Projektet är uppdelat i tydliga moduler för att underlätta utveckling, testning och framtida utbyggnad.

## Status
- **Version:** 1.2.1  
- **Nytt i denna version:**  
  - README.md tillagd  
  - Config-filer markerade som *Copy Always* för enklare distribution  

---

## Arkitektur och moduler

### InputHandler
- Tar emot tangenttryckningar och översätter dem till PT200-sekvenser.
- Exponeras via publika metoder för att mata in tecken eller sekvenser.
- Används av UI för att skicka input till parsern/transportlagret.

### Logging
- Central loggning med stöd för olika nivåer (`Info`, `Debug`, `Trace`).
- Loggnivå kan ändras dynamiskt via UI (combo-box).
- Loggkonsol kan slås på/av.

### Parser
- Tolkar inkommande byte-strömmar och identifierar ESC-sekvenser.
- Exponeras via `Feed(byte b)` eller motsvarande.
- Kända sekvenser hanteras direkt, okända loggas som *Other*.

### Rendering
- Ansvarar för att rita text, attribut och färger på skärmen.
- Stöd för olika skärmformat (80×24, 132×27 m.fl.).
- Hanterar färgteman (grön, amber, vit, blå, fullfärg).

### Transport
- Hanterar anslutning till host/port via TCP.
- Exponeras via `Connect`, `Disconnect`, `Send`, `Receive`.
- Konfigureras via `transportConfig.json`.

---