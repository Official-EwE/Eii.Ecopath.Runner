# Eii.Ecopath.RunConsole
Automation run console for EwE

```mermaid
flowchart TD
    A["CLI args\n(--runinfofile, --output)"] --> B["Program.cs\nParses args"]
    B --> C["JSON file\nDeserialized into cEwERunInstructions"]
    C --> D["cEwEEngine\nOrchestrates the run"]
    D --> E["Load EwE model\n(Ecopath)"]
    E --> F["Run Ecopath"]
    F --> G{"Ecosim\nscenario?"}
    G -- Yes --> H["Load & Run Ecosim\ncEcosimModifier"]
    H --> I{"Ecospace\nscenario?"}
    I -- Yes --> J["Load & Run Ecospace\ncEcospaceModifier"]
    J --> K["Done ✓"]
    G -- No --> K
    I -- No --> K

    ```