# GTA World Game Integration Layer

Questa cartella contiene il layer di progetto per collegare:

- UMA2 / Dynamic Character Avatar per sesso, DNA e vestiti.
- Opsive Third Person Controller per movimento, camera, inventario, armi e IK.
- Mappe reali generate da OpenStreetMap.

## Menu Unity

Dopo la compilazione trovi i menu in **Tools > Game**:

1. **ONE CLICK - Create Complete GTA Demo**: bottone principale; ora copia prima la scena demo Opsive `Third Person Shooter`, aggiorna input tastiera/mouse e sostituisce/prepara il player con UMA.
2. **Create Complete Playable Scene**: fallback sandbox generato da zero; crea avatar UMA/Opsive, camera, manager Opsive, anchor OSM, pickup armi, cavallo/veicolo interagibili, controller demo runtime, luce e terreno temporaneo.
3. **Create Project Folders**: crea la struttura base sotto `Assets/Game`.
4. **Create > UMA + Opsive Avatar Template**: crea un GameObject/prefab template con componenti di base, bridge UMA e mount armi.
5. **Prepare Selected UMA Avatar For Opsive**: usa questo su un avatar UMA già presente in scena.
6. **Create > Scene Gameplay Bootstrap**: crea l'oggetto `Game` con manager Opsive opzionali e un anchor per la mappa OSM.
7. **Create Weapon Mounts On Selected Avatar**: aggiunge mount su mano destra, mano sinistra, schiena e fianco per armi/IK.
8. **Build Full Opsive Character + Item Loadout**: comando avanzato sul player selezionato; aggiunge componenti Inventory/ItemHandler/InventoryHandler/ControllerHandler, ItemPlacement sui mount e genera ItemType/loadout Opsive sotto `Assets/Game/OpsiveGenerated`.

## Workflow consigliato

1. Per partire subito, esegui **Tools > Game > ONE CLICK - Create Complete GTA Demo**. Questo è il percorso consigliato: parte dalla scena già funzionante di Opsive TPC invece di ricreare tutto da zero.
2. Importa/genera la mappa OSM e mettila sotto `World_Map_Setup/OSM_Map_Root`; finche non importi dati reali, il menu crea una mini-citta placeholder piu grande con terreno verde, strade grigie e edifici chiari.
3. Se hai già un UMA Dynamic Character Avatar, selezionalo ed esegui **Tools > Game > Prepare Selected UMA Avatar For Opsive**.
4. Il menu aggiorna gli assi dell'Input Manager di Opsive e prova a impostare `Active Input Handling = Both`; se Unity mostra ancora errori `UnityEngine.Input`, imposta manualmente **Project Settings > Player > Active Input Handling > Both** e riavvia Unity.
5. Configura vestiti e DNA dal componente `GameAvatarIntegration` o chiamando i suoi metodi da UI/gameplay.
6. Configura prefab armi usando i mount creati da `GameWeaponMounts`.
7. Premi Play e usa il pannello demo: i bottoni cambiano sesso/race UMA, randomizzano alcuni DNA demo e mostrano armi preview sui mount; la riga Status conferma ogni click. Puoi anche usare F1/F2/F3/1-4/H quando la Game view ha il focus.
8. Muovi il player con WASD/frecce e Shift per sprint usando il mover fallback compatibile con Input System; il mover pilota anche i parametri `Horizontal Input` / `Forward Input`, disabilita root motion e aggiunge un layer procedurale di camminata se il controller Opsive resta bloccato in idle.
9. Premi `E` vicino al cavallo o al veicolo placeholder per montare/smontare nella demo temporanea.
10. Il menu prova prima il prefab personaggio di UMA Settings e poi `Assets/UMA/Getting Started/UMADynamicCharacterAvatar.prefab`; se nessuno dei due esiste/importa correttamente, segnala che bisogna configurare un prefab UMA reale.
11. I bottoni armi assegnano automaticamente alcuni prefab reali disponibili nel progetto al controller demo e li montano sulla mano/schiena disattivando script/collider da pickup. In parallelo il menu costruisce anche un loadout Opsive vero con ItemType e Inventory.DefaultLoadout.
12. Per poter mostrare una demo giocabile subito, `GameFallbackWeaponController` equipaggia almeno un'arma reale in mano, spara con click sinistro, mostra un tracer, gestisce munizioni e reload con `R` mentre finiamo di tarare il pipeline Opsive completo.
13. Il bridge runtime prova a chiamare via reflection equip con tasti `1-4`, uso con click sinistro e reload con `R` sui componenti Opsive disponibili; il caricamento automatico del default loadout resta disattivato di default per evitare duplicati ItemType finché il pipeline Opsive viene tarato.
14. Il menu assegna anche il controller animator demo `Third Person Shooter/Animator/Shooter.controller` all'avatar UMA quando l'asset è presente; nella scena copiata da Opsive prova a preservare il player/controller Opsive originale e a sostituire solo la visual UMA. I materiali della mappa placeholder usano shader URP/Unlit per evitare palazzi rosa. Per completare il sistema Opsive al 100% vanno ancora verificati in Play Mode ItemSet/equip/use/fire/IK con la versione esatta del pacchetto Opsive installato.

## Nota networking

Il vecchio UNet di Unity non è necessario per l'obiettivo attuale. Mantieni disattivata l'opzione `Is Networked` finché non scegli un sistema multiplayer moderno.

## Compatibilita legacy UNet

Il progetto include anche uno shim minimale `Assets/Game/Compatibility/UnityEngine.Networking` per permettere al DLL legacy di Opsive di caricarsi in Unity moderno quando manca il vecchio assembly `UnityEngine.Networking`. Non implementa multiplayer reale: serve solo a evitare che il plugin venga scaricato come assembly rotto. Per multiplayer vero scegli in seguito una soluzione moderna e rimuovi/sostituisci questo shim se installi un pacchetto che fornisce davvero `UnityEngine.Networking`.
