# Schulungsportal

## Neue Verwalter/Admins aufnehmen
Man kann sich nur noch per invite registrieren, dafür muss ein Verwalter/Admin unter `/Manage/AddManager` die E-Mail Adresse des Neulings submitten, damit dieser eine Invite-Mail bekommt.

## Konfiguration

### Admin
Damit es einfacher ist, das Schulungsportal einzurichten kann man in der release config (`appsettings.Release.json`) einen default Admin user hinterlegen, damit dieser und die Verwaltungsrolle angelegt werden kann

### Datenbank
Um das Projekt auf einem Produktivsystem laufen zu lassen muss ein anderer Datenbankstring verwendet werden, der unter der Sektion `ConnectionString` und dann `DefaultConnection` eingetragen wird

### Email
Um zu vermeiden, dass Passwörter eingeschickt werden müssen, wird die Konfiguration des Email-Kontos ebenfalls in der Konfiguration vollbracht, der Aufbau sieht wie folgt aus:
```
  "EMailOptions": {
    "Mailserver": "smtp.office365.com",
    "Port": 587,
    "Absender": "someone@something.org",
    "Passwort": "p4ssw0rd"
  }
```
Diese Konfiguration kann man anlegen wenn man unter Schulungsportal 2 Rechtsklickt um dann `Geheime Benutzerschlüssel verwalten` auswählt

Leider scheint es damit nicht möglich zu sein, sich selber Termine zu schicken, da office365 sich querstellt

### Beispiel appsettings.Release.json
```
{
  "EMailOptions": {
    "Mailserver": "smtp.office365.com",
    "Port": 587,
    "Absender": "EMailAdresse des Schulungsportals",
    "Passwort": "Passwort für den EMail Account"
  },
  "DefaultAdminUser": {
    "UserEmail": "EMail des Default Nutzers des Schulungsportals",
    "UserPassword": "Passwort für den default Nutzer"
  },
  "ConnectionStrings": {
    "type": "mysql",
    "DefaultConnection": "server=localhost;port=3306;database=databasename;user=databaseuser;password=databasepassword"
  }
}
```



## Development

Um dynamische Elemente auf der Clientseite zu ermöglichen, wird das Javascript-Framework Vue verwendet. Um die Dateien zusammenzufassen und so zu "bundlen" wird der Parcel-Bundler mit npm verwendet.

Als erstes muss npm über node.js heruntergeladen und installiert werden: [download](https://nodejs.org/en/). Dann muss in der Kommandozeile im Ordner `Schulungsportal 2` der Befehl `npm install` ausgeführt werden, um alle Abhängigkeiten zu installieren. Diese Abhängigkeiten und auch alle `npm run` Befehle sind in `package.json` konfiguriert.

Die Source Dateien für die JS-Client-Seite befinden sich in `js/src`, Vue mit TypeScript.

Um das Javascript zu bauen muss man `npm run build` ausführen, die fertigen Dateien liegen dann in `wwwroot/dist`. Um während des Developments alle Änderungen zu überwachen, muss man `npm run watch` ausführen.

Um das gesammte Programm mit allen Komponenten releasefertig vorzubereiten, übernimmt `npm run release` die Schritte des JavaScript und C# Übersetzens.
