# dm42dll

Bibliothèque C# (.NET Framework 4.7.2) qui **encode** un listing RPN HP-42S / DM42 / Free42 en fichier `.raw`
et **décode** un `.raw` en listing lisible.

Elle est utilisée par [DM42 Editor](https://github.com/philgerm57/DM42Editor) et contient aussi
**AppTestDll**, une petite application Windows Forms pour essayer l'encodeur et le décodeur.

## Compiler

Prérequis : [pack de développement .NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/net472),
ou Visual Studio avec la charge de travail .NET desktop.

`build.bat` compile la solution (DLL et AppTestDll) avec le MSBuild de Visual Studio, ou à défaut `dotnet build` ;
`build.bat test` lance ensuite la validation décrite plus bas, `build.bat release` compile en Release.

```bash
dotnet build dm42dll.csproj -c Debug
```

La DLL est produite dans `bin\Debug\dm42dll.dll`, l'emplacement attendu par DM42 Editor.
La solution `dm42dll.sln` compile la bibliothèque et AppTestDll.

## Utilisation

### Encoder un listing en `.raw`

```csharp
using DM42.encoder;

Encoder42.initialize();
var doc = new List<string>();
foreach (string line in listing)            // une instruction par ligne, commençant par { }
    doc.Add(Encoder42.FormatRpnLine(line));

var parser = new RpnParser();
parser.SetDocument(doc);
parser.parse();
foreach (var program in parser.programs)
    foreach (RawLine l in program.rawLines)
    {
        Encoder42.toRaw(l, "free42");
        // l.raw : octets en hexadécimal, ou l.error si l'instruction est refusée
    }
```

Le listing doit commencer par une ligne d'en-tête `{ }`.
La division s'écrit `÷` (le caractère `/` est lu comme `+/-`).

### Décoder un `.raw`

```csharp
using DM42.decoder;

Decoder42.initialize();
var decoder = new dm42decoder();
decoder.SetDocument(hex);                    // contenu du .raw en hexadécimal : "C0 00 F6 00 …"
decoderresult result = decoder.decode();
string rpn = result.getRpn("\n", true);      // listing avec numéros de ligne
```

## Licence

GNU GPLv3 pour l'ensemble. Le cœur de l'encodeur / décodeur est une traduction en C# du code TypeScript
de J. Heilingbrunner (projet vscode-hp42s / Free42), publié sous licence MIT : voir [LICENCE.txt](LICENCE.txt).

## Validation

`Tests/Validation` génère une instruction d'exemple pour chaque forme de la table d'encodage
(ST X…L, IND, registres 00-99, noms, labels locaux, drapeaux, nombres, chaînes…), soit environ 2 100 instructions.
Chacune est encodée puis décodée, et le texte décodé doit redonner les mêmes octets.

```bash
dotnet build dm42dll.csproj -c Debug
dotnet run --project Tests/Validation -- Tests/Validation
```

Résultats dans `Tests/Validation` :

- `validation-dm42dll.txt` : bilan et détail des écarts ;
- `VALID.free42` et `VALID.raw` : le programme complet, à charger dans Free42 ou la DM42 pour comparer avec la calculatrice.
