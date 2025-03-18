# Projektplanung: C#HAT – Terminal-Chat mit WebSockets

### 1. Ziele & Anforderungen

Eine einfache Terminal-Chat-Anwendung in C# mit WebSockets, die eine Echtzeit-Kommunikation zwischen Clients ermöglicht.

**Funktionale Anforderungen:**

- Ein Server, der mehrere Clients verwalten kann.
- Clients können sich verbinden, Nachrichten senden und empfangen.
- Anzeigen von Benutzernamen zu jeder Nachricht
- Option zum beenden der Verbindung (logout)

**Erweiterungen:**

- Private Nachrichten (Flüstern an einzelne User)
- Benutzerliste anzeigen (Verbundende Clients)
- Nachrichten in einer Datenbank (SQLite) loggen

**Tech-Stack**

- Sprache: C#
- Framework: .NET 8/9
- Bibliotheken: `System-Net.Websockets`
- Versionskontrolle: Git und GitHub

**Architektur**

- Chat-Server (Konsolen-App)
  - Verwaltet verbundene Clients
  - Broadcastet Nachrichten an alle Clients
  - Handhabt Verbindungen & Trennungen
- Chat-Client
  - Verbindet sich mit dem Server
  - Sendet und empfängt Nachrichten
  - Stellt Nachrichten in der Konsole dar

**Kommunikationsfluss:**

1. Client verbindet sich mit dem Server
2. Server akzeptiert die Verbindung und speichert sie.
3. Client sendet eine Nachricht
4. Server leitet die Nachricht an alle verbundenen Clients weiter
5. Clients empfangen die Nachricht und geben Sie in der Konsole aus.

<br />

# Zeitplanung

**Tag 1:** Grundstruktur & WebSocket-Server

- Git-Repository einrichten
- Grundstruktur des Projekts erstellen (+ Docker)
- WebSocket-Server mit Verbindungsverwaltung implementieren
- Clients können sich verbinden und trennen
- Einfache Nachrichtenübertragung zwischen Server und Clients

**Tag 2:** Client-Entwicklung & Verbesserungen

- Nachrichten senden & empfangen optimieren
- Benutzername beim Verbinden eingeben lassen
- Formatierung der Nachrichten verbessern
- Fehlerbehandlung & Verbindungsverlust handhaben

**Tag 3:** Features & Tests

- Erweiterte Features (Flüstern, Benutzerliste, Logging)
- Stabilitätstests mit mehreren Clients
- Code-Refactoring & Dokumentation
- Finaler Testlauf & Präsentation vorbereiten
