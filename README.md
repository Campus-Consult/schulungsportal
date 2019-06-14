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