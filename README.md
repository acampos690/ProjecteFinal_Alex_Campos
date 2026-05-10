🎮 Manual d’usuari – Adventure Time Platformer

🌟 Introducció

Aquest projecte és un videojoc de plataformes 2D desenvolupat amb Unity inspirat en l’univers d’Adventure Time. El jugador controla Finn i ha d’explorar diferents nivells, derrotar enemics, superar obstacles i avançar guardant el seu progrés mitjançant una API connectada a una base de dades MySQL.

El joc inclou sistema de login i registre, guardat de partida, estadístiques del jugador i diferents mecàniques de combat i exploració.

🛠️ Com començar

1. Crear un compte

En iniciar el joc apareix el menú principal.
Per jugar és necessari registrar-se primer.

Passos:

Entrar al menú Register
Introduir:
Nom d’usuari
Contrasenya
Prémer el botó de registre

(Images/imagen2.png)
Si el registre és correcte, el compte quedarà guardat a la base de dades.

2. Iniciar sessió

Després del registre:

Entrar al menú Login
Escriure el nom d’usuari i la contrasenya
Prémer el botó d’iniciar sessió
(Images/IMAGEN1.png)
Quan el login és correcte, el jugador accedeix al joc i es carreguen automàticament les seves dades guardades.

⌨️ Controls del joc

Acció	Teclat
Moure’s	A / D
Saltar	Espai
Atacar	Shift esquerre
Interactuar	E
Pausa	ESC

🗺️ Mecàniques principals

🏃 Moviment

El jugador pot desplaçar-se lateralment pels escenaris utilitzant les tecles A i D. El moviment inclou físiques de plataformes i salts.

⚔️ Combat

Finn pot atacar enemics utilitzant la seva espasa prement la tecla Shift esquerre. Els enemics poden causar dany al jugador, reduint la vida actual.

❤️ Sistema de vida

El jugador disposa d’una quantitat de vida màxima.
Quan rep dany, la barra de vida disminueix.

La vida queda guardada automàticament a la base de dades mitjançant la API.

💾 Guardat de progrés

El joc guarda:

Posició del jugador
Escena actual
Monedes
Vida
Estadístiques

Quan el jugador torna a iniciar sessió, el progrés es carrega automàticament.

⚙️ Sistema d’ajustos

El joc permet modificar:

Volum
Resolució

Els ajustos també es guarden a la base de dades per mantenir la configuració entre sessions.

👾 Elements del joc

Personatge principal
Finn és el protagonista controlat pel jugador.

Enemics
Durant els nivells apareixen enemics que ataquen el jugador i dificulten el progrés.

NPCs
Hi ha personatges interactius que mostren diàlegs i poden donar informació al jugador.

🌍 Escenaris

El joc està format per diferents nivells i zones connectades.
Cada escena té el seu propi punt de respawn i progrés guardat.

🚀 Requisits del sistema

Component	Requisit
Sistema Operatiu	Windows 10 o superior
Memòria RAM	4 GB
Processador	Dual Core
Motor gràfic	Compatible amb Unity
Connexió	Internet per utilitzar la API

🔒 Sistema de dades

El projecte utilitza:

Unity com a motor gràfic
ASP.NET Core API REST per gestionar les dades
MySQL per guardar usuaris i progrés

Les contrasenyes es guarden xifrades per millorar la seguretat.

🤝 Crèdits

Apartat	Autor
Desenvolupament del joc	Alex Campos
Programació API	Alex Campos
Base de dades	Alex Campos
Disseny i integració	Alex Campos
