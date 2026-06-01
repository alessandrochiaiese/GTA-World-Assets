# GTA World Game Integration Layer

Questa cartella contiene il layer di progetto per collegare:

- UMA2 / Dynamic Character Avatar per sesso, DNA e vestiti.
- Opsive Third Person Controller per movimento, camera, inventario, armi e IK.
- Mappe reali generate da OpenStreetMap.

## Menu Unity

Dopo la compilazione trovi i menu in **Tools > Game**:

1. **Create Complete Playable Scene**: crea con un click avatar UMA/Opsive, camera, manager Opsive, anchor OSM, pickup armi, cavallo/veicolo placeholder, controller demo runtime, luce e terreno temporaneo.
2. **Create Project Folders**: crea la struttura base sotto `Assets/Game`.
3. **Create > UMA + Opsive Avatar Template**: crea un GameObject/prefab template con componenti di base, bridge UMA e mount armi.
4. **Prepare Selected UMA Avatar For Opsive**: usa questo su un avatar UMA già presente in scena.
5. **Create > Scene Gameplay Bootstrap**: crea l'oggetto `Game` con manager Opsive opzionali e un anchor per la mappa OSM.
6. **Create Weapon Mounts On Selected Avatar**: aggiunge mount su mano destra, mano sinistra, schiena e fianco per armi/IK.

## Workflow consigliato

1. Per partire subito, esegui **Tools > Game > Create Complete Playable Scene**.
2. Importa/genera la mappa OSM e mettila sotto `World_Map_Setup/OSM_Map_Root`.
3. Se hai già un UMA Dynamic Character Avatar, selezionalo ed esegui **Tools > Game > Prepare Selected UMA Avatar For Opsive**.
4. Usa il Character Builder di Opsive solo in modalità locale/non-networked per ora.
5. Configura vestiti e DNA dal componente `GameAvatarIntegration` o chiamando i suoi metodi da UI/gameplay.
6. Configura prefab armi usando i mount creati da `GameWeaponMounts`.
7. Premi Play e usa il pannello demo: F1/F2 cambiano sesso/race UMA, F3 randomizza alcuni DNA demo, 1-4 mostrano armi preview sui mount.
8. I pickup armi e i placeholder cavallo/veicolo creati dal menu sono una base da rifinire con gli item type, animator e prefab definitivi di Opsive.

## Nota networking

Il vecchio UNet di Unity non è necessario per l'obiettivo attuale. Mantieni disattivata l'opzione `Is Networked` finché non scegli un sistema multiplayer moderno.

## Compatibilita legacy UNet

Il progetto include anche uno shim minimale `Assets/Game/Compatibility/UnityEngine.Networking` per permettere al DLL legacy di Opsive di caricarsi in Unity moderno quando manca il vecchio assembly `UnityEngine.Networking`. Non implementa multiplayer reale: serve solo a evitare che il plugin venga scaricato come assembly rotto. Per multiplayer vero scegli in seguito una soluzione moderna e rimuovi/sostituisci questo shim se installi un pacchetto che fornisce davvero `UnityEngine.Networking`.
