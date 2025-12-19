PASUL 2 - Cum deschizi si rulezi proiectul (Windows 11)

1) Instaleaza Visual Studio 2022 (Community e ok)
   - Workload: "Desktop development with .NET"

2) Deschide solutia:
   FinanteRaportare.sln

3) Build:
   Build > Build Solution

4) Ruleaza:
   F5

5) Baza de date:
   Se creeaza automat in:
   %LOCALAPPDATA%\FinanteRaportare\finante.db

In acest pas ai:
- CRUD Clienti
- CRUD Furnizori
- Disponibilitati zilnice (Seif/BT/BCR/Trezorerie)

- CRUD Facturi furnizori (Document, Data, Scadenta, CEC/OP, Suma, Achitata)
- Raport PDF: FURNIZORI NEACHITATI LA DATA (stil listare)
  * subtotal pe zi (dupa data scadentei)
  * subtotal saptamanal la fiecare DUMINICA
  * TOTAL GENERAL la final

Urmatorul pas:
- Facturi clienti + raport CLIENTI NEINCASATI (aceeasi logica subtotal)
- Total solduri / client si / furnizor
