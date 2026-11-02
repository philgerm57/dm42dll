// Validation de dm42dll : une instruction d'exemple pour chaque forme de la table d'encodage,
// encodée puis décodée ; le texte décodé doit redonner les mêmes octets.
using System.Text;
using System.Text.RegularExpressions;
using DM42.encoder;
using DM42.decoder;

string outDir = args.Length > 0 ? args[0] : ".";
Encoder42.initialize();
Decoder42.initialize();

// valeurs d'essai : pour chaque groupe du motif, on garde celles qu'il accepte
string[] pool =
{
    "X", "Y", "Z", "T", "L",
    "00", "01", "05", "09", "10", "11", "14", "15", "16", "18", "25", "35", "81", "99",
    "0", "1", "9",
    "A", "J", "a", "e",
    "\"ABC\"", "\"ABCDEFGHIJKLMN\"", "\"Σ÷×\"", "\"ABCDEFGHIJKLMNO\"",
};

var lines = new List<string>();
var report = new StringBuilder();
int nInst = 0, nExpected = 0, nOk = 0, nAlias = 0, nBad = 0, nEncErr = 0, nNoSample = 0;

foreach (var kv in Encoder42.rpnMap.OrderBy(k => k.Key, StringComparer.Ordinal))
    foreach (var pat in kv.Value)
    {
        var samples = Instantiate(pat.regex);
        if (samples.Count == 0)
        {
            nNoSample++;
            report.AppendLine($"? pas d'exemple généré pour le motif {kv.Key} : {pat.regex}");
            continue;
        }
        foreach (var s in samples)
            if (s != "END" && s != ".END.") Check(s, $"{kv.Key} ({pat.regex})");   // END : fin de programme, testé partout
    }

// nombres et chaînes : traités hors de la table
foreach (var s in new[] { "ACCEL", "DATE+", "0", "1", "-1", "1.5", "-2.5", "0.5", "123456789012", "1E-10", "-1.234E99", "6.02E23",
                          "\"\"", "\"A\"", "\"HELLO WORLD 15\"", "⊢\"APPEND\"", "⊢\"ABCDEFGHIJKLMN\"" })
    Check(s, "nombre / chaîne");

File.WriteAllText(Path.Combine(outDir, "validation-dm42dll.txt"),
    $"Instructions testées : {nInst}\n  identiques après décodage : {nOk}\n  équivalentes (autre écriture, mêmes octets) : {nAlias}\n" +
    $"  ÉCARTS (octets différents après décodage) : {nBad}\n  refusées par l'encodeur : {nEncErr}\n  motifs sans exemple : {nNoSample}\n\n" + report);

// listing complet chargeable dans Free42 / DM42, et son .raw
var listing = new List<string> { "LBL \"VALID\"" };
listing.AddRange(lines);
listing.Add("END");
File.WriteAllText(Path.Combine(outDir, "VALID.free42"), "{ }\r\n" + string.Join("\r\n", listing) + "\r\n");
var all = Enc(listing, out var errAll);
if (all != null)
    File.WriteAllBytes(Path.Combine(outDir, "VALID.raw"), all.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(h => Convert.ToByte(h, 16)).ToArray());
Console.WriteLine($"{nInst} instructions : {nExpected} refus attendus, {nOk} identiques, {nAlias} équivalentes, {nBad} écarts, {nEncErr} refusées, {nNoSample} motifs sans exemple" +
                  (errAll != null ? $" | listing complet : {errAll}" : ""));
// code de sortie 1 s'il reste un écart, un refus inattendu ou un motif non testé (pour build.bat test)
Environment.ExitCode = nBad + nEncErr + nNoSample > 0 || errAll != null ? 1 : 0;

void Check(string inst, string origin)
{
    nInst++;
    var hex = Enc(new[] { "LBL \"T\"", inst, "END" }, out var err);
    // refus voulus : chaîne vide, nom trop long pour ASSIGN / KEY (octet Fn limité à 15)
    if (hex == null && (err == "empty string" || err.StartsWith("name too long"))) { nExpected++; report.AppendLine($"✓ REFUS ATTENDU {inst,-30} {err}"); return; }
    if (hex == null) { nEncErr++; report.AppendLine($"✖ ENCODAGE  {inst,-34} {err}   [{origin}]"); return; }
    string dec = Dec(hex);
    lines.Add(inst);
    if (Norm(dec) == Norm(inst)) { nOk++; return; }
    var hex2 = dec == null ? null : Enc(new[] { "LBL \"T\"", dec, "END" }, out _);
    if (hex2 == hex) { nAlias++; report.AppendLine($"≈ ÉQUIVALENT {inst,-34} → {dec}"); return; }
    nBad++;
    report.AppendLine($"✖ ÉCART     {inst,-34} → {dec ?? "(rien)"}   octets {Body(hex)} / {(hex2 == null ? "non ré-encodable" : Body(hex2))}   [{origin}]");
}

static string Norm(string s) => s == null ? null : Regex.Replace(s.Trim(), @"\s+", " ");

// octets de l'instruction seule (sans LBL "T" ni END)
static string Body(string hex)
{
    var b = hex.Split(' ');
    return string.Join(" ", b.Skip(5).Take(b.Length - 5 - 3));
}

static string Enc(IEnumerable<string> src, out string err)
{
    var doc = new List<string> { "{ }" };
    doc.AddRange(src.Select(Encoder42.FormatRpnLine));
    var p = new RpnParser(); p.SetDocument(doc); p.parse();
    err = null;
    var hex = new List<string>();
    foreach (var prog in p.programs)
        foreach (RawLine l in prog.rawLines)
        {
            try { Encoder42.toRaw(l, "free42"); }
            catch (Exception ex) { err = "exception : " + ex.Message; return null; }
            if (l.error != null) { err = l.error.Message; return null; }
            if (string.IsNullOrEmpty(l.raw)) continue;
            var bad = l.raw.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(h => !Regex.IsMatch(h, "^[0-9A-Fa-f]{2}$"));
            if (bad != null) { err = $"octets invalides « {l.raw.Trim()} » pour « {l.docCode} »"; return null; }
            hex.Add(l.raw.Trim());
        }
    return hex.Count == 0 ? null : string.Join(" ", hex);
}

// texte de l'instruction entre LBL "T" et END
static string Dec(string hex)
{
    try
    {
        var d = new dm42decoder(); d.SetDocument(hex);
        var rpn = d.decode().getRpn("\n", false).Split('\n').Select(x => x.Trim()).ToList();
        int i = rpn.IndexOf("LBL \"T\"");
        if (i < 0 || i + 1 >= rpn.Count) return null;
        var body = rpn.Skip(i + 1).TakeWhile(x => x != "END" && x != ".END.").ToList();
        return body.Count == 0 ? null : string.Join(" | ", body);
    }
    catch (Exception ex) { return "exception : " + ex.Message; }
}

// remplace chaque groupe (...) du motif par des valeurs qu'il accepte
List<string> Instantiate(string rx)
{
    var groups = new List<(int start, int end)>();
    int depth = 0, st = -1;
    for (int i = 0; i < rx.Length; i++)
    {
        if (rx[i] == '\\') { i++; continue; }
        if (rx[i] == '(') { if (depth++ == 0) st = i; }
        else if (rx[i] == ')' && --depth == 0) groups.Add((st, i));
    }
    var choices = groups.Select(g =>
    {
        var inner = new Regex("^(?:" + rx.Substring(g.start + 1, g.end - g.start - 1) + ")$");
        return pool.Where(v => inner.IsMatch(v)).ToList();
    }).ToList();
    if (choices.Any(c => c.Count == 0)) return new List<string>();
    int n = choices.Count == 0 ? 1 : choices.Max(c => c.Count);
    var full = new Regex("^(?:" + rx + ")$");
    var res = new List<string>();
    for (int k = 0; k < n; k++)
    {
        var sb = new StringBuilder();
        int pos = 0;
        for (int g = 0; g < groups.Count; g++)
        {
            sb.Append(Literal(rx.Substring(pos, groups[g].start - pos)));
            sb.Append(choices[g][k % choices[g].Count]);
            pos = groups[g].end + 1;
        }
        sb.Append(Literal(rx.Substring(pos)));
        var s = sb.ToString();
        if (full.IsMatch(s) && !res.Contains(s)) res.Add(s);
    }
    return res;
}

static string Literal(string part) => Regex.Replace(part, @"\\(.)", "$1");
