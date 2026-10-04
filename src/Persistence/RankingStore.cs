using System.Globalization;
using System.Text;
using System.Text.Json;
using OpenGDS2.Domain;

namespace OpenGDS2.Persistence;

public static class RankingStore
{
    public static RankingBook Load(string path)
    {
        if (!File.Exists(path)) return new();
        if (new FileInfo(path).Length>100_000) throw new InvalidDataException("Ranking file is too large.");
        var book=JsonSerializer.Deserialize<RankingBook>(File.ReadAllText(path)) ?? throw new InvalidDataException("Empty ranking file.");
        RankingService.Validate(book);return book;
    }
    public static void Save(string path,RankingBook book)
    {
        RankingService.Validate(book);Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string temporary=path+".tmp";
        File.WriteAllText(temporary,JsonSerializer.Serialize(book,new JsonSerializerOptions {WriteIndented=true}));
        File.Move(temporary,path,true);
    }
    public static RankingBook ImportOriginal(byte[] bytes)
    {
        if (bytes.Length>100_000) throw new InvalidDataException("Original ranking file is too large.");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding=Encoding.GetEncoding(932,EncoderFallback.ExceptionFallback,DecoderFallback.ExceptionFallback);
        var rows=ReadRows(encoding.GetString(bytes));
        if (rows.Count!=7 || rows.Any(row=>row.Count!=6)) throw new InvalidDataException("Unsupported GameRank.GDS shape.");
        var book=new RankingBook();
        for (int i=0;i<6;i++)
        {
            if (rows[0][i]=="-1") continue;
            if (rows[0][i]!="1") throw new InvalidDataException("Unknown ranking marker.");
            if (!int.TryParse(rows[4][i],NumberStyles.Integer,CultureInfo.InvariantCulture,out int year) ||
                !int.TryParse(rows[5][i],NumberStyles.Integer,CultureInfo.InvariantCulture,out int month) ||
                !int.TryParse(rows[6][i],NumberStyles.Integer,CultureInfo.InvariantCulture,out int week)) throw new InvalidDataException("Invalid original ranking date.");
            book.Records[i]=new() {Company=rows[1][i],OriginalGameDateText=rows[2][i],RecordedOn=rows[3][i],AchievedOn=new(year,month,week)};
        }
        RankingService.Validate(book);return book;
    }
    private static List<List<string>> ReadRows(string text)
    {
        var rows=new List<List<string>>();var row=new List<string>();var cell=new StringBuilder();
        bool quoted=false,closed=false;
        for (int i=0;i<text.Length;i++)
        {
            char c=text[i];
            if (quoted)
            {
                if (c=='"') {if (i+1<text.Length && text[i+1]=='"') {cell.Append('"');i++;} else {quoted=false;closed=true;}}
                else cell.Append(c);
                continue;
            }
            if (c=='"') {if (cell.Length>0 || closed) throw new InvalidDataException("Malformed quoted ranking field.");quoted=true;continue;}
            if (c==',' || c=='\r' || c=='\n')
            {
                row.Add(cell.ToString());cell.Clear();closed=false;
                if (c!=',') {rows.Add(row);row=[];if (c=='\r' && i+1<text.Length && text[i+1]=='\n') i++;}
            }
            else {if (closed) throw new InvalidDataException("Trailing quoted ranking field data.");cell.Append(c);}
        }
        if (quoted) throw new InvalidDataException("Unterminated ranking field.");
        if (cell.Length>0 || row.Count>0 || closed) {row.Add(cell.ToString());rows.Add(row);}
        return rows;
    }
}
